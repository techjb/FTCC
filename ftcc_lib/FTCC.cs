using System.Text;
using ZstdNet;

namespace ftcc_lib
{
    public class FTCC
    {
        private readonly FTCCOptions FTCCOptions;
        private readonly Dictionary<string, List<byte[]>> TrainingList = new();
        private readonly Dictionary<string, List<List<byte[]>>> TrainingListChunked = new();
        private readonly Dictionary<string, List<byte[]>> Dictionaries = new();
        private int Total = 0;
        private int Processed = 0;
        private int Sucess = 0;

        public FTCC(FTCCOptions fTCCOptions)
        {
            FTCCOptions = fTCCOptions;
            if (FTCCOptions.DictionariesPath != null)
            {
                Dictionaries = Serialization.Deserialize(FTCCOptions.DictionariesPath);
            }
            else
            {
                Initialize();
            }
        }

        public double PredictFile(string testFile)
        {
            var list = Csv.ToList(FTCCOptions, testFile);
            Total = list.Count;
            Processed = 0;
            Sucess = 0;

            if (FTCCOptions.ParallelismOnTestFile)
            {
                PredictWithParalellism(list);
            }
            else
            {
                PredictWithoutParalellism(list);
            }

            return GetSucessRatio();
        }

        private void PredictWithParalellism(List<Tuple<string, string>> testList)
        {
            Parallel.ForEach(testList, test =>
            {
                string predictedClass = Predict(test.Item1);
                SetPrediction(test, predictedClass);
            });
        }

        private void PredictWithoutParalellism(List<Tuple<string, string>> testList)
        {
            foreach (var test in testList)
            {
                string predictedClass = Predict(test.Item1);
                SetPrediction(test, predictedClass);
            }
        }

        private void SetPrediction(Tuple<string, string> test, string predictedClass)
        {
            bool predicionSucess = predictedClass.Equals(test.Item2.Trim());
            if (FTCCOptions.ParallelismOnTestFile)
            {
                Interlocked.Increment(ref Processed);
                if (predicionSucess)
                {
                    Interlocked.Increment(ref Sucess);
                }
            }
            else
            {
                Processed++;
                if (predicionSucess)
                {
                    Sucess++;
                }
            }

            OutputConsole();
        }

        private void OutputConsole()
        {
            if (!FTCCOptions.ConsoleOutput)
            {
                return;
            }
            var successRatio = GetSucessRatio();
            var processedPercentage = Math.Round((float)Processed * 100 / Total, 2);

            Console.WriteLine(
                "Processed: " + Processed + "/" + Total + " (" + processedPercentage + "%) " +
                "Sucess: " + Sucess + " (" + successRatio + ")");
        }

        private double GetSucessRatio()
        {
            return Math.Round((float)Sucess / Processed, 3);
        }

        private void Initialize()
        {
            var list = Csv.ToList(FTCCOptions, FTCCOptions.TrainFile);
            InitializeTraingList(list);
            MakeTrainingListChunks();
            InitializeDictionaries();
        }

        private void InitializeTraingList(List<Tuple<string, string>> list)
        {
            TrainingList.Clear();
            if (FTCCOptions.ParallelismToInitialize)
            {
                InitializeWithParalellism(list);
            }
            else
            {
                InitializeWithoutParalellism(list);
            }
        }


        private void InitializeWithParalellism(List<Tuple<string, string>> list)
        {
            var sync = new object();
            Parallel.ForEach(list, item =>
            {
                string key = item.Item2;
                var bytes = Encoding.UTF8.GetBytes(item.Item1);
                lock (sync)
                {
                    AddToTrainigList(key, bytes);
                }
            });
        }

        private void InitializeWithoutParalellism(List<Tuple<string, string>> list)
        {
            foreach (var item in list)
            {
                string key = item.Item2;
                var bytes = Encoding.UTF8.GetBytes(item.Item1);
                AddToTrainigList(key, bytes);
            }
        }

        private void AddToTrainigList(string key, byte[] bytes)
        {
            if (TrainingList.ContainsKey(key))
            {
                TrainingList[key].Add(bytes);
            }
            else
            {
                var listItems = new List<byte[]>
                {
                    bytes
                };
                TrainingList.Add(key, listItems);
            }
        }

        private void MakeTrainingListChunks()
        {
            TrainingListChunked.Clear();
            foreach (var item in TrainingList)
            {
                var values = DivideList(item.Value);
                TrainingListChunked.Add(item.Key, values);
            }
        }

        List<List<byte[]>> DivideList(List<byte[]> mainList)
        {
            int size = mainList.Count / FTCCOptions.CompressorsPerClass;
            int remainder = mainList.Count % FTCCOptions.CompressorsPerClass;

            List<List<byte[]>> dividedList = new();

            int startIndex = 0;
            for (int i = 0; i < FTCCOptions.CompressorsPerClass; i++)
            {
                int currentSize = size + (i < remainder ? 1 : 0);
                List<byte[]> subList = mainList.GetRange(startIndex, currentSize);
                dividedList.Add(subList);
                startIndex += currentSize;
            }

            return dividedList;
        }

        private void InitializeDictionaries()
        {
            Dictionaries.Clear();
            if (FTCCOptions.ParallelismToInitialize)
            {
                InitializeDictionariesWithParalellism();
            }
            else
            {
                InitializeDictionariesWithoutParalellism();
            }
        }

        private void InitializeDictionariesWithParalellism()
        {
            var sync = new object();
            Parallel.ForEach(TrainingListChunked, items =>
            {
                AddToDictionary(items);
            });
        }

        private void InitializeDictionariesWithoutParalellism()
        {
            foreach (var items in TrainingListChunked)
            {
                AddToDictionary(items);
            }
        }

        private void AddToDictionary(KeyValuePair<string, List<List<byte[]>>> items, object? sync = null)
        {
            List<byte[]> list = new();
            foreach (var item in items.Value)
            {
                byte[] dictionary = DictBuilder.TrainFromBuffer(item);
                list.Add(dictionary);

            }
            if (sync != null)
            {
                lock (sync)
                {
                    Dictionaries.Add(items.Key, list);
                }
            }
            else
            {
                Dictionaries.Add(items.Key, list);
            }
        }

        public string Predict(string text)
        {
            byte[] bytes = Encoding.UTF8.GetBytes(text);
            double? minimumSize = null;
            string? predictedClass = null;
            foreach (var dictionary in Dictionaries)
            {
                var length = GetLength(dictionary.Value, bytes);
                if (length ==null)
                {
                    continue;
                }
                if (minimumSize == null || length < minimumSize)
                {
                    minimumSize = length;
                    predictedClass = dictionary.Key;
                }
            }
            return predictedClass!;
        }

        private double? GetLength(List<byte[]> dictionaries, byte[] bytes)
        {
            List<int> list = new();
            foreach (var dictionary in dictionaries)
            {
                var compressionOptions = new CompressionOptions(dictionary, FTCCOptions.CompressionLevel);
                using var compressor = new Compressor(compressionOptions);
                var compressed = compressor.Wrap(bytes);
                list.Add(compressed.Length);
            }
            if(list.Count.Equals(0))
            {
                return null;
            }
            return list.Average();            
        }

        public void SerializeDiccionaries(string path)
        {
            Serialization.Serialize(path, Dictionaries);
        }
    }
}
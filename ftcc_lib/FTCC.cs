using CsvHelper;
using CsvHelper.Configuration;
using System.Globalization;
using System.Text;
using ZstdNet;

namespace ftcc_lib
{
    public class FTCC
    {
        private readonly FTCCOptions FTCCOptions;
        private readonly Dictionary<string, List<byte[]>> TrainingList = new();
        private readonly Dictionary<string, byte[]> TrainingDictionary = new();
        private int Total = 0;
        private int Processed = 0;
        private int Sucess = 0;

        public FTCC(FTCCOptions fTCCOptions)
        {
            FTCCOptions = fTCCOptions;
            var list = CsvToList(FTCCOptions.TrainFile);
            Initialize(list);
        }

        public double PredictFile(string testFile)
        {
            var list = CsvToList(testFile);
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
                "Sucess ratio: " + Sucess + " (" + successRatio + ")");
        }

        private double GetSucessRatio()
        {
            return Math.Round((float)Sucess / Processed, 3);
        }

        private void Initialize(List<Tuple<string, string>> list)
        {
            InitializeTraingList(list);
            InitializeTrainingDictionary();
        }

        private void InitializeTraingList(List<Tuple<string, string>> list)
        {
            TrainingList.Clear();
            if (FTCCOptions.ParallelismOnTestFile)
            {
                //InitializeWithParalellism(list);
            }
            else
            {
                InitializeWithoutParalellism(list);
            }
        }


        //private void InitializeWithParalellism(List<Tuple<string, string>> list)
        //{
        //    var sync = new object();
        //    Parallel.ForEach(list, item =>
        //    {
        //        long compressedLength = GZipLength(item.Item1);
        //        var tuple = Tuple.Create(item.Item1, item.Item2, compressedLength);
        //        lock (sync)
        //        {
        //            TrainingList.Add(tuple);
        //        }
        //    });
        //}

        private void InitializeWithoutParalellism(List<Tuple<string, string>> list)
        {
            foreach (var item in list)
            {
                string key = item.Item2;
                var bytes = Encoding.UTF8.GetBytes(item.Item1);
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
        }

        private void InitializeTrainingDictionary()
        {
            TrainingDictionary.Clear();
            foreach (var item in TrainingList)
            {
                byte[] dictionary = DictBuilder.TrainFromBuffer(item.Value);
                TrainingDictionary.Add(item.Key, dictionary);
            }
        }

        public string Predict(string text)
        {
            byte[] bytes = Encoding.UTF8.GetBytes(text);
            int? minimumSize = null;
            string? predictedClass = null;
            foreach (var item in TrainingDictionary)
            {
                var compreswsorOptions = new CompressionOptions(item.Value);
                var compressor = new Compressor(compreswsorOptions);                
                var compressed = compressor.Wrap(bytes);
                if (minimumSize == null || compressed.Length < minimumSize)
                {
                    minimumSize = compressed.Length;
                    predictedClass = item.Key;
                }
            }
            return predictedClass;
        }

        private List<Tuple<string, string>> CsvToList(string file)
        {
            using var streamReader = new StreamReader(file);
            var csvConfiguration = new CsvConfiguration(CultureInfo.InvariantCulture)
            {
                BadDataFound = null,
                HasHeaderRecord = FTCCOptions.HasHeaderRecord,
            };
            using var csvReader = new CsvReader(streamReader, csvConfiguration);
            if (FTCCOptions.HasHeaderRecord)
            {
                csvReader.Read();
            }
            var records = new List<Tuple<string, string>>();
            while (csvReader.Read())
            {
                var text = csvReader.GetField<string>(FTCCOptions.TextColumn);
                var label = csvReader.GetField<string>(FTCCOptions.LabelColumn);

                if (text == null || label == null)
                {
                    continue;
                }
                Tuple<string, string> record = Tuple.Create(text, label);
                records.Add(record);
            }
            return records;
        }
    }
}
using ftcc_lib;

namespace ftcc_console
{
    internal class Program
    {
        private static DateTime DateStart;
        static void Main(string[] args)
        {
            Start();
            Run();
            End();
        }

        private static void Start()
        {
            DateStart = DateTime.Now;
            string textStarted =
                "STARTED at " + DateStart.ToShortDateString() + " " + DateStart.ToString(@"hh\:mm\:ss") + "\n";
            Console.WriteLine(textStarted);
        }

        private static void Run()
        {
            //string dictionariesPath = @"C:\Users\Chus\Downloads\dictionaries.ftcc";

            //string trainFile = @"C:\Users\Chus\Downloads\ag_news_train.csv";
            //string testFile = @"C:\Users\Chus\Downloads\ag_news_test.csv";

            //string trainFile = @"C:\Users\Chus\Downloads\DBPEDIA_train.csv";
            //string testFile = @"C:\Users\Chus\Downloads\DBPEDIA_test.csv";

            //string trainFile = @"C:\Users\Chus\Downloads\oh-train-stemmed.csv";
            //string testFile = @"C:\Users\Chus\Downloads\oh-test-stemmed.csv";

            //string trainFile = @"C:\Users\Chus\Downloads\kinnwes-train.csv";
            //string testFile = @"C:\Users\Chus\Downloads\kinnwes-test.csv";

            string trainFile = @"C:\Users\Chus\Downloads\yahooanswer_train.csv";
            string testFile = @"C:\Users\Chus\Downloads\yahooanswer_test.csv";

            FTCCOptions fTCCOptions = new()
            {
                DictionariesPath = null,                // File path for preloaded dictionaries (ignores training file). Default: null;
                TrainFile = trainFile,                  // File path for csv train file
                ParallelismToInitialize = false,        // Use paralelism to initialize dictionaries. Default: false (if true, diccionaries will be a bit different for each execution)
                ParallelismOnTestFile = true,           // Use paralelism for each test. Default: false
                CompressionLevel = 3,                   // Compression level for dictionaries. Default: 3
                CompressorsPerClass = 5,                // Number of compressors per class. Default: 3
                TextColumn = 1,                         // Text column number in csv file. Default: 0
                LabelColumn = 0,                        // Label column number in csv file. Default: 1
                HasHeaderRecord = false,                 // Csv has header record. Deault: true
                ConsoleOutput = true,                   // Output console during file prediction. Default: true
            };

            FTCC fTCC = new(fTCCOptions);
            double result = fTCC.PredictFile(testFile);
            Console.WriteLine();
            Console.WriteLine(result);

            //string text = "Fears for T N pension after talks Unions representing workers at Turner   Newall say they are 'disappointed' after talks with stricken parent firm Federal Mogul.";
            //var prediction = fTCC.Predict(text); // must be 2
            //Console.WriteLine(prediction);

            
            //fTCC.SerializeDiccionaries(dictionariesPath);
        }
        private static void End()
        {
            DateTime dateFinished = DateTime.Now;
            string textFinished =
                "FINISHED at " + dateFinished.ToShortDateString() + " " + dateFinished.ToString(@"hh\:mm\:ss") +
                "\nDuration: " + (dateFinished - DateStart).ToString(@"dd\:hh\:mm\:ss\.fff") + ". ";
            Console.WriteLine("\n" + textFinished);
        }
    }
}
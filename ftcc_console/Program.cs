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
            /* 
            
             */
            string trainFile = @"C:\Users\Chus\Downloads\ag_news_train.csv";
            string testFile = @"C:\Users\Chus\Downloads\ag_news_test.csv";

            FTCCOptions fTCCOptions = new()
            {
                TrainFile = trainFile,          // File path for csv train file
                ParallelismOnCalc = false,       // Use paralelism on calc. Default: false
                ParallelismOnTestFile = false,  // Use paralelism for each test. Default: false
                TextColumn = 0,                 // Text column number in csv file. Default: 0
                LabelColumn = 1,                // Label column number in csv file. Default: 1
                HasHeaderRecord = true,         // Csv has header record. Deault: true
                ConsoleOutput = true,           // Output console during file prediction. Default: true
            };

            FTCC fTCC = new(fTCCOptions);
            double result = fTCC.PredictFile(testFile);
            Console.WriteLine(result);

            //string text = "Socialites unite dolphin groups Dolphin groups, or \"pods\", rely on socialites to keep them from collapsing, scientists claim.";
            //var prediction = fTCC.Predict(text); // should be 3
            //Console.WriteLine(prediction);
        }
        private static void End()
        {
            DateTime dateFinished = DateTime.Now;
            string textFinished =
                "FINISHED at " + dateFinished.ToShortDateString() + " " + dateFinished.ToString(@"hh\:mm\:ss") +
                "\nDuration: " + (dateFinished - DateStart).ToString(@"dd\:hh\:mm\:ss\.fff") + ". ";
            Console.WriteLine("\n" + textFinished);

            Console.Beep(500, 500);
        }
    }
}
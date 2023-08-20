namespace ftcc_lib
{
    public struct FTCCOptions
    {
        /// <summary>
        /// File path for preloaded dictionaries (ignores training file).
        /// </summary>
        public string? DictionariesPath { get; set; } = null;

        /// <summary>
        /// File path for csv train file.
        /// </summary>
        public string TrainFile { get; set; } = string.Empty;        

        /// <summary>
        /// Use paralelism to initialize dictionaries (if true, diccionaries will be a bit different for each execution).
        /// </summary>
        public bool ParallelismToInitialize { get; set; } = false;

        /// <summary>
        /// se paralelism for each test.
        /// </summary>
        public bool ParallelismOnTestFile { get; set; } = false;

        /// <summary>
        /// Compression level for dictionaries.
        /// </summary>
        public int CompressionLevel { get; set; } = 3;

        /// <summary>
        /// Number of compressors per class.
        /// </summary>
        public int CompressorsPerClass { get; set; } = 3;

        /// <summary>
        /// Text column number in csv file.
        /// </summary>
        public int TextColumn { get; set; } = 0;

        /// <summary>
        /// Label column number in csv file.
        /// </summary>
        public int LabelColumn { get; set; } = 1;
        
        /// <summary>
        /// Csv has header record.
        /// </summary>
        public bool HasHeaderRecord { get; set; } = true;

        /// <summary>
        /// Output console during file prediction.
        /// </summary>
        public bool ConsoleOutput { get; set; } = true;
        

        public FTCCOptions()
        {

        }
    }
}

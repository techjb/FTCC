namespace ftcc_lib
{
    public struct FTCCOptions
    {
        public string TrainFile { get; set; } = string.Empty;

        public bool ParallelismOnCalc { get; set; } = false;

        public bool ParallelismToInitialize { get; set; } = false;

        public bool ParallelismOnTestFile { get; set; } = false;

        public int CompressionLevel { get; set; } = 3;

        public int CompressorsPerClass { get; set; } = 3;

        public int TextColumn { get; set; } = 0;

        public int LabelColumn { get; set; } = 1;

        public bool ConsoleOutput { get; set; } = true;

        public bool HasHeaderRecord { get; set; } = true;

        public FTCCOptions()
        {

        }
    }
}

# FTCC C#

FTCC: Fast Text Classification with Compressors dictionary.

This project is a C# implementation of the [FTCC](https://github.com/cyrilou242/ftcc) project for text classification.

The main advantage of this type of classification is that it uses [ZSTD](https://github.com/facebook/zstd) compression, 
which allows for compression using dictionaries that have been generated beforehand, 
thereby achieving significant speed improvements when classifying a text.

Available as a [Nuget package](https://www.nuget.org/packages/ftcc/).

## Features

- Set the number of compressors per class.
- Enable parallelism during dictionary creation.
- Determine the desired compression level.
- Handle the serialization and deserialization of dictionaries.
- Evaluate training files.

## Performance

All tests have been done at a compression level of 3, which is the default compression level.

Compressors per class represents the number of dictionaries for each class.


| Compressors per class    | AGNews  | DBpedia  | YahooAnswers  | R8       | R52      | Ohsumed  | Kinnews  |
|--------------------------|---------|----------|---------------|----------|----------|----------|----------|
| CPC 1                    | 0.837   | 0.892    | 0.417         | 0.908    | 0.818    | 0.426    | 0.754    |
| CPC 3                    | 0.876   | 0.918    | 0.517         | 0.912    | 0.803    | 0.374    | 0.766    |
| CPC 5                    | 0.881   | 0.925    | 0.551         | 0.896    | 0.776    | 0.370    | 0.756    |


## Usage

### Initialization:

```cs

string trainFile = @"C:\Users\Chus\Downloads\ag_news_train.csv";

FTCCOptions fTCCOptions = new()
{
    DictionariesPath = null,                // File path for preloaded dictionaries (ignores training file). Default: null;
    TrainFile = trainFile,                  // File path for csv train file
    ParallelismToInitialize = false,        // Use paralelism to initialize dictionaries. Default: false (if true, diccionaries will be a bit different for each execution)
    ParallelismOnTestFile = true,           // Use paralelism for each test. Default: false
    CompressionLevel = 3,                   // Compression level for dictionaries. Default: 3
    CompressorsPerClass = 3,                // Number of compressors per class. Default: 3
    TextColumn = 0,                         // Text column number in csv file. Default: 0
    LabelColumn = 1,                        // Label column number in csv file. Default: 1
    HasHeaderRecord = true,                 // Csv has header record. Deault: true
    ConsoleOutput = true,                   // Output console during file prediction. Default: true
};

FTCC fTCC = new(fTCCOptions);

```

### Predict csv test file:

```cs

string testFile = @"C:\Users\Chus\Downloads\ag_news_test.csv";
double result = fTCC.PredictFile(testFile);
Console.WriteLine(result);

```

### Single text prediction:

```cs
string text = "Socialites unite dolphin groups Dolphin groups, or \"pods\", rely on socialites to keep them from collapsing, scientists claim.";
var prediction = fTCC.Predict(text);
Console.WriteLine(prediction);
```

### Dictionary serialisation:

Serializing the dictionaries allows for preloading the FTCC objects later on, 
so that it doesn't need to create the dictionaries, but instead reads them from file. 
This way, it allows for immediate text classification.

```cs

string dictionariesPath = @"C:\Users\Chus\Downloads\dictionaries.ftcc";
fTCC.SerializeDiccionaries(dictionariesPath);

```

To create the FTCC object with the serialized dictionaries:

```cs

FTCCOptions fTCCOptions = new()
{
    DictionariesPath = dictionariesPath
};

FTCC fTCC = new(fTCCOptions);

```





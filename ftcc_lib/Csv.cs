using CsvHelper.Configuration;
using CsvHelper;
using System.Globalization;

namespace ftcc_lib
{
    public class Csv
    {
        public static List<Tuple<string, string>> ToList(FTCCOptions fTCCOptions, string file)
        {
            using var streamReader = new StreamReader(file);
            var csvConfiguration = new CsvConfiguration(CultureInfo.InvariantCulture)
            {
                BadDataFound = null,
                HasHeaderRecord = fTCCOptions.HasHeaderRecord,
            };
            using var csvReader = new CsvReader(streamReader, csvConfiguration);
            if (fTCCOptions.HasHeaderRecord)
            {
                csvReader.Read();
            }
            var records = new List<Tuple<string, string>>();
            while (csvReader.Read())
            {
                var text = csvReader.GetField<string>(fTCCOptions.TextColumn);
                var label = csvReader.GetField<string>(fTCCOptions.LabelColumn);

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

using System.Text.Json;

namespace ftcc_lib
{
    public class Serialization
    {
        public static void Serialize(string path, Dictionary<string, List<byte[]>> dictionaries)
        {
            var jsonSerializerOptions = new JsonSerializerOptions { WriteIndented = true };
            string jsonString = JsonSerializer.Serialize(dictionaries, jsonSerializerOptions);
            File.WriteAllText(path, jsonString);
        }

        public static Dictionary<string, List<byte[]>> Deserialize(string path)
        {
            string jsonString = File.ReadAllText(path);
            return JsonSerializer.Deserialize<Dictionary<string, List<byte[]>>>(jsonString)!;
        }
    }
}

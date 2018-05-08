using Newtonsoft.Json;
using System;
using System.IO;

namespace FormatSave
{
    class Program
    {
        static void Main(string[] args)
        {
            if (args.Length == 0)
            {
                throw new Exception("You need to indicate the file path in argument");
            }

            var saveLines = File.ReadAllLines(args[0]);

            var parsedJson = JsonConvert.DeserializeObject(saveLines[1]);

            string formattedFilName;

            if (args.Length == 2)
            {
                formattedFilName = args[1];
            }
            else
            {
                formattedFilName = $"{Path.GetFileNameWithoutExtension(args[0])}.formatted";
                var existingFiles = Directory.GetFiles(Environment.CurrentDirectory, formattedFilName + "*");

                if (existingFiles.Length > 0)
                {
                    if (existingFiles.Length == 1)
                    {
                        formattedFilName += ".1";
                    }
                    else
                    {
                        var tempfilePath = Path.GetFileNameWithoutExtension(existingFiles[existingFiles.Length - 2]);
                        var lastDotIndex = tempfilePath.LastIndexOf(".") + 1;
                        formattedFilName = tempfilePath.Substring(0, lastDotIndex) + (int.Parse(tempfilePath.Substring(lastDotIndex)) + 1);
                    }
                }
            }
            File.WriteAllText($"{formattedFilName}.json", JsonConvert.SerializeObject(parsedJson, Formatting.Indented, new DoubleJsonConverter()));
        }
    }
}

using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Text.RegularExpressions;

namespace WarCommander_Decode_Strings
{
    public class Helpers
    {

        public static List<byte[]> binaryDatas = new List<byte[]>();

        public static string bin1 = @"WCFiles\1.bin";

        public static string bin2 = @"WCFiles\2.bin";

        public static string bin3 = @"WCFiles\3.bin";

        public static string ScriptFolderPath = @"WCFiles\scripts";

        public static string ERROR = $">>::ERROR::<<{Environment.NewLine}";

        public static string WARNING = $">>::WARNING::<<{Environment.NewLine}";

        public static string INFO = $">>::INFO::<<{Environment.NewLine}";

        public static string BinFilesMissing = string.Join(Environment.NewLine,
            "Place bin file(s) in WCFiles Folder.",
            "Type: Retry to scan again.",
            "Press ENTER to exit.");

        public static string LastChance = string.Join(Environment.NewLine,
            "Last chance to quit.",
            "Type: Exit to quit program.",
            "Or press enter to continue.");

        public static string Exit = string.Join(Environment.NewLine,
            "Files have been decoded.",
            "Press ENTER to exit");

        public static string WCFilesDoesntExist = string.Join(Environment.NewLine,
            "WCFiles Folder does not exist.",
            "Would you like to create it?",
            "Type: Yes to create it and rescan",
            "Press ENTER to exit");


        public static void CheckFiles()
        {
            Console.Clear();
            binaryDatas.Clear();

            int missingBinfile = 0;
            int missingScriptFolder = 0;

            if (!Directory.Exists(@"WCFiles"))
            {
                Console.WriteLine($"{ERROR}{WCFilesDoesntExist}");
                if (Console.ReadLine().ToLower() == "yes")
                { Directory.CreateDirectory("WCFiles"); CheckFiles(); }
                else { Environment.Exit(0); }
            }
            if (!File.Exists(bin1))
            { Console.WriteLine($"{ERROR}'{bin1}' is missing.{Environment.NewLine}"); missingBinfile++; }
            if (!File.Exists(bin2))
            { Console.WriteLine($"{ERROR} {bin2} is missing.{Environment.NewLine}"); missingBinfile++; }
            if (!File.Exists(bin3))
            { Console.WriteLine($"{ERROR} {bin3} is missing.{Environment.NewLine}"); missingBinfile++; }
            if (!Directory.Exists(ScriptFolderPath))
            { Console.WriteLine($"{ERROR} {ScriptFolderPath} is missing.{Environment.NewLine}"); }
            if (missingBinfile == 0 && missingScriptFolder == 0)
            { SetupDecoder(); }
            else
            {
                Console.WriteLine(ERROR + BinFilesMissing);
                if (Console.ReadLine().ToLower() == "retry")
                { CheckFiles(); }
                else { Environment.Exit(0); }
            }
        }

        private static void SetupDecoder()
        {
            Console.Clear();
            Console.WriteLine(string.Join(Environment.NewLine,
                "Found - " + bin1,
                "Found - " + bin2,
                "Found - " + bin3,
                "Found - " + ScriptFolderPath));

            binaryDatas.Add(File.ReadAllBytes(bin1));
            binaryDatas.Add(File.ReadAllBytes(bin2));
            binaryDatas.Add(File.ReadAllBytes(bin3));

            Console.WriteLine($"{Environment.NewLine}{WARNING}{LastChance}");

            if (Console.ReadLine().ToLower() == "exit")
            { Environment.Exit(0); }
            else
            { DecodeStrings(); }
        }

        private static void DecodeStrings()
        {
            int fileCount = 0;
            string[] filePath = Directory.GetFiles(ScriptFolderPath, "*.as*", SearchOption.AllDirectories);
            foreach (string path in filePath)
            {
                string originalText = File.ReadAllText(path);
                string regexPattern = @"class_2.method_7\(-([0-9]*)\)";
                Match match = Regex.Match(originalText, regexPattern);
                if (match.Success)
                {
                    string result = Regex.Replace(originalText, regexPattern, m =>
                     {
                         int value = -(Convert.ToInt32(m.Groups[1].Value));
                         return "==[[ " + Class_2.GetDefinitionName(value, Helpers.binaryDatas) + " ]]==";
                     });

                    Console.WriteLine("FOUND - " + path);
                    File.WriteAllText(path.Remove(path.Length - 3) + "___DECODED.as", result);
                    fileCount++;
                }
            }
            Console.WriteLine($"{Environment.NewLine}{INFO}{fileCount} {Helpers.Exit}");
            Console.ReadLine();
            Environment.Exit(0);
        }
    }


    #region BigEndianReader
    // BigEndianReader found here:
    // https://gist.github.com/Fuebar/7495914

    public class BigEndianReader : BinaryReader
    {
        public BigEndianReader(Stream input) : base(input) { }

        public override short ReadInt16()
        {
            byte[] b = ReadBytes(2);
            return (short)(b[1] + (b[0] << 8));
        }
        public override int ReadInt32()
        {
            byte[] b = ReadBytes(4);
            return b[3] + (b[2] << 8) + (b[1] << 16) + (b[0] << 24);
        }
        public override long ReadInt64()
        {
            byte[] b = ReadBytes(8);
            return b[7] + (b[6] << 8) + (b[5] << 16) + (b[4] << 24) + (b[3] << 32) + (b[2] << 40) + (b[1] << 48) + (b[0] << 56);
        }

        public bool ReadInt32AsBool()
        {
            byte[] b = ReadBytes(4);
            if (b[0] == 0 || b[1] == 0 || b[2] == 0 || b[3] == 0)
                return false;
            else
                return true;
        }

        public string ReadString32BitPrefix()
        {
            int length = ReadInt32();
            return Encoding.ASCII.GetString(ReadBytes(length));
        }

        public float ReadFloat() => (float)ReadDouble();

    }
    #endregion

}
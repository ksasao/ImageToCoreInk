using System;
using System.Collections.Generic;
using System.IO;

namespace ImageToCoreInk
{
    class Program
    {
        static void Main(string[] args)
        {
            if (args.Length == 0)
            {
                Console.WriteLine("usage: ImageToCoreInk [file_name|folder_name]");
                return;
            }
            string path = args[0];
            bool isDirectory = File.GetAttributes(path).HasFlag(FileAttributes.Directory);

            if (isDirectory)
            {
                string[] files = Directory.GetFiles(path,"*");
                List<string> output = new List<string>();
                for(int i=0; i<files.Length; i++)
                {
                    try
                    {
                        Console.Write($"{files[i]}...");
                        ImageConverter ic = new ImageConverter();
                        (string val, string[] text) = ic.ConvertToString(files[i]);
                        output.AddRange(text);
                        Console.WriteLine($"Converted({val}).");
                    }
                    catch
                    {
                        Console.WriteLine("Skipped.");
                    }
                }
                string name = Path.GetFileNameWithoutExtension(path);
                File.WriteAllLines($"{name}.h", output.ToArray());
            }
            else
            {
                ImageConverter ic = new ImageConverter();
                (string name, string[] text) = ic.ConvertToString(path);
                File.WriteAllLines($"{name}.h", text);
            }

        }
    }
}

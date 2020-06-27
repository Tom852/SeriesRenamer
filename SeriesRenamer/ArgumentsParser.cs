using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace SeriesRenamer
{
    public class ArgumentsParser
    {
        private string[] Args { get; }
        public ArgumentsParser(string[] args) => Args = args;




        public void ParseArguments()
        {
            if (Args.Length > 0 && Args[0] == "-h")
            {
                Console.WriteLine(@"USAGE:
* -f folder on filesystem
* -n series name
* -u wiki url
* -l lang   [de / original]");
                return;
            }


            if (Args.Length % 2 == 1)
            {
                throw new ArgumentException("Invalid argument amount.");
            }

            for (int i = 0; i < Args.Length; i += 2)
            {
                switch (Args[i])
                {
                    case "-f":
                        E.folder = Args[i + 1];
                        break;

                    case "-n":
                        E.seriesName = Args[i + 1];
                        break;

                    case "-u":
                    case "-w":
                        E.wikiURL = Args[i + 1];
                        break;

                    case "-l":
                        E.lang = Args[i + 1];
                        break;
                    default:
                        throw new ArgumentException("Invalid argument identifier provided: " + Args[i]);
                }
            }

            if (!Directory.Exists(E.folder))
            {
                Console.WriteLine("**WARNING**: Provided Directory not existing. Ignoring...");
                E.folder = string.Empty;
            }

        }

    }
}

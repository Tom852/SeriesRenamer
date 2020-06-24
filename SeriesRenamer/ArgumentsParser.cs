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
            }


            if (Args.Length % 2 == 1)
            {
                throw new ArgumentException("Invalid argument length provided.");
            }

            for (int i = 0; i < Args.Length; i += 2)
            {
                switch (Args[i])
                {
                    case "-f":
                        Env.folder = Args[i + 1];
                        break;

                    case "-n":
                        Env.seriesName = Args[i + 1];
                        break;

                    case "-u":
                    case "-w":
                        Env.wikiURL = Args[i + 1];
                        break;

                    case "-l":
                        Env.lang = Args[i + 1];
                        break;
                    default:
                        throw new ArgumentException("Invalid argument identifier provided: " + Args[i]);
                }
            }

            if (!Directory.Exists(Env.folder))
            {
                Console.WriteLine("**WARNING**: Provided Directory not existing. Ignoring...");
                Env.folder = string.Empty;
            }

        }

    }
}

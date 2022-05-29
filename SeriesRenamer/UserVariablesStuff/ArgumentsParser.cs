using System;

namespace SeriesRenamer.UserVariablesStuff
{
    public class ArgumentsParser
    {
        private string[] Args { get; }
        public ArgumentsParser(string[] args) => Args = args;


        public UserVariables ParseArguments()
        {
            if (Args.Length > 0 && (Args[0] is "-h" or "/h" or "/?" or "/help" or "-help" or "-?" or "?"))
            {
                Console.WriteLine(@"USAGE:
* -f folder on filesystem
* -n series name
* -u wiki url
* -l lang   [de / original]");
                return null;
            }


            if (Args.Length % 2 == 1)
            {
                throw new ArgumentException("Invalid argument amount.");
            }

            UserVariables result = new UserVariables();

            for (int i = 0; i < Args.Length; i += 2)
            {
                switch (Args[i])
                {
                    case "-f":
                        result.Folder = Args[i + 1];
                        break;

                    case "-n":
                        result.SeriesName = Args[i + 1];
                        break;

                    case "-u":
                    case "-w":
                        result.WikiURL = Args[i + 1];
                        break;

                    case "-l":
                        result.Lang = Args[i + 1];
                        break;
                    default:
                        throw new ArgumentException("Invalid argument identifier provided: " + Args[i]);
                }
            }

            return result;
        }

    }
}

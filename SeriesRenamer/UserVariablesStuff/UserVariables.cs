using System;

namespace SeriesRenamer.UserVariablesStuff
{
    public class UserVariables
    {
        public string Lang { get; set; } = "deutsch";
        public string Folder { get; set; } = string.Empty;
        public string SeriesName { get; set; } = string.Empty;
        public string WikiURL { get; set; } = string.Empty;

        public void Print()
        {
                Console.WriteLine("INFO: Local Folder set to: " + Folder);
                Console.WriteLine("INFO: Series Name set to:  " + SeriesName);
                Console.WriteLine("INFO: Wiki URL set to:     " + WikiURL);
                Console.WriteLine("INFO: Language set to:     " + Lang);
        }
    }
}

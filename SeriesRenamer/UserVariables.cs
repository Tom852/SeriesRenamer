using System;
using System.Collections.Generic;
using System.Text;

namespace SeriesRenamer
{
    public static class E
    {
        public static string lang = "deutsch";
        public static string folder = string.Empty;
        public static string seriesName = string.Empty;
        public static string wikiURL = string.Empty;

        public static void Print()
        {

                Console.WriteLine("INFO: Local Folder set to: " + folder);
                Console.WriteLine("INFO: Series Name set to:  " + seriesName);
                Console.WriteLine("INFO: Wiki URL set to:     " + wikiURL);
                Console.WriteLine("INFO: Language set to:     " + lang);
            
        }
    }
}

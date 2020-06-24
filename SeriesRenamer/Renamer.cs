using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace SeriesRenamer
{
    public class Renamer
    {

        public Dictionary<string, string> Renaming { get; }
        public Renamer(Dictionary<string, string> r) => Renaming = r;
            
        public void Rename()
        {

            Console.WriteLine("\n\n**INFO**: Matching Files on System");
            Console.WriteLine("==================================");

            if (Renaming != null && Renaming.Count > 0)
            {

                Console.WriteLine("\n\n**INFO**: Mapping Found!");
                Console.WriteLine("============================");
                DisplayRenaming();

                Console.WriteLine("Enter 'Rename' to Rename");
                string input = Console.ReadLine();
                if (input == "Rename")
                {
                    PerformRenaming();
                    Console.WriteLine("SUCCESS!!!\nSoftware by Tom");

                }
                else
                {
                    Console.WriteLine("KK!!!\nSoftware by Tom");
                }
            }
            else
            {
                Console.WriteLine("No Mapping Rules found. Try better next time...");
            }



        }



        private void DisplayRenaming()
        {
            var maxlength = Renaming.Keys.Max(e => e.Length);

            char changeIndicator = '-';
            foreach (var pair in Renaming)
            {
                changeIndicator = pair.Key == pair.Value ? '-' : '*';
                Console.WriteLine(pair.Key.PadRight(maxlength) + " -" + changeIndicator + "->  " + pair.Value);
            }

            Console.WriteLine($"\n\n**SUCCESS**: {Renaming.Count} file renamings ready.");

        }

        private void PerformRenaming()
        {
            foreach (var pair in Renaming)
            {
                System.IO.File.Move(pair.Key, pair.Value);
            }
        }

        
    }
}

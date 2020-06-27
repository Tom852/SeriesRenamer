using System;
using System.Collections.Generic;
using System.Linq;

namespace SeriesRenamer.Renaming
{
    public class Renamer
    {

        public Dictionary<string, string> Renaming { get; }
        public Renamer(Dictionary<string, string> r) => Renaming = r;
            
        public void Rename()
        {



            if (Renaming != null && Renaming.Count > 0)
            {

                Console.WriteLine("\n\n**INFO**: Mapping Available!");
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
                Console.WriteLine("No Mapping Rules found. Something went wrong...");
            }



        }



        private void DisplayRenaming()
        {
            var maxlength = Renaming.Keys.Max(e => e.Length);

            foreach (var pair in Renaming)
            {
                var changeIndicator = pair.Key == pair.Value ? '-' : '*';
                Console.WriteLine(pair.Key.PadRight(maxlength) + " -" + changeIndicator + "->  " + pair.Value);
            }

            Console.WriteLine($"\n\n**SUCCESS**: {Renaming.Count} file renamings ready.");

        }

        private void PerformRenaming()
        {
            foreach (var (key, value) in Renaming)
            {
                System.IO.File.Move(key, value);
            }
        }

        
    }
}

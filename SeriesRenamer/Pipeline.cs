using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace SeriesRenamer
{
    class Engine
    {

        public void Run(string[] args)
        {
            new ArgumentsParser(args).ParseArguments();
            new EnvValidator().Validate();
            Env.Print();

            var fileNamePool = new WikiAnalyzer().Analyze();
            var filesOnSystem = Directory.GetFiles(Env.folder);

            Dictionary<string, string> renaming = new FileMatcher(fileNamePool, filesOnSystem).Match();
            new Renamer(renaming).Rename();

            Console.WriteLine("Pres Any Key to Exit...");
            Console.ReadKey();

        }
    }
}

using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace SeriesRenamer
{
    class Pipeline
    {

        public void Run(string[] args)
        {
            UserVariables userVars = new ArgumentsParser(args).ParseArguments();
            UserVariables validatedVars = new UserVariablesValidator(userVars).Validate();
            validatedVars.Print();

            var fileNamePool = new WikiAnalyzer().Analyze();
            var filesOnSystem = Directory.GetFiles(UserVariables.folder);

            Dictionary<string, string> renaming = new FileMatcher(fileNamePool, filesOnSystem).Match();
            new Renamer(renaming).Rename();

            Console.WriteLine("Pres Any Key to Exit...");
            Console.ReadKey();

        }
    }
}

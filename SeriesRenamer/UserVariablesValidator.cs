using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;

namespace SeriesRenamer
{
    public class UserVariablesValidator
    {
        public UserVariables Result { get; }
        public UserVariablesValidator(UserVariables uv) => Result = uv;

        public UserVariables Validate()
        {
            ObtainFolder();
            ObtainSeriesName();
            ObtainURL();
            ObtainLang();
            return Result;
        }

        private void ObtainLang()
        {
            if (Result.Lang.ToLower().Substring(0, 2) == "de") Result.Lang = "deutsch";
            else if (Result.Lang.ToLower().Substring(0, 2) == "or") Result.Lang = "original";
            else if (Result.Lang.ToLower().Substring(0, 2) == "en") Result.Lang = "original";
            else Result.Lang = "deutsch";
        }

       

        private void ObtainSeriesName()
        {
            if (Result.SeriesName == string.Empty)
            {
                Result.SeriesName = Result.Folder.Split("\\").Last();
                Console.WriteLine("Enter the Name of your series.");
                Console.WriteLine(@"Example: Rick and Morty");
                Console.WriteLine($"--> Leave Empty to go with '{Result.SeriesName}'");
                string seriesNameUser = Console.ReadLine();
                if (seriesNameUser != string.Empty)
                {
                    Result.SeriesName = seriesNameUser;
                }
            }
        }

        private void ObtainFolder()
        {
            while (!Directory.Exists(Result.Folder))
            {
                Console.WriteLine("Enter Folder with the Files from your series.");
                Console.WriteLine(@"Example: F:\New Files\_series\Rick and Morty");
                Result.Folder = Console.ReadLine();
            }
        }

    }
}

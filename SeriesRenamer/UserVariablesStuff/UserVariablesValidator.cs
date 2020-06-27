using System;
using System.IO;
using System.Linq;

namespace SeriesRenamer.UserVariablesStuff
{
    public class UserVariablesValidator
    {
        public UserVariables UserVars { get; }
        public UserVariablesValidator(UserVariables uv) => UserVars = uv;

        public UserVariables Validate()
        {
            ObtainFolder();
            ObtainSeriesName();
            ObtainLang();
            ObtainURL();
            return UserVars;
        }

        private void ObtainLang()
        {
            if (UserVars.Lang.ToLower().Substring(0, 2) == "de") UserVars.Lang = "deutsch";
            else if (UserVars.Lang.ToLower().Substring(0, 2) == "or") UserVars.Lang = "original";
            else if (UserVars.Lang.ToLower().Substring(0, 2) == "original") UserVars.Lang = "original";
            else if (UserVars.Lang.ToLower().Substring(0, 2) == "en") UserVars.Lang = "original";
            else if (UserVars.Lang.ToLower().Substring(0, 2) == "english") UserVars.Lang = "original";
            else UserVars.Lang = "deutsch";
        }


        private void ObtainURL()
        {
            UserVars.WikiURL = new WikiURLProvider(UserVars.WikiURL, UserVars.SeriesName).ObtainURL();
        }



        private void ObtainSeriesName()
        {
            if (UserVars.SeriesName == string.Empty)
            {
                UserVars.SeriesName = UserVars.Folder.Split("\\").Last();
                Console.WriteLine("Enter the Name of your series.");
                Console.WriteLine(@"Example: Rick and Morty");
                Console.WriteLine($"--> Leave Empty to go with '{UserVars.SeriesName}'");
                string seriesNameUser = Console.ReadLine();
                if (seriesNameUser != string.Empty)
                {
                    UserVars.SeriesName = seriesNameUser;
                }
            }
        }

        private void ObtainFolder()
        {
            while (!Directory.Exists(UserVars.Folder))
            {
                Console.WriteLine("Enter Folder with the Files from your series.");
                Console.WriteLine(@"Example: F:\New Files\_series\Rick and Morty");
                UserVars.Folder = Console.ReadLine();
            }
        }

    }
}

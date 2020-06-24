using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;

namespace SeriesRenamer
{
    public class EnvValidator
    {

        public void Validate()
        {
            ObtainFolder();
            ObtainSeriesName();
            ObtainURL();
            ObtainLang();
        }

        private static void ObtainLang()
        {
            if (Env.lang.ToLower().Substring(0, 2) == "de") Env.lang = "deutsch";
            else if (Env.lang.ToLower().Substring(0, 2) == "or") Env.lang = "original";
            else if (Env.lang.ToLower().Substring(0, 2) == "en") Env.lang = "original";
            else Env.lang = "deutsch";
        }

        private static void ObtainURL()
        {
            if (Env.wikiURL != string.Empty && !CheckUrl(Env.wikiURL))
            {
                Console.WriteLine("**WARNING**: Given URL seems not right. Ignoring argument...");
                Env.wikiURL = string.Empty;
            }

            if (Env.wikiURL == string.Empty)
            {
                Env.wikiURL = CreateWikiURL(Env.seriesName);
            }
        }

        private static void ObtainSeriesName()
        {
            if (Env.seriesName == string.Empty)
            {
                Env.seriesName = Env.folder.Split("\\").Last();
                Console.WriteLine("Enter the Name of your series.");
                Console.WriteLine(@"Example: Rick and Morty");
                Console.WriteLine($"--> Leave Empty to go with '{Env.seriesName}'");
                string seriesNameUser = Console.ReadLine();
                if (seriesNameUser != string.Empty)
                {
                    Env.seriesName = seriesNameUser;
                }
            }
        }

        private static void ObtainFolder()
        {
            while (!Directory.Exists(Env.folder))
            {
                Console.WriteLine("Enter Folder with the Files from your series.");
                Console.WriteLine(@"Example: F:\New Files\_series\Rick and Morty");
                Env.folder = Console.ReadLine();
            }
        }

        private static string CreateWikiURL(string seriesName)
        {
            string option1 = @"https://de.wikipedia.org/wiki/";
            option1 += seriesName.Replace(' ', '_');
            option1 += "/Episodenliste";

            string option2 = @"https://de.wikipedia.org/wiki/";
            option2 += (seriesName + " (Fernsehserie)").Replace(' ', '_');
            option2 += "/Episodenliste";

            string option3 = @"https://de.wikipedia.org/wiki/";
            option3 += seriesName.Replace(' ', '_');


            if (CheckUrl(option1))
            {
                return option1;
            }
            else if (CheckUrl(option2))
            {
                return option2;
            }
            else if (CheckUrl(option3))
            {
                Console.WriteLine("WARN: Non-dedicated (but general) wiki URL chosen. Please confirm or provide manually");
                Console.WriteLine("URL: " + option3);
                Console.WriteLine("Enter nothing to confirm or enter another url");
                string userinput = Console.ReadLine();
                if (userinput == "") return option3;
                else
                {
                    return AskUserForURLUntilValid(userinput);
                }
            }
            else
            {
                Console.WriteLine("Cannot deduce Wiki Episode List URL. Please Enter manually.");
                return AskUserForURLUntilValid("");
            }

        }

        private static string AskUserForURLUntilValid(string userinput)
        {
            while (true)
            {
                if (CheckUrl(userinput))
                {
                    return userinput;
                }

                Console.WriteLine("Entered URL seems not right. Please try again.");
                Console.WriteLine(@"Example: https://de.wikipedia.org/wiki/Rick_and_Morty/Episodenliste");
                Console.WriteLine(@"Example: https://de.wikipedia.org/wiki/Fuller_House_(Fernsehserie)/Episodenliste");
                Console.WriteLine(
                    @"Example: https://de.wikipedia.org/wiki/Sliders_%E2%80%93_Das_Tor_in_eine_fremde_Dimension/Episodenliste");
                userinput = Console.ReadLine();
                userinput = CropURL(userinput);
            }
        }

        private static bool CheckUrl(string url)
        {
            string dl1;
            try
            {
                using (WebClient client = new WebClient())
                {
                    dl1 = client.DownloadString(new Uri(url));
                }
            }
            catch
            {
                return false;
            }

            return dl1.Contains("Episoden");   //(optional) TODO: and not contain: hauptartikel: Die Simpopns/Episodenliste
        }



        private static string CropURL(string i)
        {

            if (i.Contains("#Episodenliste"))
            {
                int pos = i.IndexOf("#Episodenliste");
                return i.Remove(pos);
            }
            return i;
        }
    }
}

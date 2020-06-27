using System;
using System.Collections.Generic;
using System.Net;
using System.Runtime.ConstrainedExecution;
using System.Text;

namespace SeriesRenamer
{
    public class WikiURLProvider
    {
        private string Result { get; set; }
        private string SeriesName { get; }

        public WikiURLProvider(string initialSetting, string seriesName)
        {
            SeriesName = seriesName;

            if (initialSetting != string.Empty && !URLIsPromising(initialSetting))
            {
                Console.WriteLine("**WARNING**: Given URL seems not right. Ignoring argument...");
                Result = string.Empty;
            }
            else
            {
                Result = initialSetting;
            }
        }

        public string ObtainURL()
        {
            if (Result == string.Empty)
            {
                Result = CreateWikiURL(SeriesName);
            }

            return Result;
        }


        private string CreateWikiURL(string seriesName)
        {
            //Best Bet: A Dedicated Episode List Page
            //https://de.wikipedia.org/wiki/Rick_and_Morty/Episodenliste

            string option1 = @"https://de.wikipedia.org/wiki/";
            option1 += seriesName;
            option1 += "/Episodenliste";

            //If movies with the same name exist, 'Fernsehserie' is added:
            //https://de.wikipedia.org/wiki/Rick_and_Morty_(Fernsehserie)

            string option2 = @"https://de.wikipedia.org/wiki/";
            option2 += (seriesName + " (Fernsehserie)");
            option2 += "/Episodenliste";


            //Some lesser known series contain the episode list within the general page:
            //e.g. https://de.wikipedia.org/wiki/Terminator:_The_Sarah_Connor_Chronicles
            //https://de.wikipedia.org/wiki/Rick_and_Morty

            string option3 = @"https://de.wikipedia.org/wiki/";
            option3 += seriesName;

            option1 = ReplaceSpaces(option1);
            option2 = ReplaceSpaces(option2);
            option3 = ReplaceSpaces(option3);


            if (URLIsPromising(option1))
            {
                return option1;
            }
            else if (URLIsPromising(option2))
            {
                return option2;
            }
            else if (URLIsPromising(option3))
            {
                Console.WriteLine("WARN: Non-dedicated wiki URL chosen. Make sure it contains the episode list. Please confirm or provide manually.");
                Console.WriteLine("URL: " + option3);
                Console.WriteLine("Is this URL containing the episode list? (Y/N)");
                string userinput = Console.ReadLine();
                if (userinput.ToUpper() == "Y")
                {
                    return option3;
                }
                else
                {
                    return AskUserForURLUntilValid();
                }
            }
            else
            {
                Console.WriteLine("Cannot deduce Wiki URL with Episode List. This may happen if Your Series Name does not correspond to the official name, e.g. 'TSSC' vs. 'Terminator: The Sarah Connor Chronicles'. Please Enter the wiki URL manually.");
                return AskUserForURLUntilValid();
            }

        }

        private string ReplaceSpaces(string s)
        {
            return s.Replace(' ', '_');
        }

        private static string AskUserForURLUntilValid()
        {
            string userinput;
            while (true)
            {
                Console.WriteLine("Entered URL seems not right. Please try again.");
                Console.WriteLine(@"Example: https://de.wikipedia.org/wiki/Rick_and_Morty/Episodenliste");
                Console.WriteLine(@"Example: https://de.wikipedia.org/wiki/Fuller_House_(Fernsehserie)/Episodenliste");
                Console.WriteLine(@"Example: https://de.wikipedia.org/wiki/Sliders_%E2%80%93_Das_Tor_in_eine_fremde_Dimension/Episodenliste");
                userinput = Console.ReadLine();
                userinput = RemoveHTMLAncor(userinput);

                if (URLIsPromising(userinput))
                {
                    return userinput;
                }
            }
        }

        private static bool URLIsPromising(string url)
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

            //Todo wenn es wieder kompiliert testen ob das funzt mit dem plaintext oder ob das html mässig is.
            return dl1.Contains("Episoden") && dl1.Contains("→ Hauptartikel: Rick and Morty/Episodenliste");   //TODO: and not contain: hauptartikel: Die Simpopns/Episodenliste
        }



        private static string RemoveHTMLAncor(string i)
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

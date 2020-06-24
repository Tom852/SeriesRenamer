using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;

namespace SeriesRenamer
{
    class Program
    {
        static string language_keyword; //TODO - series name und so könnte man eig auch global static machen von mir aus, aber man müsst ja vlt eh mal noch ne klasse oder so machen.

        static void Main(string[] args)
        {
            /*USAGE:
            *
            * -f "folder on filesystem"
            * -n "series name"
            * -u "wiki url"
            * -l "lang"   [de / original]
            * 
            * */

            ParseArguments(args, out string folder, out string seriesName, out string wikiURL, out string lang);
            SetAndCheckVariables(ref folder, ref seriesName, ref wikiURL, ref lang);
            language_keyword = lang;

            DisplayVariables(folder, seriesName, wikiURL, lang);

            Console.WriteLine("\n\n**INFO**: Starting Wiki Analysis");
            Console.WriteLine("================================");
            LinkedList<FileNameRepresentation> deducedFileNames = AnalyzeWiki(seriesName, wikiURL);
            Console.WriteLine($"\n\n**SUCCESS**: {deducedFileNames.Count} potential file names registered."); //Todo kann auch 0 sein....

            Console.WriteLine("\n\n**INFO**: Matching Files on System");
            Console.WriteLine("==================================");
            Dictionary<string, string> renameMapping = MatchFilesOnSystem(deducedFileNames, folder);

            if (renameMapping != null && renameMapping.Count > 0)
            {

                Console.WriteLine("\n\n**INFO**: Mapping Found!");
                Console.WriteLine("============================");
                DisplayRenaming(renameMapping);

                Console.WriteLine("Enter 'Rename' to Rename");
                string input = Console.ReadLine();
                if (input == "Rename")
                {
                    PerformRenaming(renameMapping);
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

            Console.WriteLine("Pres Any Key to Exit...");
            Console.ReadKey();

        }


        #region in_out___setting_vars
        private static void DisplayVariables(string folder, string seriesName, string wikiURL, string lang)
        {
            Console.WriteLine("INFO: Local Folder set to: " + folder);
            Console.WriteLine("INFO: Series Name set to:  " + seriesName);
            Console.WriteLine("INFO: Wiki URL set to:     " + wikiURL);
            Console.WriteLine("INFO: Language set to:     " + lang);
        }

        private static void SetAndCheckVariables(ref string folder, ref string seriesName, ref string wikiURL, ref string lang)
        {

            if (!Directory.Exists(folder))
            {
                Console.WriteLine("**WARNING**: Provided Directory not existing. Ignoring...");
                folder = "";
            }

            if (folder == "")
            {
                while (!Directory.Exists(folder))
                {
                    Console.WriteLine("Enter Folder with the Files from your series.");
                    Console.WriteLine(@"Example: F:\New Files\_series\Rick and Morty");
                    folder = Console.ReadLine();
                }
            }


            if (seriesName == "")
            {
                seriesName = folder.Split("\\").Last();
                Console.WriteLine("Enter the Name of your series.");
                Console.WriteLine(@"Example: Rick and Morty");
                Console.WriteLine($"--> Leave Empty to go with '{seriesName}'");
                string seriesNameUser = Console.ReadLine();
                if (seriesNameUser != "")
                {
                    seriesName = seriesNameUser;
                }
            }

            if (wikiURL != "" && !CheckUrl(wikiURL))
            {
                Console.WriteLine("**WARNING**: Given URL seems not right. Ignoring argument...");
                wikiURL = "";
            }

            if (wikiURL == "")
            {
                wikiURL = CreateWikiURL(seriesName);
            }

            if (lang.ToLower().Substring(0, 2) == "de") lang = "deutsch";
            else if (lang.ToLower().Substring(0, 2) == "or") lang = "original";
            else if (lang.ToLower().Substring(0, 2) == "en") lang = "original";
            else lang = "deutsch";
        }

        private static void ParseArguments(string[] args, out string folder, out string seriesName, out string wikiURL, out string lang)
        {
            //Setting default values.
            lang = "deutsch";
            folder = "";
            seriesName = "";
            wikiURL = "";

            if (args.Length % 2 == 1)
            {
                throw new ArgumentException("Invalid arguments provided.");
            }

            for (int i = 0; i < args.Length; i += 2)
            {
                switch (args[i])
                {
                    case "-f":
                        folder = args[i + 1];
                        break;

                    case "-n":
                        seriesName = args[i + 1];
                        break;

                    case "-u":
                    case "-w":
                        wikiURL = args[i + 1];
                        break;

                    case "-l":
                        lang = args[i + 1];
                        break;
                    default:
                        throw new ArgumentException("Invalid argument identifier provided: " + args[i]);
                }
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
                    //TODO Duplicated code mit unten dran.
                    while (true)
                    {
                        if (CheckUrl(userinput))
                        {
                            return userinput;
                        }
                        Console.WriteLine("Entered URL seems not right. Please try again.");
                        Console.WriteLine(@"Example: https://de.wikipedia.org/wiki/Rick_and_Morty/Episodenliste");
                        Console.WriteLine(@"Example: https://de.wikipedia.org/wiki/Fuller_House_(Fernsehserie)/Episodenliste");
                        Console.WriteLine(@"Example: https://de.wikipedia.org/wiki/Sliders_%E2%80%93_Das_Tor_in_eine_fremde_Dimension/Episodenliste");
                        userinput = Console.ReadLine();
                        userinput = CropURL(userinput);

                    }
                }
            }
            else
            {
                Console.WriteLine("Cannot deduce Wiki Episode List URL. Please Enter manually.");
                while (true)
                {
                    Console.WriteLine(@"Example: https://de.wikipedia.org/wiki/Rick_and_Morty/Episodenliste");
                    Console.WriteLine(@"Example: https://de.wikipedia.org/wiki/Fuller_House_(Fernsehserie)/Episodenliste");
                    Console.WriteLine(@"Example: https://de.wikipedia.org/wiki/Sliders_%E2%80%93_Das_Tor_in_eine_fremde_Dimension/Episodenliste");
                    string userinput = Console.ReadLine();
                    userinput = CropURL(userinput);
                    if (CheckUrl(userinput))
                    {
                        return userinput;
                    }
                    Console.WriteLine("Entered URL seems not right. Please try again.");
                }
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
        #endregion


        #region renaming
        private static void DisplayRenaming(Dictionary<string, string> renameMapping)
        {
            var maxlength = renameMapping.Keys.Max(e => e.Length);

            char changeIndicator = '-';
            foreach (var pair in renameMapping)
            {
                changeIndicator = pair.Key == pair.Value ? '-' : '*';
                Console.WriteLine(pair.Key.PadRight(maxlength) + " -" + changeIndicator + "->  " + pair.Value);
            }

            Console.WriteLine($"\n\n**SUCCESS**: {renameMapping.Count} file renamings ready.");

        }

        private static void PerformRenaming(Dictionary<string, string> renameMapping)
        {
            foreach (var pair in renameMapping)
            {
                System.IO.File.Move(pair.Key, pair.Value);
            }
        }

        private static Dictionary<string, string> MatchFilesOnSystem(LinkedList<FileNameRepresentation> deducedFileNames, string folder)
        {
            Console.WriteLine("\n\nDEBUG: Analyzing Files on Filesystem...");
            Dictionary<string, string> renameMapping = new Dictionary<string, string>();
            try
            {
                string[] files = Directory.GetFiles(folder);

                foreach (string filesystemFile in files)
                {
                    string cleanedFilename = filesystemFile;
                    cleanedFilename = cleanedFilename.Replace("dd51", "");
                    cleanedFilename = cleanedFilename.Replace("dd20", "");
                    cleanedFilename = cleanedFilename.Replace("DD51", "");
                    cleanedFilename = cleanedFilename.Replace("x264", "");
                    cleanedFilename = cleanedFilename.Replace("X264", "");
                    cleanedFilename = cleanedFilename.Replace("1024", "");
                    cleanedFilename = cleanedFilename.Replace("720", "");
                    cleanedFilename = cleanedFilename.Replace("7p", "");
                    cleanedFilename = cleanedFilename.Replace("72p", "");
                    cleanedFilename = cleanedFilename.Replace("4K", "");
                    cleanedFilename = cleanedFilename.Replace("2K", "");
                    cleanedFilename = cleanedFilename.Replace("4NOX", "");

                    cleanedFilename = cleanedFilename.Split('\\').ToList().Last();

                    bool filesystemFileSuccessfullyMatched = false;
                    Regex r = new Regex(@"(\d?\d).*?(\d{2})");  //TODO make this more robust like blabla3blabla-112
                    Match m = r.Match(cleanedFilename);

                    if (!m.Success)
                    {
                        Console.WriteLine($"WARN: File {Path.GetFileName(filesystemFile)} does not contain a season and episode index of the form ...02..12... / File is ignored");
                    }

                    int.TryParse(m.Groups[1].Value, out int fileSeason);
                    int.TryParse(m.Groups[2].Value, out int fileEpisode);

                    //Console.WriteLine($"DEBUG: Filename Analysis result:\nFile: {filesystemFile}\nCropped: {cleanedFilename}\nSeason: {fileSeason} - Ep: {fileEpisode}\n");
                    string extension = Path.GetExtension(filesystemFile);
                    foreach (FileNameRepresentation deducedNewName in deducedFileNames)
                    {
                        if (deducedNewName.Season == fileSeason && deducedNewName.Episode == fileEpisode)
                        {
                            string targetName = Path.Combine(folder, deducedNewName.FullName + extension);
                            if (renameMapping.Values.Contains(targetName))
                            {
                                Console.WriteLine("**ERROR**: Duplicate target file name: " + Path.GetFileName(filesystemFile) + " --> " + targetName);
                            }
                            else
                            {
                                renameMapping.Add(filesystemFile, targetName);
                                filesystemFileSuccessfullyMatched = true;
                            }
                            break;
                        }

                    }
                    if (!filesystemFileSuccessfullyMatched)
                    {
                        Console.WriteLine("WARN: No Mapping found for file: " + Path.GetFileName(filesystemFile));
                    }

                }


            }
            catch (IOException e)
            {
                Console.WriteLine("ERROR: " + e.Message);
                return null;
            }

            return renameMapping;
        }
        #endregion


        #region WikiAnalysis
        private static LinkedList<FileNameRepresentation> AnalyzeWiki(string seriesName, string wiki)
        {

            HtmlWeb web = new HtmlWeb();
            HtmlDocument wikiDocument;
            wikiDocument = web.Load(new Uri(wiki));
            wikiDocument.OptionUseIdAttribute = true;


            var h2Nodes = wikiDocument.DocumentNode.SelectNodes("//h2");
            LinkedList<FileNameRepresentation> result = AnalyzeByHeadings(h2Nodes, seriesName);

            if (!result.IsEmpty())
            {
                return result;
            }

            Console.WriteLine("\n**WARN**: Analysis for dedicated episode page failed... trying attempt for a non dedicated episode list page...");
            var h2Head = wikiDocument.DocumentNode.SelectSingleNode("//h2[contains(.,'Episodenliste')]");
            if (h2Head is null)
            {
                h2Head = wikiDocument.DocumentNode.SelectSingleNode("//h2[contains(.,'Episoden')]");

            }

            if (h2Head != null)
            {
                var h3Nodes = h2Head.SelectNodes("./following-sibling::h3");
                if (!(h3Nodes is null))
                {
                    result = AnalyzeByHeadings(h3Nodes, seriesName);
                }
                else
                {
                    Console.WriteLine($"\n**WARN**: No h3 Headings after H2-Heading 'Episoden(liste)' at line {h2Head.Line}. Skipping attempt...");
                }

            }
            else
            {
                Console.WriteLine("\n**WARN**: No h2 named 'Episoden(liste)' found. Skipping attempt...");
            }

            if (!result.IsEmpty())
            {
                return result;
            }

            Console.WriteLine("\n**WARN**: H3-Analysis attempt failed too, trying a generic table scan");
            result = AnalyzeByTableaggressive(wikiDocument, seriesName, false);

            if (!result.IsEmpty())
            {
                return result;
            }

            Console.WriteLine("\n\n**WARN**: Generic Table Scan failed as well, maybe you can help me here...");
            result = AnalyzeByTableaggressive(wikiDocument, seriesName, true);

            return result;

        }
        private static LinkedList<FileNameRepresentation> AnalyzeByHeadings(HtmlNodeCollection headings, string seriesName)
        {
            LinkedList<FileNameRepresentation> result = new LinkedList<FileNameRepresentation>();
            int indexOfNameColumn = -1, indexOfEpColumn = -1;

            foreach (var header in headings)
            {

                int season = AnalyzeHeadingForSeasonNumber(header);
                if (season == -1)
                {
                    continue;
                }

                var tableNode = header.SelectSingleNode("./following-sibling::table");

                if (season == 1)
                {
                    try
                    {
                        AnalyzeHeadingRowForIndexes(tableNode, ref indexOfNameColumn, ref indexOfEpColumn);
                    }
                    catch
                    {
                        Console.WriteLine("ERROR: Table seems legit - but could not parse table headings.");
                        Console.WriteLine("INFO: Expected something like nr/nummer/st. and deutsch/titel");
                        Console.WriteLine("INFO: This may happen if the column is called 'Originaltitel'");
                        Console.WriteLine("INFO: You may try again but select English as language");
                        result.Clear();
                        return result;
                    }
                }


                var rows = tableNode.SelectNodes($".//tr[position()>1]");

                Console.WriteLine("DEBUG: Analyzing Table...");
                foreach (var row in rows)
                {
                    FileNameRepresentation f = AnalyzeRow(row, season, indexOfNameColumn, indexOfEpColumn, seriesName);
                    if (f is null)
                    {
                        continue;
                    }
                    result.AddLast(f);
                }
            }

            return result;

        }

        private static LinkedList<FileNameRepresentation> AnalyzeByTableaggressive(HtmlDocument wikiDocument, string seriesName, bool SetManualIndexes)
        {
            var tableNodes = wikiDocument.DocumentNode.SelectNodes("//table");
            LinkedList<FileNameRepresentation> result = new LinkedList<FileNameRepresentation>();

            int season = 1, indexOfNameColumn = -1, indexOfEpColumn = -1;


            if (SetManualIndexes)
            {
                Console.WriteLine("Locate Table with Episode Numbers and Episode Titles.\nEnter Column index for *season number* (start to count at 1)");
                int.TryParse(Console.ReadLine(), out int a);
                indexOfEpColumn = a;

                Console.WriteLine("Enter Column index for episode *name* (start with 1)");
                int.TryParse(Console.ReadLine(), out int b);
                indexOfNameColumn = b;
            }

            foreach (var tbl in tableNodes)
            {
                Console.WriteLine("\n\nDEUBG: Checking Table at line: " + tbl.Line);
                if (!SetManualIndexes)
                {
                    try
                    {
                        AnalyzeHeadingRowForIndexes(tbl, ref indexOfNameColumn, ref indexOfEpColumn);
                    }
                    catch (InvalidOperationException)
                    {
                        Console.WriteLine("DEBUG: Table does not contain parseable headings for ep no and title. Skipping table...");
                        continue;
                    }
                }

                if (indexOfNameColumn <= 0 || indexOfEpColumn <= 0)
                {
                    Console.WriteLine("DEBUG: No suitable heading detected... skipping table");
                    continue;
                }

                bool newEntriesWereAdded = false;
                var rows = tbl.SelectNodes($".//tr");  //including first row in case there is no header at all

                foreach (var row in rows)
                {
                    FileNameRepresentation f = AnalyzeRow(row, season, indexOfNameColumn, indexOfEpColumn, seriesName);
                    if (f is null)
                    {
                        continue;
                    }
                    result.AddLast(f);
                    newEntriesWereAdded = true;
                }
                if (newEntriesWereAdded)
                {
                    season++;
                    newEntriesWereAdded = false;
                    Console.WriteLine("DEUBG: Season Entries Found, advancing to season " + season);
                }
                else
                {
                    Console.WriteLine("DEUBG: No Season Entries Found, advancing to next table. Scanning for Season " + season);

                }

            }
            return result;
        }

        private static FileNameRepresentation AnalyzeRow(HtmlNode row, int season, int indexOfNameColumn, int indexOfEpColumn, string seriesName)
        {

            HtmlNode cellWithNr = row.SelectSingleNode($"./td[{indexOfEpColumn}]");
            HtmlNode cellWithName = row.SelectSingleNode($"./td[{indexOfNameColumn}]");

            if (cellWithNr is null || cellWithName is null)
            {
                if (row.SelectNodes($"./th") != null)
                {
                    Console.WriteLine("DEBUG: Header Row detected - skipping");
                    return null;
                }
                else
                {
                    Console.WriteLine("WARN: Irregular Row detected - skipping");
                    Console.WriteLine("INFO: This may happen on episode summaries or on the aggressive approaches");
                    return null;
                }

            }

            string episodeName = cellWithName.InnerText.Trim();
            int.TryParse(cellWithNr.InnerText, out int episode);

            if (episodeName.Length < 2)
            {
                Console.WriteLine($"WARN: Season {season} Episode {episode} named {episodeName} - Name too short, ignoring");
                return null;
            }

            if (episode <= 0 || episode > 99)
            {
                Console.WriteLine($"WARN: Season {season} Episode {episode} named {episodeName} - Invalid Episode Index, ignoring");
                Console.WriteLine($"INFO: This may happen on double episodes...");
                return null;
            }


            FileNameRepresentation result = new FileNameRepresentation(seriesName, season, episode, episodeName);
            Console.WriteLine("SUCCESS: Added episode: " + result.FullName);
            return result;
        }

        private static void AnalyzeHeadingRowForIndexes(HtmlNode tableNode, ref int indexOfNameColumn, ref int indexOfEpColumn)
        {
            Console.WriteLine("\nDEBUG: Analyizing Table Headers for Indexes...");
            int index = 1;
            var tableHeaders = tableNode.SelectNodes(".//tr[1]/th");
            if (tableHeaders is null)
            {
                throw new InvalidOperationException("No heading row detected.");

            }
            foreach (var col in tableHeaders)
            {
                string cellText = col.InnerText.Trim();
                Console.WriteLine($"DEBUG: Currently Checking: '{cellText}'");

                if (cellText.ToLower().Contains(language_keyword) && cellText.ToLower().Contains("titel"))
                {
                    Console.WriteLine("SUCCESS: Found column index for the Name: " + index);
                    indexOfNameColumn = index;
                }
                if ((cellText.ToLower().Contains("nr.") || cellText.ToLower().Contains("nummer")) && !cellText.ToLower().Contains("ges."))
                {
                    Console.WriteLine("SUCCESS: Found column index for episode number: " + index);
                    indexOfEpColumn = index;
                }

                index++;
            }

            if (indexOfEpColumn == -1 || indexOfNameColumn == -1)
            {
                throw new InvalidOperationException("Could not parse proper columns");
            }
            else
            {
                Console.WriteLine("SUCCESS: Table Layout successfully analyzed\n");
            }

        }

        private static int AnalyzeHeadingForSeasonNumber(HtmlNode header)
        {
            Console.WriteLine("\nDEBUG: Analyzing Heading: " + header.InnerText);

            Regex r = new Regex(@"Staffel (\d+)");
            Match m = r.Match(header.InnerText);

            if (!m.Success)
            {
                Console.WriteLine("DEBUG: No Season index found");
                Console.WriteLine("DEBUG: Skipping Heading");
                return -1;
            }


            int.TryParse(m.Groups[1].Value, out int s);
            Console.WriteLine("SUCCESS: Found Season Index: " + s);
            return s;

        }
        #endregion



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

    static class Extension
    {
        public static bool IsEmpty<T>(this ICollection<T> coll)
        {
            return coll.Count == 0;
        }
    }
}

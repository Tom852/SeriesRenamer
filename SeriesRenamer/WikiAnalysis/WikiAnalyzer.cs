using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using HtmlAgilityPack;
using SeriesRenamer.UserVariablesStuff;

namespace SeriesRenamer.WikiAnalysis
{
    public class WikiAnalyzer
    {
        public LinkedList<FileNameRepresentation> DeducedFileNames { get; set; } = new LinkedList<FileNameRepresentation>();
        public string WikiURL { get; }
        public string SeriesName { get; }
        public string Lang { get; }

        private int tableIndexOfSeriesName = -1;
        private int tableIndexOfEpisodeNr = -1;

        public WikiAnalyzer(UserVariables uv)
        {
            WikiURL = uv.WikiURL;
            SeriesName = uv.SeriesName;
            Lang = uv.Lang;
        }

        public LinkedList<FileNameRepresentation> Analyze()
        {
            Console.WriteLine("\n\n**INFO**: Starting Wiki Analysis");
            Console.WriteLine("================================");
            AnalyzeWiki();
            if (DeducedFileNames.Any())
            {
                Console.WriteLine($"\n\n**SUCCESS**: {DeducedFileNames.Count} potential file names registered."); //Todo kann auch 0 sein....
            }
            else
            {
                Console.WriteLine("\n\n**ERROR**: No potential file names were found. Was the URL wrong?");
            }
            return DeducedFileNames;
        }


        private void AnalyzeWiki()
        {

            HtmlWeb web = new HtmlWeb();
            HtmlDocument wikiDocument;
            wikiDocument = web.Load(new Uri(WikiURL));
            wikiDocument.OptionUseIdAttribute = true;


            var h2Nodes = wikiDocument.DocumentNode.SelectNodes("//h2");
            AnalyzeByHeadings(h2Nodes);

            if (!DeducedFileNames.IsEmpty())
            {
                return;
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
                    AnalyzeByHeadings(h3Nodes);
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

            if (!DeducedFileNames.IsEmpty())
            {
                return;
            }

            Console.WriteLine("\n**WARN**: H3-Analysis attempt failed too, trying a generic table scan");
            AnalyzeByTableaggressive(wikiDocument, false);

            if (!DeducedFileNames.IsEmpty())
            {
                return;
            }

            Console.WriteLine("\n\n**WARN**: Generic Table Scan failed as well, maybe you can help me here...");
            AnalyzeByTableaggressive(wikiDocument, true);

        }
        private void AnalyzeByHeadings(HtmlNodeCollection headings)
        {
            tableIndexOfSeriesName = -1;
            tableIndexOfEpisodeNr = -1;

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
                        AnalyzeHeadingRowForTableLayout(tableNode);
                    }
                    catch
                    {
                        Console.WriteLine("ERROR: Table seems legit - but could not parse table headings.");
                        Console.WriteLine("INFO: Expected something like nr/nummer/st. and deutsch/titel");
                        Console.WriteLine("INFO: This may happen if the column is called 'Originaltitel'");
                        Console.WriteLine("INFO: You may try again but select English as language");
                        DeducedFileNames.Clear();
                    }
                }

                if (tableNode != null)
                {
                    var rows = tableNode.SelectNodes($".//tr[position()>1]");

                    Console.WriteLine("DEBUG: Analyzing Table...");
                    foreach (var row in rows)
                    {
                        FileNameRepresentation f = AnalyzeRow(row, season);
                        if (f is null)
                        {
                            continue;
                        }
                        DeducedFileNames.AddLast(f);
                    }
                }

            }
        }

        private void AnalyzeByTableaggressive(HtmlDocument wikiDocument, bool SetManualIndexes)
        {
            var tableNodes = wikiDocument.DocumentNode.SelectNodes("//table");

            int season = 1;
            tableIndexOfEpisodeNr = -1;
            tableIndexOfSeriesName = -1;

            if (SetManualIndexes)
            {
                Console.WriteLine("Locate Table with Episode Numbers and Episode Titles.\nEnter Column index for *season number* (start to count at 1)");
                int.TryParse(Console.ReadLine(), out int a);
                tableIndexOfEpisodeNr = a;

                Console.WriteLine("Enter Column index for episode *name* (start with 1)");
                int.TryParse(Console.ReadLine(), out int b);
                tableIndexOfSeriesName = b;
            }

            foreach (var tbl in tableNodes)
            {
                Console.WriteLine("\n\nDEUBG: Checking Table at line: " + tbl.Line);
                if (!SetManualIndexes)
                {
                    try
                    {
                        AnalyzeHeadingRowForTableLayout(tbl);
                    }
                    catch (InvalidOperationException)
                    {
                        Console.WriteLine("DEBUG: Table does not contain parseable headings for ep no and title. Skipping table...");
                        continue;
                    }
                }

                if (tableIndexOfSeriesName <= 0 || tableIndexOfEpisodeNr <= 0)
                {
                    Console.WriteLine("DEBUG: No suitable heading detected... skipping table");
                    continue;
                }

                bool newEntriesWereAdded = false;
                var rows = tbl.SelectNodes($".//tr");  //including first row in case there is no header at all

                foreach (var row in rows)
                {
                    FileNameRepresentation f = AnalyzeRow(row, season);
                    if (f is null)
                    {
                        continue;
                    }
                    DeducedFileNames.AddLast(f);
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
        }

        private FileNameRepresentation AnalyzeRow(HtmlNode row, int season)
        {

            HtmlNode cellWithNr = row.SelectSingleNode($"./td[{tableIndexOfEpisodeNr}]");
            HtmlNode cellWithName = row.SelectSingleNode($"./td[{tableIndexOfSeriesName}]");

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

            // select first text node
            // works if multiple children text nodes are available (like simpsons-> amazon different names) and also if they are nested in a a (link)
            string episodeName = cellWithName.SelectSingleNode(".//text()").InnerText.Trim();

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


            FileNameRepresentation result = new FileNameRepresentation(SeriesName, season, episode, episodeName);
            Console.WriteLine("SUCCESS: Added episode: " + result.FullName);
            return result;
        }

        private void AnalyzeHeadingRowForTableLayout(HtmlNode tableNode)
        {
            Console.WriteLine("\nDEBUG: Analyizing Table Headers for Layout...");
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

                if (cellText.ToLower().Contains(Lang) && cellText.ToLower().Contains("titel"))
                {
                    Console.WriteLine("SUCCESS: Found column index for the Name: " + index);
                    tableIndexOfSeriesName = index;
                }
                if ((cellText.ToLower().Contains("nr.") || cellText.ToLower().Contains("nummer")) && !cellText.ToLower().Contains("ges."))
                {
                    Console.WriteLine("SUCCESS: Found column index for episode number: " + index);
                    tableIndexOfEpisodeNr = index;
                }

                index++;
            }

            if (tableIndexOfEpisodeNr == -1 || tableIndexOfSeriesName == -1)
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

    }
}

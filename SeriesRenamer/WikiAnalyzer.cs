using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using HtmlAgilityPack;

namespace SeriesRenamer
{
    public class WikiAnalyzer
    {
        public LinkedList<FileNameRepresentation> DeducedFileNames { get; set; }

        public LinkedList<FileNameRepresentation> Analyze()
        {
            Console.WriteLine("\n\n**INFO**: Starting Wiki Analysis");
            Console.WriteLine("================================");
            DeducedFileNames = AnalyzeWiki();
            Console.WriteLine($"\n\n**SUCCESS**: {DeducedFileNames.Count} potential file names registered."); //Todo kann auch 0 sein....
            return DeducedFileNames;
        }


        private static LinkedList<FileNameRepresentation> AnalyzeWiki()
        {

            HtmlWeb web = new HtmlWeb();
            HtmlDocument wikiDocument;
            wikiDocument = web.Load(new Uri(Env.wikiURL));
            wikiDocument.OptionUseIdAttribute = true;


            var h2Nodes = wikiDocument.DocumentNode.SelectNodes("//h2");
            LinkedList<FileNameRepresentation> result = AnalyzeByHeadings(h2Nodes, Env.seriesName);

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
                    result = AnalyzeByHeadings(h3Nodes, Env.seriesName);
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
            result = AnalyzeByTableaggressive(wikiDocument, false);

            if (!result.IsEmpty())
            {
                return result;
            }

            Console.WriteLine("\n\n**WARN**: Generic Table Scan failed as well, maybe you can help me here...");
            result = AnalyzeByTableaggressive(wikiDocument, true);

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
                    FileNameRepresentation f = AnalyzeRow(row, season, indexOfNameColumn, indexOfEpColumn);
                    if (f is null)
                    {
                        continue;
                    }
                    result.AddLast(f);
                }
            }

            return result;

        }

        private static LinkedList<FileNameRepresentation> AnalyzeByTableaggressive(HtmlDocument wikiDocument, bool SetManualIndexes)
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
                    FileNameRepresentation f = AnalyzeRow(row, season, indexOfNameColumn, indexOfEpColumn);
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

        private static FileNameRepresentation AnalyzeRow(HtmlNode row, int season, int indexOfNameColumn, int indexOfEpColumn)
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


            FileNameRepresentation result = new FileNameRepresentation(Env.seriesName, season, episode, episodeName);
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

                if (cellText.ToLower().Contains(Env.lang) && cellText.ToLower().Contains("titel"))
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

    }
}

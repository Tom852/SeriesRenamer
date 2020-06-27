using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace SeriesRenamer.FolderAnalysis
{
    public class FolderAnalyzer
    {
        public string Folder { get; set; }
        private BadKeywordFilter F { get; } = new BadKeywordFilter();

        public FolderAnalyzer(string folder)
        {
            Folder = folder;
        }

        public LinkedList<RawFileSystemFile> Analyze()
        {
            var result = new LinkedList<RawFileSystemFile>();

            var filesOnSystem = Directory.GetFiles(Folder);

            foreach (var filesystemFile in filesOnSystem)
            {
                string cleanedFilePath = F.Filter(filesystemFile);

                string cleanedFilename = cleanedFilePath.Split('\\').ToList().Last();

                bool filesystemFileSuccessfullyMatched = false;
                Regex r = new Regex(@"([0-3]?\d)[x\-_eE]?(\d{2})"); //goal: 112 s1e12 S01E12 1x12 1-12 etc; max 39 seasons to be more robust against stuff like "720p"
                Match m = r.Match(cleanedFilename);

                if (!m.Success)
                {
                    Console.WriteLine(
                        $"WARN: File {Path.GetFileName(filesystemFile)} does not contain a season and episode index of the form ...02..12... / File is ignored");
                }

                int.TryParse(m.Groups[1].Value, out int s);
                int.TryParse(m.Groups[2].Value, out int e);
                result.AddLast(new RawFileSystemFile(filesystemFile, s, e));
            }

            return result;
        }
    }
}

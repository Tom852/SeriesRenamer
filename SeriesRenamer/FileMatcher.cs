using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace SeriesRenamer
{
    public class FileMatcher
    {
        public LinkedList<FileNameRepresentation> FileNamePool { get; }
        public string[] FilesOnSystem { get; }

        public FileMatcher(LinkedList<FileNameRepresentation> pool, string[] fos)
        {
            FileNamePool = pool;
            FilesOnSystem = fos;
        }


        public Dictionary<string, string> Match()
        {

            Dictionary<string, string> result = new Dictionary<string, string>();
            var f = new BadKeywordFilter();
            try
            {
                foreach (string filesystemFile in FilesOnSystem)
                {
                    string cleanedFilePath = f.FilterIt(filesystemFile);

                    string cleanedFilename = cleanedFilePath.Split('\\').ToList().Last();

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
                    foreach (FileNameRepresentation deducedNewName in FileNamePool)
                    {
                        if (deducedNewName.Season == fileSeason && deducedNewName.Episode == fileEpisode)
                        {
                            string targetName = Path.Combine(UserVariables.folder, deducedNewName.FullName + extension);
                            if (result.Values.Contains(targetName))
                            {
                                Console.WriteLine("**ERROR**: Duplicate target file name: " + Path.GetFileName(filesystemFile) + " --> " + targetName);
                            }
                            else
                            {
                                result.Add(filesystemFile, targetName);
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

            return result;

        }
    }
}

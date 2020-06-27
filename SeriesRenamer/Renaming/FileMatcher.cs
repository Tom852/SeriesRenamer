using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using SeriesRenamer.WikiAnalysis;

namespace SeriesRenamer.Renaming
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
            try
            {
                foreach (string filesystemFile in FilesOnSystem)
                {

                    bool filesystemFileSuccessfullyMatched = false;
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

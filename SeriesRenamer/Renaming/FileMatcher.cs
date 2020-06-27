using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using SeriesRenamer.FolderAnalysis;
using SeriesRenamer.WikiAnalysis;

namespace SeriesRenamer.Renaming
{
    public class FileMatcher
    {
        public LinkedList<FileNameRepresentation> FileNamePool { get; }
        public LinkedList<RawFileSystemFile> FilesOnSystem { get; }

        public FileMatcher(LinkedList<FileNameRepresentation> pool, LinkedList<RawFileSystemFile> fos)
        {
            FileNamePool = pool;
            FilesOnSystem = fos;
        }


        public Dictionary<string, string> Match()
        {
            Console.WriteLine("\n\n**INFO**: Matching Files on System");
            Console.WriteLine("==================================");

            Dictionary<string, string> result = new Dictionary<string, string>();
            try
            {
                foreach (var fileOnSystem in FilesOnSystem)
                {

                    bool filesystemFileSuccessfullyMatched = false;
                    //Console.WriteLine($"DEBUG: Filename Analysis result:\nFile: {filesystemFile}\nCropped: {cleanedFilename}\nSeason: {fileSeason} - Ep: {fileEpisode}\n");
                    foreach (FileNameRepresentation fileInPool in FileNamePool)
                    {
                        if (fileInPool.Season == fileOnSystem.Season && fileInPool.Episode == fileOnSystem.Episode)
                        {
                            string targetPath = Path.Combine(fileOnSystem.JustFolder, fileInPool.FullName + fileOnSystem.Extension);
                            if (result.Values.Contains(targetPath))
                            {
                                Console.WriteLine("**ERROR**: Duplicate target file name: " + fileOnSystem.JustFileName + " --> " + targetPath);
                            }
                            else
                            {
                                result.Add(fileOnSystem.PathToFile, targetPath);
                                filesystemFileSuccessfullyMatched = true;
                            }
                            break;
                        }

                    }
                    if (!filesystemFileSuccessfullyMatched)
                    {
                        Console.WriteLine("WARN: No Mapping found for file: " + fileOnSystem.JustFileName);
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

using System.Collections.Generic;
using System.IO;
using System.Reflection;

namespace SeriesRenamer.FolderAnalysis
{
    public class BadKeywordFilter
    {

        public HashSet<string> WordList { get; }

        public BadKeywordFilter()
        {
            WordList = GetWords();
        }
        private HashSet<string> GetWords()
        {

            Assembly asm = Assembly.GetExecutingAssembly();
            string path = System.IO.Path.GetDirectoryName(asm.Location);
            string p = Path.Combine(path, "badWords.txt");

            string[] words;
            try
            {
                words = File.ReadAllLines(p);
            }
            catch (IOException)
            {
                words = new[]
                {
                    "dd51",
                    "dd20",
                    "DD51",
                    "x264",
                    "X264",
                    "x265",
                    "X265",
                    "144",
                    "240",
                    "360",
                    "480",
                    "720",
                    "1024",
                    "7p",
                    "72p",
                    "4K",
                    "2K"
                };
            }

            HashSet<string> result = new HashSet<string>(words);
                
            return result;
        }

        public string Filter(string word)
        {
            string result = word;
            foreach (var badThing in WordList)
            {
                result = result.Replace(badThing, "");
            }

            return result;

        }
    }
}
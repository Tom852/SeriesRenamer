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
            string p = Path.Combine(path, "keywords.txt");

            string[] words = File.ReadAllLines(p);

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
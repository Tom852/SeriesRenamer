using System.IO;

namespace SeriesRenamer.FolderAnalysis
{
    public class RawFileSystemFile
    {
        public string PathToFile { get; set; }
        public int Season { get; set; }
        public int Episode { get; set; }
        public string Extension => Path.GetExtension(PathToFile);
        public string JustFileName => Path.GetFileNameWithoutExtension(PathToFile);
        public string JustFolder => Path.GetDirectoryName(PathToFile);

        public RawFileSystemFile(string p, int s, int e)
        {
            PathToFile = p;
            Season = s;
            Episode = e;
        }
    }
}

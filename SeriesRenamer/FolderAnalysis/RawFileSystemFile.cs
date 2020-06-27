namespace SeriesRenamer.FolderAnalysis
{
    public class RawFileSystemFile
    {
        public string PathToFile { get; set; }
        public int Season { get; set; }
        public int Episode { get; set; }

        public RawFileSystemFile(string p, int s, int e)
        {
            PathToFile = p;
            Season = s;
            Episode = e;
        }
    }
}

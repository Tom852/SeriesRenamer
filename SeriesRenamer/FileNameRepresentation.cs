using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Web;

namespace SeriesRenamer
{
    public class FileNameRepresentation
    {

        public FileNameRepresentation(string seriesName, int season, int episode, string episodeName)
        {
            Series = seriesName;
            Season = season;
            Episode = episode;
            Name = episodeName;
          
        }

        public static string Series { get; set; }
        public int Season { get; set; }
        public int Episode { get; set; }

        private string _name;
        public string Name {
            get => HttpUtility.HtmlDecode(_name).Replace("ß", "ss");
            set => _name = string.Concat(value.Split(Path.GetInvalidFileNameChars()));
            
        }

        public string FullName { get => string.Format("{0} - {1:00}x{2:00} - {3}", Series, Season, Episode, Name); }
    }
}

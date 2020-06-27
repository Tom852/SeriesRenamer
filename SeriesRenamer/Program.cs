using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;

namespace SeriesRenamer
{
    public class Program
    {

        static void Main(string[] args)
        {

            new Pipeline().Run(args);
        }
    }

}

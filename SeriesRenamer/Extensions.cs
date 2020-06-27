using System;
using System.Collections.Generic;
using System.Text;

namespace SeriesRenamer
{

    public static class Extensions
    {
        public static bool IsEmpty<T>(this ICollection<T> coll)
        {
            return coll.Count == 0;
        }
    }

}

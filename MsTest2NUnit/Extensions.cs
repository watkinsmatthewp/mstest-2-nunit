using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MsTest2NUnit
{
    public static class Extensions
    {
        public static bool Contains(this string value, string searchString, StringComparison strComp)
        {
            return value.IndexOf(searchString, strComp) > -1;
        }
    }
}

using System.Linq;

namespace FFLogsLookup.Utils
{
    internal static class Mathx
    {
        public static T Max<T>(params T[] v)
        {
            return v.Max();
        }
    }
}

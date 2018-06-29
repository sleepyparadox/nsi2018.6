using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace System
{
    public static class RandomUtility
    {
        public static bool NextBool(this Random rand)
        {
            return rand.Next(0, 100) >= 50;
        }

        public static int NextInt(this Random rand, int min = 0, int max = int.MaxValue)
        {
            return rand.Next(min, max);
        }
    }
}

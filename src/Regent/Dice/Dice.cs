using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Regent
{
    public class Dice
    {
        static DieResult[] Results = Enum.GetValues(typeof(DieResult)).Cast<DieResult>().ToArray();
        public static DieResult Roll()
        {
            return Results[Game.Rand.NextInt(0, Results.Length)];
        }
    }
}

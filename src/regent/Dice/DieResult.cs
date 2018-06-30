using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Regent
{
    public enum DieResult
    {
        CriticalFailure = -3,
        DropItem = -2,
        Disadvantage = -1,
        Assassinate = 3,
        TakeItem = 2,
        Advantage = 1,
    }
}

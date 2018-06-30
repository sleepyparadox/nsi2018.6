using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Regent.Cards
{
    public class WeaponCard : ICard, IWeapon
    {
        public string Name { get; set; }
        public int Intrigue { get; set; }

        public WeaponCard()
        {
            Name = Game.Grammars.Write("weapon");

            Intrigue = Game.Rand.NextInt(1, 5);
        }

        public override string ToString()
        {
            return Name;
        }
    }
}

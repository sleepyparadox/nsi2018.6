using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Regent.Cards
{
    public class ItemCard : ICard
    {
        public string Name { get; set; }

        public ItemCard()
        {
            Name = Game.Grammars.Write("weapon");
        }
    }
}

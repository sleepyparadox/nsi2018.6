using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Regent.Cards
{
    public class Deck
    {
        public ICard DrawCard()
        {
            var cardType = Game.Rand.NextInt(0, 100);

            ICard card;
            if(cardType < 20)
            {
                var highborn = Game.Rand.NextInt(0, 100) >= 80;
                card = new AgentCard(highborn);
            }
            else
            {
                card = new WeaponCard();
            }

            return card;
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Regent.Cards
{
    public class Deck
    {
        public ICard DrawCard(Player player)
        {
            var cardType = Game.Rand.NextInt(0, 100);

            ICard card;
            if(cardType < 40)
            {
                var highborn = Game.Rand.NextInt(0, 100) >= 80;
                card = new AgentCard(highborn);
            }
            else
            {
                card = new WeaponCard();
            }


            if(player.IsHuman)
            {
                Game.Log("{0} draws {1}", player, card);
            }
            else
            {
                Game.Log("{0} draws a card", player);
            }

            return card;
        }
    }
}

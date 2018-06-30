using Regent.Cards;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Regent
{
    public class Player
    {
        public Chamber Chamber;
        public readonly bool IsHuman;
        public List<AgentCard> Agents;
        public List<ICard> Hand;
        public List<ICard> Discards;

        public int Power { get { return Discards.Sum(c => (c is AgentCard) ? (c as AgentCard).Power : 0); } }
        public bool Active { get { return Agents.Any(); } }

        public Player(Chamber chamber, bool isPlayer)
        {
            Chamber = chamber;
            IsHuman = isPlayer;
            Agents = new List<AgentCard>()
            {
                new AgentCard(true),
                new AgentCard(false),
            };
            Hand = new List<ICard>()
            {
                Game.Deck.DrawCard(this),
                Game.Deck.DrawCard(this),
                Game.Deck.DrawCard(this),
                Game.Deck.DrawCard(this),
            };
            Discards = new List<ICard>();
        }

        public override string ToString()
        {
            return Chamber.ToString();
        }
    }
}

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
            Hand = new List<ICard>();
            Discards = new List<ICard>();
            Agents = new List<AgentCard>()
            {
                new AgentCard(true),
                new AgentCard(false),
            };
            DrawCard();
            DrawCard();
            DrawCard();
            DrawCard();
        }

        public void DrawCard()
        {
            var card = Game.Deck.DrawCard();
            Hand.Add(card);
            if (IsHuman)
            {
                Log.Line("{0} draws {1}", this, card);
            }
            else
            {
                Log.Line("{0} draws a card", this);
            }
        }

        public override string ToString()
        {
            return string.Format("{0} Faction", Chamber);
        }
    }
}

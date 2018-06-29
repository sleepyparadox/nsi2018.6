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
        public readonly bool IsPlayer;
        public List<ICard> Cards;
        public bool Lost { get { return GetAgent() == null; } }

        public Player(bool isPlayer)
        {
            IsPlayer = isPlayer;
            Cards = new List<ICard>()
            {
                new AgentCard()
            };
        }

        public AgentCard GetAgent()
        {
            return Cards.FirstOrDefault(c => c is AgentCard) as AgentCard;
        }

        public override string ToString()
        {
            var agent = GetAgent();
            if (agent != null)
                return agent.ToString();
            else
                return "(DEAD)";
        }
    }
}

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
        public readonly bool IsHuman;
        public List<ICard> Cards;
        public bool Active { get { return GetAgent() != null; } }

        public Player(bool isPlayer)
        {
            IsHuman = isPlayer;
            Cards = new List<ICard>()
            {
                new AgentCard(true)
            };
        }

        public AgentCard GetAgent()
        {
            return Cards.FirstOrDefault(c => c is AgentCard) as AgentCard;
        }

        public override string ToString()
        {
            return IsHuman ? (GetAgentString() + " (HUMAN)") : GetAgentString();
        }

        string GetAgentString()
        {
            var agent = GetAgent();
            if (agent != null)
            {
                return agent.ToString();
            }
            else
            {
                return "(DEAD)";
            }
        }
    }
}

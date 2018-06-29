using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Regent.Cards
{
    public class AgentCard : ICard
    {
        public string Name { get; set; }
        public int Level { get; set; }

        public AgentCard()
        {
            var highborn = Game.Rand.NextBool();

            Name = Game.Grammars.Write("agent.name");
            Level = Game.Rand.Next(0, 10);
        }

        public override string ToString()
        {
            return Name;
        }
    }
}

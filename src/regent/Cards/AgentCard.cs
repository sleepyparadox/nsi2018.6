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

        public int Advantage { get; set; }

        static readonly int[] LowbornRange = new int[] { 1, 5 };
        static readonly int[] HighbornRange = new int[] { 6, 10 };

        public AgentCard(bool highborn)
        {
            Name = Game.Grammars.Write(highborn ? "agent.name.highborn": "agent.name.lowborn");
            var range = (highborn ? HighbornRange : LowbornRange);
            Level = Game.Rand.Next(range[0], range[1]);
        }



        public override string ToString()
        {
            return Name;
        }
    }
}

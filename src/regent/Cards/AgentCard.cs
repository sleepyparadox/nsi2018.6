using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Regent.Cards
{
    public class AgentCard : ICard, IWeapon
    {
        public string Name { get; set; }
        public int Power { get; set; }
        public int Intrigue { get; set; }
        public bool Tapped { get; set; }
        public TapReason TapReason { get; set; }

        static readonly int[] LowbornRange = new int[] { 1, 5 };
        static readonly int[] HighbornRange = new int[] { 6, 10 };

        public AgentCard(bool highborn)
        {
            Name = Game.Grammars.Write(highborn ? "agent.name.highborn": "agent.name.lowborn");
            Power = Game.Rand.Next(1, 5);
            var range = (highborn ? HighbornRange : LowbornRange);
            Intrigue = Game.Rand.Next(range.Min(), range.Max());
        }

        public void Tap(TapReason reason)
        {
            Tapped = true;
            TapReason = reason;
            Game.Log("{0} is {1} and cannot leave {2}", this, TapReason, Game.GetOwner(this));
        }

        public void Untap()
        {
            Tapped = false;
            Game.Log("{0} is no longer {1}", this, TapReason);
        }

        public override string ToString()
        {
            var owner = Game.GetOwner(this);

            var tappedReason = Tapped ? (" " + TapReason) : "";

            if(owner == null)
                return string.Format("{0} (dead)", Name);
            else
                return string.Format("{0} ({1} +{2} intrigue{3})", Name, owner, Intrigue, tappedReason);
        }

        public bool GetIsUsed()
        {
            return Game.Moves.Any(m => m.Agent == this);
        }
    }
}

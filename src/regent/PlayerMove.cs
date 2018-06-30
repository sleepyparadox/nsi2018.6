using Regent.Cards;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Regent
{
    public class PlayerMove
    {
        public Player Player;
        public AgentCard Agent;
        public ICard FacedownCard;
        public Chamber Chamber;

        public int GetIntrigue()
        {
            var weapon = FacedownCard as IWeapon;

            return Agent.Intrigue + (weapon != null ? weapon.Intrigue : 0);
        }

        public override string ToString()
        {
            if(FacedownCard is IWeapon)
            {
                return string.Format("{0} with {1} ({2} +{3} Intrigue)", Agent.Name, FacedownCard.Name, Player.Chamber, GetIntrigue());
            }

            return base.ToString();
        }
    }
}

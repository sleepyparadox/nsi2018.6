﻿using Regent.Cards;
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

        public bool IsAttackMove()
        {
            return FacedownCard is IWeapon 
                && IsDefendMove() == false;
        }

        public bool IsDefendMove()
        {
            return Chamber == Player.Chamber;
        }

        public void LogInitialState()
        {
            if (Player.IsHuman)
            {
                if (IsDefendMove())
                    Log.Line("(defend) {0} hides inside {1} armed with {2}", Agent, Chamber, FacedownCard);
                else
                    Log.Line("(attack) {0} waits outside {1} armed with {2}", Agent, Chamber, FacedownCard);
            }
            else
            {
                if (IsDefendMove())
                    Log.Line("{0} was seen retreating to their {1}", Agent, Chamber);
                else
                    Log.Line("{0} was seen near {1}", Agent, Chamber);
            }

            Log.Sleep();
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

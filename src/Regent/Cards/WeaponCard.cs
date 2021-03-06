﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Regent.Cards
{
    public class WeaponCard : ICard, IWeapon
    {
        public string Name { get; set; }
        public int Intrigue { get; set; }

        public WeaponCard()
        {
            Name = Game.Grammars.Write("weapon");

            Intrigue = Game.Rand.NextInt(5, 10);
        }

        public override string ToString()
        {
            return string.Format("{0} (+{2} intrigue)", Name, Game.GetOwner(this), Intrigue);
        }
    }
}

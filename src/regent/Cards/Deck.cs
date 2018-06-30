﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Regent.Cards
{
    public class Deck
    {
        public ICard DrawCard()
        {
            var highborn = Game.Rand.NextInt(0, 100) >= 80;

            return new AgentCard(highborn);
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Regent.Cards
{
    public interface ICard
    {
        string Name { get; set; }
    }

    public interface ICardEvent : ICard
    {

    }

    public interface IWeapon : ICard
    {
        int Intrigue { get; set; }
    }
}

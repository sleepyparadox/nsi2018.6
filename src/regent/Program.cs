using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Regent
{
    class Program
    {
        static void Main(string[] args)
        {
            var game = new Game();
            game.Start();
            while (true)
            {
                game.Step();
            }
        }
    }
}

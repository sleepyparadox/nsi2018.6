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
            while(true)
            {
                var game = new Game();
                game.Start();
                while (game.Active)
                {
                    game.Step();
                }

                string newGame = "New Game";
                string quit = "Quit";

                var result = Controls.ChooseOne(new string[] { newGame, quit }, true);
                if (result == quit)
                    break;
            }
            
        }
    }
}

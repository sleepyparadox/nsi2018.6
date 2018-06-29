using Regent.Cards;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Regent
{
    public class Game
    {
        public static Random Rand;
        public static Deck Deck;
        public static List<Player> Players;
        public static Grammars Grammars;

        public void Start()
        {
            Rand = new Random();
            Deck = new Deck();
            Players = new List<Player>();
            Grammars = new Grammars("Grammars/Grammars.txt");

            Players = new List<Player>()
            {
                new Player(true),
                new Player(false),
                new Player(false),
            };

            Log("New game starring:");
            foreach (var player in Players)
            {
                Log(player);
            }
        }

        public void Step()
        {
            foreach(var player in Players)
            {
                StepPlayer(player);
                Console.ReadLine();
            }
        }

        static void StepPlayer(Player player)
        {
            var faceup = Deck.DrawCard();
            Log("{0} drew {1}", player, faceup);
        }

        public static void Log(object arg)
        {
            Log("{0}", arg);
        }

        public static void Log(string msg, params object[] args)
        {
            Console.WriteLine(msg, args);
        }
    }
}

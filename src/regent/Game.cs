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
        public bool Active { get { return Players.Any(p => p.IsHuman && p.Active); } }

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
            foreach(var player in Players.Where(p => p.Active))
            {
                StepPlayer(player);
                Console.ReadLine();
            }

            if(Active == false)
            {
                // Game over
                Log("Game over");
            }
        }

        static void StepPlayer(Player player)
        {
            var faceup = Deck.DrawCard();
            //Log("{0} drew {1}", player, faceup);

            if(faceup is AgentCard)
            {
                Fight(null, faceup as AgentCard, player);
            }
        }

        static void Fight(Player attackingPlayer, AgentCard attackingAgent, Player defendingPlayer)
        {
            Log("{0} tries to assissate {1}", attackingAgent, defendingPlayer);
            var defendingAgent = defendingPlayer.GetAgent();

            DieResult result;
            if (attackingAgent.Level > defendingAgent.Level
                    && attackingPlayer != null && attackingPlayer.IsHuman)
            {
                // Attacker Choice (human)
                result = GetChoice(Dice.Roll(), Dice.Roll());
            }
            else if (attackingAgent.Level > defendingAgent.Level)
            {
                // Attacker Choice
                result = new DieResult[] { Dice.Roll(), Dice.Roll() }.Max();
            }
            else if (defendingAgent.Level > attackingAgent.Level
                    && defendingPlayer != null && defendingPlayer.IsHuman)
            {
                // Defender Choice (human)
                result = GetChoice(Dice.Roll(), Dice.Roll());
            }
            else if (defendingAgent.Level > attackingAgent.Level)
            {
                // Defender Choice
                result = new DieResult[] { Dice.Roll(), Dice.Roll() }.Min();
            }
            else
            {
                // Tied
                result = Dice.Roll();
            }

            if (result == DieResult.Assassinate)
                Discard(defendingAgent);
            else if (result == DieResult.Advantage)
                defendingAgent.Advantage = -10;
            else if (result == DieResult.TakeItem)
                StealItem(attackingPlayer, defendingPlayer);
            else if (result == DieResult.CriticalFailure)
                Discard(attackingAgent);
            else if (result == DieResult.Disadvantage)
                attackingAgent.Advantage = -10;
            else if (result == DieResult.DropItem)
                StealItem(defendingPlayer, attackingPlayer);
            else
                throw new NotImplementedException(result.ToString());

        }

        static void StealItem(Player attacker, Player defender)
        {
            ItemCard item;
            if(defender == null)
            {
                item = new ItemCard();
            }
            else
            {
                item = defender.Cards.FirstOrDefault(c => c is ItemCard) as ItemCard;
                if (item != null)
                    defender.Cards.Remove(item);
                else
                    item = new ItemCard();
            }

            if(attacker == null)
            {
                Discard(item);
            }
            else
            {
                attacker.Cards.Add(item);
            }
            Log("{0} was stolen", item);
        }

        static DieResult GetChoice(DieResult zChoice, DieResult xChoice)
        {
            // no brainer choices
            if (zChoice == DieResult.Assassinate || (zChoice > 0 && xChoice < 0))
                return zChoice;
            if (xChoice == DieResult.Assassinate || (xChoice > 0 && zChoice < 0))
                return xChoice;

            Console.WriteLine("Choose:");
            Console.WriteLine("[z] {0}", zChoice);
            Console.WriteLine("[x] {0}", xChoice);
            ConsoleKeyInfo key;
            do
            {
                key = Console.ReadKey();
            }
            while (key.Key != ConsoleKey.Z && key.Key != ConsoleKey.X);

            if (key.Key == ConsoleKey.Z)
                return zChoice;
            else if (key.Key == ConsoleKey.X)
                return xChoice;
            else
                throw new Exception();
        }

        static void Discard(ICard card, bool silent = false)
        {
            if(silent == false)
            {
                Log("{0} was discarded", card);
            }

            foreach(var player in Players)
            {
                if(player.Cards.Contains(card))
                {
                    player.Cards.Remove(card);
                }
            }
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

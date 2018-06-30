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
        public static Dictionary<Chamber, Player> Players;
        public static Grammars Grammars;

        public bool Active { get { return Players.Values.Any(p => p.IsHuman && p.Active); } }

        public void Start()
        {
            Rand = new Random();
            Deck = new Deck();
            Players = new Dictionary<Chamber, Player>();
            Grammars = new Grammars("Grammars/Grammars.txt");

            Players = new Dictionary<Chamber, Player>()
            {
                { Chamber.BlueChamber, new Player(Chamber.BlueChamber, true) },
                { Chamber.GreenChamber, new Player(Chamber.GreenChamber, false) },
                { Chamber.RedChamber, new Player(Chamber.RedChamber, false) },
            };

            Log("New game starring:");
            foreach (var player in Players)
            {
                Log("New player {0}", player);
            }
        }

        public void Step()
        {
            var moves = new List<PlayerMove>();
            foreach(var player in Players.Values.Where(p => p.Active))
            {
                moves.Add(StepMainPhase(player));
            }

            foreach (var move in moves)
            {
                ResolveEvents(move);
            }

            foreach (var move in moves)
            {
                ResolveMove(move);
            }

            if (Active == false)
            {
                // Game over
                Log("Game over");
            }
        }

        static PlayerMove StepMainPhase(Player player)
        {
            var chambers = Players.Keys.ToList();
            chambers.Insert(0, Chamber.Court);
            chambers.Remove(player.Chamber);

            var facedown = Deck.DrawCard();
            player.Hand.Add(facedown);

            var move = new PlayerMove();
            move.Player = player;
            move.Agent = Controls.ChooseOne(player.AgentsCards, player.IsHuman, "Choose an agent:");
            move.FacedownCard = Controls.ChooseOne<ICard>(player.Hand, player.IsHuman, "Choose an accomplice:");
            move.Chamber = Controls.ChooseOne(chambers, player.IsHuman, "Select a chamber to infiltrate:");

            return move;
        }

        public static void ResolveEvents(PlayerMove move)
        {

        }

        public static void ResolveMove(PlayerMove move)
        {
            if(move.Chamber == Chamber.Court)
            {
                Log("{0} trades in {1}", move.Agent, move.FacedownCard);
                Discard(move.FacedownCard);
                move.Player.Hand.Add(Deck.DrawCard());
                move.Player.Hand.Add(Deck.DrawCard());
                return;
            }

            var weapon = move.FacedownCard as IWeapon;
            if (weapon == null)
                return;

            var defendingPlayer = Players[move.Chamber];
            Fight(move, move.Player, move.Agent, defendingPlayer);
            Discard(move.FacedownCard);
        }

        static void Fight(PlayerMove move, Player attackingPlayer, AgentCard attackingAgent, Player defendingPlayer)
        {
            Log("{0} tries to assassinate {1}", attackingAgent, defendingPlayer);
            var defendingAgent = defendingPlayer.AgentsCards.Last();

            DieResult result;
            if (attackingAgent.Level > defendingAgent.Level
                    && attackingPlayer != null && attackingPlayer.IsHuman)
            {
                // Attacker Choice (human)
                result = Controls.ChooseOne(new[] { Dice.Roll(), Dice.Roll() }, true);
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
                result = Controls.ChooseOne(new[] { Dice.Roll(), Dice.Roll() }, true);
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

            if (result == DieResult.Kill)
            {
                Discard(defendingAgent);
            }
            else if (result == DieResult.AllKill)
            {
                Discard(defendingAgent);
                Discard(attackingAgent);
            }
            else if (result == DieResult.InspireFear)
            {
                defendingAgent.Tapped = true;
            }
            else if(result == DieResult.RaiseSuspicion)
            {
                attackingAgent.Tapped = true;
            }
            else if (result == DieResult.DropItem)
            {
                Discard(move.FacedownCard);
            }
            else if(result == DieResult.Caught)
            {
                Discard(attackingAgent);
            }
            else
            {
                // unknown result
                throw new NotImplementedException(result.ToString());
            }
        }

        static void Discard(ICard card, bool silent = false)
        {
            if(silent == false)
            {
                Log("{0} was discarded", card);
            }

            foreach(var player in Players.Values)
            {
                if(player.AgentsCards.Contains(card))
                {
                    player.AgentsCards.Remove(card as AgentCard);
                }
                if(player.Hand.Contains(card))
                {
                    player.Hand.Remove(card);
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

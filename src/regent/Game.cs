using Regent.Cards;
using System;
using System.Collections;
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

        public bool Active { get { return Players.Values.Count(p => p.Active) > 1; } }

        public void Start()
        {
            Rand = new Random();
            Deck = new Deck();
            Players = new Dictionary<Chamber, Player>();
            Grammars = new Grammars("Grammars/Grammars.txt");

            Players = new Dictionary<Chamber, Player>()
            {
                { Chamber.Blue, new Player(Chamber.Blue, false) },
                { Chamber.Green, new Player(Chamber.Green, false) },
                { Chamber.Red, new Player(Chamber.Red, true) },
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
                Log();
            }

            for (int i = 0; i < moves.Count;)
            {
                var move = moves[i];
                if(ResolveEvents(move))
                {
                    Log();
                    // this card is done
                    moves.Remove(move);
                }
                else
                {
                    // next card
                    i++;
                }

            }
            
            while(moves.Any())
            {
                var activeMove = moves.First();
                var allAttackers = moves.Where(m => m.Chamber == activeMove.Chamber).ToList();
                foreach (var attacker in allAttackers)
                    moves.Remove(attacker);

                ResolveAttack(allAttackers);
                Log();
            }

            if (Active == false)
            {
                // Game over
                Log("Game over");

                if (Players.Values.Any(p => p.Active && p.IsHuman))
                {
                    Log("You win");
                }
                else
                {
                    Log("You lose");
                }
            }
        }

        static PlayerMove StepMainPhase(Player player)
        {
            var chambers = Players.Keys.ToList();
            chambers.Insert(0, Chamber.Court);
            chambers.Remove(player.Chamber);

            var facedown = Deck.DrawCard(player);
            player.Hand.Add(facedown);

            var move = new PlayerMove();
            move.Player = player;
            move.Agent = Controls.ChooseOne(player.AgentsCards, player.IsHuman, "Choose an agent:");
            move.FacedownCard = Controls.ChooseOne(player.Hand, player.IsHuman, "Choose an accomplice:");
            move.Chamber = Controls.ChooseOne(chambers, player.IsHuman, "Select a chamber to infiltrate:");

            if(player.IsHuman)
            {
                Log("{0} moves into position", move.Agent);
            }
            else
            {
                System.Threading.Thread.Sleep(1000);
                Log("{0} was seen near {1}", move.Agent, move.Chamber);
            }

            return move;
        }

        public static bool ResolveEvents(PlayerMove move)
        {
            if (move.Chamber == Chamber.Court)
            {
                Log("{0} trades in {1}", move.Agent, move.FacedownCard);
                Discard(move.FacedownCard);
                move.Player.Hand.Add(Deck.DrawCard(move.Player));
                move.Player.Hand.Add(Deck.DrawCard(move.Player));
                return true;
            }

            return false;
        }

        static void ResolveAttack(IEnumerable<PlayerMove> attackers)
        {
            var primaryAttacker = attackers.First();

            var defendingPlayer = Game.Players[primaryAttacker.Chamber];

            Log("{0} sneak into the {1}", attackers, defendingPlayer);

            if (defendingPlayer.Active == false)
            {
                Log("But {0} was empty", defendingPlayer);
                return;
            }

            var attackingIntrigue = attackers.Sum(a => a.GetIntrigue());

            var defendingAgent = defendingPlayer.AgentsCards.Last();
            var defendingIntruge = defendingAgent.Intrigue;

            Log("and find {0}", defendingAgent);

            DieResult result;
            if (attackingIntrigue > defendingIntruge)
            {
                Log("The attackers have overwhelming advantage");

                // Attacker Choice
                result = Controls.ChooseOne(new[] { Dice.Roll(), Dice.Roll() }, primaryAttacker.Player.IsHuman);
            }
            else if (defendingIntruge > attackingIntrigue)
            {
                Log("The attackers are under prepared");
              
                // Defender Choice
                result = Controls.ChooseOne(new[] { Dice.Roll(), Dice.Roll() }, defendingPlayer.IsHuman);
            }
            else
            {
                Log("It is a tense encounter");
               
                // Tied
                result = Dice.Roll();
            }

            if (result == DieResult.KillTarget)
            {
                Log("The attackers succeed");

                Discard(defendingAgent);
            }
            else if (result == DieResult.EveryoneDies)
            {
                Log("Everyone manages to kill themselves");

                foreach (var attacker in attackers)
                {
                    Discard(attacker.Agent);
                    Discard(attacker.FacedownCard);
                }
                Discard(defendingAgent);
            }
            else if (result == DieResult.InspireFear)
            {
                Log("{0} survives but is frightened", defendingAgent);

                defendingAgent.Tapped = true;
            }
            else if(result == DieResult.RaiseSuspicion)
            {
                Log("{0} fail and raise suspicion", attackers);

                foreach (var attacker in attackers)
                {
                    attacker.Agent.Tapped = true;
                }
            }
            else if (result == DieResult.DropItem)
            {
                foreach (var attacker in attackers)
                {
                    Discard(attacker.FacedownCard);
                }
            }
            else if(result == DieResult.GetHanged)
            {
                foreach (var attacker in attackers)
                {
                    Discard(attacker.Agent);
                    Discard(attacker.FacedownCard);
                }
            }
            else
            {
                // unknown result
                throw new NotImplementedException(result.ToString());
            }
        }

        static void Discard(ICard card, bool silent = false)
        {
            if (GetOwner(card) == null)
            {
                // already discarded
                return;
            }

            Log("{0} discarded", card);

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

        public static Player GetOwner(ICard card)
        {
            foreach (var player in Players.Values)
            {
                if(player.AgentsCards.Contains(card)
                    || player.Hand.Contains(card))
                {
                    return player;
                }
            }

            return null;
        }

        public static void Log()
        {
            Log("");
        }

        public static void Log(object arg)
        {
            Log("{0}", arg);
        }

        public static void Log(string msg, params object[] args)
        {
            for (int i = 0; i < args.Length; i++)
            {
                if(args[i] is IEnumerable)
                {
                    var groupString = string.Join(", ", (args[i] as IEnumerable).Cast<object>().Select(o => o.ToString()).ToArray());
                    var lastComma = groupString.LastIndexOf(',');

                    if(lastComma > 0)
                    {
                        groupString = groupString.Substring(0, lastComma)
                                                + " and"
                                                + groupString.Substring(lastComma + 1, groupString.Length - (lastComma + 1));
                    }
                    
                    args[i] = groupString;
                }
            }

            Console.WriteLine(msg, args);
        }
    }
}

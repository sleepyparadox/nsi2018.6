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
        public static int Day = 0;
        public static Random Rand;
        public static Deck Deck;
        public static Dictionary<Chamber, Player> Players;
        public static Grammars Grammars;

        public static List<PlayerMove> Moves;
        static bool LoggingEnabled = false;
        static int LogDelayMs = 1000;

        public bool Active { get { return Players.Values.Count(p => p.Active) > 1; } }

        public void Start()
        {
            Rand = new Random();
            Deck = new Deck();
            Players = new Dictionary<Chamber, Player>();
            Grammars = new Grammars("Grammars/Grammars.txt");
            Moves = new List<PlayerMove>();

            Players = new Dictionary<Chamber, Player>()
            {
                { Chamber.Blue, new Player(Chamber.Blue, false) },
                { Chamber.Green, new Player(Chamber.Green, false) },
                { Chamber.Red, new Player(Chamber.Red, true) },
            };

            LoggingEnabled = true;
            Log("You are playing as {0}", Players.Values.FirstOrDefault(p => p.IsHuman));
            Log("You must defeat {0}", Players.Values.Where(p => p.IsHuman == false).ToList());
            Log();
        }

        public void Step()
        {
            Day++;
            Log("Day {0}", Day);
            Log();

            Moves.Clear();
            foreach(var player in Players.Values.Where(p => p.Active))
            {
                Moves.Add(StepMainPhase(player));
                Log();
                Sleep();
            }

            LogLaterThatNight();

            foreach (var agent in Players.Values.SelectMany(p => p.Agents).Where(a => a.Tapped))
            {
                agent.Untap();
            }

            var events = Moves.ToList();
            for (int i = 0; i < events.Count;)
            {
                var @event = events[i];
                if(ResolveEvents(@event))
                {
                    Log();
                    Sleep();
                    // this card is done
                    events.Remove(@event);
                }
                else
                {
                    // next card
                    i++;
                }

            }

            var attackMoves = Moves.Where(m => m.IsAttackMove()).ToList();
            while(attackMoves.Any())
            {
                var firstAttack = attackMoves.First();
                var allAttacks = attackMoves.Where(m => m.Chamber == firstAttack.Chamber).ToList();
                foreach (var attack in allAttacks)
                    attackMoves.Remove(attack);

                ResolveAttack(allAttacks);
                Log();
                Sleep();
            }

            foreach(var defendMove in Moves.Where(m => m.IsDefendMove()))
            {
                var chamberActvity = Moves.Count(m => m.Chamber == defendMove.Chamber);
                if(chamberActvity == 1) // alone
                {
                    Log("Nothing happened in {0}", defendMove.Chamber);
                    Log();
                }
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
            var allChambers = Players.Keys.ToList();
            allChambers.Insert(0, Chamber.Court);
            var tappedChamber = new List<Chamber>() { player.Chamber };

            var facedown = Deck.DrawCard(player);
            player.Hand.Add(facedown);

            // Play agents from hand
            while(true)
            {
                var plot = "Plot";
                var recruit = string.Format("Recruit from hand ({0})", player.Hand.Where(c => c is AgentCard).Count());
                var inspectChamber = string.Format("Inspect Chamber ({0})", player.Agents.Count);
                var inspectHand = string.Format("Inspect Hand ({0})", player.Hand.Count);
                var inspectEnemy = string.Format("Inspect Enemy Chamber");

                var choices = new List<string>(){ plot, recruit, inspectChamber, inspectHand};
                if (player.IsHuman)
                    choices.Add(inspectEnemy);

                var choice = Controls.ChooseOne(choices, player.IsHuman);

                if (choice == plot)
                    break;

                else if(choice == recruit && player.Hand.Any(c => c is AgentCard))
                {
                    var recruitables = player.Hand.Where(c => c is AgentCard).ToList();
                    var recruited = Controls.ChooseOne(recruitables, player.IsHuman);
                    player.Hand.Remove(recruited);
                    player.Agents.Add(recruited as AgentCard);
                    Log("{0} joined the {1} faction", recruited, player);
                }
                else if (choice == inspectChamber && player.IsHuman)
                {
                    foreach(var card in player.Agents)
                    {
                        Log("Chamber contains {0}", card);
                    }
                }
                else if (choice == inspectHand && player.IsHuman)
                {
                    foreach (var card in player.Hand)
                    {
                        Log("Hand contains {0}", card);
                    }
                }
                else if(choice == inspectEnemy)
                {
                    var enemy = Controls.ChooseOne<Player>(Players.Values.Where(p => p.IsHuman == false).ToArray(), true);
                    Log("{0} has {1} cards in hand", enemy, player.Hand.Count);
                    foreach (var card in enemy.Agents.Where(a => Moves.Any(m => m.Agent == a) == false))
                    {
                        Log("{0} contains {1}", enemy, card);
                    }
                    var enemyMove = Moves.FirstOrDefault(m => m.Player == enemy);
                    if (enemyMove != null)
                        enemyMove.LogInitialState();
                }
            }
      
            // Plot
            var move = new PlayerMove();
            move.Player = player;
            move.Agent = Controls.ChooseOne(player.Agents, player.IsHuman, "Choose an agent:");
            move.FacedownCard = Controls.ChooseOne(player.Hand, player.IsHuman, "Choose an accomplice:");
            move.Chamber = Controls.ChooseOne(move.Agent.Tapped ? tappedChamber : allChambers, player.IsHuman, "Select a chamber to infiltrate:");

            move.LogInitialState();

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

        static void ResolveAttack(IEnumerable<PlayerMove> attacks)
        {
            var primaryAttack = attacks.First();

            var defendingPlayer = Game.Players[primaryAttack.Chamber];

            Log("{0} sneak into the {1}", attacks, defendingPlayer);

            var defendingMove = Game.Moves.FirstOrDefault(m => m.Player == defendingPlayer && m.IsDefendMove());
            AgentCard defendingAgent;
            if(defendingMove != null)
                defendingAgent = defendingMove.Agent;
            else
            {
                
                defendingAgent = defendingPlayer.Agents.LastOrDefault(a => a.GetIsUsed() == false);
            }

            if (defendingAgent == null)
            {
                Log("But {0} was empty", defendingPlayer);

                if(defendingMove != null)
                {
                    Log("because {0} is in {1}", defendingMove.Agent, defendingMove.Chamber);
                }
                return;
            }

            var attackingIntrigue = attacks.Sum(a => a.GetIntrigue());
            int defendingIntruge;
            if(defendingMove != null)
            {
                defendingIntruge = defendingMove.GetIntrigue();
                Log("and find {0}", defendingMove);
            }
            else
            {
                defendingIntruge = defendingAgent.Intrigue;
                Log("and find {0}", defendingAgent);
            }

            DieResult result;
            if (attackingIntrigue > defendingIntruge)
            {
                Log("The attackers have overwhelming advantage");

                // Attacker Choice
                result = Controls.ChooseOne(new[] { Dice.Roll(), Dice.Roll() }, primaryAttack.Player.IsHuman);
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

            Sleep();

            if (result == DieResult.KillTarget)
            {
                Log("The attackers succeed");

                Discard(defendingAgent);

                if (defendingMove != null)
                    Discard(defendingMove.FacedownCard);
            }
            else if (result == DieResult.EveryoneDies)
            {
                Log("Everyone manages to kill themselves");

                foreach (var attacker in attacks)
                {
                    Discard(attacker.Agent);
                    Discard(attacker.FacedownCard);
                }
                Discard(defendingAgent);
                if (defendingMove != null)
                    Discard(defendingMove.FacedownCard);
            }
            else if (result == DieResult.InspireFear)
            {
                Log("{0} survives but is frightened", defendingAgent);

                defendingAgent.Tap(TapReason.Frightened);
            }
            else if(result == DieResult.RaiseSuspicion)
            {
                Log("{0} fail and raise suspicion", attacks);

                foreach (var attack in attacks)
                {
                    attack.Agent.Tap(TapReason.Suspicious);
                }
            }
            else if (result == DieResult.DropItem)
            {
                foreach (var attacker in attacks)
                {
                    Discard(attacker.FacedownCard);
                }
            }
            else if(result == DieResult.GetHanged)
            {
                foreach (var attacker in attacks)
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
                if(player.Agents.Contains(card))
                {
                    player.Agents.Remove(card as AgentCard);
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
                if(player.Agents.Contains(card)
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
            if (LoggingEnabled == false)
                return;

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

        public static void Sleep()
        {
            System.Threading.Thread.Sleep(LogDelayMs);
        }

        void LogLaterThatNight()
        {
            Console.Write("Later that night");
            System.Threading.Thread.Sleep(LogDelayMs);
            for (int i = 0; i < 3; i++)
            {
                Console.Write(".");
                System.Threading.Thread.Sleep(LogDelayMs/3);
            }
            Console.WriteLine();
            Console.WriteLine();
        }
    }
}

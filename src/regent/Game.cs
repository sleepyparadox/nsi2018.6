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
                { Chamber.Gold, new Player(Chamber.Gold, false) },
                { Chamber.Silver, new Player(Chamber.Silver, false) },
                { Chamber.Your, new Player(Chamber.Your, true) },
            };

            Log.LoggingEnabled = true;
            Log.Line("You are playing as {0}", Players.Values.FirstOrDefault(p => p.IsHuman));
            foreach (var enemy in Players.Values.Where(p => p.IsHuman == false))
            {
                Log.Line("You must defeat {0}", enemy);
            }
            Log.Line();
        }

        public void Step()
        {
            Day++;
            Log.Line("--- Day {0} ---", Day);
            Log.Line();

            // Main step
            Moves.Clear();
            foreach(var player in Players.Values.Where(p => p.Active))
            {
                Log.Line("{0} has {1} agents", player, player.Agents.Count);
                Moves.Add(StepMainPhase(player));
                Log.Line();
                Log.Sleep();
            }

            Log.LaterThatNight();

            // Untap step
            var tappedAgents = Players.Values.SelectMany(p => p.Agents).Where(a => a.Tapped).ToList();
            if (tappedAgents.Any())
            {
                foreach (var agent in tappedAgents)
                {
                    agent.Untap();
                }
                Log.Line();
            }
             
            // Event step
            var events = Moves.ToList();
            for (int i = 0; i < events.Count;)
            {
                var @event = events[i];
                if(ResolveEvents(@event))
                {
                    Log.Line();
                    Log.Sleep();
                    // this card is done
                    events.Remove(@event);
                }
                else
                {
                    // next card
                    i++;
                }

            }

            // Combat step
            Combat.Resolve();

            // Was productive in court step
            if (Moves.Where(m => m.Chamber == Chamber.Court).Count() == 1)
            {
                var courtMove = Moves.FirstOrDefault(m => m.Chamber == Chamber.Court);
                Log.Line("{0} was productive at court", courtMove.Agent);
                courtMove.Player.DrawCard();
            }

            // Log about nothing
            foreach (var defendMove in Moves.Where(m => m.IsDefendMove()))
            {
                var chamberActvity = Moves.Count(m => m.Chamber == defendMove.Chamber);
                if (chamberActvity == 1) // alone
                {
                    Log.Line("Nothing happened in {0}", defendMove.Chamber);
                    Log.Line();
                }
            }

            // Game over handling
            if (Active == false)
            {
                Log.Line("--- Game over ---");

                if (Players.Values.Any(p => p.Active && p.IsHuman))
                {
                    Log.Line("You win");
                }
                else
                {
                    Log.Line("You lose");
                }
            }
        }

        static PlayerMove StepMainPhase(Player player)
        {
            var allChambers = Players.Keys.ToList();
            allChambers.Insert(0, Chamber.Court);
            var tappedChamber = new List<Chamber>() { player.Chamber };

            player.DrawCard();

            // Play agents from hand
            while (true)
            {
                var plot = "Plot";
                var recruit = string.Format("Recruit from hand ({0})", player.Hand.Where(c => c is AgentCard).Count());
                var inspect = "Inspect";
                
                var choices = new List<string>(){ plot, recruit, inspect};

                var choice = Controls.ChooseOne(choices, player.IsHuman);

                if (choice == plot)
                    break;

                else if(choice == recruit && player.Hand.Any(c => c is AgentCard))
                {
                    var recruitables = player.Hand.Where(c => c is AgentCard).ToList();
                    var recruited = Controls.ChooseOne(recruitables, player.IsHuman);
                    player.Hand.Remove(recruited);
                    player.Agents.Add(recruited as AgentCard);
                    Log.Line("{0} puts {1} into play", player, recruited);
                }
                else if (choice == inspect && player.IsHuman)
                {
                    var inspectHand = string.Format("Your hand ({0})", player.Hand.Count);
                    var inspectMoves = string.Format("Recent Activity ({0})", Moves.Count);

                    var inspectTargets = new List<object> { inspectHand, inspectMoves };
                    inspectTargets.AddRange(Players.Values);

                    var inspectChoice = Controls.ChooseOne(inspectTargets, true);

                    if(inspectChoice is string && (string)inspectChoice == inspectHand)
                    {
                        foreach (var card in player.Hand)
                        {
                            Log.Line("Your hand contains {0}", card);
                        }
                    }
                    else if (inspectChoice is string && (string)inspectChoice == inspectMoves)
                    {
                        foreach (var m in Moves)
                        {
                            m.LogInitialState();
                        }
                    }
                    else if(inspectChoice is Player)
                    {
                        var inspectPlayer = inspectChoice as Player;

                        if(inspectPlayer.IsHuman == false)
                            Log.Line("{0} has {1} cards in hand", inspectPlayer, player.Hand.Count);
                        var inspectChamber = inspectPlayer.Agents.Where(a => Moves.Any(m => m.Agent == a) == false).ToList();
                        Log.Line("In the chamber ({0}):", inspectChamber.Count);
                        foreach (var card in inspectChamber)
                        {
                            Log.Line("{0}", card);
                        }
                        Log.Line("Activity:");
                        var inspectMove = Moves.FirstOrDefault(m => m.Player == inspectPlayer);
                        if(inspectMove != null)
                        {
                            inspectMove.LogInitialState();
                        }
                        else
                        {
                            Log.Line("(none)");
                        }
                    }
                }
            }
      
            // Plot
            var move = new PlayerMove();
            move.Player = player;
            move.Agent = Controls.ChooseOne(player.Agents, player.IsHuman, "Choose an agent:");
            move.FacedownCard = Controls.ChooseOne(player.Hand, player.IsHuman, "Choose an item:");
            move.Chamber = Controls.ChooseOne(move.Agent.Tapped ? tappedChamber : allChambers, player.IsHuman, "Select a chamber to infiltrate:");

            Log.Line();
            move.LogInitialState();

            return move;
        }

        public static bool ResolveEvents(PlayerMove move)
        {
            return false;
        }

        public static void Discard(ICard card, bool silent = false)
        {
            if (GetOwner(card) == null)
            {
                // already discarded
                return;
            }

            Log.Line("{0} discarded", card);

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
    }
}

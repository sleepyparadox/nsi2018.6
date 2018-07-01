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
                { Chamber.Green, new Player(Chamber.Green, false) },
                { Chamber.Yellow, new Player(Chamber.Yellow, false) },
                { Chamber.Blue, new Player(Chamber.Blue, true) },
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
            var attackMoves = Moves.Where(m => m.IsAttackMove()).ToList();
            while(attackMoves.Any())
            {
                var firstAttack = attackMoves.First();
                var allAttacks = attackMoves.Where(m => m.Chamber == firstAttack.Chamber).ToList();
                foreach (var attack in allAttacks)
                    attackMoves.Remove(attack);

                ResolveAttack(allAttacks);
                Log.Line();
                Log.Sleep();
            }

            // Didn't do anything step
            foreach(var defendMove in Moves.Where(m => m.IsDefendMove()))
            {
                var chamberActvity = Moves.Count(m => m.Chamber == defendMove.Chamber);
                if(chamberActvity == 1) // alone
                {
                    Log.Line("Nothing happened in {0}", defendMove.Chamber);
                    Log.Line();
                }
            }

            // Game over handling
            if (Active == false)
            {
                Log.Line("Game over");

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
                    Log.Line("{0} puts {1} into play", recruited, player);
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
            if (move.Chamber == Chamber.Court)
            {
                Log.Line("{0} trades in {1}", move.Agent, move.FacedownCard);
                Discard(move.FacedownCard);
                move.Player.DrawCard();
                move.Player.DrawCard();
                return true;
            }

            return false;
        }

        static void ResolveAttack(IEnumerable<PlayerMove> attacks)
        {
            var primaryAttack = attacks.First();

            var defendingPlayer = Game.Players[primaryAttack.Chamber];

            foreach (var attack in attacks)
            {
                Log.Line("{0} with {1} is sneaking into the {2}", attack.Agent, attack.FacedownCard, defendingPlayer);
            }

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
                Log.Line("But {0} was empty", defendingPlayer);

                if(defendingMove != null)
                {
                    Log.Line("because {0} is in {1}", defendingMove.Agent, defendingMove.Chamber);
                }
                return;
            }

            var attackingIntrigue = attacks.Sum(a => a.GetIntrigue());
            int defendingIntruge;
            if(defendingMove != null)
            {
                defendingIntruge = defendingMove.GetIntrigue();
                Log.Line("{0} is waiting and ready", defendingMove);
            }
            else
            {
                defendingIntruge = defendingAgent.Intrigue;
                Log.Line("{0} is suprised", defendingAgent);
            }

            DieResult result;
            if (attackingIntrigue > defendingIntruge)
            {
                Log.Line("The attackers have overwhelming advantage");

                // Attacker Choice
                result = Controls.ChooseOne(new[] { Dice.Roll(), Dice.Roll() }, primaryAttack.Player.IsHuman);
            }
            else if (defendingIntruge > attackingIntrigue)
            {
                Log.Line("The attackers are under prepared");
              
                // Defender Choice
                result = Controls.ChooseOne(new[] { Dice.Roll(), Dice.Roll() }, defendingPlayer.IsHuman);
            }
            else
            {
                Log.Line("It is a tense encounter");
               
                // Tied
                result = Dice.Roll();
            }

            Log.Sleep();

            if (result == DieResult.Target_dies)
            {
                Log.Line("The attack succeeds");

                Discard(defendingAgent);

                if (defendingMove != null)
                    Discard(defendingMove.FacedownCard);
            }
            else if (result == DieResult.Everyone_dies)
            {
                Log.Line("Everyone manages to kill themselves in confusion");

                foreach (var attacker in attacks)
                {
                    Discard(attacker.Agent);
                    Discard(attacker.FacedownCard);
                }
                Discard(defendingAgent);
                if (defendingMove != null)
                    Discard(defendingMove.FacedownCard);
            }
            else if (result == DieResult.Inspires_fear)
            {
                Log.Line("The attack discovered but inspires fear", defendingAgent);

                defendingAgent.Tap(TapReason.Frightened);
            }
            else if(result == DieResult.Raises_suspicion)
            {
                Log.Line("The attack discovered and raises suspicion");
                foreach (var attack in attacks)
                {
                    attack.Agent.Tap(TapReason.Suspicious);
                }
            }
            else if (result == DieResult.Weapons_dropped)
            {
                Log.Line("The attack is discovered and weapons are dropped at scene");
                foreach (var attacker in attacks)
                {
                    Discard(attacker.FacedownCard);
                }
            }
            else if(result == DieResult.Hang_attackers)
            {
                Log.Line("The attack is discovered and the attackers are hanged");
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

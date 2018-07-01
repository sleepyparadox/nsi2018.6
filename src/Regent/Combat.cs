using Regent.Cards;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Regent
{
    public static class Combat
    {
        public static void Resolve()
        {
            var chambersToResolve = Game.Moves.Select(m => m.Chamber).Distinct();

            foreach (var chamber in chambersToResolve)
            {
                var attackingMoves = Game.Moves.Where(m => m.Chamber == chamber).ToList();

                // Find the defender
                PlayerMove defendingMove = attackingMoves.FirstOrDefault(m => m.Player.Chamber == chamber 
                                                                              || chamber == Chamber.Court);
                if (defendingMove != null)
                {
                    attackingMoves.Remove(defendingMove);
                }

                AgentCard defendingAgent = null;
                if(defendingMove != null)
                {
                    defendingAgent = defendingMove.Agent;
                }
                else if(chamber != Chamber.Court)
                {
                    defendingAgent = Game.Players[chamber].Agents.LastOrDefault(a => a.IsInChamber());
                }

                // Are there any attackers?
                if (attackingMoves.Count == 0)
                {
                    // no attackers
                    continue;
                }

                // Start the attack
                foreach (var attackMove in attackingMoves)
                {
                    Log.Line("{0} with {1} is sneaking into the {2}", attackMove.Agent, attackMove.FacedownCard, chamber);
                }

                // Run the numbers
                var attackingIntrigue = attackingMoves.Sum(a => a.GetIntrigue());
                int defendingIntruge;

                if (defendingMove != null)
                {
                    // prepared
                    defendingIntruge = defendingMove.GetIntrigue();
                    Log.Line("{0} is waiting and ready", defendingMove);
                }
                else if (defendingAgent != null)
                {
                    // suprised
                    defendingIntruge = defendingAgent.Intrigue;
                    Log.Line("{0} is suprised", defendingAgent);
                }
                else
                {
                    // empty
                    Log.Line("But {0} was empty", chamber);
                    Log.Line();
                    Log.Sleep();
                    continue;
                }

                // Result
                var attackerIsHuman = attackingMoves.First().Player.IsHuman;
                var defenderIsHuman = Game.GetOwner(defendingAgent).IsHuman;

                var result = GetResult(attackingIntrigue, defendingIntruge, attackerIsHuman, defenderIsHuman);

                if (result == DieResult.Target_dies)
                {
                    Log.Line("The attack succeeds");

                    Game.Discard(defendingAgent);

                    if (defendingMove != null)
                        Game.Discard(defendingMove.FacedownCard);
                }
                else if (result == DieResult.Everyone_dies)
                {
                    Log.Line("Everyone manages to kill themselves in confusion");

                    foreach (var attackMove in attackingMoves)
                    {
                        Game.Discard(attackMove.Agent);
                        Game.Discard(attackMove.FacedownCard);
                    }
                    Game.Discard(defendingAgent);
                    if (defendingMove != null)
                        Game.Discard(defendingMove.FacedownCard);
                }
                else if (result == DieResult.Inspires_fear)
                {
                    Log.Line("The attack discovered but inspires fear", defendingAgent);

                    defendingAgent.Tap(TapReason.Frightened);
                }
                else if (result == DieResult.Raises_suspicion)
                {
                    Log.Line("The attack discovered and raises suspicion");
                    foreach (var attackMove in attackingMoves)
                    {
                        attackMove.Agent.Tap(TapReason.Suspicious);
                    }
                }
                else if (result == DieResult.Weapons_dropped)
                {
                    Log.Line("The attack is discovered and weapons are dropped at scene");
                    foreach (var attackMove in attackingMoves)
                    {
                        Game.Discard(attackMove.FacedownCard);
                    }
                }
                else if (result == DieResult.Hang_attackers)
                {
                    Log.Line("The attack is discovered and the attackers are hanged");
                    foreach (var attackMove in attackingMoves)
                    {
                        Game.Discard(attackMove.Agent);
                        Game.Discard(attackMove.FacedownCard);
                    }
                }

                //// Request key press if player didn't do anything
                //var hadInput = (attackingIntrigue > defendingIntruge && attackerIsHuman)
                //                || (defendingIntruge > attackingIntrigue && defenderIsHuman);
                //if (hadInput == false)
                //{
                //    Controls.PressAnyKey();
                //}

                // finished
                Log.Line();
                Log.Sleep();
            }
        }

        static DieResult GetResult(int attackingIntrigue, int defendingIntruge, bool attackerIsHuman, bool defenderIsHuman)
        {
            DieResult result;
            if (attackingIntrigue > defendingIntruge)
            {
                Log.Line("The attackers have overwhelming advantage");

                // Attacker Choice
                result = Controls.ChooseOne(new[] { Dice.Roll(), Dice.Roll() }, attackerIsHuman);
            }
            else if (defendingIntruge > attackingIntrigue)
            {
                Log.Line("The attackers are under prepared");

                // Defender Choice
                result = Controls.ChooseOne(new[] { Dice.Roll(), Dice.Roll() }, attackerIsHuman);
            }
            else
            {
                Log.Line("It is a tense encounter");

                // Tied
                result = Dice.Roll();
            }
            return result;
        }
    }
}

# Regent
Regent is a text based adventure game
_Fight for dominance room by room in the castle. 
Elemimate the opposing agents while protecting your own.
Recruit allies and control the court._


Download
----------------
Latest Version: [Regent.2018.7.1.zip](https://github.com/sleepyparadox/nsi2018.6/files/2155361/Regent.2018.7.1.zip)

Screenshots
----------------
![unholy pie](https://raw.githubusercontent.com/sleepyparadox/nsi2018.6/master/img/unholy_pie.png)
![squid axe](https://raw.githubusercontent.com/sleepyparadox/nsi2018.6/master/img/squid_axe.png)

Boardgame
----------------
This window's console port of a boardgame prototype, which has Agent Cards, Weapon Cards and Event Cards (incomplete). where player is always last player to move
Each phase is resolved in order of players starting with the current regent and then each consective player to their left. This gives the current regent the most choice but the last player to move the most infomation. After each round the regent card moves left. Constantly changing the dynamic
_The regent has the most power, but is also the most closely watched_

**Main phase**
Performed by each player (starting with the current Regent)
* Draw a card
* (optional) Put agents from hand into play
* Put a card from hand facedown onto any of your agents in play
* Move agent with facedown card to any player's chamber or to the deck (court)

**Untap phase**
Untapp all frightened or suscpicous agents

**Event phase**
Reveal each facedown card in order they were played
Events apply when they are revealed
(Revealed agents and weapons are solved in next step)

**Combat phase**
Resolve each combat in order agents were moved
A combat will include all attackers in the chamber so each chamber only needs to be resolved once

**Court phase**
If an agent is alone in the court chamber, their owner draws a card

**Regent phase**
Return all revealed cards that wern't discard this round to their owner's hands
Return all agents to their chambers
Move the regent card to the player left of the current regent and begin the next mainphase

Combat
----------------
1. Select **single** the defender in the chamber
The defender is the first matching agent in order of priority: 
An agent with revealed weapon **from** this chamber (_a prepared defender_)
The leftmost agent **from** this chamber (_a suprised defender_)
The **First** agent to enter this chamber (_a ambused defender_)
2. Every other agent that has moved into this chamber with a weapon (or revealed card with intrigue score) is an attacker
3. Compare the total intrigue from the **defender** and their revealed card, against **all attackers** and all attacker revealed cards
4. The *first attacker* (who we are resolving) rolls dice and the strongest side picks a result (see table)

| Condition                     | Dice rolled | Choice                        |
| ----------------------------- |-------------| ------------------------------|
| Attackers are twice as strong | 3           | First attacker chooses result |
| Attackers are stronger        | 2           | First attacker chooses result |
| Tie                           | 1           |                               |
| Defender is stronger          | 2           | Defender chooses result       |
| Defender is twice as strong   | 3           | Defender chooses result       |

5. Handling the die results

| Result | Effect        | Actions                                                            |
| ------ | ------------- | ------------------------------------------------------------------ |
| 6      | Success       | Discard Defender and their revealed cards                          |
| 5      | Everyone dies | Discard the defender, all attackers and their revealed cards       |
| 4      | Frightened    | Tap the defender, they cannot leave their chamber next round       |
| 3      | Suspicous     | Tap all the attackers, they cannot leave their chambers next round |
| 2      | Dropped Item  | Discard all attacker's revealed cards                              |
| 1      | Hanged        | Discard all attackers and their revealed cards                     |


Meta Notes
----------------
**The Court and Empty Chambers**
The court can be treachous and even hiding in empty chambers can pose a danger
Because the first agent in an empty chamber is the defender if another agent follows them

**Team up**
Assisting another agent's attack can guarantee advantage, but the can you really trust them to pick the best result?
using Regent.Cards;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Regent
{
    public static class Log
    {
        public static bool LoggingEnabled { get; set; }
        public static int LogDelayMs = 1000;
        public static void Line()
        {
            Line("");
        }

        public static void Line(object arg)
        {
            Line("{0}", arg);
        }

        public static void Line(string msg, params object[] args)
        {
            if (LoggingEnabled == false)
                return;

            for (int c = 0; c < msg.Length; c++)
            {
                var cha = msg[c];

                if (c < msg.Length - 2
                    && msg[c] == '{'
                    && msg[c + 2] == '}')
                {
                    var objIndex = int.Parse(msg[c + 1].ToString());
                    var objValue = args[objIndex];
                    if (objValue != null)
                    {
                        Console.ForegroundColor = GetColor(objValue);

                        if(objValue is Chamber && ((Chamber)objValue) != Chamber.Court)
                            Console.Write(string.Format("{0} Chamber", objValue));
                        else
                            Console.Write(objValue.ToString());
                    }
                    c += 2;
                    continue;
                }

                Console.ForegroundColor = ConsoleColor.White;
                Console.Write(msg[c]);
            }

            Console.WriteLine();
        }

        static ConsoleColor GetColor(object obj)
        {
            if (obj == null)
            {
            }
            else if (obj.GetType() == typeof(Chamber))
            {
                return ChamberColors[(Chamber)obj];
            }
            else if (obj is Player)
            {
                return GetColor((obj as Player).Chamber);
            }
            else if (obj is PlayerMove)
            {
                return GetColor((obj as PlayerMove).Agent);
            }
            else if (obj is ICard)
            {
                var owner = Game.GetOwner(obj as ICard);
                if(owner != null )
                {
                    return GetColor(owner.Chamber);
                }
            }

            return ConsoleColor.White;
        }

        public static void Sleep()
        {
            System.Threading.Thread.Sleep(LogDelayMs);
        }

        public static void LaterThatNight()
        {
            Console.ForegroundColor = ConsoleColor.White;
            Console.Write("Later that night");
            System.Threading.Thread.Sleep(Log.LogDelayMs);
            for (int i = 0; i < 3; i++)
            {
                Console.Write(".");
                System.Threading.Thread.Sleep(Log.LogDelayMs / 3);
            }
            Console.WriteLine();
            Console.WriteLine();
        }

        static Dictionary<Chamber, ConsoleColor> ChamberColors = new Dictionary<Chamber, ConsoleColor>()
        {
            { Chamber.Court, ConsoleColor.White },
            { Chamber.Red, ConsoleColor.Red },
            { Chamber.Blue, ConsoleColor.Blue },
            { Chamber.Green, ConsoleColor.Green },
            { Chamber.Yellow, ConsoleColor.Yellow },
        };
    }
}

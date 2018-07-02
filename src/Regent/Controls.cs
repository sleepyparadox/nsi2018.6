using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Regent
{
    public static class Controls
    {
        const string ClearLine = "                                                                                                      ";
        public static DieResult ChooseOne(IEnumerable<DieResult> items, bool isHuman, string message = "Choose one:")
        {
            if(isHuman)
            {
                return ChooseOne<DieResult>(items, isHuman, message);
            }

            var min = items.Min();
            return min;
        }

        public static T ChooseOne<T>(IEnumerable<T> items, bool isHuman, string message = "Choose one:")
        {
            var itemArray = items.ToArray();
            if(isHuman == false)
            {
                return itemArray[Game.Rand.NextInt(0, itemArray.Length)];
            }

            var bindings = new Dictionary<ConsoleKey, T>();
            for (int i = 0; i < itemArray.Length && i < Keys.Count; i++)
            {
                bindings.Add(Keys[i], itemArray[i]);
            }

            Console.ForegroundColor = ConsoleColor.White;

            Console.WriteLine();
            Console.WriteLine(message);
            foreach (var pair in bindings)
            {
                Log.Line("[{0}] {1}", pair.Key, pair.Value);
            }

            ConsoleKeyInfo key;
            do
            {
                key = Console.ReadKey();

                // clear last character
                Console.CursorLeft -= 1;
                Console.Write(' ');
                Console.CursorLeft -= 1;
            }
            while (bindings.ContainsKey(key.Key) == false);

            // clear
            Console.CursorTop -= bindings.Count + 2;
            for (int i = 0; i < bindings.Count + 2; i++)
            {
                Console.WriteLine(ClearLine); //clear
            }

            // write final choice
            Console.CursorTop -= bindings.Count + 2;
            Console.ForegroundColor = ConsoleColor.Gray;
            Console.WriteLine(">[{0}] {1}", key.Key, bindings[key.Key]);

            return bindings[key.Key];
        }

        public static void PressAnyKey()
        {
            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine("Press any key");
            Console.ReadKey();

            // clear
            Console.CursorTop -= 1;
            Console.CursorLeft -= 1;
            Console.WriteLine(ClearLine); //clear
            Console.CursorTop -= 1;
        }

        static List<ConsoleKey> Keys = new List<ConsoleKey>()
        {
            ConsoleKey.Z,
            ConsoleKey.X,
            ConsoleKey.C,
            ConsoleKey.V,
            ConsoleKey.B,
            ConsoleKey.N,
            ConsoleKey.M,
            ConsoleKey.A,
            ConsoleKey.S,
            ConsoleKey.D,
            ConsoleKey.F,
            ConsoleKey.G,
            ConsoleKey.H,
            ConsoleKey.J,
            ConsoleKey.K,
            ConsoleKey.L,
        };
    }
}

using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Regent
{
    public class Grammars
    {
        const string WhiteSpace = " ,\t\n";

        Dictionary<string, Grammar> _definitions;
        Random _rand;

        public Grammars(string file)
        {
            _definitions = new Dictionary<string, Grammar>();
            var lines = File.ReadAllLines(file);

            Grammar currentGrammar = null;
            foreach (var line in lines)
            {
                if (string.IsNullOrWhiteSpace(line))
                    continue;

                if(line.Contains('{') && line.Contains('}'))
                {
                    currentGrammar = JsonConvert.DeserializeObject<Grammar>(line);
                    currentGrammar.Items = new List<string>();

                    _definitions.Add(currentGrammar._id, currentGrammar);
                }
                else
                {
                    // cleanup smart characters
                    var formattedLine = line.Replace("\\n", "\n");

                    currentGrammar.Items.Add(formattedLine);
                }
            }

            _rand = new Random();
        }

        public string Write(string key = "story")
        {
            var builder = new StringBuilder(1024);
            WriteRecursive(builder, key);
            return builder.ToString();
        }

        public void WriteRecursive(StringBuilder builder, string key)
        {
            Grammar[] grammars;
            if(key.Contains("|"))
            {
                grammars = key.Split('|')
                    .Select(k => _definitions[k])
                    .ToArray();
            }
            else
            {
                grammars = new Grammar[] { _definitions[key] };
            }

            var grammar = grammars[_rand.Next(grammars.Length)];
            var choice = grammar.GetItem(_rand);

            for (int i = 0; i < choice.Length; i++)
            {
                var cha = choice[i];
                if(cha == '#')
                {
                    var endOfKey = GetEndOfKey(choice, i);
                    var nextKey = choice.Substring(i + 1, (endOfKey - i) - 1);

                    WriteRecursive(builder, nextKey);

                    // contiune from whitespace charater
                    i = (endOfKey - 1);
                }
                else
                {
                    builder.Append(cha);
                }
            }
        }

        int GetEndOfKey(string value, int startIndex)
        {
            var bestEnd = value.Length;

            for (int i = 0; i < WhiteSpace.Length; i++)
            {
                var endAt = value.IndexOf(WhiteSpace[i], startIndex + 1);
                if(endAt != -1 && endAt < bestEnd)
                {
                    bestEnd = endAt;
                }
            }

            return bestEnd;
        }

    }
}

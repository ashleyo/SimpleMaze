using System;
using System.Collections.Generic;
using System.Threading;
using static System.ConsoleColor;

namespace Version4
{

    // Need a delegate to define the type for a command
    public delegate void CommandDispatcher(string commandline);

    // This class dispatches commands
    static class CommandParser
    {
        // Allows multiple forms of a command to be accepted by
        // creating a dictionary mapping between legitimate string representations of directions
        // and the actual string used.
        // Accepted at this time [North] [north] [N] [n] => North 
        public static readonly Dictionary<string, string> DirCanonical = new Dictionary<string, string>();
        private static string[] names = Enum.GetNames(typeof(Dir));

        static CommandParser()
        {
            foreach (string name in names)
            {
                DirCanonical[name] = name;
                DirCanonical[name.ToLower()] = name;
                DirCanonical[name.Substring(0, 1).ToLower()] = name;
                DirCanonical[name.Substring(0, 1).ToUpper()] = name;
            }
            validCommands["whut"] = (string s) => Console.WriteLine($"I don't know how to {s}");
        }

        private static Dictionary<string,CommandDispatcher> validCommands 
                            = new Dictionary<string,CommandDispatcher>();

        public static void AddCommand(string text, CommandDispatcher dispatcher)
            => validCommands[text] = dispatcher;
        
        public static void ExecuteCommand(string commandline)
        {          
            string keyword = commandline.Split(' ')[0]; 
            if (validCommands.ContainsKey(keyword))
                validCommands[keyword](commandline);
            else validCommands["whut"](commandline); 
        }
        public static void GetNextCommand()
        {
            Util.Write(Green, "What next? ");
            ExecuteCommand(Console.ReadLine().ToLower()); //force input to lower case 
        }
    }
}


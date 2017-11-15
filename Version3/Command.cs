using System;
using System.Collections.Generic;
using System.Threading;

namespace Version3
{

    // Need a delegate to define the type for a command
    public delegate void CommandDispatcher(string commandline);

    // This class dispatches commands
    static class CommandParser
    {
        static CommandParser()
        {
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
            Console.Write("What next? ");
            ExecuteCommand(Console.ReadLine().ToLower()); //force input to lower case 
        }
    }
}


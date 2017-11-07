using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
            ExecuteCommand(Console.ReadLine());
        }
    }

    // This class implements commands (simple commands may be declared ad hoc using lambda 
    // notation, see the example in the static constructor of CommandParser
    // which provides a 'default' action)
    // TODO What would be cool -- can we think of a way of adding a 'generic' CommandAction for all direction in the Dir enum?
    public static class CommandActions
    {       
        public static void Look(string command="")
        {
            Console.WriteLine($"You are in {RoomSet.Instance.CurrentRoom.Description}");
        }
    }
}

/* Mini-spec for the 'generic command described above
 * It is probably best for the direction commands to come last 
 * 
 * Is this a specific command?
 * Is this a direction?
 * I don't recognize the command
 * 
 * This removes the need to 'rethrow' unimplemented directions - they are just unrecognized commands
 * and could drop through
 * Also allows specific commands to override directions - might be useful
 * Implies need to be able to remove commands //TODO ability to remove commands
 * 
 * So, it should check whether the command is in the Dir enum
 * if not bail - execute 'whut' command
 * 
 * If yes use the command as an indexer to update currentRoom
 */

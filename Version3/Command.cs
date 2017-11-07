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

    // This class implements commands (simple commands may be declared ad hoc using lambda 
    // notation, see the example in the static constructor of CommandParser
    // which provides a 'default' action)
    // TODO What would be cool -- can we think of a way of adding a 'generic' CommandAction for all direction in the Dir enum?
    class CommandActions
    {
        private Game G;

        public CommandActions(Game G)
        {
            this.G = G;

            //Add all direction to commands and point them at 'Move'
            foreach (string s in Enum.GetNames(typeof(Dir)))
            {
                CommandParser.AddCommand(s.ToLower(),this.Move);
            }
        }

        public void Look(string command = "")
        {
            Console.WriteLine($"You are in {G.PlayerOne.CurrentRoom.Description}");
            Console.WriteLine($"{G.PlayerOne.CurrentRoom.GetValidExitsAsString()}");
        }

        public void Quit(string command="")
        {
            Console.WriteLine("The roof falls in on your head. You die.");
            Thread.Sleep(2000);
            Environment.Exit(0);
        }

        public void Move(string command)
        {
            Room cr = G.PlayerOne.CurrentRoom;
            Dir d = (Dir)Enum.Parse(typeof(Dir), command, true);
            Console.WriteLine($"Going {d}");
            List<Dir> validExits = cr.GetValidExits();
            if (!validExits.Contains(d)) { Console.WriteLine("I can't go that way!"); return; }
            else {
                G.PlayerOne.CurrentRoom = cr[d];
               
                if (!G.PlayerOne.CurrentRoom.Equals(Exit.Instance))
                {
                    G.Turn++;
                    this.Look();
                }                                
                else this.GameOver();
            }
        }

        public void GameOver(string command = "")
        {
            Console.WriteLine($"{G.Turn}: Congratulations {G.PlayerOne.Name}! You have escaped" +
                $" the Maze of Doom and are assured fame, riches and a free coffee.");
            Console.WriteLine($"You escaped in {G.Turn} turns. Press any key to quit.");
            Console.ReadKey();
            Environment.Exit(0);
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

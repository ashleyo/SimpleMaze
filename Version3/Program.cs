using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Version3.Dir;
using System.Threading;

namespace Version3
{
    class Game
    {
        /// <summary>
        ///  Game Commands
        ///  Command need to be of the delegate type CommandDipatcher
        ///  void(string)
        ///  The full command line is passed in the string for commands 
        ///  that need to inspect it - single-word commands can generally 
        ///  ignore this parameter.
        ///  Simple commands can also be added on the fly using lambda 
        ///  functions, see the static constructor of CommandParser for
        ///  an example of this.
        /// </summary>

        public void InitializeCommandActions()
        {
            //Add all direction variants to commands and point them at 'Move'
            foreach (string S in DirCanonical.Keys)
            {
                CommandParser.AddCommand(S, this.Move);
            }

            CommandParser.AddCommand("look", Look);
            CommandParser.AddCommand("quit", Quit);
            CommandParser.AddCommand("help", (s) => Console.WriteLine("'look' will tell you what you can see\nenter a direction to move that way"));

        }

        public void Look(string command = "") =>
            Console.WriteLine($"You are in {PlayerOne.CurrentRoom.Description}\n" +
                $"{PlayerOne.CurrentRoom.GetValidExitsAsString()}");

        public void Quit(string command = "")
        {
            Console.WriteLine("The roof falls in on your head. You die.");
            Thread.Sleep(2000);
            Environment.Exit(0);
        }

        public void Move(string command)
        {
            command = command.Split(' ')[0];
            RoomSet.Room cr = this.PlayerOne.CurrentRoom;
            Dir d = (Dir)Enum.Parse(typeof(Dir), DirCanonical[command], true);
            List<Dir> validExits = cr.GetValidExits();
            if (!validExits.Contains(d)) { Console.WriteLine("I can't go that way!"); return; }
            else
            {
                Console.WriteLine($"Going {d}");
                this.PlayerOne.CurrentRoom = cr[d];

                if (!this.PlayerOne.CurrentRoom.Equals(RoomSet.Exit.Instance))
                {
                    this.Turn++;
                    this.Look();
                }
                else this.GameOver();
            }
        }

        public void GameOver(string command = "")
        {
            Console.WriteLine($"{Turn}: Congratulations {PlayerOne.Name}! You have escaped" +
                $" the Maze of Doom and are assured fame, riches and a free coffee.");
            Console.WriteLine($"You escaped in {Turn} turns. Press any key to quit.");
            Console.ReadKey();
            Environment.Exit(0);
        }

        // Allows multiple forms of a command to be accepted by
        // creating a dictionary mapping between legitimate string representations of directions
        // and the actual string used
        // Accepted at this time [North] [north] [N] [n] => North
        
        private static readonly Dictionary<string, string> DirCanonical = new Dictionary<string, string>();
        private static string[] names = Enum.GetNames(typeof(Dir));
        static Game()
        {
            foreach (string name in names)
            {
                DirCanonical[name] = name;
                DirCanonical[name.ToLower()] = name;
                DirCanonical[name.Substring(0, 1).ToLower()] = name;
                DirCanonical[name.Substring(0, 1).ToUpper()] = name;
            }
        }

        /// <summary>
        /// Properties, fields and non-command methods
        /// </summary>

        public int Turn { get; private set; } = 1;
        public Player PlayerOne { get; private set; }
        public readonly RoomSet maze = new RoomSet();
        public Game(Player P)
        {
            PlayerOne = P;
            this.RoomInitializor();
            this.InitializeCommandActions();
        }

        static void Main(string[] args)
        {
            Game G = new Game(new Player() { Name = "Ashley" });

            while (true)
            {
                Console.Write($"{G.Turn}: ");
                CommandParser.GetNextCommand();
            }
        }

        // Sets up rooms. Will need to be driven by a map file ultimately.
        public void RoomInitializor()
        {
            //Create a set of rooms
            RoomSet rooms = this.maze;
            rooms.AddMultipleRooms(new string[] {   "a room",
                                                    "another room",
                                                    "the third room",
                                                    "Room 101"
                                                });
            //initialize start point  
            this.PlayerOne.CurrentRoom = rooms[0];

            Util.Link(rooms[0], East, rooms[1], West);
            Util.Link(rooms[1], South, rooms[3], North);
            Util.Link(rooms[3], West, rooms[2], East);
            Util.Link(rooms[2], North, rooms[0], South);

            //Place exit 
            rooms[3][South] = RoomSet.Exit.Instance;
        }
    }

    //TODO - Think about this - if the first element were always 'None' then useful exits could be treated as a 1 based array rather than a 0 based array which might lead to cleaner code.
    enum Dir { North, East, South, West, Up, Down }

    static class Util
    {
        static public string DirHelpText()
        {
            StringBuilder s = new StringBuilder();
            foreach (string t in Enum.GetNames(typeof(Dir)))
                s.AppendFormat("{0} ", t);
            return s.ToString().TrimEnd();
        }

        static public void Link(RoomSet.Room r1, Dir d1, RoomSet.Room r2, Dir d2)
        {
            Link(r1, d1, r2);
            Link(r2, d2, r1);
        }

        static public void Link(RoomSet.Room r1, Dir d1, RoomSet.Room r2)
        {
            r1[d1] = r2;
        }
    }

    sealed class RoomSet : IEnumerable<RoomSet.Room>
    {
        public class Room
        {
            private Room[] exits = new Room[Enum.GetNames(typeof(Dir)).Length];

            public string Description { get; set; }

            // An indexer. This is how you make objects indexable, 
            // ie. how you can use O[n] syntax
            // In this case we want to be able to index a room by a direction to get another room 
            public Room this[Dir D]
            {
                get { return this.exits[(int)D]; }
                set { this.exits[(int)D] = value; }
            }

            public Room(string s)
            {
                Description = s;
            }

            public List<Dir> GetValidExits()
            {
                List<Dir> VE = new List<Dir>();
                foreach (Dir D in Enum.GetValues(typeof(Dir)))
                    if (this[D] != null) VE.Add(D);
                return VE;
            }

            public string GetValidExitsAsString()
            {
                StringBuilder s = new StringBuilder();
                foreach (Dir d in GetValidExits())
                {
                    s.Append($"{d} ");
                }
                return $"There are exits {s.ToString()}";
            }
        }

        /// <summary>
        /// Singleton class: a reference may be obtained as Exit.Instance
        /// Purpose is to ensure that there is only one exit and that it can be recognized.
        /// </summary>
        public class Exit : Room
        {
            private static readonly Exit instance = new Exit("The exit");

            private Exit(string s) : base(s) { }

            public static Exit Instance { get { return instance; } }
        }

        public RoomSet()
        {
            roomset[0] = Exit.Instance;
        }

        //Important - Exit should be allocated index 0
        private static int roomkey = 1;
        private Dictionary<int, Room> roomset = new Dictionary<int, Room>();

        public int Count { get { return roomset.Count; } }

        public int? GetKeyFromDescription(string desc) //returns null if not found, returns first match only
        {
            foreach (KeyValuePair<int, Room> KV in roomset)
                if (desc.Equals(KV.Value.Description)) return KV.Key;
            return null;
        }

        //Adding a single room returns the key in case you wish to stash it 
        //e.g. for linking rooms 
        public int AddRoom(Room R) { roomset[roomkey] = R; return roomkey++; }

        // Indexer to allow retrieving rooms by id, thin wrapper on the inner dictionary
        public Room this[int i]
        {
            get { return roomset[i]; }
        }

        //Note Values not Keys .... Iterate rooms by descriptions not keys
        public IEnumerator<Room> GetEnumerator()
        {
            return roomset.Values.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }

        //Allow multiple rooms to be added from an array of descriptions
        public void AddMultipleRooms(string[] descs)
        {
            foreach (string s in descs)
            {
                AddRoom(new Room(s));
            }

        }
    }



    class Player
    {
        public RoomSet.Room CurrentRoom { get; set; }
        public string Name { get; set; }
    }
}

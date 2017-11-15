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
    //TODO - Think about this - if the first element were always 'None' then useful exits could be treated as a 1 based array rather than a 0 based array which might lead to cleaner code.
    enum Dir { North, East, South, West, Up, Down }

    class Game
    {     
        /// <summary>
        /// Properties, fields and non-command methods
        /// </summary>

        public int Turn { get; private set; } = 1;
        public Player PlayerOne { get; private set; }
        public readonly RoomSet maze; 
        public Game(Player P)
        {
            PlayerOne = P;
            maze = Map.GetRoomSet(new Map());
            PlayerOne.CurrentRoom = maze[1]; //arbitrary, but starting in room 0 (the exit) is not supported
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
        ///  'Private' commands such as GameOver may be used: just ensure that they do not get added
        ///  to the CommandParser. This prevents the player from accessing them but still allows 
        ///  other commands to redirect to them.
        /// </summary>

        public void InitializeCommandActions()
        {
            //Add all direction variants to commands and point them at 'Move'
            foreach (string S in CommandParser.DirCanonical.Keys)
            {
                CommandParser.AddCommand(S, this.Move);
            }

            CommandParser.AddCommand("look", Look);
            CommandParser.AddCommand("quit", Quit);
            CommandParser.AddCommand("help", (s) => Console.WriteLine("'look' will tell you what you can see\nenter a direction to move that way"));

        }

        #region Commands

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
            Dir d = (Dir)Enum.Parse(typeof(Dir), CommandParser.DirCanonical[command], true);
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

        #endregion
    }

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

        static public void Link (RoomSet RS, int index1, Dir d1, int index2, Dir d2 )
        {
            Link(RS[index1], d1, RS[index2], d2);
        }
    }

    class RoomSet : IEnumerable<RoomSet.Room>
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

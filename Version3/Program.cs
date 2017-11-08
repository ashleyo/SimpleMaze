using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Version3.Dir;

namespace Version3
{
    class Game
    {
        public CommandActions CA { get; set; }
        public int Turn { get; set; } = 1;
        public Player PlayerOne { get; private set; }
        public readonly RoomSet maze = RoomSet.Instance;
        public Game(Player P) {
            PlayerOne = P;
            CA = new CommandActions(this);
            this.RoomInitializor();
            this.CommandInitializor();
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

            Util.BiLink(rooms[0], East, rooms[1], West);
            Util.BiLink(rooms[1], South, rooms[3], North);
            Util.BiLink(rooms[3], West, rooms[2], East);
            Util.BiLink(rooms[2], North, rooms[0], South);

            //Place exit 
            rooms[3][South] = Exit.Instance;
        }

        public void CommandInitializor()
        {
            CommandParser.AddCommand("look", CA.Look);
            CommandParser.AddCommand("quit", CA.Quit);
            CommandParser.AddCommand("help", (s) => Console.WriteLine("'look' will tell you what you can see\nenter a direction to move that way"));
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

        static public void BiLink(Room r1, Dir d1, Room r2, Dir d2)
        {
            r1[d1] = r2; r2[d2] = r1;
        }

        
    }

    class Room
    {
        private Room[] exits = new Room[Enum.GetNames(typeof(Dir)).Length];

        public string Description { get; set; }

        // An indexer. This is how you make objects indexable, 
        // ie. how you can use O[n] syntax
        public Room this[Dir D]
        {
            get { return this.exits[(int)D]; }
            set { this.exits[(int)D] = value; }
        }

        public Room()
        {
            RoomSet.Instance.AddRoom(this);
        }

        public Room(string s) : this()
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
    /// Singleton class: the only instance of it is accessible as RoomSet.Instance
    /// </summary>
    sealed class RoomSet : IEnumerable<Room>
    {
        private static readonly RoomSet instance = new RoomSet();
        private List<Room> roomset = new List<Room>();
        //public Room CurrentRoom { get; set; }
        public int Count { get { return roomset.Count; } }
        public void AddRoom(Room R) => roomset.Add(R);
        private RoomSet() { }

        public static RoomSet Instance { get { return instance; } }

        public Room this[int i]
        {
            get { return roomset[i]; }
        }

        public IEnumerator<Room> GetEnumerator()
        {
            return roomset.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }

        public void AddMultipleRooms(string[] descs)
        {
            foreach (string s in descs)
                new Room(s);
        }
    }

    /// <summary>
    /// Singleton class: a reference may be obtained as Exit.Instance
    /// Purpose is to ensure that there is only one exit and that it can be recognized.
    /// </summary>
    /// <remarks>
    /// Created automatically and added to RoomSet like any other Room - this means RoomSet.Instance.Count 
    /// is always at least one.
    /// </remarks>
    sealed class Exit : Room
    {
        private static readonly Exit instance = new Exit() { Description = "The exit" };

        private Exit() { }

        public static Exit Instance { get { return instance; } }
    }

    //move currentRoom into player - player should know where he is!
    class Player
    {
        public Room CurrentRoom { get; set; }
        public string Name { get; set; }
    }
}

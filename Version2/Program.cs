using System;
using System.Collections; 
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Version2.Dir; //means that you can refer to 'East' 
                           //rather than 'Dir.East'

namespace Version2
{
    class Program
    {
        
        //updated to use all the new toys
        //gotos rejected in favour of while+break
        static void Main(string[] args)
        {
            //TODO CurrentRoom should be a property of Player to allow for multi-player expansion

            Game currentGame = new Game(new Player() { Name = "Ashley" });

            RoomSet rooms = currentGame.maze;
            rooms.AddMultipleRooms(new string[] {   "a room",
                                                    "another room",
                                                    "the third room",
                                                    "Room 101"
                                                });
            //initialize start point  
            rooms.CurrentRoom = rooms[0];

            Util.BiLink(rooms[0], East, rooms[1], West);
            Util.BiLink(rooms[1], South, rooms[3], North);
            Util.BiLink(rooms[3], West, rooms[2], East);
            Util.BiLink(rooms[2], North, rooms[0], South);

            //Place exit 
            rooms[3][South] = Exit.Instance;

            //game loop might consist of

            while (true)
            {
                Console.WriteLine($"You are in {rooms.CurrentRoom.Description}");

                List<Dir> validExits = rooms.CurrentRoom.GetValidExits();

                Console.WriteLine("Valid exits for current room include ..");
                for (int i = 1; i <= validExits.Count; i++) Console.Write($"{i}) {validExits[i - 1]} ");
                Console.Write("\nPick One!  ");

                // No input validation here - but note that this is not how it will
                // be done in reality, user will type command words that will get converted to 
                // directions
                Dir ChosenDir = validExits[int.Parse(Console.ReadLine()) - 1];
                rooms.CurrentRoom = rooms.CurrentRoom[ChosenDir];

                if (rooms.CurrentRoom.Equals(Exit.Instance)) { Console.WriteLine("woohoo!"); break; }
            }
            Console.Read();
        }

    }

    //TODO - Think about this - if the first element were always 'None' then useful exits could be treated as a 1 based array rather than a 0 based array which might lead to cleaner code.
    enum Dir { North, East, South, West, Up, Down }

    //Some useful utilities for the other code
    static class Util
    {
        //Generates a textual list of directions from the enum
        static public string DirHelpText()
        {
            StringBuilder s = new StringBuilder();
            foreach (string t in Enum.GetNames(typeof(Dir)))
                s.AppendFormat("{0} ", t);
            return s.ToString().TrimEnd();
        }

        //Saves some typing by setting up reciprocal links in one operation
        static public void BiLink(Room r1, Dir d1, Room r2, Dir d2)
        {
            r1[d1] = r2; r2[d2] = r1;
        }
    }

    //Adds a GetValidExit method to return a list of exits that are connected
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
    }

    /// <summary>
    /// Singleton class: the only instance of it is accessible as RoomSet.Instance
    /// </summary>
    //Added an 'Add Multiple' method to make initial setup less tedious
    //Refactored and tidied code a bit
    //Note: you can use 'fat arrow' notation with void methods too, see line !!
    //Added an indexer which allows roomset[n] to return the nth room
    //Useful because if they have the same description their list index
    //is the only way to tell them apart!
    //TODO: Consider whether Dictionary<key, Room> would offer advantages over List<Room>
    sealed class RoomSet : IEnumerable<Room>
    {
        private static readonly RoomSet instance = new RoomSet();
        private List<Room> roomset = new List<Room>();
        public Room CurrentRoom { get; set; }
        public int Count { get { return roomset.Count; } }
        public void AddRoom(Room R) => roomset.Add(R);  // !!
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

    //Neither of these classes yet in (very) serious use
    class Player
    {
        public string Name { get; set; }
    }

    class Game
    {
        public Player PlayerOne { get; private set; }
        public readonly RoomSet maze = RoomSet.Instance;
        public Game(Player P) { PlayerOne = P; }
    }
}


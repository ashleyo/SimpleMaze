using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static EnumsAsIndexerTest.Dir;

namespace EnumsAsIndexerTest
{
    class Program
    {
        static void Main(string[] args)
        {
            RoomSet rooms = RoomSet.Instance;
            Room R1 = new Room() { Description = "a room" };
            Room R2 = new Room() { Description = "another room" };
            Room R3 = new Room() { Description = "the third room" };

            //initialize start point
            rooms.CurrentRoom = R1;

            //can create links like this 
            R1[East] = R2;
            R1[South] = R3;
            R2[West] = R1;
            R3[Down] = R3;


            //Can identify exit like this
            R2[East] = Exit.Instance;

            //Can get a set of directions like this
            //foreach (string s in Enum.GetNames(typeof(Dir))) Console.WriteLine(s);

            //Can find valid exits for a given room (eg. R2)
            //Console.Write("Valid exits for R2 include ..");
            //foreach (Dir D in Enum.GetValues(typeof(Dir)))
                //if (R2[D] != null) Console.Write($" {D}");
            //Console.WriteLine();

        //So a simple game loop might consist of

        Step1:
            // 1. Get current room
            // 1a. Display description
            Console.WriteLine($"You are in {rooms.CurrentRoom.Description}");
            
            // 2. Get a list of valid exits         
            List<Dir> validExits = new List<Dir>();        
            foreach (Dir D in Enum.GetValues(typeof(Dir)))
                if (rooms.CurrentRoom[D] != null) validExits.Add(D);
            
            // 3. Offer as 'pick one' to user
            Console.WriteLine("Valid exits for current room include ..");
            for (int i = 1; i <= validExits.Count; i++) Console.Write($"{i}) {validExits[i-1]} ");
            Console.Write("\nPick One!  ");

            // 4. Use input to update current room
            Dir ChosenDir = validExits[int.Parse(Console.ReadLine()) - 1];
            rooms.CurrentRoom = rooms.CurrentRoom[ChosenDir];
            
            // 5. Check if current room is exit, if yes woohoo
            if (rooms.CurrentRoom.Equals(Exit.Instance)) { Console.WriteLine("woohoo!"); goto end; }
            
            // 6. Goto 1
            goto Step1;

            
            // can iterate over rooms like this (with IEnumerator support in RoomSet)
            //foreach (Room R in rooms) Console.WriteLine("{0}", R.Description);
            end:
            Console.Read();
        }

    }
    enum Dir { North, East, South, West, Up, Down }

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
    }

    /// <summary>
    /// Singleton class: the only instance of it is accessible as RoomSet.Instance
    /// </summary>
    sealed class RoomSet : IEnumerable<Room>
    {
        private static readonly RoomSet instance = new RoomSet();
        private List<Room> roomset = new List<Room>();
        public Room CurrentRoom { get; set; }
        public int Count {  get { return roomset.Count;} }
        public void AddRoom(Room R) { roomset.Add(R); }
        private RoomSet() { }

        public static RoomSet Instance { get { return instance; } }

        public IEnumerator<Room> GetEnumerator()
        {
            return roomset.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
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
    sealed class Exit:Room
    {
        private static readonly Exit instance = new Exit() { Description = "The exit" };

        private Exit() { }

        public static Exit Instance { get { return instance; } }
    }

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

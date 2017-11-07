﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Version3.Dir;

namespace Version3
{
    class Program
    {
        static void Main(string[] args)
        {
            //Create a set of rooms
            RoomSet rooms = RoomSet.Instance;
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

            //test Command thingies ...

            CommandParser.AddCommand("look", CommandActions.Look);
            CommandParser.AddCommand("help", (s) => Console.WriteLine("help not implemented yet"));
            
            //test parser
            while (true)
                CommandParser.GetNextCommand();

            //while (true)
            //{
            //    Console.WriteLine($"You are in {rooms.CurrentRoom.Description}");

            //    List<Dir> validExits = rooms.CurrentRoom.GetValidExits();

            //    Console.WriteLine("Valid exits for current room include ..");
            //    for (int i = 1; i <= validExits.Count; i++) Console.Write($"{i}) {validExits[i - 1]} ");
            //    Console.Write("\nPick One!  ");

            //    // No input validation here - but note that this is not how it will
            //    // be done in reality, user will type command words that will get converted to 
            //    // directions
            //    Dir ChosenDir = validExits[int.Parse(Console.ReadLine()) - 1];
            //    rooms.CurrentRoom = rooms.CurrentRoom[ChosenDir];

            //    if (rooms.CurrentRoom.Equals(Exit.Instance)) { Console.WriteLine("woohoo!"); break; }
            //}
            Console.Read();
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
    }

    /// <summary>
    /// Singleton class: the only instance of it is accessible as RoomSet.Instance
    /// </summary>
    sealed class RoomSet : IEnumerable<Room>
    {
        private static readonly RoomSet instance = new RoomSet();
        private List<Room> roomset = new List<Room>();
        public Room CurrentRoom { get; set; }
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
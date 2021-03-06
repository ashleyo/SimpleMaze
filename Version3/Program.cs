﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using static Version4.Dir;
using System.Threading;
using static System.ConsoleColor;
using Microsoft.Data.Sqlite.Internal;

namespace Version4
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
        public string Name { get; private set; }
        public Game(Player P)
        {
            PlayerOne = P;
            Map M = new Map();
            Name = M.MapName;
            maze = Map.GetRoomSet(M);
            PlayerOne.CurrentRoom = maze[1]; //arbitrary, but starting in room 0 (the exit) is not supported
            InitializeCommandActions();
            Splash();
        }

        static void Main(string[] args)
        {
            //dbtest.Test();

            //Experimental objects for testing, need to be loaded as part of Map eventually
            OwnedItem I = new OwnedItem(G.PlayerOne.CurrentRoom, "A sparkling egg", 5, "a bejewelled Egg, the name 'Faberge' is engraved on the base.");
            ItemBase.AddNewItem(I);
            I = new OwnedItem(G.PlayerOne.CurrentRoom, "A rotten egg", 1, "smells of sulphur ...");
            ItemBase.AddNewItem(I);
            ////////////////////////////////////////////////////////////////////////////////

            while (true)
            {
                Util.Write(Green, $"{G.Turn}: ");
                CommandParser.GetNextCommand();
            }

        }

        private void Splash()
        {
            Util.Write(Green,$@"
            Welcome, {PlayerOne.Name}, to the {this.Name}.
            Good luck with it!

            (c) A P Oliver 2017
        
        ");
            Console.WriteLine();

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

        private void InitializeCommandActions()
        {
            //Add all direction variants to commands and point them at 'Move'
            foreach (string S in CommandParser.DirCanonical.Keys)
            {
                CommandParser.AddCommand(S, this.Move);
            }

            CommandParser.AddCommand("help", (s) => Console.WriteLine($@"
    'look' or 'inspect' will tell you what you can see
    'inv' will tell you what you have
    you can 'take' or 'drop' items, use enough of their name to be unique
    enter a direction to move that way, potential directions are {Util.DirHelpText()}
    'quit' to /ragequit
    Find the exit in the fewest possible turns.

    "));

            CommandParser.AddCommand("look", Look);
            CommandParser.AddCommand("quit", Quit);
            CommandParser.AddCommand("take", Take);
            CommandParser.AddCommand("inv", Inv);
            CommandParser.AddCommand("drop", Drop);
            CommandParser.AddCommand("inspect", Inspect);
        }

        #region Commands

        //these can be private and probably should be to avoid cluttering Game's public namespace too much

        private void Look(string command = "")
        {
            Console.WriteLine($"You are in {PlayerOne.CurrentRoom.Description}\n" +
                $"{PlayerOne.CurrentRoom.GetValidExitsAsString()}");
            List<Item> itemshere = PlayerOne.CurrentRoom.GetInventory;
            if (itemshere.Count > 0)
            {
                Console.WriteLine("You see here:");
                Console.Write(ItemBase.Formatter(itemshere));
            }
        }

        private void Quit(string command = "")
        {
            Console.WriteLine("The roof falls in on your head. You die.");
            Thread.Sleep(2000);
            Environment.Exit(0);
        }

        private void Move(string command)
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

        private void GameOver(string command = "")
        {
            Util.Write(Yellow,$"{Turn}: Congratulations {PlayerOne.Name}!\nYou have escaped" +
                $" the {this.Name} and are assured fame, riches and a free coffee.\n");
            Util.Write(Yellow, $"You escaped in {Turn} turns. Press any key to quit.\n");
            Console.ReadKey();
            Environment.Exit(0);
        }
        
        private void Take(string command) {
            string request = ExtractDataFromCommand(command);
            List<Item> present = ItemBase.FindAllMatchingItems(PlayerOne.CurrentRoom, request);

            // If we have 0 or more than 1 handle the 'error'
            if (present.Count !=1 )
            {
                if (present.Count == 0) Console.WriteLine($"I don't see that.");
                else Console.WriteLine($"{request}? Can you be more specific?");
                return;
            }

            //If we have precisely 1 do the transfer
            OwnedItem taken = present[0] as OwnedItem;
            Console.WriteLine($"{taken.Name} is taken");
            taken.TransferTo(PlayerOne);
        }

        private void Drop(string command)
        {
            string request = ExtractDataFromCommand(command);
            List<Item> playerhas = ItemBase.FindAllMatchingItems(PlayerOne, request);

            // If we have 0 or more than 1 handle the 'error'
            if (playerhas.Count != 1)
            {
                if (playerhas.Count == 0) Console.WriteLine($"I don't have that.");
                else Console.WriteLine($"{request}? Can you be more specific?");
                return;
            }

            //If we have precisely 1 do the transfer
            OwnedItem dropped = playerhas[0] as OwnedItem;
            Console.WriteLine($"{dropped.Name} was dropped");
            dropped.TransferTo(PlayerOne.CurrentRoom);
        }

        private void Inv(string command="")
        {
            Console.WriteLine("You have:");
            Console.Write(ItemBase.Formatter(ItemBase.GetItemsByOwner(PlayerOne)));
        }

        private void Inspect(string command)
        {
            string request = ExtractDataFromCommand(command);
            List<Item> itemshere = ItemBase.FindAllMatchingItems(PlayerOne.CurrentRoom, request);
            if (itemshere.Count > 0)
            {
                Console.WriteLine("You see here:");
                Console.Write(ItemBase.Formatter(itemshere, true));
            }

        }

        // Utility to get e.g. 'rotten egg' from 'drop rotten egg'
        private string ExtractDataFromCommand(string command) => Regex.Match(command, @"(\w*) (\w.*)").Groups[2].Value;       

        #endregion
    }

    static class Util
    {
        static public void Write(ConsoleColor C, string toWrite)
        {
            ConsoleColor old = Console.ForegroundColor;
            Console.ForegroundColor = C;
            Console.Write(toWrite);
            Console.ForegroundColor = old;
        }

        static public void WriteLine(ConsoleColor C, string toWrite)
        {
            ConsoleColor old = Console.ForegroundColor;
            Console.ForegroundColor = C;
            Console.WriteLine(toWrite);
            Console.ForegroundColor = old;
        }

        static public string DirHelpText()
        {
            StringBuilder s = new StringBuilder();
            foreach (string t in Enum.GetNames(typeof(Dir)))
                s.AppendFormat("{0} ", t);
            return s.ToString().TrimEnd();
        }       

        static public IEnumerable<string> GetWords(string phrase)
        {
            Regex regex = new Regex(@"\b[\s,\.\-:;]*");
            var words = regex.Split(phrase).Where(x => !string.IsNullOrEmpty(x));
            return words;
        }
    }

    class RoomSet : IEnumerable<RoomSet.Room>
    {
        public class Room : ObjectOwner
        {
            //public enum ExitStatus { Open, Closed}

            //public struct RoomExit
            //{
            //    public Room ExitTo { get; set; }
            //    public ExitStatus status { get; set; }
            //    public string Description { get; set; }
           // }

            private Room[] exits = new Room[Enum.GetNames(typeof(Dir)).Length];
            //private RoomExit[] exits = new RoomExit[Enum.GetNames(typeof(Dir)).Length];

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

        //Utility methods

        //Allow multiple rooms to be added from an array of descriptions
        public void AddMultipleRooms(string[] descs)
        {
            foreach (string s in descs)
            {
                AddRoom(new Room(s));
            }

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

        static public void Link(RoomSet RS, int index1, Dir d1, int index2, Dir d2)
        {
            Link(RS[index1], d1, RS[index2], d2);
        }

    }

    class Player : ObjectOwner
    {
        public RoomSet.Room CurrentRoom { get; set; }
        public string Name { get; set; }
    }

    //slight overkill at the moment, could just use object references to identify owners 
    //but Guids might be useful if we ever want to persist inventories 
    abstract class ObjectOwner
    {
        private Guid id = Guid.NewGuid();
        public Guid Id => id;
        public List<Item> GetInventory => ItemBase.GetItemsByOwner(this);
        // and if we ever do want to restore players/rooms etc from a save file 
        // we'll need a constructor that allows us to set Id back to a given value
        public ObjectOwner() { }
        public ObjectOwner(Guid newguid) { id = newguid; } 
    }
}

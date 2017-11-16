using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Version4.Dir;

namespace Version4
{
    // A Map object needs to define 
    // i) a name 
    // ii) an array/list of room description 
    // iii) A set list of links 
    // iv) An exit
    class Map
    {
        public Map(string name, string[] descriptions, BiLink[] links, ExitDef exit)
        {
            MapName = name;
            Descriptions = descriptions;
            Links = links;
            ExitLocation = exit;
        }

        //Parameterless constructor only intended for testing
        public Map() : this (
            "Use for testing only",
            new string[] {
            //1
            "A room",
            //2
            "another room",
            //3
            "a third room",
            //4
            "Room 101"
            },
            new BiLink[] {
                new BiLink(1,East,2,West),
                new BiLink(2,South,4,North),
                new BiLink(4,West,3,East),
                new BiLink(3,North,1,South)
            },
            new ExitDef(4, South))
        { }
         
        public string MapName { get; private set; }

        //Meta -- rooms will be allocated indices starting with '1' in
        //the order that they appear here - the same indices should be
        //used for defining links
        public string[] Descriptions { get; set; }      

        public struct BiLink
        {
            public int index1;
            public Dir d1;
            public int index2;
            public Dir d2;
            public BiLink(int index1, Dir d1, int index2, Dir d2)
            {
                this.index1 = index1;
                this.d1 = d1;
                this.index2 = index2;
                this.d2 = d2;
            }
        }
        public struct ExitDef
        {
            public int RoomIndex;
            public Dir Direction;
            public ExitDef(int i, Dir d) { RoomIndex = i; Direction = d; }
        }

        public BiLink[] Links { get; set; }

        public ExitDef ExitLocation { get; set; }

        public static RoomSet GetRoomSet(Map M)
        {
            RoomSet rooms = new RoomSet();

            rooms.AddMultipleRooms(M.Descriptions);
            foreach (BiLink BL in M.Links)
                Util.Link(rooms,BL.index1, BL.d1, BL.index2, BL.d2);
              
            //set exit and yield
            rooms[M.ExitLocation.RoomIndex][M.ExitLocation.Direction] 
                = RoomSet.Exit.Instance;
            return rooms;
        }
    }
}

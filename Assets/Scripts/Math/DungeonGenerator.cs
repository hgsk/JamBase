using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

public static class DungeonGenerator
{
    public enum TileType
    {
        Wall,
        Floor,
        Door
    }

    public static class SimpleRandomWalk
    {
        public static TileType[,] Generate(int width, int height, int steps)
        {
            var map = new TileType[width, height];
            for (int innerX = 0; innerX < width; innerX++)
                for (int innerY = 0; innerY < height; innerY++)
                    map[innerX, innerY] = TileType.Wall;

            int x = width / 2;
            int y = height / 2;
            map[x, y] = TileType.Floor;

            Random rand = new Random();
            for (int i = 0; i < steps; i++)
            {
                int direction = rand.Next(4);
                switch (direction)
                {
                    case 0: if (x > 0) x--; break;
                    case 1: if (x < width - 1) x++; break;
                    case 2: if (y > 0) y--; break;
                    case 3: if (y < height - 1) y++; break;
                }
                map[x, y] = TileType.Floor;
            }

            return map;
        }
    }

    public static class RoomAndCorridors
    {
        private class Room
        {
            public int X, Y, Width, Height;
            public Room(int x, int y, int width, int height)
            {
                X = x; Y = y; Width = width; Height = height;
            }
        }

        public static TileType[,] Generate(int width, int height, int roomCount, int minRoomSize, int maxRoomSize)
        {
            var map = new TileType[width, height];
            for (int x = 0; x < width; x++)
                for (int y = 0; y < height; y++)
                    map[x, y] = TileType.Wall;

            var rooms = new List<Room>();
            var random = new Random();

            // Generate rooms
            for (int i = 0; i < roomCount; i++)
            {
                int roomWidth = random.Next(minRoomSize, maxRoomSize + 1);
                int roomHeight = random.Next(minRoomSize, maxRoomSize + 1);
                int roomX = random.Next(1, width - roomWidth - 1);
                int roomY = random.Next(1, height - roomHeight - 1);

                var newRoom = new Room(roomX, roomY, roomWidth, roomHeight);

                bool intersects = rooms.Any(room =>
                    newRoom.X < room.X + room.Width && newRoom.X + newRoom.Width > room.X &&
                    newRoom.Y < room.Y + room.Height && newRoom.Y + newRoom.Height > room.Y);

                if (!intersects)
                {
                    rooms.Add(newRoom);
                    CarveRoom(map, newRoom);
                }
            }

            // Connect rooms
            for (int i = 1; i < rooms.Count; i++)
            {
                ConnectRooms(map, rooms[i - 1], rooms[i]);
            }

            return map;
        }

        private static void CarveRoom(TileType[,] map, Room room)
        {
            for (int x = room.X; x < room.X + room.Width; x++)
                for (int y = room.Y; y < room.Y + room.Height; y++)
                    map[x, y] = TileType.Floor;
        }

        private static void ConnectRooms(TileType[,] map, Room room1, Room room2)
        {
            int x1 = room1.X + room1.Width / 2;
            int y1 = room1.Y + room1.Height / 2;
            int x2 = room2.X + room2.Width / 2;
            int y2 = room2.Y + room2.Height / 2;

            while (x1 != x2 || y1 != y2)
            {
                if (x1 != x2)
                {
                    x1 += x1 < x2 ? 1 : -1;
                }
                else if (y1 != y2)
                {
                    y1 += y1 < y2 ? 1 : -1;
                }
                map[x1, y1] = TileType.Floor;
            }
        }
    }

    public static class CellularAutomata
    {
        public static TileType[,] Generate(int width, int height, float fillProbability, int iterations)
        {
            var map = new TileType[width, height];
            var random = new Random();

            // Initialize map randomly
            for (int x = 0; x < width; x++)
                for (int y = 0; y < height; y++)
                    map[x, y] = random.NextDouble() < fillProbability ? TileType.Wall : TileType.Floor;

            // Run cellular automata iterations
            for (int i = 0; i < iterations; i++)
            {
                var newMap = new TileType[width, height];
                for (int x = 0; x < width; x++)
                {
                    for (int y = 0; y < height; y++)
                    {
                        int wallCount = CountNeighborWalls(map, x, y);
                        newMap[x, y] = wallCount > 4 ? TileType.Wall : TileType.Floor;
                    }
                }
                map = newMap;
            }

            return map;
        }

        private static int CountNeighborWalls(TileType[,] map, int x, int y)
        {
            int count = 0;
            for (int i = -1; i <= 1; i++)
                for (int j = -1; j <= 1; j++)
                    if (x + i >= 0 && x + i < map.GetLength(0) && y + j >= 0 && y + j < map.GetLength(1))
                        count += map[x + i, y + j] == TileType.Wall ? 1 : 0;
            return count;
        }
    }

    public static string VisualizeMap(TileType[,] map)
    {
        var sb = new StringBuilder();
        for (int y = 0; y < map.GetLength(1); y++)
        {
            for (int x = 0; x < map.GetLength(0); x++)
            {
                switch (map[x, y])
                {
                    case TileType.Wall: sb.Append("█"); break;
                    case TileType.Floor: sb.Append("."); break;
                    case TileType.Door: sb.Append("+"); break;
                }
            }
            sb.AppendLine();
        }
        return sb.ToString();
    }
}

public class DungeonGeneratorExample
{
    public void DemonstrateDungeonGeneration()
    {
        // Simple Random Walk
        var randomWalkMap = DungeonGenerator.SimpleRandomWalk.Generate(40, 20, 200);
        Console.WriteLine("Random Walk Dungeon:");
        Console.WriteLine(DungeonGenerator.VisualizeMap(randomWalkMap));

        // Rooms and Corridors
        var roomsAndCorridorsMap = DungeonGenerator.RoomAndCorridors.Generate(50, 30, 10, 3, 8);
        Console.WriteLine("\nRooms and Corridors Dungeon:");
        Console.WriteLine(DungeonGenerator.VisualizeMap(roomsAndCorridorsMap));

        // Cellular Automata
        var cellularAutomataMap = DungeonGenerator.CellularAutomata.Generate(40, 20, 0.45f, 4);
        Console.WriteLine("\nCellular Automata Dungeon:");
        Console.WriteLine(DungeonGenerator.VisualizeMap(cellularAutomataMap));
    }
}

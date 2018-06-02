using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

public class MapGenerator : MonoBehaviour
{

    public GameObject player;
    public Transform collectable;
    //public bool playerAreSetted = false;

    public int width;
    public int height;

    public string seed;
    public bool useRandomSeed;

    [Range(0, 100)]
    public int randomFillPercent;

    int[,] map;

    void Start()
    {
        GenerateMap();
    }

    void Update()
    {
        /*
        if (Input.GetMouseButtonDown(0))
        {
            GenerateMap();
        }
        */
    }

    public void GenerateMap()
    {
        map = new int[width, height];
        RandomFillMap();

        CreateCentralRoom();       
    
        for (int i = 0; i < 5; i++)
        {
            SmoothMap();
        }        

        ProcessMap();

        int borderSize = 40;
        int[,] borderedMap = new int[width + borderSize * 2, height + borderSize * 2];

        for (int x = 0; x < borderedMap.GetLength(0); x++)
        {
            for (int y = 0; y < borderedMap.GetLength(1); y++)
            {
                if (x >= borderSize && x < width + borderSize && y >= borderSize && y < height + borderSize)
                {
                    borderedMap[x, y] = map[x - borderSize, y - borderSize];
                }
                else
                {
                    borderedMap[x, y] = 1;
                }
            }
        }

       

        MeshGenerator meshGen = GetComponent<MeshGenerator>();
        meshGen.GenerateMesh(borderedMap, 3);
       
    }

    void ProcessMap()
    {
        List<List<Coord>> wallRegions = GetRegions(1);
        int wallThresholdSize = 150;        

        foreach (List<Coord> wallRegion in wallRegions)
        {
            if (wallRegion.Count < wallThresholdSize)
            {
                foreach (Coord tile in wallRegion)
                {
                    map[tile.tileX, tile.tileY] = 0;
                }
            }
        }

        List<List<Coord>> roomRegions = GetRegions(0);
        int roomThresholdSize = 50;
        List<Room> survivingRooms = new List<Room>();

        foreach (List<Coord> roomRegion in roomRegions)
        {
            if (roomRegion.Count < roomThresholdSize)
            {
                foreach (Coord tile in roomRegion)
                {
                    map[tile.tileX, tile.tileY] = 1;
                }
            }
            else
            {
                survivingRooms.Add(new Room(roomRegion, map));
            }
        }
        survivingRooms.Sort();
        survivingRooms[0].isMainRoom = true;
        survivingRooms[0].isAccessableFromMainRoom = true;


        ConnectClosestRooms(survivingRooms);

        player.transform.position = new Vector3(0, 0, -1);        
        
       // collectable.transform.position = survivingRooms[survivingRooms.Count - 1].GetRoomCoords(survivingRooms[survivingRooms.Count - 1].tiles, map);
    }

    void ConnectClosestRooms(List<Room> allRooms, bool forceAccessabilityFromMainRoom = false)
    {
        List<Room> roomListA = new List<Room>();
        List<Room> roomListB = new List<Room>();

        if (forceAccessabilityFromMainRoom)
        {
            foreach (Room room in allRooms)
            {
                if (room.isAccessableFromMainRoom)
                {
                    roomListB.Add(room);
                }
                else
                {
                    roomListA.Add(room);
                }
            }
        }
        else
        {
            roomListA = allRooms;
            roomListB = allRooms;
        }

        int bestDistance = 0;
        Coord bestTileA = new Coord();
        Coord bestTileB = new Coord();
        Room bestRoomA = new Room();
        Room bestRoomB = new Room();
        bool possibleConnectionFound = false;

        foreach (Room roomA in roomListA)
        {
            if (!forceAccessabilityFromMainRoom)
            {
                possibleConnectionFound = false;
                if (roomA.connectedRooms.Count >0)
                {
                    continue;
                }
            }            

            foreach (Room roomB in roomListB)
            {
                if(roomA == roomB||roomA.IsConnected(roomB))
                {
                    continue;
                }
                

                for (int tileIndexA = 0; tileIndexA < roomA.edgeTiles.Count; tileIndexA++)
                {
                    for (int tileIndexB = 0; tileIndexB < roomB.edgeTiles.Count; tileIndexB++)
                    {
                        Coord tileA = roomA.edgeTiles[tileIndexA];
                        Coord tileB = roomB.edgeTiles[tileIndexB];
                        int distanceBetweenRooms = (int)(Mathf.Pow(tileA.tileX - tileB.tileX, 2) + Mathf.Pow(tileA.tileY - tileB.tileY, 2));

                        if (distanceBetweenRooms < bestDistance || !possibleConnectionFound)
                        {
                            bestDistance = distanceBetweenRooms;
                            possibleConnectionFound = true;
                            bestTileA = tileA;
                            bestTileB = tileB;
                            bestRoomA = roomA;
                            bestRoomB = roomB;
                        }
                    }
                }
            }

            if (possibleConnectionFound&&!forceAccessabilityFromMainRoom)
            {
                CreatePassage(bestRoomA, bestRoomB, bestTileA, bestTileB);
            }
        }

        if (possibleConnectionFound && forceAccessabilityFromMainRoom)
        {
            CreatePassage(bestRoomA, bestRoomB, bestTileA, bestTileB);
            ConnectClosestRooms(allRooms, true);
        }

        if (!forceAccessabilityFromMainRoom)
        {
            ConnectClosestRooms(allRooms, true);
        }
    }

    void CreatePassage(Room roomA, Room roomB,Coord tileA, Coord tileB)
    {
        Room.ConnectRooms(roomA, roomB);
        //Debug.DrawLine(CoordToWorldPoint(tileA), CoordToWorldPoint(tileB), Color.green, 100);
        
        List<Coord> line = GetLine(tileA, tileB);
        foreach (Coord c in line)
        {
            DrawCircle(c, 5);
        }
    }

    void DrawCircle(Coord c, int r)
    {
        for (int x = -r;x <=r; x++)
        {
            for (int y = -r; y <= r; y++)
            {
                if (x * x + y * y <= r * r)
                {
                    int drawX = c.tileX + x;
                    int drawY = c.tileY + y;
                    if (IsInMapRange(drawX,drawY))
                    {
                        map[drawX, drawY] = 0;
                    }
                }
            }
        }
    }

    List<Coord> GetLine(Coord from, Coord to)
    {
        List<Coord> line = new List<Coord>();
        int x = from.tileX;
        int y = from.tileY;

        int dx = to.tileX - from.tileX;
        int dy = to.tileY - from.tileY;

        bool inverted = false;
        int step = Math.Sign (dx);
        int gradientStep = Math.Sign(dy);

        int longest = Mathf.Abs(dx);
        int shortest = Mathf.Abs(dy);

        if (longest<shortest)
        {
            inverted = true;
            longest = Mathf.Abs(dy);
            shortest = Mathf.Abs(dx);

            step = Math.Sign(dy);
            gradientStep = Math.Sign(dx);

        }

        int gradientAccumulation = longest / 2;
        for (int i = 0; i < longest; i++)
        {
            line.Add(new Coord(x, y));

            if (inverted)
            {
                y += step;
            }
            else
            {
                x += step;
            }

            gradientAccumulation += shortest;
            if (gradientAccumulation>=longest)
            {
                if (inverted)
                {
                    x += gradientStep;
                }
                else
                {
                    y += gradientStep;
                }
                gradientAccumulation -= longest;
            }
        }

        return line;
    }

    Vector3 CoordToWorldPoint(Coord tile)
    {
        return new Vector3(-width / 2 + .5f + tile.tileX, -height / 2 + .5f + tile.tileY, -1);
    }

    List<List<Coord>> GetRegions(int tileType)
    {
        List<List<Coord>> regions = new List<List<Coord>>();
        int[,] mapFlags = new int[width, height];

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                if (mapFlags[x, y] == 0 && map[x, y] == tileType)
                {
                    List<Coord> newRegion = GetRegionTiles(x, y);
                    regions.Add(newRegion);

                    foreach (Coord tile in newRegion)
                    {
                        mapFlags[tile.tileX, tile.tileY] = 1;
                    }
                }
            }
        }

        return regions;
    }

    List<Coord> GetRegionTiles(int startX, int startY)
    {
        List<Coord> tiles = new List<Coord>();
        int[,] mapFlags = new int[width, height];
        int tileType = map[startX, startY];

        Queue<Coord> queue = new Queue<Coord>();
        queue.Enqueue(new Coord(startX, startY));
        mapFlags[startX, startY] = 1;

        while (queue.Count > 0)
        {
            Coord tile = queue.Dequeue();
            tiles.Add(tile);

            for (int x = tile.tileX - 1; x <= tile.tileX + 1; x++)
            {
                for (int y = tile.tileY - 1; y <= tile.tileY + 1; y++)
                {
                    if (IsInMapRange(x, y) && (y == tile.tileY || x == tile.tileX))
                    {
                        if (mapFlags[x, y] == 0 && map[x, y] == tileType)
                        {
                            mapFlags[x, y] = 1;
                            queue.Enqueue(new Coord(x, y));
                        }
                    }
                }
            }
        }

        return tiles;
    }

    bool IsInMapRange(int x, int y)
    {
        return x >= 0 && x < width && y >= 0 && y < height;
    }


    void RandomFillMap()
    {
        if (useRandomSeed)
        {
            seed = Time.time.ToString();
        }

        System.Random pseudoRandom = new System.Random(seed.GetHashCode());

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                if (x == 0 || x == width - 1 || y == 0 || y == height - 1)
                {
                    map[x, y] = 1;
                }
                else
                {
                    map[x, y] = (pseudoRandom.Next(0, 100) < randomFillPercent) ? 1 : 0;
                }
            }
        }
    }

    void RandomlySpawnCollectables(int count)
    {
        int placeX = 0, placeY = 0;
        System.Random pseudoRandom = new System.Random(seed.GetHashCode());
        for (int i = 0; i < count; i++)
        {
            placeX = pseudoRandom.Next(0, width);
            placeY = pseudoRandom.Next(0, height);
        }        
        Instantiate(collectable, new Vector3(-width / 2 + .5f + placeX, -height / 2 + .5f + placeY, -1), Quaternion.identity);        
    }

    void CreateCentralRoom()
    {
        for (int x = width / 2 - 5; x < width / 2 + 5; x++)
        {
            for (int y = height / 2 - 5; y < height / 2 + 5; y++)
            {
                map[x, y] = 0;
            }
        }
    }

    void SmoothMap()
    {
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                int neighbourWallTiles = GetSurroundingWallCount(x, y);

                if (neighbourWallTiles > 4)
                    map[x, y] = 1;
                else if (neighbourWallTiles < 4)
                    map[x, y] = 0;

            }
        }
    }

    int GetSurroundingWallCount(int gridX, int gridY)
    {
        int wallCount = 0;
        for (int neighbourX = gridX - 1; neighbourX <= gridX + 1; neighbourX++)
        {
            for (int neighbourY = gridY - 1; neighbourY <= gridY + 1; neighbourY++)
            {
                if (IsInMapRange(neighbourX, neighbourY))
                {
                    if (neighbourX != gridX || neighbourY != gridY)
                    {
                        wallCount += map[neighbourX, neighbourY];
                    }
                }
                else
                {
                    wallCount++;
                }
            }
        }

        return wallCount;
    }

    struct Coord
    {
        public int tileX;
        public int tileY;

        public Coord(int x, int y)
        {
            tileX = x;
            tileY = y;
        }
    }

    /*Vector3 GetRoomCoords(List<Coord> roomTiles, int[,] map)
    {
        int outX, outY;
        int niceTileX = 0, niceTileY = 0;
        int maxTileX = Int32.MinValue, maxTileY = Int32.MinValue;
        int minTileX = Int32.MaxValue, minTileY = Int32.MaxValue;
        // Coord idealTile;
        int sqSize = 3;
        foreach (Coord tile in roomTiles)
        {
            if (map[tile.tileX, tile.tileY] == 0)
            {
                maxTileX = (tile.tileX > maxTileX) ? tile.tileX : maxTileX;
                maxTileY = (tile.tileY > maxTileY) ? tile.tileY : maxTileY;

                minTileX = (tile.tileX < minTileX) ? tile.tileX : minTileX;
                minTileY = (tile.tileY < minTileY) ? tile.tileY : minTileY;

                // niceTileX = (maxTileX - minTileX) / 2;
                // niceTileY = (maxTileY - minTileY) / 2;
                // idealTile = tile;

               

            }
        }

        int midTileX = (maxTileX - minTileX) / 2;
        int midTileY = (maxTileY - minTileY) / 2;

        int circleOfSearch = 3;
        
       // for (int x = midTileX- circleOfSearch; x < midTileX+ circleOfSearch; x++)
       // {
       //     for (int y = midTileY- circleOfSearch; y < midTileY+ circleOfSearch; y++)
       //     {
       //         if (map[x, y] == 0 && map[x - 3, y - 3] == 0 && map[x + 3, y + 3] == 0 && map[x + 3, y - 3] == 0 && map[x + 3, y - 3] == 0
       //             && map[x - 2, y - 2] == 0 && map[x + 2, y + 2] == 0 && map[x + 2, y - 2] == 0 && map[x + 2, y - 2] == 0)
       //         {
       //             niceTileX = x;
       //             niceTileY = y;
       //            
       //         }
       //     }
       // }
        
        
        for (int x = minTileX + 3; x <= maxTileX - 3; x++)
        {
            for (int y = minTileY + 3; y <= maxTileY - 3; y++)
            {
                if (map[x, y] == 0 && map[x - 3, y - 3] == 0 && map[x + 3, y + 3] == 0 && map[x + 3, y - 3] == 0 && map[x + 3, y - 3] == 0
                    && map[x - 2, y - 2] == 0 && map[x + 2, y + 2] == 0 && map[x + 2, y - 2] == 0 && map[x + 2, y - 2] == 0)
                {
                    niceTileX = x;
                    niceTileY = y;
                     //Debug.DrawLine(new Vector3(midTileX - map.GetLength(0) / 2, midTileY- map.GetLength(1) / 2, -1), 
                     //   new Vector3(niceTileX- map.GetLength(0) / 2, niceTileY - map.GetLength(1) / 2, -1),Color.green, 20);
                }
            }
        }
       
        outX = niceTileX;
        outY = niceTileY;


        return new Vector3(outX+midTileX - map.GetLength(0)/2, outY+midTileY - map.GetLength(1)/2, -1);
        // return new Vector3(-(maxTileX-minTileX) / 2 + .5f + niceTileX, 2, -(maxTileY-minTileY) / 2 + .5f + niceTileY);
    }*/

    class Room : IComparable<Room> {

        public List<Coord> tiles;
        public List<Coord> edgeTiles;
        public List<Room> connectedRooms;
        public int roomSize;
        public bool isAccessableFromMainRoom;
        public bool isMainRoom;

        public Room()
        {

        }

        public Room(List<Coord> roomTiles, int[,] map)
        {
            tiles = roomTiles;
            roomSize = tiles.Count;
            connectedRooms = new List<Room>();

            edgeTiles = new List<Coord>();
            foreach (Coord tile in tiles)
            {
                for (int x = tile.tileX-1; x <= tile.tileX+1; x++)
                {
                    for (int y = tile.tileY-1; y <= tile.tileY+1; y++)
                    {
                        if(x== tile.tileX || y == tile.tileY)
                        {
                            if(map[x,y] == 1)
                            {
                                edgeTiles.Add(tile);
                            }
                        }
                    }
                }
            }
        }

        

        public void SetAccesableFromMainRoom()
        {
            if (!isAccessableFromMainRoom)
            {
                isAccessableFromMainRoom = true;
                foreach (Room connectedRoom in connectedRooms)
                {
                    connectedRoom.SetAccesableFromMainRoom();
                }
            }
        }

        public static void ConnectRooms(Room roomA,Room roomB)
        {
            if (roomA.isAccessableFromMainRoom)
            {
                roomB.SetAccesableFromMainRoom();
            }
            else if (roomB.isAccessableFromMainRoom)
            {
                roomA.SetAccesableFromMainRoom();
            }
            roomA.connectedRooms.Add(roomB);
            roomB.connectedRooms.Add(roomA);
        }

        public bool IsConnected(Room otherRoom)
        {
            return connectedRooms.Contains(otherRoom);
        }

        public int CompareTo(Room otherRoom)
        {
            return otherRoom.roomSize.CompareTo(roomSize);
        }

    }

}
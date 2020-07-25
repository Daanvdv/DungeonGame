using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DungeonGeneration : MonoBehaviour
{
    [Header("Prefabs")]
    public GameObject startPointPrefab;
    public GameObject endPointPrefab;
    public GameObject roomPrefab;
    public GameObject hallwayPrefab;

    [Header("Dungeon Settings")]
    public Vector2Int dungeonSize;
    public Vector2Int minRoomSize;
    public int hallwayWidth;
    public int wallThickness;
    public bool generateDungeon = false;

    [Header("Dungeon Generation Options")]
    public int maxIterations;
    public int seed;
    public int roomOffsetSize;

    private List<Node> nodeList;
    private List<RoomNode> roomList;
    private List<GameObject> rooms;
    private List<GameObject> hallways;
    private List<GameObject> points;

    enum TileType
    {
        empty = 1024,
        wall,
    }

    enum CutType
    {
        x = 2048,
        y,
    }

    // Start is called before the first frame update
    void Start()
    {
        UnityEngine.Random.InitState(seed);
        GenerateDungeon(dungeonSize, maxIterations, minRoomSize);
        CreateRooms(minRoomSize);
        CreateHallways(hallwayWidth);
        PlacePoints(startPointPrefab, endPointPrefab);
        CheckPath();
    }

    //Crude generate dungeon,does not fully work yet
    private void OnValidate()
    {
        if (generateDungeon)
        {
            var children = this.transform.GetComponentInChildren<Transform>();
            foreach (Transform child in children)
            {
                DestroyImmediate(child);
            }

            UnityEngine.Random.InitState(seed);
            GenerateDungeon(dungeonSize, maxIterations, minRoomSize);
            CreateRooms(minRoomSize);
            CreateHallways(hallwayWidth);
            PlacePoints(startPointPrefab, endPointPrefab);
            CheckPath();
            generateDungeon = false;
        }
    }

    //I have looked at certain diffrent implementations and from what I have seen I decided on using Binary Space Partioning
    //as it gives the result that I want.
    //For implementation I decided to start the generation at 0,0 and then expand on the x and y
    /// <summary>
    /// Generate Binary Space Partioning grid for the base of dungeon generation.
    /// </summary>
    /// <param name="dungeonSize">Overall size to generate the dungeon in.</param>
    /// <param name="maxBSPIterations">Amount of times to iterate over dungeon generation algorithm.</param>
    /// <param name="minRoomSize">Minium room size to allow in the generation.</param>
    private void GenerateDungeon(Vector2Int dungeonSize, int maxBSPIterations, Vector2Int minRoomSize)
    {
        //Setup first node and node list
        nodeList = new List<Node>();
        roomList = new List<RoomNode>();
        RoomNode rootNode = new RoomNode(new Vector2Int(0, 0), dungeonSize, 0, null);
        nodeList.Add(rootNode);
        roomList.Add(rootNode);

        ///TODO: Make index increment only once the tree goes down another layer
        int index = 1;

        for (int i = 0; i < maxIterations; i++)
        {
            //Declare nodes and variables to be assigned
            RoomNode nodeOne;
            RoomNode nodeTwo;
            Vector2Int nodeTwoPos;
            Vector2Int nodeOneSize;
            Vector2Int nodeTwoSize;

            //Choose wether to split on x or y axis
            //Crude refactor
            CutType cutType;

            if (rootNode.Size.x > rootNode.Size.y)
            {
                cutType = CutType.x;
            }
            else if (rootNode.Size.x < rootNode.Size.y)
            {
                cutType = CutType.y;
            }
            else
            {
                cutType = (CutType)UnityEngine.Random.Range((int)CutType.x, (int)(CutType.y));
            }

            //TODO: Fix issues with higher iterations causing smaller than minium size rooms
            //Potential fix, if node size too small, skip current cut
            if (cutType == CutType.x)
            {
                //Choose a random postion between the current root nodes position and size to split and set the child nodes within
                int cutPosX = UnityEngine.Random.Range(minRoomSize.x, rootNode.Size.x - minRoomSize.x); /*RandomInt(minRoomSize.x, rootNode.Size.x - minRoomSize.x);*/

                //Calculate the cut and the size of the new room
                nodeOneSize = new Vector2Int(cutPosX, rootNode.Size.y);
                nodeTwoSize = new Vector2Int(rootNode.Size.x - cutPosX, rootNode.Size.y);
                nodeTwoPos = new Vector2Int(rootNode.Position.x + nodeOneSize.x, rootNode.Position.y);

                nodeOne = new RoomNode(rootNode.Position, nodeOneSize, index, rootNode);
                nodeTwo = new RoomNode(nodeTwoPos, nodeTwoSize, index, rootNode);
            }
            else
            {
                //Choose a random postion between the current root nodes position and size to split and set the child nodes within
                int cutPosY = UnityEngine.Random.Range(minRoomSize.y, rootNode.Size.y - minRoomSize.y); /*RandomInt(minRoomSize.y, rootNode.Size.y - minRoomSize.y);*/

                //Calculate the size and postion of the new rooms
                nodeOneSize = new Vector2Int(rootNode.Size.x, cutPosY);
                nodeTwoSize = new Vector2Int(rootNode.Size.x, rootNode.Size.y - cutPosY);
                nodeTwoPos = new Vector2Int(rootNode.Position.x, rootNode.Position.y + nodeOneSize.y);

                nodeOne = new RoomNode(rootNode.Position, nodeOneSize, index, rootNode);
                nodeTwo = new RoomNode(nodeTwoPos, nodeTwoSize, index, rootNode);
            }

            //Add new rooms as children to the root then move on to the next root node
            nodeList.Add(nodeOne);
            nodeList.Add(nodeTwo);
            rootNode.AddChild(nodeOne);
            rootNode.AddChild(nodeTwo);
            nodeOne.ParentNode = rootNode;
            nodeTwo.ParentNode = rootNode;
            roomList.Add(nodeOne);
            roomList.Add(nodeTwo);

            rootNode.Visited = true;
            roomList.Remove(rootNode);

            //Set new root node
            rootNode = roomList[0];
        }
    }

    /// <summary>
    /// Generate the rooms within the binary space partition grid.
    /// </summary>
    /// <param name="minimumSize">Minium size of rooms.</param>
    private void CreateRooms(Vector2Int minimumSize)
    {
        //Spawn rooms as planes
        rooms = new List<GameObject>();

        //TODO: Fix issues where rooms will be right up next to each other with bigger room sizes and smaller overall spaces
        //Last room with settings seed 1703 min hallway 4,4 dungeon size 30,30 iterations 15
        foreach (RoomNode roomNode in roomList)
        {
            //Carve room areas into rooms leaving small gaps between rooms
            Vector2Int pos = new Vector2Int(
                UnityEngine.Random.Range(roomNode.Position.x, roomNode.Position.x + roomNode.Size.x - minRoomSize.x - roomOffsetSize),
                UnityEngine.Random.Range(roomNode.Position.y, roomNode.Position.y + roomNode.Size.y - minRoomSize.x - roomOffsetSize));
            Vector2Int scale = new Vector2Int(
                UnityEngine.Random.Range(minRoomSize.x, roomNode.Size.x - (pos.x - roomNode.Position.x) - roomOffsetSize),
                UnityEngine.Random.Range(minimumSize.y, roomNode.Size.y - (pos.y - roomNode.Position.y) - roomOffsetSize));
            roomNode.Position = pos;
            roomNode.Size = scale;

            //Spawn in room floors
            GameObject room = Instantiate(roomPrefab,
                new Vector3(roomNode.Position.x, roomPrefab.transform.position.y, roomNode.Position.y),
                roomPrefab.transform.rotation,
                this.transform);
            room.transform.localScale = new Vector3(roomNode.Size.x, roomPrefab.transform.localScale.y, roomNode.Size.y);
            NodeInfo info = room.AddComponent<NodeInfo>();
            info.pos = roomNode.Position;
            info.size = roomNode.Size;
            rooms.Add(room);
        }
    }

    /// <summary>
    /// Generate hallways between rooms and other hallways.
    /// </summary>
    /// <param name="width">Width of hallways.</param>
    private void CreateHallways(int width)
    {
        hallways = new List<GameObject>();

        //TODO: Multiple iteartions of creating hallways
        RoomNode roomStart;
        RoomNode roomEnd;

        //TODO: Make more variation, maybe start with y at times then x
        //TODO: Make connections between all hallways
        //TODO: Fix issues where too many hallways are spwaned
        //TODO: Connect hallways together from the same parent to reduce the amount of overlap
        for (int i = 0; i < roomList.Count - 1; i++)
        {
            if (roomList[i].Visited)
            {
                continue;
            }
            for (int k = roomList.Count - 1; k != 0; k--)
            {
                if (roomList[k].Visited)
                {
                    continue;
                }
                roomStart = roomList[i];
                roomEnd = roomList[k];

                //Choose a random point in the rooms to generate from
                Vector2Int startLoc = new Vector2Int(
                    roomStart.Position.x + roomStart.Size.x,
                    UnityEngine.Random.Range(roomStart.Position.y, roomStart.Position.x + roomStart.Size.y));
                Vector2Int endLoc = new Vector2Int(
                    UnityEngine.Random.Range(roomEnd.Position.x, roomEnd.Position.x + roomEnd.Size.x),
                    roomEnd.Position.y);

                //Get start point and distance to travel in x and y
                Vector2Int roomDistance = endLoc - startLoc;
                roomDistance = new Vector2Int(roomDistance.x, roomDistance.y);
                int hallwayOneEnd = startLoc.x + roomDistance.x;

                //Create hallway sepertly through taxicab geometry (first go via x then y)
                //TODO: Add overlap reduction with room spaces
                GameObject hallwayOne = Instantiate(hallwayPrefab,
                    new Vector3(startLoc.x, hallwayPrefab.transform.position.y, startLoc.y),
                    hallwayPrefab.transform.rotation,
                    this.transform);
                hallwayOne.transform.localScale = new Vector3(roomDistance.x, hallwayPrefab.transform.localScale.y, width);
                HallwayInfo info = hallwayOne.AddComponent<HallwayInfo>();
                info.startPosition = startLoc;
                info.endPosition = new Vector2Int(hallwayOneEnd, startLoc.x + roomDistance.x);
                GameObject hallwayTwo = Instantiate(hallwayPrefab,
                    new Vector3(hallwayOneEnd, hallwayPrefab.transform.position.y, startLoc.y),
                    hallwayPrefab.transform.rotation,
                    this.transform);
                hallwayTwo.transform.localScale = new Vector3(width, hallwayPrefab.transform.localScale.y, roomDistance.y);
                info = hallwayTwo.AddComponent<HallwayInfo>();
                info.startPosition = new Vector2Int(hallwayOneEnd, startLoc.x + roomDistance.x);
                info.endPosition = endLoc;

                roomList[i].Visited = true;
                roomList[k].Visited = true;

                hallways.Add(hallwayOne);
                hallways.Add(hallwayTwo);
            }
        }
    }

    /// <summary>
    /// Place the start and end points for the player within the dungeon.
    /// </summary>
    /// <param name="startPrefab">Start point prefab.</param>
    /// <param name="endPrefab">End point prefab.</param>
    private void PlacePoints(GameObject startPrefab, GameObject endPrefab)
    {
        points = new List<GameObject>();

        RoomNode roomStart = roomList[0];
        RoomNode roomEnd = roomList[roomList.Count - 1];

        //Get a random spot from a room
        Vector2Int startPos = new Vector2Int(
            roomStart.Position.x,
            roomStart.Position.y);
        Vector2Int endPos = new Vector2Int(
            roomEnd.Position.x,
            roomEnd.Position.y);

        GameObject startPoint = Instantiate(startPrefab,
            new Vector3(startPos.x, startPrefab.transform.position.x, startPos.y),
            startPrefab.transform.rotation,
            this.transform);

        GameObject endPoint = Instantiate(endPrefab,
            new Vector3(endPos.x, endPrefab.transform.position.y, endPos.y),
            endPrefab.transform.rotation,
            this.transform);

        points.Add(startPoint);
        points.Add(endPoint);
    }

    //TODO: Add A* path finding to see if it is possible to complete the dungeon
    /// <summary>
    /// Check path betwene start and end point.
    /// </summary>
    private void CheckPath()
    {

    }
}
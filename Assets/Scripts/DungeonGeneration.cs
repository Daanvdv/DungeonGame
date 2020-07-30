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
    public int hallwayIteratioons;

    //Interal usuage
    private List<Node> nodeList;
    private List<RoomNode> roomList;
    private List<Hallway> hallwayList;
    private List<GameObject> rooms;
    private List<GameObject> hallways;
    private List<GameObject> points;

    //Used for BSP cutting
    enum CutType
    {
        x = 2048,
        y,
    }

    // Start is called before the first frame update
    void Start()
    {
        //I decided to make each part into diffrent sections to allow myself to work better on
        // implementation on a step by step basis as well as allowing me to later on move the 
        // functions elswhere if needed.
        UnityEngine.Random.InitState(seed);
        GenerateDungeon(dungeonSize, maxIterations, minRoomSize);
        CreateRooms(minRoomSize);
        CreateHallways(hallwayWidth);
        PlacePoints(startPointPrefab, endPointPrefab);
        CheckPath(points[0], points[1]);
    }

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
        RoomNode rootNode = new RoomNode(new Vector2Int(0, 0), dungeonSize, null);
        nodeList.Add(rootNode);
        roomList.Add(rootNode);

        for (int i = 0; i < maxIterations; i++)
        {
            //Declare nodes and variables to be assigned
            RoomNode nodeOne;
            RoomNode nodeTwo;
            Vector2Int nodeTwoPos;
            Vector2Int nodeOneSize;
            Vector2Int nodeTwoSize;

            //Choose wether to split on x or y axis
            //TODO: Refactor and make less janky
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
                int cutPosX = UnityEngine.Random.Range(minRoomSize.x, rootNode.Size.x - minRoomSize.x);

                //Calculate the cut and the size of the new room
                nodeOneSize = new Vector2Int(cutPosX, rootNode.Size.y);
                nodeTwoSize = new Vector2Int(rootNode.Size.x - cutPosX, rootNode.Size.y);
                nodeTwoPos = new Vector2Int(rootNode.Position.x + nodeOneSize.x, rootNode.Position.y);

                nodeOne = new RoomNode(rootNode.Position, nodeOneSize, rootNode);
                nodeTwo = new RoomNode(nodeTwoPos, nodeTwoSize, rootNode);
            }
            else
            {
                //Choose a random postion between the current root nodes position and size to split and set the child nodes within
                int cutPosY = UnityEngine.Random.Range(minRoomSize.y, rootNode.Size.y - minRoomSize.y);

                //Calculate the size and postion of the new rooms
                nodeOneSize = new Vector2Int(rootNode.Size.x, cutPosY);
                nodeTwoSize = new Vector2Int(rootNode.Size.x, rootNode.Size.y - cutPosY);
                nodeTwoPos = new Vector2Int(rootNode.Position.x, rootNode.Position.y + nodeOneSize.y);

                nodeOne = new RoomNode(rootNode.Position, nodeOneSize, rootNode);
                nodeTwo = new RoomNode(nodeTwoPos, nodeTwoSize, rootNode);
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
        hallwayList = new List<Hallway>();

        RoomNode roomStart;
        RoomNode roomEnd;

        //TODO: Fix issues where hallways going backwards have a big gap
        //TODO: Connect hallways together from the same parent to reduce the amount of overlap
        for (int i = 0; i < roomList.Count - 1; i++)
        {
            roomStart = roomList[i];
            roomEnd = (RoomNode)roomList[i].ParentNode.ChildNodesList[1];

            //Check wether a room is alreayd connected or not
            if (roomStart.Visited || roomEnd.Visited)
            {
                continue;
            }

            //Calulcate the start points of the hallway
            Vector2Int startLoc = new Vector2Int(
                roomStart.Position.x + roomStart.Size.x,
                UnityEngine.Random.Range(roomStart.Position.y, roomStart.Position.x + roomStart.Size.y));
            Vector2Int endLoc = new Vector2Int(
                UnityEngine.Random.Range(roomEnd.Position.x, roomEnd.Position.x + roomEnd.Size.x),
                roomEnd.Position.y);

            //Save as node with info
            Hallway hallway = new Hallway(startLoc, endLoc, true, null);
            hallway.ConnectRoom(roomStart);
            hallway.ConnectRoom(roomEnd);
            hallwayList.Add(hallway);

            //Choose a random point in the rooms to generate from
            //TODO: Make more variation, maybe start with y at times then x
            if (UnityEngine.Random.Range(0, 2) == 1)
            {
                SpawnHallways(width, startLoc, endLoc, true);
            }
            else
            {
                SpawnHallways(width, startLoc, endLoc, false);
            }

            roomStart.Visited = true;
            roomEnd.Visited = true;
        }

        //Keep track of the hallway count to not loop over them again later leading to a shorter for loop
        int firstStageHallwayCount = hallwayList.Count;

        //Make connections between hallways
        for (int i = 1; i < firstStageHallwayCount; i += 2)
        {
            Hallway hallwayStart = hallwayList[i - 1];
            Hallway hallwayEnd = hallwayList[i];

            //Pick start and end locations using the distance between the hallways
            Vector2Int startLoc = new Vector2Int(
                hallwayStart.StartPoint.x + (hallwayStart.EndPoint.x - hallwayStart.StartPoint.x) / 2,
                hallwayStart.EndPoint.y);
            Vector2Int endLoc = new Vector2Int(
                hallwayEnd.StartPoint.x + (hallwayEnd.EndPoint.x - hallwayEnd.StartPoint.x) / 2,
                hallwayEnd.StartPoint.y);

            //Save connected rooms to hallway node
            Hallway hallway = new Hallway(startLoc, endLoc, true, hallwayStart);
            foreach (var room in hallwayStart.ConnectedRooms)
            {
                hallway.ConnectRoom(room);

            }
            foreach (var room in hallwayEnd.ConnectedRooms)
            {
                hallway.ConnectRoom(room);
            }
            hallwayList.Add(hallway);

            SpawnHallways(width, startLoc, endLoc, true);

            hallwayStart.ConnectHallway(hallwayEnd);
            hallwayEnd.ConnectHallway(hallwayStart);
        }

        //Make more hallway connections
        //TODO: Clean up to not be copy paste of previous loop
        for (int i = firstStageHallwayCount; i < hallwayList.Count; i += 2)
        {
            Hallway hallwayStart = hallwayList[i - 1];
            Hallway hallwayEnd = hallwayList[i];

            if (hallwayStart.Visited || hallwayEnd.Visited)
            {
                continue;
            }

            //Pick start and end locations using the distance between the hallways
            Vector2Int startLoc = new Vector2Int(
                hallwayStart.StartPoint.x + (hallwayStart.EndPoint.x - hallwayStart.StartPoint.x) / 2,
                hallwayStart.EndPoint.y);
            Vector2Int endLoc = new Vector2Int(
                hallwayEnd.StartPoint.x + (hallwayEnd.EndPoint.x - hallwayEnd.StartPoint.x) / 2,
                hallwayEnd.StartPoint.y);

            //Save connected rooms to hallway node
            Hallway hallway = new Hallway(startLoc, endLoc, true, hallwayStart);
            foreach (var room in hallwayStart.ConnectedRooms)
            {
                hallway.ConnectRoom(room);

            }
            foreach (var room in hallwayEnd.ConnectedRooms)
            {
                hallway.ConnectRoom(room);
            }
            hallwayList.Add(hallway);

            SpawnHallways(width, startLoc, endLoc, true);

            hallwayStart.ConnectHallway(hallwayEnd);
            hallwayEnd.ConnectHallway(hallwayStart);
        }
    }

    /// <summary>
    /// Spawn in a hallway using hallway prefab.
    /// </summary>
    /// <param name="width">Width of hallways.</param>
    /// <param name="startLoc">Start location of hallway.</param>
    /// <param name="endLoc">End location of hallway.</param>
    /// <param name="xFirst">Spawn on x axis first.</param>
    private void SpawnHallways(int width, Vector2Int startLoc, Vector2Int endLoc, bool xFirst)
    {
        //Get start point and distance to travel in x and y
        Vector2Int roomDistance = endLoc - startLoc;
        roomDistance = new Vector2Int(roomDistance.x, roomDistance.y);
        int hallwayOneEnd = startLoc.x + roomDistance.x;

        //Create hallway sepertly through taxicab geometry (first go via x then y)
        //I decided to use taxicab geomerty as it was simple and quick and would fit in well with the structure
        //TODO: Add overlap reduction with room spaces
        //TODO: Fix backwards gap issues
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

        hallways.Add(hallwayOne);
        hallways.Add(hallwayTwo);
    }

    /// <summary>
    /// Place the start and end points for the player within the dungeon.
    /// </summary>
    /// <param name="startPrefab">Start point prefab.</param>
    /// <param name="endPrefab">End point prefab.</param>
    private void PlacePoints(GameObject startPrefab, GameObject endPrefab)
    {
        points = new List<GameObject>();
        StartingPoint startingPointScript;
        EndingPoint endingPointScript;

        RoomNode roomStart = roomList[0];
        RoomNode roomEnd = roomList[roomList.Count - 1];

        //Get a random spot from a room
        //TODO: Pick random spot between room using wall width to spawn within the room
        Vector2Int startPos = new Vector2Int(
            roomStart.Position.x,
            roomStart.Position.y);
        Vector2Int endPos = new Vector2Int(
            roomEnd.Position.x,
            roomEnd.Position.y);

        //TODO: Save start and end points in nodes as well
        GameObject startPoint = Instantiate(startPrefab,
            new Vector3(startPos.x, startPrefab.transform.position.x, startPos.y),
            startPrefab.transform.rotation,
            this.transform);
        startingPointScript = startPoint.GetComponent<StartingPoint>();
        startingPointScript.room = roomStart;

        GameObject endPoint = Instantiate(endPrefab,
            new Vector3(endPos.x, endPrefab.transform.position.y, endPos.y),
            endPrefab.transform.rotation,
            this.transform);
        endingPointScript = endPoint.GetComponent<EndingPoint>();
        endingPointScript.room = roomEnd;

        points.Add(startPoint);
        points.Add(endPoint);
    }

    /// <summary>
    /// Check wether the dungeon can be completed.
    /// </summary>
    /// <param name="start">StartPoint gameobject.</param>
    /// <param name="end">EndPoint gameobject.</param>
    private void CheckPath(GameObject start, GameObject end)
    {
        StartingPoint startPoint = start.GetComponent<StartingPoint>();
        EndingPoint endPoint = end.GetComponent<EndingPoint>();

        RoomNode startRoom = startPoint.room;
        RoomNode endRoom = endPoint.room;

        int connections = 0;

        //Search for connections between the start and end point
        //Due to my lack of knowledge of AI in unity as well as issues with the visualisation I was unable to use
        //AI agnets to check if the path is able to be completed like I wanted
        foreach (Hallway hallway in hallwayList)
        {
            if (hallway.ConnectedRooms.Contains(startRoom) && hallway.ConnectedRooms.Contains(endRoom))
            {
                connections++;
                Debug.Log("Connection found");
            }
        }
    }
}
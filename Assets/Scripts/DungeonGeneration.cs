using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DungeonGeneration : MonoBehaviour
{
    [Header("Prefabs")]
    public GameObject startPoint;
    public GameObject endPoint;

    [Header("Dungeon Settings")]
    public Vector2Int dungeonSize;
    public Vector2Int minRoomSize;
    public int hallwayWidth;

    [Header("Dungeon Generation Options")]
    public int maxIterations;
    public int seed;

    private List<Node> nodeList;

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
        PlacePoints(startPoint, endPoint);
        CheckPath();
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
        List<RoomNode> roomNodesToCut = new List<RoomNode>();
        RoomNode rootNode = new RoomNode(new Vector2Int(0, 0), dungeonSize, 0, null);
        nodeList.Add(rootNode);
        roomNodesToCut.Add(rootNode);

        //Visualisation
        NodeInfo nodeInfoR;
        GameObject rootNodeGO = new GameObject("Node0");
        rootNodeGO.transform.position = new Vector3(rootNode.Position.x, 0.0f, rootNode.Position.y);
        nodeInfoR = rootNodeGO.AddComponent<NodeInfo>();
        nodeInfoR.size = rootNode.Size;
        nodeInfoR.pos = rootNode.Position;

        ///TODO: Make index increment only once the tree goes down another layer
        int index = 1;

        for (int i = 1; i < maxIterations; i++)
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

            if (cutType == CutType.x)
            {
                //Choose a random postion between the current root nodes position and size to split and set the child nodes within
                int cutPosX = UnityEngine.Random.Range(minRoomSize.x, rootNode.Size.x);

                if (cutPosX < minRoomSize.x || rootNode.Size.x - cutPosX < minRoomSize.x)
                {
                    continue;
                }

                //Calculate the cut and the size of the new room
                nodeTwoPos = new Vector2Int(cutPosX, rootNode.Position.y);
                nodeOneSize = new Vector2Int(cutPosX, rootNode.Size.y);
                nodeTwoSize = new Vector2Int(rootNode.Size.x - cutPosX, rootNode.Size.y);

                nodeOne = new RoomNode(rootNode.Position, nodeOneSize, index, rootNode);
                nodeTwo = new RoomNode(nodeTwoPos, nodeTwoSize, index, rootNode);
            }
            else
            {
                //Choose a random postion between the current root nodes position and size to split and set the child nodes within
                int cutPosY = UnityEngine.Random.Range(minRoomSize.y, rootNode.Size.y);

                if (cutPosY < minRoomSize.y || rootNode.Size.y - cutPosY < minRoomSize.y)
                {
                    continue;
                }

                //Calculate the size and postion of the new rooms
                nodeTwoPos = new Vector2Int(rootNode.Position.x, cutPosY);
                nodeOneSize = new Vector2Int(rootNode.Size.x, cutPosY);
                nodeTwoSize = new Vector2Int(rootNode.Size.x, rootNode.Size.y - cutPosY);

                nodeOne = new RoomNode(rootNode.Position, nodeOneSize, index, rootNode);
                nodeTwo = new RoomNode(nodeTwoPos, nodeTwoSize, index, rootNode);
            }

            //Visualisation
            NodeInfo nodeInfo;
            GameObject nodeOneGO = new GameObject("Node1");
            nodeOneGO.transform.position = new Vector3(nodeOne.Position.x, 0.0f, nodeOne.Position.y);
            nodeOneGO.transform.parent = rootNodeGO.transform;
            nodeInfo = nodeOneGO.AddComponent<NodeInfo>();
            nodeInfo.size = nodeOne.Size;
            nodeInfo.pos = nodeOne.Position;
            GameObject nodeTwoGO = new GameObject("Node2");
            nodeTwoGO.transform.position = new Vector3(nodeTwo.Position.x, 0.0f, nodeOne.Position.y);
            nodeTwoGO.transform.parent = rootNodeGO.transform;
            nodeInfo = nodeTwoGO.AddComponent<NodeInfo>();
            nodeInfo.size = nodeTwo.Size;
            nodeInfo.pos = nodeTwo.Position;

            //Add new rooms as children to the root then move on to the next root node
            nodeList.Add(nodeOne);
            nodeList.Add(nodeTwo);
            rootNode.AddChild(nodeOne);
            rootNode.AddChild(nodeTwo);
            roomNodesToCut.Add(nodeOne);
            roomNodesToCut.Add(nodeTwo);

            rootNode.visited = true;
            roomNodesToCut.Remove(rootNode);

            //Set new root node
            rootNode = roomNodesToCut[0];

            //Visualisation
            rootNodeGO.transform.parent = nodeOneGO.transform;
            rootNodeGO.transform.position = new Vector3(rootNode.Position.x, 0.0f, rootNode.Position.y);
            nodeInfoR = rootNodeGO.AddComponent<NodeInfo>();
            nodeInfoR.size = rootNode.Size;
            nodeInfoR.pos = rootNode.Position;
        }
    }

    //TODO: Add rooms within the generated BSP grid
    /// <summary>
    /// Generate the rooms within the binary space partition grid.
    /// </summary>
    /// <param name="minimumSize">Minium size of rooms</param>
    private void CreateRooms(Vector2Int minimumSize)
    {
        
    }

    //TODO: Add create hallways between rooms within sections and then connect those hallways that have been created
    /// <summary>
    /// Generate hallways between rooms and other hallways.
    /// </summary>
    /// <param name="width">Width of hallways.</param>
    private void CreateHallways(int width)
    {

    }

    /// <summary>
    /// Place the start and end points for the player within the dungeon.
    /// </summary>
    /// <param name="startPoint">Start point prefab.</param>
    /// <param name="endPoint">End point prefab.</param>
    private void PlacePoints(GameObject startPoint, GameObject endPoint)
    {

    }

    //TODO: Add A* path finding to see if it is possible to complete the dungeon
    private void CheckPath()
    {

    }
}
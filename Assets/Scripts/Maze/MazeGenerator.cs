using System.Collections.Generic;
using Unity.Netcode;

namespace Maze
{
    public enum RoomType
{
    Library,
    Hallway,
    Warehouse,
    Lab,
    Idkwtf
}

public struct MazeGeneratorCell : INetworkSerializable
{
    public int x;
    public int y;

    public bool wallBottom;
    public bool wallLeft;

    public bool visited;
    public int roomNumber;
    
    public int zone;

    public MazeGeneratorCell(int x, int y)
    {
        this.x = x;
        this.y = y;
        wallBottom = true; 
        wallLeft = true;
        visited = false;
        roomNumber = 0;
        zone = -1;
    }
    
    public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
    {
        serializer.SerializeValue(ref x);
        serializer.SerializeValue(ref y);
        serializer.SerializeValue(ref wallBottom);
        serializer.SerializeValue(ref wallLeft);
        serializer.SerializeValue(ref visited);
        serializer.SerializeValue(ref roomNumber);
    }
}

public class KruskallTree
{
    private KruskallTree parent = null;
    public int number;

    public KruskallTree Root()
    {
        if (parent == null)
            return this;
        return parent = parent.Root();
    }

    public void Connect(KruskallTree tree)
    {
        KruskallTree root1 = this.Root();
        KruskallTree root2 = tree.Root();
        if (root1 != root2)
            root1.parent = root2;  // Merge trees
    }

    public bool IsConnected(KruskallTree tree)
    {
        return this.Root() == tree.Root();
    }
}

public class MazeGenerator
{
    public MazeGeneratorCell[,] GenerateMaze(int width, int length)
    {
        MazeGeneratorCell[,] maze = new MazeGeneratorCell[width, length];

        for (int i = 0; i < maze.GetLength(0); ++i)
        {
            for (int j = 0; j < maze.GetLength(1); ++j)
            {
                maze[i, j] = new MazeGeneratorCell(i, j);
                maze[i, j].roomNumber = i * length + j;
            }
        }

        KruskallMazeAlgorithm(maze);
        
        List<(int, int)> centers = new List<(int, int)> ();
        centers.Add((width / 2, length / 2));
        centers.Add((width - 2, length - 2));
        centers.Add((2, 2));
        centers.Add((width - 2, 2));
        centers.Add((2, length - 2));

        ZoneClustering.AssignZones(maze, centers);

        return maze;
    }

    private void KruskallMazeAlgorithm(MazeGeneratorCell[,] maze)
    {
        int width = maze.GetLength(0);
        int length = maze.GetLength(1);
        int numOfSections = width * length;
        KruskallTree[,] trees = new KruskallTree[width, length];
        for (int i = 0; i < maze.GetLength(0); ++i)
        {
            for (int j = 0; j < maze.GetLength(1); ++j)
            {
                trees[i, j] = new KruskallTree();
                trees[i, j].number = i * length + j;
            }
        }

        while (numOfSections != 1)
        {
            int randomWidth = UnityEngine.Random.Range(0, width);
            int randomlength = UnityEngine.Random.Range(0, length);
            MazeGeneratorCell curr = maze[randomWidth, randomlength];

            List<MazeGeneratorCell> notVisitedNei = new List<MazeGeneratorCell>();
            int x = curr.x;
            int y = curr.y;

            if (x > 0)
            {
                notVisitedNei.Add(maze[x - 1, y]);
            }

            if (y > 0)
            {
                notVisitedNei.Add(maze[x, y - 1]);
            }

            if (x < width - 2)
            {
                notVisitedNei.Add(maze[x + 1, y]);
            }

            if (y < length - 2)
            {
                notVisitedNei.Add(maze[x, y + 1]);
            }

            if (notVisitedNei.Count > 0)
            {
                MazeGeneratorCell goingTo = notVisitedNei[UnityEngine.Random.Range(0, notVisitedNei.Count)];

                if (!trees[x, y].IsConnected(trees[goingTo.x, goingTo.y]))
                {
                    DurovRemoveWall(ref maze[x, y], ref maze[goingTo.x, goingTo.y]);
                    trees[x, y].Connect(trees[goingTo.x, goingTo.y]);
                    --numOfSections;
                }

            }
        }
        
        // Some debugging (Kirill probably knows the purpose. I (Igor) do not)
        //for (int i = 0; i < maze.GetLength(0); ++i)
        //{
        //    for (int j = 0; j < maze.GetLength(1); ++j)
        //    {
        //        Debug.Log(trees[i, j].Root().number);
        //    }
        //}

    }

    private void DurovRemoveWall(ref MazeGeneratorCell first, ref MazeGeneratorCell second)
    {
        if (first.x == second.x)
        {
            if (first.y > second.y)
            {
                first.wallBottom = false;
            } else
            {
                second.wallBottom = false;
            }
        } else
        {
            if (first.x > second.x)
            {
                first.wallLeft = false;
            }
            else
            {
                second.wallLeft = false;
            }
        }
    }
}
}

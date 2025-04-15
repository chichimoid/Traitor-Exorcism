using System.Collections.Generic;
using System.Linq;
using Unity.Netcode;

namespace Maze
{
    public struct MazeGeneratorCell : INetworkSerializable
    {
        public int x;
        public int y;

        public bool wallBottom;
        public bool wallLeft;

        public bool doorBottom;
        public bool doorLeft;

        public bool replaceableLeft;
        public bool replaceableBottom;

        public bool visited;
        public int roomNumber;

        public int zone;
        public bool isLeverRoom;

        public MazeGeneratorCell(int x, int y)
        {
            this.x = x;
            this.y = y;
            wallBottom = true;
            wallLeft = true;

            doorBottom = false;
            doorLeft = false;

            replaceableLeft = true;
            replaceableBottom = true;

            visited = false;
            roomNumber = -1;
            zone = -1;
            isLeverRoom = false;
        }

        public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
        {
            serializer.SerializeValue(ref x);
            serializer.SerializeValue(ref y);
            serializer.SerializeValue(ref wallBottom);
            serializer.SerializeValue(ref wallLeft);
            serializer.SerializeValue(ref doorBottom);
            serializer.SerializeValue(ref doorLeft);
            serializer.SerializeValue(ref replaceableLeft);
            serializer.SerializeValue(ref replaceableBottom);
            serializer.SerializeValue(ref visited);
            serializer.SerializeValue(ref zone);
            serializer.SerializeValue(ref roomNumber);
            serializer.SerializeValue(ref isLeverRoom);
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
        public int RoomsInZone = 3;
        public int ZoneCount = 5;

        public MazeGeneratorCell[,] GenerateMaze(int width, int length, bool isHardcore)
        {
            MazeGeneratorCell[,] maze = new MazeGeneratorCell[width, length];

            for (int i = 0; i < maze.GetLength(0); ++i)
            {
                for (int j = 0; j < maze.GetLength(1); ++j)
                {
                    maze[i, j] = new MazeGeneratorCell(i, j);
                }
            }

            KruskallMazeAlgorithm(maze);

            HashSet<MazeGeneratorCell> mazeHashset = ZoneClustering.ToHashSetManual(maze);
            List<(int, int)> centers = ZoneClustering.SelectRandomElements(mazeHashset, ZoneCount);

            ZoneClustering.AssignZones(maze, centers);
            ZoneClustering.AssignLevers(maze, ZoneCount);

            if (!isHardcore)
            {
                List<HashSet<MazeGeneratorCell>> sortedCells = Enumerable.Repeat(new HashSet<MazeGeneratorCell>(), ZoneCount).ToList();
                for (int i = 0; i < maze.GetLength(0); ++i)
                {
                    for (int j = 0; j < maze.GetLength(1); ++j)
                    {
                        sortedCells[maze[i, j].zone].Add(maze[i, j]);
                    }
                }
            
                ZoneClustering.AssignRoomsToZones(maze, sortedCells, 4);
                ZoneClustering.EncloseRoomsWithDoors(maze);
            } else
            {
                for (int i = 0; i < maze.GetLength(0); ++i)
                {
                    for (int j = 0; j < maze.GetLength(1); ++j)
                    {
                        maze[i, j].replaceableLeft = false;
                        maze[i, j].replaceableBottom = false;
                    }
                }
            }
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
                }
                else
                {
                    second.wallBottom = false;
                }
            }
            else
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

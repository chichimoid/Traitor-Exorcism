using System.Collections.Generic;
using System.Linq;
using System;
using UnityEngine;

namespace Maze
{
    public static class ZoneClustering
    {
        public static void AssignZones(MazeGeneratorCell[,] maze, List<(int x, int y)> centers)
        {
            int rows = maze.GetLength(0);
            int cols = maze.GetLength(1);

            Queue<(int x, int y, int centerIndex, int distance)> queue = new Queue<(int x, int y, int centerIndex, int distance)>();

            for (int i = 0; i < centers.Count; i++)
            {
                var center = centers[i];
                queue.Enqueue((center.x, center.y, i, 0));
                maze[center.x, center.y].zone = i; // Zone center has it's own zone
            }

            int[] dx = { -1, 1, 0, 0 };
            int[] dy = { 0, 0, -1, 1 };

            // BFS
            while (queue.Count > 0)
            {
                var current = queue.Dequeue();
                int x = current.x;
                int y = current.y;
                int centerIndex = current.centerIndex;
                int distance = current.distance;

                for (int dir = 0; dir < 4; dir++)
                {
                    int nx = x + dx[dir];
                    int ny = y + dy[dir];

                    if (nx >= 0 && nx < rows && ny >= 0 && ny < cols)
                    {
                        bool canMove = true;
                        if (dir == 0 && maze[x, y].wallLeft) canMove = false;   // Up
                        if (dir == 1 && maze[nx, ny].wallLeft) canMove = false; // Down
                        if (dir == 2 && maze[x, y].wallBottom) canMove = false; // Left
                        if (dir == 3 && maze[nx, ny].wallBottom) canMove = false; // Right

                        if (canMove && maze[nx, ny].zone == -1)
                        {
                            maze[nx, ny].zone = centerIndex;
                            queue.Enqueue((nx, ny, centerIndex, distance + 1));
                        }
                    }
                }
            }
        }

        public static void AssignLevers(MazeGeneratorCell[,] maze, int zoneCount)
        {
            for (int curZone = 0; curZone < zoneCount; curZone++)
            {
                List<MazeGeneratorCell> curZoneCells = new List<MazeGeneratorCell>();

                for (int i = 0; i < maze.GetLength(0); i++)
                {
                    for (int j = 0; j < maze.GetLength(1); j++)
                    {
                        if (maze[i, j].zone == curZone)
                        {
                            curZoneCells.Add(maze[i, j]);
                        }
                    }
                }

                MazeGeneratorCell leverRoom = curZoneCells[UnityEngine.Random.Range(0, curZoneCells.Count)];
                maze[leverRoom.x, leverRoom.y].isLeverRoom = true;
            }
        }
        public static HashSet<T> ToHashSetManual<T>(T[,] matrix)
        {
            var set = new HashSet<T>();
            for (int i = 0; i < matrix.GetLength(0); i++)
                for (int j = 0; j < matrix.GetLength(1); j++)
                    set.Add(matrix[i, j]);
            return set;
        }

        public static void EncloseRoomsWithDoors(MazeGeneratorCell[,] maze)
        {
            int width = maze.GetLength(0);
            int height = maze.GetLength(1);

            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    MazeGeneratorCell currentCell = maze[x, y];

                    if (y > 0)
                    {
                        MazeGeneratorCell bottomCell = maze[x, y - 1];

                        if (!currentCell.wallBottom && (currentCell.roomNumber != bottomCell.roomNumber || currentCell.zone != bottomCell.zone))
                        {
                            currentCell.doorBottom = true;
                        }

                        if (currentCell.wallBottom && (currentCell.roomNumber != bottomCell.roomNumber || currentCell.zone != bottomCell.zone))
                        {
                            currentCell.replaceableBottom = false;
                        }
                    }
                    else
                    {
                        currentCell.doorBottom = false;
                        currentCell.replaceableBottom = false;
                    }

                    if (x > 0)
                    {
                        MazeGeneratorCell leftCell = maze[x - 1, y];
                        if (!currentCell.wallLeft && (currentCell.roomNumber != leftCell.roomNumber || currentCell.zone != leftCell.zone))
                        {
                            currentCell.doorLeft = true;
                        }

                        if (currentCell.wallLeft && (currentCell.roomNumber != leftCell.roomNumber || currentCell.zone != leftCell.zone))
                        {
                            currentCell.replaceableLeft = false;
                        }
                    }
                    else
                    {
                        currentCell.doorLeft = false;
                        currentCell.replaceableLeft = false;
                    }

                    maze[currentCell.x, currentCell.y] = currentCell;
                }
            }
        }

        public static List<(int, int)> SelectRandomElements(HashSet<MazeGeneratorCell> set, int n)
        {
            List<MazeGeneratorCell> list = set.ToList();

            if (n > list.Count)
            {
                n = list.Count;
            }

            List<(int, int)> selectedElements = new List<(int, int)>();

            for (int i = 0; i < n; i++)
            {
                int randomIndex = UnityEngine.Random.Range(0, list.Count - i);

                selectedElements.Add((list[randomIndex].x, list[randomIndex].y));

                list[randomIndex] = list[list.Count - 1 - i];
            }

            return selectedElements;
        }

        public static void AssignRoomsToZones(MazeGeneratorCell[,] maze, List<HashSet<MazeGeneratorCell>> SortedZoneCells, int roomCount)
        {
            int numberOfZones = SortedZoneCells.Count;
            for (int i = 0; i < numberOfZones; i++)
            {
                AssignRoomsToOneZone(maze, SortedZoneCells[i], roomCount);
                foreach (MazeGeneratorCell cell in SortedZoneCells[i])
                {
                    // maze[cell.x, cell.y].roomNumber = i * roomCount + maze[cell.x, cell.y].roomNumber;
                    maze[cell.x, cell.y].roomNumber *= 5;
                }
            }
        }

        private static void AssignRoomsToOneZone(MazeGeneratorCell[,] maze, HashSet<MazeGeneratorCell> ZoneCells, int roomCount)
        {
            List<(int, int)> centers = SelectRandomElements(ZoneCells, roomCount);

            Queue<(int x, int y, int centerIndex, int distance)> queue = new Queue<(int x, int y, int centerIndex, int distance)>();

            for (int i = 0; i < centers.Count; i++)
            {
                var center = centers[i];
                queue.Enqueue((center.Item1, center.Item2, i, 0));
                maze[center.Item1, center.Item2].roomNumber = i;
            }

            int[] dx = { -1, 1, 0, 0 };
            int[] dy = { 0, 0, -1, 1 };
            int rows = maze.GetLength(0);
            int cols = maze.GetLength(1);

            // BFS
            while (queue.Count > 0)
            {
                var current = queue.Dequeue();
                int x = current.x;
                int y = current.y;
                int centerIndex = current.centerIndex;
                int distance = current.distance;

                for (int dir = 0; dir < 4; dir++)
                {
                    int nx = x + dx[dir];
                    int ny = y + dy[dir];

                    if (nx >= 0 && nx < rows && ny >= 0 && ny < cols)
                    {
                        bool canMove = true;
                        // if (dir == 0 && maze[x, y].wallLeft) canMove = false;   // Up
                        // if (dir == 1 && maze[nx, ny].wallLeft) canMove = false; // Down
                        // if (dir == 2 && maze[x, y].wallBottom) canMove = false; // Left
                        // if (dir == 3 && maze[nx, ny].wallBottom) canMove = false; // Right
                        if (!ZoneCells.Contains(maze[nx, ny])) canMove = false;

                        if (canMove && maze[nx, ny].roomNumber == -1)
                        {
                            maze[nx, ny].roomNumber = centerIndex;
                            queue.Enqueue((nx, ny, centerIndex, distance + 1));
                        }
                    }
                }
            }
        }
    }
}

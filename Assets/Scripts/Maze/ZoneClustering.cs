using System.Collections.Generic;

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
    }
}

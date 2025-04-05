using Unity.Netcode;

namespace Maze
{
    public struct Maze : INetworkSerializable
    {
        private int _width;
        private int _length;
        public int Width => _width;
        public int Length => _length;
        public MazeGeneratorCell[,] MazeGeneratorCells { get; set; }

        public Maze(int width, int length)
        {
            _width = width;
            _length = length;
            MazeGeneratorCells = new MazeGeneratorCell[width, length];
        }

        public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
        {
            serializer.SerializeValue(ref _width);
            serializer.SerializeValue(ref _length);

            if (serializer.IsReader)
            {
                // If this is a reader, initialize the Cells array
                MazeGeneratorCells = new MazeGeneratorCell[_width, _length];
            }

            // Serialize each cell in the array
            for (int x = 0; x < Width; x++)
            {
                for (int y = 0; y < Length; y++)
                {
                    MazeGeneratorCells[x, y].NetworkSerialize(serializer);
                }
            }
        }
    }
}
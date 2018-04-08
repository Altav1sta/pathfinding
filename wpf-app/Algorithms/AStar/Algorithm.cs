using System;
using System.Collections.Generic;
using System.Linq;

namespace wpf_app.Algorithms.AStar
{
    public class Algorithm
    {
        private readonly int _seed;
        private readonly int _rows;
        private readonly int _columns;
        private readonly double _obstaclesPercentage;
        private readonly bool _withDiagonalMoves;

        public bool[,] Obstacles { get; }

        public Algorithm(int seed, double obstaclesPercentage, bool withDiagonalMoves, int rows, int columns)
        {
            _seed = seed;
            _rows = rows;
            _columns = columns;
            _obstaclesPercentage = obstaclesPercentage;
            _withDiagonalMoves = withDiagonalMoves;
            
            Obstacles = new bool[_rows, _columns];
            InitObstacles();
        }
        
        public IEnumerable<CellPosition> GetPath(int rowStart, int columnStart, int rowFinish, int columnFinish)
        {
            // wiki nomenclature for A* algorithm
            var q = new HashSet<CellPosition>(); // set of nodes to check
            var u = new HashSet<CellPosition>(); // set of nodes which is already checked
            var g = new Dictionary<CellPosition, double>(); // cost of road from start node to current
            var f = new Dictionary<CellPosition, double>(); // euristic distance (g + h)
            var posStart = new CellPosition(rowStart, columnStart);
            var posFinish = new CellPosition(rowFinish, columnFinish);
            var path = new Dictionary<CellPosition, CellPosition?>();

            int H(CellPosition x) => _withDiagonalMoves
                ? Math.Max(Math.Abs(columnFinish - x.Column), Math.Abs(rowFinish - x.Row)) // chebyshev's distance from current node to final
                : Math.Abs(columnFinish - x.Column) + Math.Abs(rowFinish - x.Row); // manhatten distance from current node to final

            double D(CellPosition x, CellPosition y) => x.Row != y.Row && x.Column != y.Column ? Math.Sqrt(2) : 1;
            
            q.Add(posStart);
            path[posStart] = null;
            g[posStart] = 0;
            f[posStart] = g[posStart] + H(posStart);

            while (q.Count > 0)
            {
                var current = f.Where(x => q.Contains(x.Key)).OrderBy(x => x.Value).First().Key;

                if (current == posFinish) break;

                q.Remove(current);
                u.Add(current);

                foreach (var v in GetReachableNeighbours(current))
                {
                    var tentativeScore = g[current] + D(current, v);

                    if (u.Contains(v) && tentativeScore >= g[v]) continue;

                    path[v] = current;
                    g[v] = tentativeScore;
                    f[v] = g[v] + H(v);

                    if (!q.Contains(v)) q.Add(v);
                }
            }

            var currentPos = posFinish;
            var result = new List<CellPosition>();

            while (currentPos != posStart)
            {
                result.Add(currentPos);
                currentPos = path[currentPos].Value;
            }

            return result;
        }

        private void InitObstacles()
        {
            var r = new Random(_seed);
            var isReverseInit = _obstaclesPercentage > 0.5;
            var percentage = isReverseInit ? 1 - _obstaclesPercentage : _obstaclesPercentage;
            var count = (int)(_rows * _columns * percentage);

            if (isReverseInit)
            {
                for (var i = 0; i < _rows; i++)
                {
                    for (var j = 0; j < _columns; j++)
                    {
                        Obstacles[i, j] = true;
                    }
                }
            }

            for (var n = 0; n < count; n++)
            {
                var i = r.Next(_rows);
                var j = r.Next(_columns);

                if (i == 0 && j == 0 || Obstacles[i, j] != isReverseInit)
                {
                    n--;
                    continue;
                }

                Obstacles[i, j] ^= true;
            }
        }

        private IEnumerable<CellPosition> GetReachableNeighbours(CellPosition current)
        {
            var neighbours = new List<CellPosition>();

            foreach (var i in Enumerable.Range(-1, 3))
            {
                foreach (var j in Enumerable.Range(-1, 3))
                {
                    var newRow = current.Row + i;
                    var newCol = current.Column + j;
                    
                    // same point
                    if (i == 0 && j == 0) continue;

                    // diagonals
                    if (!_withDiagonalMoves && (i + j) % 2 == 0) continue;

                    // out of cells range
                    if (newRow == -1 || newRow == _rows || newCol == -1 || newCol == _columns) continue;

                    // can't pass through
                    if (Obstacles[newRow, newCol]) continue; 

                    neighbours.Add(new CellPosition(newRow, newCol));
                }
            }

            return neighbours;
        }

        public struct CellPosition
        {
            public int Row { get; }
            public int Column { get; }

            public CellPosition(int row, int column)
            {
                Row = row;
                Column = column;
            }

            public bool Equals(CellPosition other) => Row == other.Row && Column == other.Column;

            public override bool Equals(object obj) => obj is CellPosition && Equals((CellPosition)obj);

            public override int GetHashCode() => unchecked((Row * 397) ^ Column);

            public static bool operator ==(CellPosition x, CellPosition y) => x.Equals(y);

            public static bool operator !=(CellPosition x, CellPosition y) => !(x == y);
        }
    }
}

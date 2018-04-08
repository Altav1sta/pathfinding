using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;
using wpf_app.Algorithms.AStar;

namespace wpf_app.Controls.AStar
{
    /// <summary>
    /// Interaction logic for Container.xaml
    /// </summary>
    public partial class Container : UserControl
    {
        private int _cellSize = 20;
        private Rectangle[,] _grid;
        private Algorithm _algorithm;
        private bool _pathDrawn;

        public Container()
        {
            InitializeComponent();
        }

        private void OnRunClick(object sender, RoutedEventArgs e)
        {
            var rows = (int) AStarCanvas.ActualHeight / _cellSize;
            var columns = (int) AStarCanvas.ActualWidth / _cellSize;

            _algorithm = new Algorithm(1023, 0.3, true, rows, columns);
            _pathDrawn = false;

            InitializeGrid();
        }

        private void OnCellMouseUp(object sender, MouseButtonEventArgs e)
        {
            if (_pathDrawn) return;

            for (var i = 0; i < _grid.GetLength(0); i++)
            {
                for (var j = 0; j < _grid.GetLength(1); j++)
                {
                    var cursorPosition = e.GetPosition(AStarCanvas);
                    var top = Canvas.GetTop(_grid[i, j]);
                    var left = Canvas.GetLeft(_grid[i, j]);
                    var inSameRow = top <= cursorPosition.Y && top + _cellSize >= cursorPosition.Y;
                    var inSameColumn = left <= cursorPosition.X && left + _cellSize >= cursorPosition.X;

                    if (!inSameRow || !inSameColumn) continue;
                    if (i == 0 && j == 0) return;

                    var path = _algorithm.GetPath(0, 0, i, j);

                    foreach (var position in path)
                    {
                        _grid[position.Row, position.Column].Fill = Brushes.LightBlue;
                    }
                }
            }

            _pathDrawn = true;
        }

        private void InitializeGrid()
        {
            AStarCanvas.Children.Clear();

            var rows = _algorithm.Obstacles.GetLength(0);
            var columns = _algorithm.Obstacles.GetLength(1);
            var verticalPadding = (AStarCanvas.ActualHeight - rows * _cellSize) / 2;
            var horizontalPadding = (AStarCanvas.ActualWidth - columns * _cellSize) / 2;

            _grid = new Rectangle[rows, columns];

            for (var i = 0; i < rows; i++)
            {
                for (var j = 0; j < columns; j++)
                {
                    var hasObstacle = _algorithm.Obstacles[i, j];
                    var left = horizontalPadding + j * _cellSize;
                    var top = verticalPadding + i * _cellSize;
                    var rect = new Rectangle
                    {
                        Height = _cellSize,
                        Width = _cellSize
                    };

                    if (hasObstacle)
                    {
                        rect.Fill = Brushes.Brown;
                    }
                    else
                    {
                        rect.Stroke = Brushes.LightGray;
                        rect.StrokeThickness = 1;
                    }

                    Canvas.SetLeft(rect, left);
                    Canvas.SetTop(rect, top);

                    AStarCanvas.Children.Add(rect);

                    _grid[i, j] = rect;
                }
            }

            _grid[0, 0].Fill = Brushes.LightGreen;
        }

        private void OnRestartClick(object sender, RoutedEventArgs e)
        {
            if (!_pathDrawn) return;

            if (_algorithm == null)
            {
                OnRunClick(sender, e);
            }
            else
            {
                InitializeGrid();
            }
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace game
{
    /// <summary>
    /// Interaction logic for Window1.xaml
    /// </summary>
    public partial class Game : Window
    {
        // name, Point(x, y)
        Dictionary<string, Vector[]> pieceMovements = new Dictionary<string, Vector[]> {
            { "pawn", new[] {
                new Vector(0, 1)  // forward
            } },
            { "king", new[] {
                new Vector(0, 1),  // forward
                new Vector(-1, 1),  // forward-right
                new Vector(1, 1),  // forward-left
                new Vector(-1, 0),  // right
                new Vector(1, 0),  // left
                new Vector(0, -1),  // backward
                new Vector(-1, -1),  // backward-right
                new Vector(1, -1),  // backward-left
            } },
            { "gold", new[] {
                new Vector(0, 1),  // forward
                new Vector(-1, 1),  // forward-right
                new Vector(1, 1),  // forward-left
                new Vector(-1, 0),  // right
                new Vector(1, 0),  // left
                new Vector(0, -1),  // backward
            } },
            { "tokin", new[] {
                new Vector(0, 1),  // forward
                new Vector(-1, 1),  // forward-right
                new Vector(1, 1),  // forward-left
                new Vector(-1, 0),  // right
                new Vector(1, 0),  // left
                new Vector(0, -1),  // backward
            } },
            { "silver", new[] {
                new Vector(0, 1),  // forward
                new Vector(-1, 1),  // forward-right
                new Vector(1, 1),  // forward-left
                new Vector(-1, -1),  // backward-right
                new Vector(1, -1),  // backward-left
            } },
            { "knight", new[] {
                new Vector(-1, 2),  // 2x forward and right
                new Vector(1, 2),  // 2x forward and left
            } },
            { "bishop", new[] {
                new Vector(-double.PositiveInfinity, double.PositiveInfinity),
                // forward-right
                new Vector(double.PositiveInfinity, double.PositiveInfinity),
                // forward-left
                new Vector(-double.PositiveInfinity, -double.PositiveInfinity),
                // backward-right
                new Vector(double.PositiveInfinity, -double.PositiveInfinity),
                // backward-left
            } },
            { "rook", new[] {
                new Vector(0, double.PositiveInfinity),
                // forward
                new Vector(0, -double.PositiveInfinity),
                // backward
                new Vector(-double.PositiveInfinity, 0),
                // right
                new Vector(double.PositiveInfinity, 0),
                // left
            } },
            { "lance", new[] {
                new Vector(0, double.PositiveInfinity),
                // forward
            } },
        };

        Dictionary<string, string> promotions = new Dictionary<string, string>
        {
            { "pawn", "rook" },
            { "rook", "pawn" },
            { "silver", "bishop" },
            { "bishop", "silver" },
            { "gold", "knight" },
            { "knight", "gold" },
            { "tokin", "lance" },
            { "lance", "tokin" },
        };

        private Grid? selectedGrid = null;
        private Image? selectedPiece = null;
        private List<Grid>? placedMovements = null;

        public Game()
        {
            InitializeComponent();
        }

        private List<int> CalculateMovements(
            int index,
            UIElementCollection boardPieces,
            string piece,
            bool isBot = false
        )
        {
            var final = new List<int> {};
            var movements = pieceMovements[piece];
            var current = new Vector(index % 5, index / 5);
            foreach (var movement in movements)
            {
                var (x, y) = (movement.X, movement.Y);
                if (double.IsInfinity(x) || double.IsInfinity(y))
                {
                    var infMovement = new Vector(
                        x == 0 ? 0 : x > 0 ? 1 : -1,
                        y == 0 ? 0 : y > 0 ? 1 : -1
                    );
                    var moved = current - infMovement * (isBot ? -1 : 1);
                    while (
                        moved.X >= 0 && moved.X <= 4
                        && moved.Y >= 0 && moved.Y <= 4
                    )
                    {
                        var movementIndex = (int)(moved.X + 5 * moved.Y);
                        var grid = ((Grid)boardPieces[movementIndex]).Children;
                        if (
                            grid.Count < 2
                            || ((Image)grid[grid.Count - 1]).Name.Contains("bot") != isBot
                        )
                            final.Add(movementIndex);
                        moved = moved - infMovement * (isBot ? -1 : 1);
                    }
                }
                else
                {
                    var moved = current - movement * (isBot ? -1 : 1);
                    var movementIndex = (int)(moved.X + 5 * moved.Y);
                    if (
                        moved.X >= 0 && moved.X <= 4
                        && moved.Y >= 0 && moved.Y <= 4
                    )
                    {
                        var grid = ((Grid)boardPieces[movementIndex]).Children;
                        if (
                            grid.Count < 2
                            || ((Image)grid[grid.Count - 1]).Name.Contains("bot") != isBot
                        )
                            final.Add(movementIndex);
                    }
                }
            }
            return final;
        }

        private void BoardPieceClick(object sender, RoutedEventArgs e)
        {
            var clicked = (Image)sender;
            var grid = (Grid)clicked.Parent;
            var board = (Grid)grid.Parent;
            var pieces = board.Children;
            var index = pieces.IndexOf(grid);

            if (placedMovements != null)
                foreach (var movementGrid in placedMovements)
                {
                    var grandChildren = ((Grid)movementGrid).Children;
                    grandChildren.RemoveAt(grandChildren.Count - 1);
                }

            if (selectedPiece == clicked)
            {
                selectedPiece = null;
                selectedGrid = null;
                placedMovements = null;
                return;
            }

            placedMovements = new List<Grid> {};

            foreach (var movementIndex in CalculateMovements(
                index,
                pieces,
                clicked.Name.Replace("bot_", ""),
                clicked.Name.Contains("bot")
            ))
            {
                var gridToPlace = (Grid)pieces[movementIndex];
                gridToPlace.Children.Add(
                    GetTurnSelect(
                        gridToPlace.Children.Count > 1,
                        clicked.Name.Contains("bot")
                    )
                );
                placedMovements.Add(gridToPlace);
            }
            selectedGrid = grid;
            selectedPiece = clicked;
        }

        private Image GetShogiPiece(string name, bool isBot = false)
        {
            var image = new Image();
            image.Width = 60;
            image.Height = 60;
            var source = new BitmapImage(new Uri(
                $"pack://application:,,,/game;component/resources/{name}.png"
            ));
            image.Source = source;
            image.RenderTransformOrigin = new Point(0.5, 0.5);
            image.Cursor = Cursors.Hand;
            image.MouseDown += BoardPieceClick;
            image.Name = (isBot ? "bot_" : "") + name;
            if (isBot)
            {
                var transform = new TransformGroup();
                transform.Children.Add(new RotateTransform(180));
                image.RenderTransform = transform;
            }
            return image;
        }

        private void BoardTurnClick(object sender, RoutedEventArgs e)
        {
            var clicked = (Image)sender;
            var grid = (Grid)clicked.Parent;
            var board = (Grid)grid.Parent;
            var pieces = board.Children;
            var index = pieces.IndexOf(grid);
            if (placedMovements != null)
                foreach (var movementGrid in placedMovements)
                {
                    var grandChildren = ((Grid)movementGrid).Children;
                    grandChildren.RemoveAt(grandChildren.Count - 1);
                }
            // grid.Children.Remove(clicked);
            if (clicked.Name == "take")
                grid.Children.RemoveAt(grid.Children.Count - 1);
            if (selectedPiece != null && selectedGrid != null)
            {
                selectedGrid.Children.Remove(selectedPiece);
                if (!selectedPiece.Name.Contains("king"))
                    grid.Children.Add(GetShogiPiece(
                        promotions[selectedPiece.Name.Replace("bot_", "")],
                        selectedPiece.Name.Contains("bot")
                    ));
                else
                    grid.Children.Add(selectedPiece);
                selectedPiece = null;
                selectedGrid = null;
                placedMovements = null;
            }
        }

        private Image GetTurnSelect(bool isPiece = false, bool isBot = false)
        {
            var image = new Image();
            image.Width = 60;
            image.Height = 60;
            var source = new BitmapImage(new Uri(
                "pack://application:,,,/game;component/resources/"
                + (isPiece ? "select_piece.png" : "select.png")
            ));
            image.Source = source;
            image.RenderTransformOrigin = new Point(0.5, 0.5);
            image.Cursor = Cursors.Hand;
            image.MouseDown += BoardTurnClick;
            image.Name = isPiece ? "take" : "move";
            if (!isBot)
            {
                var transform = new TransformGroup();
                transform.Children.Add(new RotateTransform(180));
                image.RenderTransform = transform;
            }
            return image;
        }

        private void ButtonClick(object sender, RoutedEventArgs e)
        {
            var clicked = (Button)sender;
            if (clicked.Name == "return")
            {
                new MainWindow().Show();
                this.Close();
            }
        }
    }
}

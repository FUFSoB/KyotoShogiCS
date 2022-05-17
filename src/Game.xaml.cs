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
        Dictionary<string, Point[]> pieces = new Dictionary<string, Point[]> {
            { "Pawn", new[] {
                new Point(0, 1)  // forward
            } },
            { "King", new[] {
                new Point(0, 1),  // forward
                new Point(1, 1),  // forward-right
                new Point(-1, 1),  // forward-left
                new Point(1, 0),  // right
                new Point(-1, 0),  // left
                new Point(0, -1),  // backward
                new Point(1, -1),  // backward-right
                new Point(-1, -1),  // backward-left
            } },
            { "Gold", new[] {
                new Point(0, 1),  // forward
                new Point(1, 1),  // forward-right
                new Point(-1, 1),  // forward-left
                new Point(1, 0),  // right
                new Point(-1, 0),  // left
                new Point(0, -1),  // backward
            } },
            { "Tokin", new[] {
                new Point(0, 1),  // forward
                new Point(1, 1),  // forward-right
                new Point(-1, 1),  // forward-left
                new Point(1, 0),  // right
                new Point(-1, 0),  // left
                new Point(0, -1),  // backward
            } },
            { "Silver", new[] {
                new Point(0, 1),  // forward
                new Point(1, 1),  // forward-right
                new Point(-1, 1),  // forward-left
                new Point(1, -1),  // backward-right
                new Point(-1, -1),  // backward-left
            } },
            { "Knight", new[] {
                new Point(1, 2),  // 2x forward and right
                new Point(-1, 2),  // 2x forward and left
            } },
            { "Bishop", new[] {
                new Point(double.PositiveInfinity, double.PositiveInfinity),
                // forward-right
                new Point(double.NegativeInfinity, double.PositiveInfinity),
                // forward-left
                new Point(double.PositiveInfinity, double.NegativeInfinity),
                // backward-right
                new Point(double.NegativeInfinity, double.NegativeInfinity),
                // backward-left
            } },
            { "Rook", new[] {
                new Point(0, double.PositiveInfinity),
                // forward
                new Point(0, double.NegativeInfinity),
                // backward
                new Point(double.PositiveInfinity, 0),
                // right
                new Point(double.NegativeInfinity, 0),
                // left
            } },
            { "Lance", new[] {
                new Point(0, double.PositiveInfinity),
                // forward
            } },
        };

        private Grid? SelectedGrid = null;
        private Image? SelectedPiece = null;
        public Game()
        {
            InitializeComponent();
            PlaceGrids();
        }

        void PlaceGrids()
        {
            var board = (Grid)FindName("Board");
            var a = board.Children;
        }

        private void BoardPieceClick(object sender, RoutedEventArgs e)
        {
            var clicked = (Image)sender;
            var grid = (Grid)clicked.Parent;
            var board = (Grid)grid.Parent;
            var pieces = board.Children;
            var index = pieces.IndexOf(grid);
            if (clicked.Name.Contains("Pawn"))
                if (clicked.Name.Contains("Bot"))
                    ((Grid)pieces[((byte)index) + 5]).Children.Add(
                        GetTurnSelect(
                            ((Grid)pieces[((byte)index) + 5]).Children.Count != 1,
                            true
                        )
                    );
                else
                    ((Grid)pieces[((byte)index) - 5]).Children.Add(
                        GetTurnSelect(
                            ((Grid)pieces[((byte)index) - 5]).Children.Count != 1,
                            false
                        )
                    );
            SelectedGrid = grid;
            SelectedPiece = clicked;
            // var imageContainer = ((Grid)pieces[index]).Children;
            // imageContainer.Add(
            //     GetShogiPiece("resources/king.png", clicked.Name.Contains("Bot"))
            // );
        }

        private Image GetShogiPiece(string path, bool isBot = false, string name = "")
        {
            var image = new Image();
            image.Width = 60;
            image.Height = 60;
            var source = new BitmapImage(new Uri(
                "pack://application:,,,/game;component/" + path,
                UriKind.Relative
            ));
            image.Source = source;
            image.RenderTransformOrigin = new Point(0.5, 0.5);
            image.Cursor = Cursors.Hand;
            image.MouseDown += BoardPieceClick;
            image.Name = name;
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
            grid.Children.Remove(clicked);
            SelectedGrid?.Children.Remove(SelectedPiece);
            grid.Children.Add(SelectedPiece);
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
            image.Name = isPiece ? "Take" : "Move";
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

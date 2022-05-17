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
            { "Silver", new[] {
                new Point(0, 1),  // forward
                new Point(1, 1),  // forward-right
                new Point(-1, 1),  // forward-left
                new Point(1, -1),  // backward-right
                new Point(-1, -1),  // backward-left
            } }
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
                        GetTurnSelect()
                    );
                else
                    ((Grid)pieces[((byte)index) - 5]).Children.Add(
                        GetTurnSelect()
                    );
            SelectedGrid = grid;
            SelectedPiece = clicked;
            // var imageContainer = ((Grid)pieces[index]).Children;
            // imageContainer.Add(
            //     GetShogiPiece("resources/king.png", clicked.Name.Contains("Bot"))
            // );
        }

        private Image GetShogiPiece(string path, bool bot = false, string name = "")
        {
            var image = new Image();
            image.Width = 60;
            image.Height = 60;
            var source = new BitmapImage(new Uri("pack://application:,,/" + path));
            image.Source = source;
            image.RenderTransformOrigin = new Point(0.5, 0.5);
            image.Cursor = Cursors.Hand;
            image.MouseDown += BoardPieceClick;
            image.Name = name;
            if (bot)
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

        private Image GetTurnSelect()
        {
            var image = new Image();
            image.Width = 60;
            image.Height = 60;
            var source = new BitmapImage(new Uri("pack://application:,,/resources/select.png"));
            image.Source = source;
            image.Cursor = Cursors.Hand;
            image.MouseDown += BoardTurnClick;
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

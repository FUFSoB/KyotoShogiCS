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
        }

        static Image GetShogiPiece(string path)
        {
            var picture = new Image();
            picture.Width = 60;
            picture.Height = 60;
            var source = new BitmapImage(new Uri("pack://application:,,/" + path));
            picture.Source = source;
            return picture;
        }
    }
}

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

        private ShogiBoard shogiBoard;

        private Grid? selectedGrid = null;
        private Piece? selectedPiece = null;
        private List<Grid>? placedMovements = null;
        private bool isHand = false;

        public Game()
        {
            InitializeComponent();
            shogiBoard = ShogiBoard.KyotoShogi(
                (Grid)this.FindName("Board"),
                (Grid)this.FindName("bot_hand"),
                (Grid)this.FindName("player_hand"),
                ModifyPiece
            );
            shogiBoard.Render();
        }

        public Piece ModifyPiece(Piece piece)
        {
            var promoted = (
                (CheckBox)this.FindName("promote_on_drop")
            ).IsChecked ?? false;
            piece.RevertPromotion();
            if (promoted)
                piece.Promote();
            return piece;
        }

        private void HandPromoteClick(object sender, RoutedEventArgs e)
        {
            var clicked = (CheckBox)sender;

            foreach (var hand in new[] { shogiBoard.PlayerHand, shogiBoard.BotHand })
            {
                foreach (var piece in hand)
                    if (clicked.IsChecked ?? false)
                        piece.Promote();
                    else
                        piece.RevertPromotion();
                hand.Render();
            }
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

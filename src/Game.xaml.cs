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
        Dictionary<string, string> promotionsToHand = new Dictionary<string, string>
        {
            { "pawn", "rook" },
            { "rook", "rook" },
            { "silver", "bishop" },
            { "bishop", "bishop" },
            { "gold", "knight" },
            { "knight", "knight" },
            { "tokin", "lance" },
            { "lance", "lance" },
        };

        Dictionary<string, string> noPromotionsToHand = new Dictionary<string, string>
        {
            { "pawn", "pawn" },
            { "rook", "pawn" },
            { "silver", "silver" },
            { "bishop", "silver" },
            { "gold", "gold" },
            { "knight", "gold" },
            { "tokin", "tokin" },
            { "lance", "tokin" },
        };

        private ShogiBoard shogiBoard;

        private Grid? selectedGrid = null;
        private Piece? selectedPiece = null;
        private List<Grid>? placedMovements = null;
        private bool isHand = false;

        public Game()
        {
            InitializeComponent();
            shogiBoard = ShogiBoard.KyotoShogi(
                (Grid)this.FindName("Board")
            );
            shogiBoard.Render();
        }

        private List<int> CalculateMovements(
            int index,
            UIElementCollection boardPieces,
            string piece,
            bool isBot = false
        )
        {
            var final = new List<int> {};
            var movements = (Movements?)typeof(Movements)?
                .GetMethod(piece.Capitalize())?
                .Invoke(this, null);
            var current = new Vector(index % 5, index / 5);
            if (movements != null)
                foreach (var movement in movements.Calculate((5, 5), current, new List<Vector>()))
                {
                    var movementIndex = (int)(movement.X + 5 * movement.Y);
                    var grid = ((Grid)boardPieces[movementIndex]).Children;
                    if (
                        grid.Count < 2
                        || ((Piece)grid[grid.Count - 1]).Name.Contains("bot") != isBot
                    )
                        final.Add(movementIndex);
                }
            return final;
        }

        private void BoardPieceClick(object sender, RoutedEventArgs e)
        {
            var clicked = (Piece)sender;
            var grid = (Grid)clicked.Parent;
            var board = (Grid)grid.Parent;
            var pieces = board.Children;
            var index = pieces.IndexOf(grid);
            isHand = false;

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

        private void HandPieceClick(object sender, RoutedEventArgs e)
        {
            var clicked = (Piece)sender;
            var grid = (Grid)clicked.Parent;
            var board = (Grid)this.FindName("Board");
            var pieces = board.Children;
            var index = pieces.IndexOf(grid);
            isHand = true;

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

            foreach (Grid gridToPlace in pieces)
            {
                if (gridToPlace.Children.Count == 1)
                {
                    gridToPlace.Children.Add(
                        GetTurnSelect(
                            false, grid.Name.Contains("bot")
                        )
                    );
                    placedMovements.Add(gridToPlace);
                }
            }
            selectedGrid = grid;
            selectedPiece = clicked;
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

            if (clicked.Name == "take")
            {
                var takenPiece = (Piece)grid.Children[grid.Children.Count - 1];
                var toBot = !takenPiece.Name.Contains("bot");
                grid.Children.Remove(takenPiece);

                if (takenPiece.Name.Contains("king"))
                {}
                else
                {
                    var promoted = (
                        (CheckBox)this.FindName("promote_on_drop")
                    ).IsChecked ?? false;
                    var pieceHandName = (
                        promoted ? promotionsToHand : noPromotionsToHand
                    )[
                        takenPiece.Name.Replace("bot_", "")
                    ];
                    var pieceInHand = (Grid)this.FindName(
                        (toBot ? "bot" : "player")
                        + "_hand_" + noPromotionsToHand[pieceHandName]
                    );
                    pieceInHand?.Children.Add(
                        takenPiece.Change(pieceHandName, toBot).RemoveAction(BoardPieceClick).AddAction(HandPieceClick)
                    );
                }
            }

            if (selectedPiece != null && selectedGrid != null)
            {
                selectedGrid.Children.Remove(selectedPiece);
                var name = selectedPiece.Name.Replace("bot_", "");
                if (!selectedPiece.Name.Contains("king"))
                {
                    grid.Children.Add(selectedPiece.Change(
                        isHand ? name : shogiBoard.Promotions[name],
                        selectedPiece.Name.Contains("bot")
                    ));
                    if (isHand)
                        selectedPiece.RemoveAction(HandPieceClick).AddAction(BoardPieceClick);
                }
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

        private void HandPromoteClick(object sender, RoutedEventArgs e)
        {
            var clicked = (CheckBox)sender;

            foreach (var i in new[] { "player", "bot" })
            foreach (Grid grid in ((Grid)this.FindName($"{i}_hand")).Children)
                if (grid.Children.Count > 1)
                {
                    var count = grid.Children.Count - 1;
                    var name = grid.Name.Replace($"{i}_hand_", "");

                    var dict = (clicked.IsChecked ?? false) ? promotionsToHand : noPromotionsToHand;

                    var list = new List<Piece>();
                    while (grid.Children.Count > 1)
                    {
                        list.Add((Piece)grid.Children[grid.Children.Count - 1]);
                        grid.Children.RemoveAt(grid.Children.Count - 1);
                    }

                    foreach (var piece in list)
                        grid.Children.Add(piece.Change(dict[name], i == "bot"));
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

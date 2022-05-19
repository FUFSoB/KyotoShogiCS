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
using System.Reflection;

namespace game
{
    class ShogiHand : IEnumerable<Piece?>
    {
        List<Piece?> hand;

        public IEnumerator<Piece?> GetEnumerator()
        {
            foreach (var piece in hand)
                yield return piece;
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }
    }

    class ShogiBoard : IEnumerable<Piece?>
    {
        List<List<Piece?>> board;
        public Dictionary<string, string> Promotions { get; private set; }
        public (int x, int y) Size { get; private set; }

        Grid? boardGUI;

        Piece? selectedPiece;
        List<Piece>? placedMovements;
        bool isHand;

        public Piece? this[double x, double y]
        {
            get => board[(int)x][(int)y];
            set => board[(int)x][(int)y] = value;
        }

        public IEnumerable<Vector> GetPiecePositions()
        {
            foreach (var piece in this)
                if (piece != null)
                    yield return piece.Position;
        }

        public IEnumerator<Piece?> GetEnumerator()
        {
            foreach (var list in board)
                foreach (var piece in list)
                    yield return piece;
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }

        public ShogiBoard(
            List<List<Piece?>> list,
            Grid? boardGUI = null
        )
        {
            board = list;
            Size = (list.Count(), list[0].Count());
            Promotions = new Dictionary<string, string>();
            this.boardGUI = boardGUI;
            selectedPiece = null;
            placedMovements = null;
            isHand = false;
        }

        public static ShogiBoard FromSize(
            int x, int y,
            Grid? boardGUI = null,
            MouseButtonEventHandler? action = null
        )
        {
            var list = new List<List<Piece?>>();
            for (var i = 0; i < x; ++i)
            {
                var inner = new List<Piece?>();
                for (var j = 0; j < y; ++j)
                    inner.Add(null);
                list.Add(inner);
            }
            return new ShogiBoard(list, boardGUI);
        }

        public static ShogiBoard KyotoShogi(Grid? boardGUI = null)
        {
            var board = FromSize(5, 5, boardGUI);

            board[0, 0] = new Piece("pawn", true).SetPosition(0, 0).SetAction(board.PieceClick);
            board[1, 0] = new Piece("gold", true).SetPosition(1, 0).SetAction(board.PieceClick);
            board[2, 0] = new Piece("king", true).SetPosition(2, 0).SetAction(board.PieceClick);
            board[3, 0] = new Piece("silver", true).SetPosition(3, 0).SetAction(board.PieceClick);
            board[4, 0] = new Piece("tokin", true).SetPosition(4, 0).SetAction(board.PieceClick);

            board[0, 4] = new Piece("tokin", false).SetPosition(0, 4).SetAction(board.PieceClick);
            board[1, 4] = new Piece("silver", false).SetPosition(1, 4).SetAction(board.PieceClick);
            board[2, 4] = new Piece("king", false, imageName: "precious_king").SetPosition(2, 4).SetAction(board.PieceClick);
            board[3, 4] = new Piece("gold", false).SetPosition(3, 4).SetAction(board.PieceClick);
            board[4, 4] = new Piece("pawn", false).SetPosition(4, 4).SetAction(board.PieceClick);

            board.Promotions["pawn"] = "rook";
            board.Promotions["rook"] = "pawn";
            board.Promotions["silver"] = "bishop";
            board.Promotions["bishop"] = "silver";
            board.Promotions["gold"] = "knight";
            board.Promotions["knight"] = "gold";
            board.Promotions["tokin"] = "lance";
            board.Promotions["lance"] = "tokin";

            return board;
        }

        public void Render()
        {
            if (boardGUI == null)
                return;
            var pieces = boardGUI.Children;

            for (var index = 0; index < Size.x * Size.y; ++index)
            {
                var children = ((Grid)pieces[index]).Children;
                while (children.Count > 1)
                    children.RemoveAt(children.Count - 1);
            }

            for (var index = 0; index < Size.x * Size.y; ++index)
            {
                var children = ((Grid)pieces[index]).Children;
                (int x, int y) = (index % 5, index / 5);
                var piece = this[x, y];
                if (piece != null)
                {
                    Piece? subPiece = piece.SubPiece;
                    if (subPiece != null)
                        children.Add(subPiece);
                    children.Add(piece);
                }
            }
        }

        private void PieceClick(object sender, RoutedEventArgs e)
        {
            var piece = (Piece)sender;

            if (placedMovements != null)
                foreach (var pointer in placedMovements)
                    this[pointer.Position.X, pointer.Position.Y] = pointer.SubPiece;

            if (selectedPiece == piece)
            {
                selectedPiece = null;
                placedMovements = null;
                Render();
                return;
            }

            placedMovements = new List<Piece>();

            foreach (var pointer in CalculateMovements(piece))
            {
                this[pointer.Position.X, pointer.Position.Y] = pointer;
                placedMovements.Add(pointer);
            }

            selectedPiece = piece;
            Render();
        }

        private void TurnClick(object sender, RoutedEventArgs e)
        {
            var turn = (Piece)sender;

            if (placedMovements != null)
                foreach (var pointer in placedMovements)
                    this[pointer.Position.X, pointer.Position.Y] = pointer.SubPiece;

            if (turn.Name == "take" && turn.SubPiece != null)
            {
                var piece = turn.SubPiece;
            }

            if (selectedPiece != null)
            {
                this[selectedPiece.Position.X, selectedPiece.Position.Y] = null;
                this[turn.Position.X, turn.Position.Y] = selectedPiece.SetPosition(turn.Position);

                if (selectedPiece.Name != "king")
                    selectedPiece.Change(Promotions[selectedPiece.Name]);

                selectedPiece = null;
                placedMovements = null;
            }
            Render();
        }

        private IEnumerable<Piece> CalculateMovements(Piece piece)
        {
            var movements = piece.Movements;
            var position = piece.Position;
            Piece? take;
            foreach (var move in movements.Calculate(Size, position, GetPiecePositions(), piece.IsBot))
                if ((take = this[move.X, move.Y]) == null)
                    yield return new Piece(
                        "move", piece.IsBot, Movements.None(), "select"
                    ).SetPosition(move.X, move.Y).SetAction(TurnClick);
                else if (take.IsBot != piece.IsBot)
                    yield return new Piece(
                        "take", take.IsBot, Movements.None(), "select_piece"
                    )
                        .SetPosition(move.X, move.Y)
                        .SetAction(TurnClick)
                        .SetSubPiece(take);
        }
    }
}

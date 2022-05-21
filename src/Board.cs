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
    public class ShogiHand : IEnumerable<Piece>
    {
        ShogiBoard? board;
        List<Piece> hand;
        bool isBot;

        Grid? handGUI;

        public delegate Piece ModifyPieceMethod(Piece piece);
        ModifyPieceMethod ModifyPiece;

        public IEnumerator<Piece> GetEnumerator()
        {
            foreach (var piece in hand)
                yield return piece;
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }

        public ShogiHand(
            bool isBot,
            ShogiBoard? board = null,
            Grid? handGUI = null,
            ModifyPieceMethod? modifyPiece = null
        )
        {
            this.board = board;
            hand = new List<Piece>();
            this.isBot = isBot;
            this.handGUI = handGUI;
            this.ModifyPiece = modifyPiece ?? ((piece) => piece);
        }

        public ShogiHand SetGUI(Grid handGUI)
        {
            this.handGUI = handGUI;
            return this;
        }
        public ShogiHand SetBoard(ShogiBoard board)
        {
            this.board = board;
            return this;
        }

        public void Add(Piece piece) => hand.Add(ModifyPiece(piece).Change(isBot: isBot));
        public void Remove(Piece piece) => hand.Remove(piece);

        public void Render()
        {
            if (handGUI == null)
                return;

            var pieceCells = handGUI.Children;
            foreach (Grid cell in pieceCells)
            {
                var children = cell.Children;
                while (children.Count > 1)
                    children.RemoveAt(children.Count - 1);
            }

            foreach (var piece in hand)
                foreach (Grid cell in pieceCells)
                    if (cell.Name.Contains(piece.Name))
                        cell.Children.Add(piece.Change(isBot: isBot));
        }
    }

    public class ShogiBoard : IEnumerable<Piece?>
    {
        public List<List<Piece?>> Board { get; private set; }
        public Dictionary<string, string> Promotions { get; private set; }
        public Dictionary<string, string> ReversePromotions { get; private set; }
        public (int x, int y) Size { get; private set; }

        Grid? boardGUI;

        Piece? selectedPiece;
        List<Piece>? placedMovements;

        public ShogiHand BotHand { get; private set; }
        public ShogiHand PlayerHand { get; private set; }

        Bot bot;
        bool isBotTurn = false;

        public Piece? this[double x, double y]
        {
            get => Board[(int)x][(int)y];
            set => Board[(int)x][(int)y] = value;
        }

        public IEnumerable<Vector> GetPiecePositions()
        {
            foreach (var piece in this)
                if (piece != null)
                    yield return piece.Position;
        }

        public IEnumerator<Piece?> GetEnumerator()
        {
            foreach (var list in Board)
                foreach (var piece in list)
                    yield return piece;
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }

        public ShogiBoard(
            List<List<Piece?>> list,
            Grid? boardGUI = null,
            ShogiHand? botHand = null,
            ShogiHand? playerHand = null
        )
        {
            Board = list;
            Size = (list.Count(), list[0].Count());
            Promotions = new Dictionary<string, string>();
            ReversePromotions = new Dictionary<string, string>();
            this.boardGUI = boardGUI;
            selectedPiece = null;
            placedMovements = null;
            BotHand = botHand ?? new ShogiHand(true, this, null);
            PlayerHand = playerHand ?? new ShogiHand(false, this, null);
            bot = new Bot(this, BotHand, PlayerHand);
        }

        public static ShogiBoard FromSize(
            int x, int y,
            Grid? boardGUI = null,
            ShogiHand? botHand = null,
            ShogiHand? playerHand = null
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
            var result = new ShogiBoard(list, boardGUI, botHand, playerHand);
            if (botHand != null)
                botHand.SetBoard(result);
            if (playerHand != null)
                playerHand.SetBoard(result);
            return result;
        }

        public static ShogiBoard KyotoShogi(
            Grid? boardGUI = null,
            Grid? botHandGUI = null,
            Grid? playerHandGUI = null,
            ShogiHand.ModifyPieceMethod? modifyPiece = null
        )
        {
            var board = FromSize(
                5, 5,
                boardGUI,
                new ShogiHand(
                    true,
                    handGUI: botHandGUI,
                    modifyPiece: modifyPiece
                ),
                new ShogiHand(
                    false,
                    handGUI: playerHandGUI,
                    modifyPiece: modifyPiece
                )
            );

            board[0, 0] = new Piece("pawn", true, board).SetPosition(0, 0).SetAction(board.PieceClick);
            board[1, 0] = new Piece("gold", true, board).SetPosition(1, 0).SetAction(board.PieceClick);
            board[2, 0] = new Piece("king", true, board).SetPosition(2, 0).SetAction(board.PieceClick);
            board[3, 0] = new Piece("silver", true, board).SetPosition(3, 0).SetAction(board.PieceClick);
            board[4, 0] = new Piece("tokin", true, board).SetPosition(4, 0).SetAction(board.PieceClick);

            board[0, 4] = new Piece("tokin", false, board).SetPosition(0, 4).SetAction(board.PieceClick);
            board[1, 4] = new Piece("silver", false, board).SetPosition(1, 4).SetAction(board.PieceClick);
            board[2, 4] = new Piece("king", false, board, imageName: "precious_king").SetPosition(2, 4).SetAction(board.PieceClick);
            board[3, 4] = new Piece("gold", false, board).SetPosition(3, 4).SetAction(board.PieceClick);
            board[4, 4] = new Piece("pawn", false, board).SetPosition(4, 4).SetAction(board.PieceClick);

            board.Promotions["pawn"] = "rook";
            board.Promotions["rook"] = "pawn";
            board.Promotions["silver"] = "bishop";
            board.Promotions["bishop"] = "silver";
            board.Promotions["gold"] = "knight";
            board.Promotions["knight"] = "gold";
            board.Promotions["tokin"] = "lance";
            board.Promotions["lance"] = "tokin";

            board.ReversePromotions["rook"] = "pawn";
            board.ReversePromotions["bishop"] = "silver";
            board.ReversePromotions["knight"] = "gold";
            board.ReversePromotions["lance"] = "tokin";

            return board;
        }

        public void Render()
        {
            if (boardGUI == null)
                return;
            var pieceCells = boardGUI.Children;

            for (var index = 0; index < Size.x * Size.y; ++index)
            {
                var children = ((Grid)pieceCells[index]).Children;
                while (children.Count > 1)
                    children.RemoveAt(children.Count - 1);
            }

            for (var index = 0; index < Size.x * Size.y; ++index)
            {
                var children = ((Grid)pieceCells[index]).Children;
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

            BotHand.Render();
            PlayerHand.Render();
        }

        private void PieceClick(object sender, RoutedEventArgs e)
        {
            var piece = (Piece)sender;
            if (piece.IsBot != isBotTurn)
                return;

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

        public void SelectPiece(double x, double y)
            => selectedPiece = this[x, y];

        public void SelectPiece(Piece piece)
            => selectedPiece = piece;

        private void TurnClick(object sender, RoutedEventArgs e)
        {
            var turn = (Piece)sender;

            if (placedMovements != null)
                foreach (var pointer in placedMovements)
                    this[pointer.Position.X, pointer.Position.Y] = pointer.SubPiece;

            if (selectedPiece != null)
            {
                if (turn.Name == "take" && turn.SubPiece != null)
                {
                    var piece = turn.SubPiece;
                    (selectedPiece.IsBot ? BotHand : PlayerHand).Add(piece.SetAction(HandClick));
                }

                this[selectedPiece.Position.X, selectedPiece.Position.Y] = null;
                this[turn.Position.X, turn.Position.Y] = selectedPiece.SetPosition(turn.Position);

                if (selectedPiece.Name != "king")
                    selectedPiece.Promote();

                selectedPiece = null;
                placedMovements = null;
            }
            isBotTurn = !isBotTurn;
            Render();

            if (isBotTurn)
                bot.DoBestMove();
        }

        private IEnumerable<Piece> CalculateMovements(Piece piece)
        {
            var movements = piece.Movements;
            var position = piece.Position;
            foreach (var move in movements.Calculate(Size, position, GetPiecePositions(), piece.IsBot))
            {
                var movement = GetMovementPiece(piece.IsBot, move);
                if (movement != null)
                    yield return movement;
            }
        }

        public Piece? GetMovementPiece(bool isBotMove, Vector move, bool fromHand = false)
        {
            Piece? take;
            if (fromHand)
                return new Piece(
                    "move", isBotMove, this, Movements.None(), "select"
                ).SetPosition(move).SetAction(HandTurnClick);
            else if ((take = this[move.X, move.Y]) == null)
                return new Piece(
                    "move", isBotMove, this, Movements.None(), "select"
                ).SetPosition(move).SetAction(TurnClick);
            else if (take.IsBot != isBotMove)
                return new Piece(
                    "take", take.IsBot, this, Movements.None(), "select_piece"
                )
                    .SetPosition(move)
                    .SetAction(TurnClick)
                    .SetSubPiece(take);
            else
                return null;
        }

        private void HandClick(object sender, RoutedEventArgs e)
        {
            var piece = (Piece)sender;
            if (piece.IsBot != isBotTurn)
                return;

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

            for (var index = 0; index < Size.x * Size.y; ++index)
            {
                (int x, int y) = (index % 5, index / 5);
                if (this[x, y] == null)
                {
                    var pointer = GetMovementPiece(piece.IsBot, new Vector(x, y), true);
                    if (pointer != null)
                    {
                        this[x, y] = pointer;
                        placedMovements.Add(pointer);
                    }
                }
            }

            selectedPiece = piece;
            Render();
        }

        private void HandTurnClick(object sender, RoutedEventArgs e)
        {
            var turn = (Piece)sender;

            if (placedMovements != null)
                foreach (var pointer in placedMovements)
                    this[pointer.Position.X, pointer.Position.Y] = pointer.SubPiece;

            if (selectedPiece != null)
            {
                (turn.IsBot ? BotHand : PlayerHand).Remove(selectedPiece);
                this[turn.Position.X, turn.Position.Y] = selectedPiece.SetPosition(turn.Position).SetAction(PieceClick);

                selectedPiece = null;
                placedMovements = null;
            }
            isBotTurn = !isBotTurn;
            (turn.IsBot ? BotHand : PlayerHand).Render();
            Render();

            if (isBotTurn)
                bot.DoBestMove();
        }
    }
}

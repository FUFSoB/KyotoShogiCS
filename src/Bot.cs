using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;

namespace game
{
    class Result
    {
        public List<List<Piece?>> Pieces { get; private set; }
        public List<Piece> BotHand { get; private set; }
        public List<Piece> PlayerHand { get; private set; }

        public Piece Piece { get; private set; }
        public Vector Move { get; private set; }
        public bool IsBotMove { get; private set; }
        public bool IsHandMove { get; private set; }
        public bool PromoteFromHand { get; private set; }

        public float Score { get; set; }
        public Result? Parent { get; private set; }
        public string? Mate { get; set; }

        public Result(
            List<List<Piece?>> pieces,
            List<Piece> botHand,
            List<Piece> playerHand,
            Piece piece,
            Vector move,
            bool isBotMove,
            bool isHandMove,
            bool promoteFromHand,
            float score,
            Result? parent = null,
            string? mate = null
        )
        {
            Pieces = pieces;
            BotHand = botHand;
            PlayerHand = playerHand;
            Piece = piece;
            Move = move;
            IsBotMove = isBotMove;
            IsHandMove = isHandMove;
            PromoteFromHand = promoteFromHand;
            Score = score;
            Parent = parent;
            Mate = mate;
        }

        public void SetParent(Result? parent = null)
        {
            Parent = parent;
        }
    }
    public class Bot
    {
        ShogiBoard board;
        ShogiHand hand, playerHand;
        public int Level = 7;

        (float score, Piece piece, Vector move, bool promoteFromHand) emptyBestMove;

        public Bot(ShogiBoard board, ShogiHand hand, ShogiHand playerHand)
        {
            this.board = board;
            this.hand = hand;
            this.playerHand = playerHand;
            emptyBestMove = (0, new Piece(board), new Vector(), false);
        }

        public void DoBestMove()
        {
            var pieces = new List<List<Piece?>>();
            var botHand = new List<Piece>();
            var playerHand = new List<Piece>();
            var originalPieces = new Dictionary<Piece, Piece>();

            for (var x = 0; x < board.Size.x; ++x)
            {
                var inner = new List<Piece?>();
                for (var y = 0; y < board.Size.y; ++y)
                {
                    var piece = board[x, y];
                    var copy = piece?.Copy();
                    inner.Add(copy);
                    if ((piece?.IsBot ?? false) && piece != null && copy != null)
                        originalPieces.Add(copy, piece);
                }
                pieces.Add(inner);
            }
            foreach (var piece in board.BotHand)
            {
                var copy = piece.Copy();
                botHand.Add(copy);
                originalPieces.Add(copy, piece);
            }
            foreach (var piece in board.PlayerHand)
                playerHand.Add(piece.Copy());

            var result = CalculateBestMove(pieces, botHand, playerHand).Parent;
            if (result == null)
                return;

            var originalPiece = originalPieces[result.Piece];
            if (result.PromoteFromHand)
                originalPiece.RevertPromotion().Promote();

            board.SelectPiece(originalPiece);
            var movePiece = board.GetMovementPiece(true, result.Move, result.Piece.Position.X == -1);
            movePiece?.InvokeActions();
        }

        Result CalculateBestMove(
            List<List<Piece?>> pieces,
            List<Piece> botHand,
            List<Piece> playerHand
        )
        {
            var firstMoves = new Dictionary<Result, IEnumerable<Result>>();
            foreach (var result in Maximize(pieces, botHand, playerHand, true, null))
                firstMoves[result] = new List<Result> { result };

            for (var i = 1; i < Level; ++i)
            {
                foreach (var (first, results) in firstMoves)
                {
                    var current = new List<Result>();
                    var mul = i % 2 == 0 ? 1 : -1;
                    foreach (var result in results.OrderBy((x) => -x.Score).Take(3))
                        current.AddRange(Maximize(result.Pieces, result.BotHand, result.PlayerHand, mul == 1, result.Parent));
                    firstMoves[first] = current.Where((x) => x.Mate != "player");
                }
            }
            var final = firstMoves.SelectMany((x) => x.Value);
            var finalMax = final.Where((x) => x.Mate == "bot").ToArray();
            if (finalMax.Length == 0)
            {
                var max = final.Where((x) => x.Mate != "player").Select((x) => x.Score).Max();
                finalMax = final.Where((x) => x.Score == max).ToArray();
            }
            return finalMax[(new Random()).Next(finalMax.Length)];
        }

        IEnumerable<Result> Maximize(
            List<List<Piece?>> pieces,
            List<Piece> botHand,
            List<Piece> playerHand,
            bool isBotMove,
            Result? parent
        )
        {
            var (
                possibleHandPlacements,
                availablePieces,
                possiblePieceMovements
            ) = GetDataOnMovements(pieces, botHand, playerHand, isBotMove);

            foreach (var (piece, movements) in possiblePieceMovements)
            foreach (var move in movements)
            {
                bool isHandMove = movements == possibleHandPlacements;
                for (var i = 1 + (isHandMove ? 1 : 0); i > 0; --i)
                {
                    var promoteFromHand = i % 2 == 0;
                    var (newPieces, newBotHand, newPlayerHand) = DoCalculatedMove(
                        pieces, botHand, playerHand, piece, move, isBotMove, isHandMove, promoteFromHand
                    );

                    var (score, mate) = ScoreTheBoard(newPieces, newBotHand, newPlayerHand, true);
                    var result = new Result(newPieces, newBotHand, newPlayerHand, piece, move, isBotMove, isHandMove, promoteFromHand, score, mate: mate);
                    result.SetParent(parent ?? result);
                    yield return result;
                }
            }
        }

        (List<List<Piece?>>, List<Piece>, List<Piece>) DoCalculatedMove(
            List<List<Piece?>> pieces,
            List<Piece> botHand,
            List<Piece> playerHand,
            Piece piece,
            Vector move,
            bool isBotMove,
            bool isHandMove,
            bool promoteFromHand = false
        )
        {
            var newPieces = new List<List<Piece?>>();
            foreach (var list in pieces)
                newPieces.Add(list.ToList());
            var newBotHand = botHand.ToList();
            var newPlayerHand = playerHand.ToList();

            var hand = isBotMove ? botHand : playerHand;
            var realHand = isBotMove ? this.hand : this.playerHand;

            newPieces[(int)move.X][(int)move.Y] = piece.Copy().SetPosition(move);

            if (isHandMove)
            {
                hand.Remove(piece);
                if (promoteFromHand)
                    newPieces[(int)move.X][(int)move.Y]?.Promote();
            }
            else
            {
                newPieces[(int)piece.Position.X][(int)piece.Position.Y] = null;
                if (piece.Name != "king")
                    hand.Add(realHand.ModifyPiece(piece).Change(isBot: isBotMove));
            }

            return (newPieces, newBotHand, newPlayerHand);
        }

        (float, string?) ScoreTheBoard(
            List<List<Piece?>> pieces,
            List<Piece> botHand,
            List<Piece> playerHand,
            bool isBotMove
        )
        {
            var countPieces = new Dictionary<(string, bool), int>();
            foreach (var key in board.PieceCost.Keys)
            {
                countPieces[(key, true)] = 0;
                countPieces[(key, false)] = 0;
            }

            float score = 0;

            for (int x = 0; x < board.Size.x; ++x)
            for (int y = 0; y < board.Size.y; ++y)
            {
                var piece = pieces[x][y];

                if (piece == null)
                    continue;

                countPieces[(piece.Name, piece.IsBot)] += 1;
                score += board.GetPieceCost(piece.Name, x, y) * (isBotMove == piece.IsBot ? 1 : -1);
            }

            return (
                score,
                (
                    countPieces[("king", true)] == 0 ? "player"
                    : countPieces[("king", false)] == 0 ? "bot"
                    : null
                )
            );
        }

        (
            List<Vector>, List<Piece>, Dictionary<Piece, List<Vector>>
        ) GetDataOnMovements(
            List<List<Piece?>> pieces,
            List<Piece> botHand,
            List<Piece> playerHand,
            bool isBotMove
        )
        {
            var possibleHandPlacements = new List<Vector>();
            var availablePieces = new List<Piece>();
            var possiblePieceMovements = new Dictionary<Piece, List<Vector>>();

            for (var x = 0; x < pieces.Count; ++x)
                for (var y = 0; y < pieces[0].Count; ++y)
                {
                    var piece = pieces[x][y];
                    if (piece == null)
                        possibleHandPlacements.Add(new Vector(x, y));
                    else
                        availablePieces.Add(piece);
                }

            foreach (var inner in pieces)
                foreach (var piece in inner)
                    if (piece != null && piece.IsBot == isBotMove)
                        possiblePieceMovements[piece] = piece.Movements.Calculate(
                            (pieces.Count, pieces[0].Count),
                            piece.Position,
                            availablePieces,
                            isBotMove
                        ).ToList();

            foreach (var piece in botHand)
                possiblePieceMovements[piece] = possibleHandPlacements;

            return (
                possibleHandPlacements,
                availablePieces,
                possiblePieceMovements
            );
        }
    }
}

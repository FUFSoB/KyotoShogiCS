using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;

namespace game
{
    public class Bot
    {
        ShogiBoard board;
        ShogiHand hand, playerHand;
        public int Level = 5;

        public Bot(ShogiBoard board, ShogiHand hand, ShogiHand playerHand)
        {
            this.board = board;
            this.hand = hand;
            this.playerHand = playerHand;
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

            var (selectedPiece, move, promoteFromHand) = CalculateBestMove(pieces, botHand, playerHand);

            var originalPiece = originalPieces[selectedPiece];
            if (promoteFromHand)
                originalPiece.RevertPromotion().Promote();

            board.SelectPiece(originalPiece);
            var movePiece = board.GetMovementPiece(true, move, selectedPiece.Position.X == -1);
            movePiece?.InvokeActions();
        }

        (
            List<Vector>, List<Vector>, Dictionary<Piece, List<Vector>>
        ) GetDataOnMovements(
            List<List<Piece?>> pieces,
            List<Piece> botHand,
            List<Piece> playerHand,
            bool isBotMove
        )
        {
            var possibleHandPlacements = new List<Vector>();
            var availablePieces = new List<Vector>();
            var possiblePieceMovements = new Dictionary<Piece, List<Vector>>();

            for (var x = 0; x < pieces.Count; ++x)
                for (var y = 0; y < pieces[0].Count; ++y)
                    if (pieces[x][y] == null)
                        possibleHandPlacements.Add(new Vector(x, y));
                    else
                        availablePieces.Add(new Vector(x, y));

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

        (List<List<Piece?>>, List<Piece>, List<Piece>) DoCalculatedMove(
            List<List<Piece?>> pieces,
            List<Piece> botHand,
            List<Piece> playerHand,
            Piece piece,
            (int x, int y) move,
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

            newPieces[move.x][move.y] = piece.Copy().SetPosition(move.x, move.y);

            if (isHandMove)
            {
                hand.Remove(piece);
                if (promoteFromHand)
                    newPieces[move.x][move.y]?.Promote();
            }
            else
            {
                newPieces[(int)piece.Position.X][(int)piece.Position.Y] = null;
                if (piece.Name != "king")
                    hand.Add(realHand.ModifyPiece(piece).Change(isBot: isBotMove));
            }

            return (newPieces, newBotHand, newPlayerHand);
        }

        int CalculateMove(
            List<List<Piece?>> pieces,
            List<Piece> botHand,
            List<Piece> playerHand,
            int depth,
            int score,
            bool isBotMove,
            Dictionary<int, List<(Piece, Vector, bool)>>? result = null
        )
        {
            if (depth == 0)
                return score;
            var (
                possibleHandPlacements,
                availablePieces,
                possiblePieceMovements
            ) = GetDataOnMovements(pieces, botHand, playerHand, isBotMove);

            var maxValue = int.MinValue * (isBotMove ? 1 : -1);
            var best = new List<(int, Piece, Vector)>();
            foreach (var key in possiblePieceMovements.Keys)
            foreach (var value in possiblePieceMovements[key])
            {
                var (x, y) = ((int)value.X, (int)value.Y);
                var gotPiece = pieces[x][y];
                int got = score;
                if (gotPiece != null)
                    if (gotPiece.IsBot == isBotMove)
                        continue;
                    else
                        got += board.PieceCost[gotPiece.Name] * (isBotMove ? 1 : -1);
                else
                    got += isBotMove ? 1 : -1;

                maxValue = got;
                best.Add((got, key, value));
            }

            foreach (var (got, key, value) in best.OrderBy(x => -x.Item1).Take(3))
            {
                var isHandMove = possiblePieceMovements[key] == possibleHandPlacements;
                var (x, y) = ((int)value.X, (int)value.Y);
                for (var i = 1 + (isHandMove ? 1 : 0); i > 0; --i)
                {
                    var promoteFromHand = i % 2 == 0;
                    var (gotPieces, gotBotHand, gotPlayerHand) = DoCalculatedMove(
                        pieces, botHand, playerHand, key, (x, y), isBotMove, isHandMove, promoteFromHand
                    );
                    var nextGot = CalculateMove(gotPieces, gotBotHand, gotPlayerHand, depth - 1, got, !isBotMove);
                    if (result != null)
                    {
                        result.TryGetValue(nextGot, out List<(Piece, Vector, bool)>? list);
                        if (list == null)
                        {
                            list = new List<(Piece, Vector, bool)>();
                            result[nextGot] = list;
                        }
                        list.Add((key, value, promoteFromHand));
                    }
                }
            }

            if (result == null)
                return maxValue;
            return result.Keys.Max();
        }

        (Piece, Vector, bool) CalculateBestMove(
            List<List<Piece?>> pieces,
            List<Piece> botHand,
            List<Piece> playerHand,
            int depth = 1,
            int score = 0
        )
        {
            var result = new Dictionary<int, List<(Piece, Vector, bool)>>();
            var key = CalculateMove(pieces, botHand, playerHand, Level, 0, true, result);
            var top = result[key];

            var value = top[(new Random()).Next(top.Count)];
            return value;
        }
    }
}

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
        public int Level;

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

            for (var x = 0; x < board.Size.x; ++x)
            {
                var inner = new List<Piece?>();
                for (var y = 0; y < board.Size.y; ++y)
                    inner.Add(board[x, y]?.Copy());
                pieces.Add(inner);
            }
            foreach (var piece in board.BotHand)
                botHand.Add(piece.Copy());
            foreach (var piece in board.PlayerHand)
                playerHand.Add(piece.Copy());

            var (selectedPiece, move) = CalculateBestMove(pieces, botHand, playerHand);

            board.SelectPiece(selectedPiece.Position.X, selectedPiece.Position.Y);
            var movePiece = board.GetMovementPiece(true, move);
            movePiece?.InvokeActions();
        }

        (Piece, Vector) CalculateBestMove(
            List<List<Piece?>> pieces,
            List<Piece> botHand,
            List<Piece> playerHand,
            int depth = 1,
            int score = 0
        )
        {
            var result = new Dictionary<int, List<(Piece, Vector)>>();
            var key = CalculateMove(pieces, botHand, playerHand, 3, 0, true, result);
            var top = result[key];

            var value = top[(new Random()).Next(top.Count)];
            return value;
        }

        int CalculateMove(
            List<List<Piece?>> pieces,
            List<Piece> botHand,
            List<Piece> playerHand,
            int depth,
            int score,
            bool isBotMove,
            Dictionary<int, List<(Piece, Vector)>>? result = null
        )
        {
            if (depth == 0)
                return score;
            var (
                possibleHandPlacements,
                availablePieces,
                possiblePieceMovements
            ) = GetDataOnMovements(pieces, botHand, playerHand, isBotMove);

            var maxValue = int.MinValue;
            foreach (var key in possiblePieceMovements.Keys)
            foreach (var value in possiblePieceMovements[key])
            {
                var isHandMove = possiblePieceMovements[key] == possibleHandPlacements;
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

                if (got > 1000)
                    return got;

                if (got * (isBotMove ? 1 : -1) < maxValue * (isBotMove ? 1 : -1))
                    continue;
                maxValue = got;

                var (gotPieces, gotBotHand, gotPlayerHand) = DoCalculatedMove(
                    pieces, botHand, playerHand, key, (x, y), isBotMove, isHandMove
                );
                got = CalculateMove(gotPieces, gotBotHand, gotPlayerHand, depth - 1, got, !isBotMove);
                if (result != null)
                {
                    result.TryGetValue(got, out List<(Piece, Vector)>? list);
                    if (list == null)
                    {
                        list = new List<(Piece, Vector)>();
                        result[got] = list;
                    }
                    list.Add((key, value));
                }
            }

            if (result == null)
                return maxValue;
            return result.Keys.Max();
        }

        (List<List<Piece?>>, List<Piece>, List<Piece>) DoCalculatedMove(
            List<List<Piece?>> pieces,
            List<Piece> botHand,
            List<Piece> playerHand,
            Piece piece,
            (int x, int y) move,
            bool isBotMove,
            bool isHandMove
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
            }
            else
            {
                newPieces[(int)piece.Position.X][(int)piece.Position.Y] = null;
                if (piece.Name != "king")
                    hand.Add(realHand.ModifyPiece(piece).Change(isBot: isBotMove));
            }

            return (newPieces, newBotHand, newPlayerHand);
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
    }
}

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

            board.SelectPiece(selectedPiece);
            var movePiece = board.GetMovementPiece(true, move);
            movePiece?.InvokeActions();
        }

        (Piece, Vector) CalculateBestMove(List<List<Piece?>> pieces, List<Piece> botHand, List<Piece> playerHand, int depth = 1)
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
                    if (piece != null && piece.IsBot)
                        possiblePieceMovements[piece] = piece.Movements.Calculate(
                            (pieces.Count, pieces[0].Count),
                            piece.Position,
                            availablePieces,
                            piece.IsBot
                        ).ToList();

            foreach (var piece in botHand)
                possiblePieceMovements[piece] = possibleHandPlacements;

            while (true)
            {
                var piecesToMove = possiblePieceMovements.Keys.ToList();
                var rnd = (new Random()).Next(piecesToMove.Count);
                var key = piecesToMove[rnd];
                var movements = possiblePieceMovements[key];
                if (movements.Count == 0)
                    continue;
                var value = movements[(new Random()).Next(movements.Count)];
                return (key, value);
            }
        }
    }
}

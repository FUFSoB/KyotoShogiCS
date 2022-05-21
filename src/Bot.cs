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

        public void DoRandomAvailableTurn()
        {
            var list = new List<Piece>();
            foreach (var p in board)
                if (p != null && p.IsBot)
                    list.Add(p);
            foreach (var p in hand)
                if (p != null && p.IsBot)
                    list.Add(p);

            list[(new Random()).Next(list.Count)].InvokeActions();
            list.Clear();

            foreach (var p in board)
                if (p != null && new[] { "take", "move" }.Contains(p.Name))
                    list.Add(p);

            if (list.Count > 0)
                list[(new Random()).Next(list.Count)].InvokeActions();
            else
                DoRandomAvailableTurn();
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

            board.SelectPiece(selectedPiece.X, selectedPiece.Y);
            var movePiece = board.GetMovementPiece(true, move);
            movePiece?.InvokeActions();
        }

        (Vector, Vector) CalculateBestMove(List<List<Piece?>> pieces, List<Piece> botHand, List<Piece> playerHand)
        {
            return (new Vector(2, 0), new Vector(2, 2));
            // move bot king to center
        }
    }
}

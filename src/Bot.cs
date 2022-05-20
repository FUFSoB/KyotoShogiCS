using System;
using System.Collections.Generic;
using System.Linq;

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
    }
}

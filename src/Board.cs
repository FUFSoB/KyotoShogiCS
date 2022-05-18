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
    class ShogiBoard : IEnumerable<Piece?>
    {
        List<List<Piece?>> board;

        public Piece? this[int x, int y]
        {
            get
            {
                return board[x][y];
            }
            set
            {
                board[x][y] = value;
            }
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

        public ShogiBoard(List<List<Piece?>> list)
        {
            board = list;
        }

        public static ShogiBoard FromSize(int x, int y)
        {
            var list = new List<List<Piece?>>();
            for (var i = 0; i < x; ++i)
            {
                var inner = new List<Piece?>();
                for (var j = 0; j < y; ++j)
                    inner.Add(null);
                list.Add(inner);
            }
            return new ShogiBoard(list);
        }

        public static ShogiBoard KyotoShogi()
        {
            var board = FromSize(5, 5);

            board[0, 0] = new Piece("pawn", true);
            board[0, 1] = new Piece("gold", true);
            board[0, 2] = new Piece("king", true);
            board[0, 3] = new Piece("silver", true);
            board[0, 4] = new Piece("tokin", true);

            board[4, 0] = new Piece("tokin", false);
            board[4, 1] = new Piece("silver", false);
            board[4, 2] = new Piece("king", false, imageName: "precious_king");
            board[4, 3] = new Piece("gold", false);
            board[4, 4] = new Piece("pawn", false);

            return board;
        }

        // public Grid Render()
        // {}
    }
}

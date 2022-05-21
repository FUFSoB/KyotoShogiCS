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
    public class Movements
    {
        HashSet<Vector> movements;

        public Movements() => movements = new HashSet<Vector> {};
        public static Movements None() => new Movements();

        public Movements AddMovement(Vector movement)
        {
            movements.Add(movement);
            return this;
        }
        public Movements AddMovement(double x, double y)
            => AddMovement(new Vector(x, y));

        public Movements Forward() => AddMovement(0, 1);
        public Movements ForwardLeft() => AddMovement(1, 1);
        public Movements ForwardRight() => AddMovement(-1, 1);
        public Movements Advance()
            => Forward().ForwardLeft().ForwardRight();

        public Movements Left() => AddMovement(1, 0);
        public Movements Right() => AddMovement(-1, 0);
        public Movements Side() => Left().Right();

        public Movements Backward() => AddMovement(0, -1);
        public Movements BackwardLeft() => AddMovement(1, -1);
        public Movements BackwardRight() => AddMovement(-1, -1);
        public Movements Retreat()
            => Backward().BackwardLeft().BackwardRight();

        public Movements InfForward()
            => AddMovement(0, double.PositiveInfinity);
        public Movements InfBackward()
            => AddMovement(0, -double.PositiveInfinity);
        public Movements InfLeft()
            => AddMovement(double.PositiveInfinity, 0);
        public Movements InfRight()
            => AddMovement(-double.PositiveInfinity, 0);

        public Movements InfForwardLeft()
            => AddMovement(double.PositiveInfinity, double.PositiveInfinity);
        public Movements InfForwardRight()
            => AddMovement(-double.PositiveInfinity, double.PositiveInfinity);
        public Movements InfBackwardLeft()
            => AddMovement(double.PositiveInfinity, -double.PositiveInfinity);
        public Movements InfBackwardRight()
            => AddMovement(-double.PositiveInfinity, -double.PositiveInfinity);

        // Pieces
        public static Movements Pawn()
            => new Movements().Forward();
        public static Movements Gold()
            => new Movements().Advance().Side().Backward();
        public static Movements Tokin()
            => Gold();
        public static Movements King()
            => Gold().BackwardLeft().BackwardRight();
        public static Movements Silver()
            => new Movements().Advance().BackwardLeft().BackwardRight();
        public static Movements Knight()
            => new Movements().AddMovement(1, 2).AddMovement(-1, 2);
        public static Movements Bishop()
            => new Movements().InfForwardLeft()
            .InfForwardRight()
            .InfBackwardLeft()
            .InfBackwardRight();
        public static Movements Rook()
            => new Movements().InfForward()
            .InfBackward()
            .InfLeft()
            .InfRight();
        public static Movements Lance()
            => new Movements().InfForward();

        public IEnumerable<Vector> Calculate(
            (int x, int y) sizeOfField,
            Vector position,
            IEnumerable<Vector> otherPositions,
            bool isBot = false
        )
        {
            var positions = otherPositions.ToArray();
            foreach (var movement in movements)
            {
                var (x, y) = (movement.X, movement.Y);
                if (double.IsInfinity(x) || double.IsInfinity(y))
                {
                    var infMovement = new Vector(
                        x == 0 ? 0 : x > 0 ? 1 : -1,
                        y == 0 ? 0 : y > 0 ? 1 : -1
                    );
                    var moved = position - infMovement * (isBot ? -1 : 1);
                    while (
                        moved.X >= 0 && moved.X < sizeOfField.x
                        && moved.Y >= 0 && moved.Y < sizeOfField.y
                    )
                    {
                        yield return moved;
                        if (positions.Contains(moved))
                            break;
                        moved = moved - infMovement * (isBot ? -1 : 1);
                    }
                }
                else
                {
                    var moved = position - movement * (isBot ? -1 : 1);
                    if (
                        moved.X >= 0 && moved.X < sizeOfField.x
                        && moved.Y >= 0 && moved.Y < sizeOfField.y
                    )
                        yield return moved;
                }
            }
        }
    }

    public class Piece : Image
    {
        public Movements Movements { get; private set; }
        List<MouseButtonEventHandler> events;
        public bool IsBot { get; private set; }
        public Vector Position { get; private set; }
        public Piece? SubPiece { get; private set; } = null;
        ShogiBoard board;
        string imageName;

        public Piece(
            string name,
            bool isBot,
            ShogiBoard board,
            Movements? movements = null,
            string? imageName = null
        )
        {
            events = new List<MouseButtonEventHandler>();
            Name = name;
            IsBot = isBot;
            this.board = board;
            this.imageName = imageName ?? name;
            var capitalizedName = name.Capitalize();

            if (movements == null)
            {
                var possibleMovements = typeof(Movements)?
                    .GetMethod(capitalizedName)?
                    .Invoke(this, null);
                if (possibleMovements == null)
                    throw new ArgumentException(
                        $"{nameof(name)} is unknown shogi piece, please provide custom movements",
                        nameof(name)
                    );
                else
                    this.Movements = (Movements)possibleMovements;
            }
            else
                this.Movements = movements;

            Width = 60;
            Height = 60;
            Source = new BitmapImage(new Uri(
                "pack://application:,,,/game;component/"
                + $"resources/{this.imageName}.png"
            ));
            RenderTransformOrigin = new Point(0.5, 0.5);
            Cursor = Cursors.Hand;
            if (isBot)
            {
                var transform = new TransformGroup();
                transform.Children.Add(new RotateTransform(180));
                RenderTransform = transform;
            }
        }

        public Piece SetPosition(Vector position)
        {
            Position = position;
            return this;
        }

        public Piece SetPosition(double x, double y)
            => SetPosition(new Vector(x, y));

        public Piece ClearActions()
        {
            foreach (var e in events)
                MouseDown -= e;
            events.Clear();
            return this;
        }

        public Piece SetAction(MouseButtonEventHandler action)
        {
            ClearActions();
            return AddAction(action);
        }

        public Piece AddAction(MouseButtonEventHandler? action)
        {
            if (action != null)
            {
                MouseDown += action;
                events.Add(action);
            }
            return this;
        }

        public Piece RemoveAction(MouseButtonEventHandler? action)
        {
            if (action != null)
            {
                MouseDown -= action;
                events.Remove(action);
            }
            return this;
        }

        public void InvokeActions()
        {
            foreach (var e in events)
                e.Invoke(this, null);
        }

        public Piece SetSubPiece(Piece? piece)
        {
            SubPiece = piece;
            return this;
        }

        public Piece RemoveSubPiece()
        {
            SubPiece = null;
            return this;
        }

        public Piece Change(
            string? name = null,
            bool? isBot = null,
            Movements? movements = null,
            string? imageName = null
        )
        {
            var notNullName = name ?? Name;
            Name = notNullName;
            IsBot = isBot ?? IsBot;
            this.imageName = imageName ?? notNullName;
            var capitalizedName = notNullName.Capitalize();

            if (movements == null)
            {
                var possibleMovements = typeof(Movements)?
                    .GetMethod(capitalizedName)?
                    .Invoke(this, null);
                if (possibleMovements == null)
                    throw new ArgumentException(
                        $"{nameof(name)} is unknown shogi piece, please provide custom movements",
                        nameof(name)
                    );
                else
                    this.Movements = (Movements)possibleMovements;
            }
            else
                this.Movements = movements;

            Source = new BitmapImage(new Uri(
                "pack://application:,,,/game;component/"
                + $"resources/{this.imageName}.png"
            ));
            if (IsBot)
            {
                var transform = new TransformGroup();
                transform.Children.Add(new RotateTransform(180));
                RenderTransform = transform;
            }
            else
                RenderTransform = Transform.Identity;

            return this;
        }

        public Piece Promote() => Change(board.Promotions[Name]);
        public Piece RevertPromotion() => Change(board.ReversePromotions.GetValueOrDefault(Name));

        public Piece Copy()
        {
            var piece = new Piece(Name, IsBot, board, Movements, imageName);
            foreach (var e in events)
                piece.AddAction(e);
            return piece.SetPosition(Position).SetSubPiece(SubPiece);
        }
    }
}

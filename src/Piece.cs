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
    class Movements
    {
        HashSet<Vector> movements;

        public Movements() => movements = new HashSet<Vector> {};

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
            bool isOpposite = false
        )
        {
            foreach (var movement in movements)
            {
                var (x, y) = (movement.X, movement.Y);
                if (double.IsInfinity(x) || double.IsInfinity(y))
                {
                    var infMovement = new Vector(
                        x == 0 ? 0 : x > 0 ? 1 : -1,
                        y == 0 ? 0 : y > 0 ? 1 : -1
                    );
                    var moved = position - infMovement * (isOpposite ? -1 : 1);
                    while (
                        moved.X >= 0 && moved.X <= sizeOfField.x - 1
                        && moved.Y >= 0 && moved.Y <= sizeOfField.y - 1
                    )
                    {
                        yield return moved;
                        moved = moved - infMovement * (isOpposite ? -1 : 1);
                    }
                }
                else
                {
                    var moved = position - movement * (isOpposite ? -1 : 1);
                    if (
                        moved.X >= 0 && moved.X <= sizeOfField.x - 1
                        && moved.Y >= 0 && moved.Y <= sizeOfField.y - 1
                    )
                        yield return moved;
                }
            }
        }
    }

    class Piece : Image
    {
        Movements movements;

        public Piece(
            string name,
            bool isBot,
            Movements? movements = null,
            string? imageName = null
        )
        {
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
                    this.movements = (Movements)possibleMovements;
            }
            else
                this.movements = movements;

            Width = 60;
            Height = 60;
            Source = new BitmapImage(new Uri(
                "pack://application:,,,/game;component/"
                + $"resources/{imageName ?? name}.png"
            ));
            RenderTransformOrigin = new Point(0.5, 0.5);
            Cursor = Cursors.Hand;
            Name = (isBot ? "bot_" : "") + name;
            if (isBot)
            {
                var transform = new TransformGroup();
                transform.Children.Add(new RotateTransform(180));
                RenderTransform = transform;
            }
        }

        public Piece AddAction(MouseButtonEventHandler? action)
        {
            if (action != null)
                MouseDown += action;
            return this;
        }

        public Piece RemoveAction(MouseButtonEventHandler? action)
        {
            if (action != null)
                MouseDown -= action;
            return this;
        }

        public Piece Change(
            string name,
            bool? isBot = null,
            Movements? movements = null,
            string? imageName = null
        )
        {
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
                    this.movements = (Movements)possibleMovements;
            }
            else
                this.movements = movements;

            Source = new BitmapImage(new Uri(
                "pack://application:,,,/game;component/"
                + $"resources/{imageName ?? name}.png"
            ));
            var isBotNotNull = isBot ?? Name.Contains("bot_");
            Name = (isBotNotNull ? "bot_" : "") + name;
            if (isBotNotNull)
            {
                var transform = new TransformGroup();
                transform.Children.Add(new RotateTransform(180));
                RenderTransform = transform;
            }
            else
                RenderTransform = Transform.Identity;

            return this;
        }
    }
}

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SymbolRecognitionLib
{
    static class BitmapExtension
    {
        public static void DrawInConsole(this Bitmap bitmap)
        {
            for (int row = 0; row < bitmap.Height; row++)
            {
                for (int col = 0; col < bitmap.Width; col++)
                {
                    Console.Write(Math.Floor(bitmap.GetPixel(col, row).GetBrightness()));
                }

                Console.WriteLine();
            }
        }

        public static void DrawInConsole(this Bitmap bitmap, List<Rectangle> borderBoxes)
        {
            for (int row = 0; row < bitmap.Height; row++)
            {
                for (int col = 0; col < bitmap.Width; col++)
                {
                    bool isBorder = false;

                    foreach (var shape in borderBoxes)
                        if (shape.IsPointBelongsBorder(new Point(col, row)))
                        {
                            Console.Write("*");
                            isBorder = true;
                            break;
                        }

                    if (!isBorder)
                        Console.Write(Math.Floor(bitmap.GetPixel(col, row).GetBrightness()));


                }

                Console.WriteLine();
            }
        }
    }

    class Point
    {
        public int X;
        public int Y;

        public Point(int x, int y)
        {
            X = x;
            Y = y;
        }

        public Point(Point point)
        {
            X = point.X;
            Y = point.Y;
        }

        public override bool Equals(object obj)
        {
            if ((obj == null) || !this.GetType().Equals(obj.GetType()))
            {
                return false;
            }
            else
            {
                Point p = (Point)obj;
                return (X == p.X) && (Y == p.Y);
            }
        }

        public override int GetHashCode()
        {
            return X + Y;
        }
    }

    class Rectangle
    {
        public Point TopLeftCorner { get; private set; } = null;
        public Point BottomRightCorner { get; private set; } = null;

        public int Width => BottomRightCorner.X - TopLeftCorner.X + 1;
        public int Height => BottomRightCorner.Y - TopLeftCorner.Y + 1;
        public Point Location => TopLeftCorner;

        HashSet<Point> points = new HashSet<Point>();

        public Rectangle(Point topLeftCorner, Point bottomRightCorner)
        {
            TopLeftCorner = topLeftCorner;
            BottomRightCorner = bottomRightCorner;

            points.Add(topLeftCorner);
            points.Add(bottomRightCorner);
        }

        /// <summary>
        /// Returns true if the point belongs to the rectangluar shape
        /// </summary>
        public bool IsPointBelongsShape(Point point)
        {
            if (point.X >= TopLeftCorner?.X && point.X <= BottomRightCorner?.X)
                if (point.Y >= TopLeftCorner?.Y && point.Y <= BottomRightCorner?.Y)
                    return true;

            return false;
        }

        /// <summary>
        /// Returns true if the point belongs to the set of selected points
        /// </summary>
        public bool IsContainsPoint(Point point)
        {
            if (points.Contains(point))
                return true;

            return false;

        }

        public bool IsPointBelongsBorder(Point point)
        {
            if (IsPointBelongsShape(point))
            {
                if (point.Y == TopLeftCorner.Y)
                    return true;
                if (point.Y == BottomRightCorner.Y)
                    return true;
                if (point.X == TopLeftCorner.X)
                    return true;
                if (point.X == BottomRightCorner.X)
                    return true;
            }

            return false;
        }

        /// <summary>
        /// Adds point to the shape
        /// </summary>
        public void Extend(Point point)
        {
            points.Add(point);

            if (TopLeftCorner == null)
                TopLeftCorner = point;
            else if (BottomRightCorner == null)
                BottomRightCorner = point;
            else if (!IsPointBelongsShape(point))
            {
                TopLeftCorner.X = Math.Min(point.X, TopLeftCorner.X);
                TopLeftCorner.Y = Math.Min(point.Y, TopLeftCorner.Y);
                BottomRightCorner.X = Math.Max(point.X, BottomRightCorner.X);
                BottomRightCorner.Y = Math.Max(point.Y, BottomRightCorner.Y);
            }
        }

        public void MergeShapes(Rectangle other)
        {
            Extend(other.TopLeftCorner);
            Extend(other.BottomRightCorner);
        }
    }

    class SymbolsLocator
    {
        #region Public members

        public int Row { get; private set; } = 2;
        public int Column { get; private set; } = 2;

        #endregion

        #region Private members

        Bitmap originalBitmap;
        Bitmap workingBitmap;

        Rectangle currentShape = null;
        List<Rectangle> shapes;

        bool isFilledPixel => getPixel().GetBrightness() < 1;
        bool isPixelPartOfShape => getPixel().A == 0;

        #endregion

        public SymbolsLocator(Bitmap bitmap)
        {
            originalBitmap = bitmap;
        }

        public List<Rectangle> Process()
        {
            shapes = new List<Rectangle>();
            workingBitmap = copyOriginalBitmap();

            while (advanceIfPossible())
            {
                if (isFilledPixel)
                    processShape();
                else
                    currentShape = null;
            }

            return shapes;
        }

        #region Private methods

        void processShape()
        {
            if (isPixelPartOfShape)
            {
                var rect = findPointOwner();

                if (currentShape == null || rect == currentShape)
                {
                    currentShape = rect;
                    addPixelBelow();
                }
                else
                {
                    shapes.Remove(rect);
                    currentShape.MergeShapes(rect);
                }

            }
            else
                createOrExtendShape();
        }

        Rectangle findPointOwner()
        {
            var point = getPoint();

            foreach (var rect in shapes)
                if (rect.IsContainsPoint(point))
                    return rect;
                else if (rect.IsPointBelongsShape(point))
                    return rect;


            return null;
        }

        #region Helpers

        Color getPixel()
        {
            return workingBitmap.GetPixel(Column, Row);
        }

        Color getPixel(Point point)
        {
            return workingBitmap.GetPixel(point.X, point.Y);
        }

        Bitmap copyOriginalBitmap()
        {
            return new Bitmap((Image)originalBitmap.Clone());
        }

        Point getPoint()
        {
            var point = new Point(Column, Row);
            markPixel(point);
            return point;
        }

        Point getPointBelow()
        {
            int row = Row + 1;

            if (row >= originalBitmap.Height)
                return null;

            var point = new Point(Column, row);
            return point;
        }

        void markPixel(Point point)
        {
            workingBitmap.SetPixel(point.X, point.Y, Color.FromArgb(0, 0, 0, 0));
        }

        #endregion

        void addPixelBelow()
        {
            var bottomPoint = getPointBelow();

            if (bottomPoint != null && getPixel(bottomPoint).GetBrightness() < 1)
            {
                markPixel(bottomPoint);
                currentShape.Extend(bottomPoint);

            }
        }

        void createOrExtendShape()
        {
            var point = getPoint();

            if (currentShape == null)
                createShape(point);
            else
                extendShape(point);

            addPixelBelow();
        }

        void createShape(Point point)
        {
            var rect = new Rectangle(point, new Point(point));
            shapes.Add(rect);
            currentShape = rect;

        }

        void extendShape(Point point)
        {
            currentShape.Extend(point);
        }

        /// <summary>
        /// Push cursor to the next pixel, returns false if reaches the end
        /// </summary>
        bool advanceIfPossible()
        {

            Column++;

            if (Column >= originalBitmap.Width)
            {
                Column = 0;
                Row++;

                // Delete outline
                if (Row >= originalBitmap.Height - 2)
                    return false;
            }

            // Delete outline
            if (Column == 0)
                Column = 2;


            return true;
        }

        #endregion

    }
}

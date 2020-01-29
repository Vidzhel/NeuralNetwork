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
        public static void DrawInConsole(this Bitmap bitmap, int? minx = null, int? miny = null, int? maxx = null, int? maxy = null, Point point = null)
        {
            int height = maxy ?? bitmap.Height;
            int width = maxx ?? bitmap.Width;

            height = height > bitmap.Height ? bitmap.Height : height;
            width = width > bitmap.Width ? bitmap.Width : width;

            int initHeight = miny ?? 0;
            int initWidth = minx ?? 0;

            initHeight = initHeight < 0 ? 0 : initHeight;
            initWidth = initWidth < 0 ? 0 : initWidth;

            for (int row = initHeight; row < height; row++)
            {
                for (int col = initWidth; col < width; col++)
                {
                    if (point != null && point.X == col && point.Y == row)
                        Console.Write("+");
                    else
                        Console.Write(Math.Floor((decimal)(bitmap.GetPixel(col, row).GetBrightness())));
                }

                Console.WriteLine();
            }
        }

        public static void DrawInConsole(this Bitmap bitmap, List<Rectangle> borderBoxes, int? minx = null, int? miny = null, int? maxx = null, int? maxy = null, Point point = null)
        {
            int height = maxy ?? bitmap.Height;
            int width = maxx ?? bitmap.Width;

            height = height > bitmap.Height ? bitmap.Height : height;
            width = width > bitmap.Width ? bitmap.Width : width;

            int initHeight = miny ?? 0;
            int initWidth = minx ?? 0;

            for (int row = initHeight; row < height; row++)
            {
                for (int col = initWidth; col < width; col++)
                {
                    bool isBorder = false;
                    bool found = false;


                    if (point != null && point.X == col && point.Y == row)
                        Console.Write("+");
                    else
                    {
                        //foreach (var shape in borderBoxes)
                        //    if (shape.IsPointBelongsBorder(new Point(col, row)))
                        //    {
                        //        Console.Write("*");
                        //        isBorder = true;
                        //        break;
                        //    }

                        if (!isBorder)
                            if (bitmap.GetPixel(col, row).GetBrightness() < 0.6)
                            {
                                found = false;
                                for (int i = 0; i < borderBoxes.Count; i++)
                                {
                                    if (borderBoxes[i].IsPointBelongsShape(new Point(col, row)))
                                    {
                                        Console.Write(i);
                                        found = true;
                                        break;
                                    }
                                }

                                if (!found)
                                    Console.Write("#");
                            }
                            else
                            {
                                Console.Write("-");
                            }
                    }

                }

                Console.WriteLine();
            }
        }
    }


    public class Point : IComparable<Point>
    {
        public double X;
        public double Y;

        public Point(double x, double y)
        {
            X = x;
            Y = y;
        }

        public Point(Point point)
        {
            X = point.X;
            Y = point.Y;
        }

        public static Point operator +(Point first, Point second)
        {
            return new Point(first.X + second.X, first.Y + second.Y);
        }

        public static Point operator -(Point first, Point second)
        {
            return new Point(first.X - second.X, first.Y - second.Y);
        }

        public static Point operator /(Point first, int scaler)
        {
            return new Point(first.X / scaler, first.Y / scaler);
        }

        public static bool operator >(Point first, Point second)
        {
            double verticalThreshold = 50;

            double vertical = first.Y - second.Y;
            double horizontal = first.X - second.X;

            if (Math.Abs(vertical) > verticalThreshold)
            {
                if (vertical > 0)
                    return true;
                else
                    return false;
            }
            else
            {
                if (horizontal > 0)
                    return true;
                else
                    return false;
            }
        }

        public static bool operator <(Point first, Point second)
        {
            return !(first > second);
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
            return (int)(X + Y);
        }

        public override string ToString()
        {
            return $"({X}:{Y})";
        }

        public int CompareTo(Point other)
        {
            if (this > other)
                return 1;
            else if (this == other)
                return 0;
            return -1;
        }
    }

    public class Rectangle
    {
        public Point TopLeftCorner { get; private set; } = null;
        public Point BottomRightCorner { get; private set; } = null;

        public int Width => (int)(BottomRightCorner.X - TopLeftCorner.X + 1);
        public int Height => (int)(BottomRightCorner.Y - TopLeftCorner.Y + 1);
        public Point Location => TopLeftCorner;

        HashSet<Point> points = new HashSet<Point>();

        public Rectangle(Point topLeftCorner, Point bottomRightCorner)
        {
            TopLeftCorner = new Point(topLeftCorner);
            BottomRightCorner = new Point(bottomRightCorner);

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
                TopLeftCorner = new Point(point);
            else if (BottomRightCorner == null)
                BottomRightCorner = new Point(point);
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
            points.UnionWith(other.points);
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

        bool isFilledPixel => getPixel().GetBrightness() < 0.6;
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

            excludeSmallShapes();

            return shapes.OrderBy(shape => shape.TopLeftCorner).ToList();
        }

        #region Private methods

        void processShape()
        {
            //originalBitmap.DrawInConsole(shapes, 5, 5, 100, 50, new Point(Column, Row)); 

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

            return null;
        }

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

        void excludeSmallShapes(int minnimalPossibleArea = 100)
        {
            var newShapes = new List<Rectangle>();

            foreach (var shape in shapes)
                if (shape.Width * shape.Height > minnimalPossibleArea)
                    newShapes.Add(shape);

            shapes = newShapes;
        }

        #region Helpers

        Color getPixel()
        {
            return workingBitmap.GetPixel(Column, Row);
        }

        Color getPixel(Point point)
        {
            return workingBitmap.GetPixel((int)point.X, (int)point.Y);
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
            workingBitmap.SetPixel((int)point.X, (int)point.Y, Color.FromArgb(0, 0, 0, 0));
        }

        #endregion

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

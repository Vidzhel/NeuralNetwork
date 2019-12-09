using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SymbolRecognitionLib
{
    public enum HorizonlSkew
    {
        Top,
        Bottom
    }

    public enum VerticalSkew
    {
        Left,
        Right
    }

    public static class BitmapDistortionsProducer
    {
        static Graphics currentGraphics;

        public static void BeginTransformation(this Bitmap sourceBitmap) {
            currentGraphics = Graphics.FromImage(sourceBitmap);
        }

        public static void EndTransformation(this Bitmap sourceBitmap)
        {
            currentGraphics.Dispose();
            currentGraphics = null;
        }

        public static Bitmap Rotate(this Bitmap sourceBitmap, float angle)
        {
            Bitmap returnBitmap = new Bitmap(sourceBitmap.Width, sourceBitmap.Height);
            returnBitmap.SetResolution(sourceBitmap.HorizontalResolution, sourceBitmap.VerticalResolution);

            using (var brush = new SolidBrush(Color.White))
            using (Graphics g = Graphics.FromImage(returnBitmap))
            {
                g.FillRectangle(brush, 0, 0, returnBitmap.Width, returnBitmap.Height);

                // Translate rotation point
                g.TranslateTransform((float)sourceBitmap.Width / 2, (float)sourceBitmap.Height / 2);
                g.RotateTransform(angle);
                g.TranslateTransform(-(float)sourceBitmap.Width / 2, -(float)sourceBitmap.Height / 2);
                g.DrawImage(sourceBitmap, 0, 0);
            }

            return returnBitmap;
        }

        public static Bitmap Scale(this Bitmap sourceBitmap, float xFactor, float yFactor)
        {
            Bitmap returnBitmap = new Bitmap((int)(sourceBitmap.Width*xFactor), (int)(sourceBitmap.Height*yFactor));
            returnBitmap.SetResolution(sourceBitmap.HorizontalResolution, sourceBitmap.VerticalResolution);

            using (var brush = new SolidBrush(Color.White))
            using (Graphics g = Graphics.FromImage(returnBitmap))
            {
                g.FillRectangle(brush, 0, 0, returnBitmap.Width, returnBitmap.Height);

                g.ScaleTransform(xFactor, yFactor);
                g.DrawImage(sourceBitmap, 0, 0);
            }

            return returnBitmap;
        }

        public static Bitmap AddPadding(this Bitmap sourceBitmap, int horizontalPadding, int verticalPadding)
        {
            int newWidth = sourceBitmap.Width + 2 * horizontalPadding;
            int newHeight = sourceBitmap.Height + 2 * verticalPadding;

            Bitmap returnBitmap = new Bitmap(newWidth, newHeight);
            returnBitmap.SetResolution(sourceBitmap.HorizontalResolution, sourceBitmap.VerticalResolution);

            using (var brush = new SolidBrush(Color.White))
            using (Graphics g = Graphics.FromImage(returnBitmap))
            {
                g.FillRectangle(brush, 0, 0, returnBitmap.Width, returnBitmap.Height);
                g.DrawImage(sourceBitmap, new System.Drawing.Rectangle(horizontalPadding, verticalPadding, sourceBitmap.Width, sourceBitmap.Height), new System.Drawing.Rectangle(0, 0, sourceBitmap.Width, sourceBitmap.Height), GraphicsUnit.Pixel);
            }

            return returnBitmap;
        }

        public static Bitmap SkewHorizontaly(this Bitmap sourceBitmap, int offset, HorizonlSkew type)
        {
            if (offset < 0)
                throw new ArgumentException("Negative offset is forbidden");

            int newWidth = sourceBitmap.Width + offset;

            Bitmap returnBitmap = new Bitmap(newWidth, sourceBitmap.Height);
            returnBitmap.SetResolution(sourceBitmap.HorizontalResolution, sourceBitmap.VerticalResolution);

            int topLeftOffset;
            int topRightOffset;
            int bottomLeftOffset;

            if (type == HorizonlSkew.Top)
            {
                topLeftOffset = offset;
                topRightOffset = newWidth;
                bottomLeftOffset = 0;
            }
            else
            {
                topLeftOffset = 0;
                topRightOffset = sourceBitmap.Width;
                bottomLeftOffset = offset;
            }

            System.Drawing.Point[] points = {
                    new System.Drawing.Point(topLeftOffset, 0),
                    new System.Drawing.Point(topRightOffset, 0),
                    new System.Drawing.Point(bottomLeftOffset, sourceBitmap.Height)
            };

            using (var brush = new SolidBrush(Color.White))
            using (Graphics g = Graphics.FromImage(returnBitmap))
            {
                g.FillRectangle(brush, 0, 0, returnBitmap.Width, returnBitmap.Height);
                g.DrawImage(sourceBitmap, points);
            }

            return returnBitmap;
        }

        public static Bitmap SkewVeritcaly(this Bitmap sourceBitmap, int offset, VerticalSkew type)
        {
            if (offset < 0)
                throw new ArgumentException("Negative offset is forbidden");

            int newHeight = sourceBitmap.Height + offset;

            Bitmap returnBitmap = new Bitmap(sourceBitmap.Width, newHeight);
            returnBitmap.SetResolution(sourceBitmap.HorizontalResolution, sourceBitmap.VerticalResolution);

            int topLeftOffset;
            int topRightOffset;
            int bottomLeftOffset;

            if (type == VerticalSkew.Left)
            {
                topLeftOffset = offset;
                topRightOffset = 0;
                bottomLeftOffset = newHeight;
            }
            else
            {
                topLeftOffset = 0;
                topRightOffset = offset;
                bottomLeftOffset = sourceBitmap.Height;
            }

            System.Drawing.Point[] points = {
                    new System.Drawing.Point(0, topLeftOffset),
                    new System.Drawing.Point(sourceBitmap.Width, topRightOffset),
                    new System.Drawing.Point(0, bottomLeftOffset)
            };

            using (var brush = new SolidBrush(Color.White))
            using (Graphics g = Graphics.FromImage(returnBitmap))
            {
                g.FillRectangle(brush, 0, 0, returnBitmap.Width, returnBitmap.Height);
                g.DrawImage(sourceBitmap, points);
            }

            return returnBitmap;
        }
    }
}

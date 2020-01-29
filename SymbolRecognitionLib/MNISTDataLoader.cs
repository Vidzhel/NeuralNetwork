using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.IO.Compression;
using System.Linq;

namespace SymbolRecognitionLib
{
    public enum DataType
    {
        Images,
        Labels
    }

    public class MNISTDataLoader : DataLoader
    {
        int IMAGE_SIZE = 28 * 28;

        public MNISTDataLoader(string filePath, DataType type) : base(filePath, type)
        {
        }

        public override IEnumerable<double[][]> LoadData(int bathSize, int start = 0, int count = -1)
        {
            if (type == DataType.Images)
            {
                double[][] buffer;

                while (readImages(out buffer, bathSize, start) != 0 && count != 0)
                {
                    yield return buffer;

                    start += bathSize;
                    count--;
                }
            }
            else
            {
                double[] buffer;

                while (readLabels(out buffer, bathSize, start) != 0 && count != 0)
                {
                    yield return convertLabels(buffer);

                    start += bathSize;
                    count--;
                }
            }
        }

        public override double[][] LoadData(int start = 0)
        {
            if (type == DataType.Images)
            {
                double[][] buffer;

                readImages(out buffer, start: start);

                return buffer;
            }
            else
            {
                double[] buffer;

                readLabels(out buffer, start: start);

                return convertLabels(buffer);
            }
        }

        internal virtual double[][] convertLabels(double[] labels, int outputsCount = 10)
        {
            double[][] convertedLabels = new double[labels.Length][];

            for (int label = 0; label < labels.Length; label++)
            {
                double[] testingOutput = new double[outputsCount];
                testingOutput[(int)labels[label]]++;

                convertedLabels[label] = testingOutput;
            }

            return convertedLabels;
        }

        public static Bitmap ConvertImageToBitmap(double[] image)
        {
            Bitmap bmp = new Bitmap(28, 28, System.Drawing.Imaging.PixelFormat.Format24bppRgb);

            for (int row = 0; row < 28; row++)
                for (int col = 0; col < 28; col++)
                {
                    int brightness = (int)(image[row * 28 + col] * 100);
                    brightness = brightness == 0 ? 255 : brightness;
                    bmp.SetPixel(col, row, Color.FromArgb(brightness, brightness, brightness));
                }

            return bmp;
        }

        /// <summary>
        /// Loads Mnist images
        /// </summary>
        int readImages(out double[][] buffer, int? count = null, int start = 0)
        {
            int imagesCount;

            try
            {
                using (BinaryReader reader = new BinaryReader(file))
                {
                    reader.ReadBytes(4); // First four bytes are a magic number, we don't need it as we know the structure of the file (3 demensions, ubyte type of data)

                    imagesCount = BitConverter.ToInt32(reader.ReadBytes(4).Reverse().ToArray(), 0) - start; // As the values incoded in big-endian, we need to reverse bytes
                    imagesCount = count != null && count > 0 && count < imagesCount ? (int)count : imagesCount;

                    int rowsCount = BitConverter.ToInt32(reader.ReadBytes(4).Reverse().ToArray(), 0);
                    int colsCount = BitConverter.ToInt32(reader.ReadBytes(4).Reverse().ToArray(), 0);

                    file.Seek((start * IMAGE_SIZE) + 16, SeekOrigin.Begin);

                    buffer = new double[imagesCount][];

                    for (int img = 0; img < imagesCount; img++)
                    {
                        double[] pixels = new double[colsCount * rowsCount];

                        for (int pixel = 0; pixel < rowsCount * colsCount; pixel++)
                        {
                            pixels[pixel] = reader.ReadByte();
                            pixels[pixel] /= 100;
                        }

                        buffer[img] = pixels;
                    }
                }

                return imagesCount;
            }
            catch (EndOfStreamException e)
            {
                buffer = new double[0][];
                return 0;
            }
        }

        /// <summary>
        /// Loads Emnist images
        /// </summary>
        /// <returns></returns>
        //public static int ReadImagesAndFlip(string filePath, out double[][] buffer, int? count = null, int start = 0)
        //{
        //    var bitmaps = new List<Bitmap>();

        //    var readCount = ReadImages(filePath, out buffer, count, start);

        //    foreach (var img in buffer)
        //    {
        //        var bmp = ConvertImageToBitmap(img);
        //        bmp.RotateFlip(RotateFlipType.Rotate270FlipY);
        //        bitmaps.Add(bmp);
        //    }

        //    buffer = ConvertBitmapsToArray(bitmaps.ToArray());

        //    return readCount;
        //}

        int readLabels(out double[] buffer, int? count = null, int start = 0)
        {
            int labelsCount;

            try
            {
                using (BinaryReader reader = new BinaryReader(file))
                {
                    reader.ReadBytes(4); // First four bytes are a magic number, we don't need it as we know the structure of the file (3 demensions, ubyte type of data)


                    labelsCount = BitConverter.ToInt32(reader.ReadBytes(4).Reverse().ToArray(), 0) - start; // As the values incoded in big-endian, we need to reverse bytes
                    labelsCount = count != null && count > 0 && count < labelsCount ? (int)count : labelsCount;

                    file.Seek(start + 8, SeekOrigin.Begin);

                    buffer = new double[labelsCount];

                    for (int label = 0; label < labelsCount; label++)
                    {
                        buffer[label] = reader.ReadByte();
                    }
                }

            }
            catch (EndOfStreamException e)
            {
                buffer = new double[0];
                labelsCount = 0;
            }

            return labelsCount;
        }
    }
}

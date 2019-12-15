using System;
using System.Drawing;
using System.IO;
using System.IO.Compression;
using System.Linq;

namespace SymbolRecognitionLib
{
    public static class MNISTDataLoader
    {
        public static void PrepeareData(string folderPath, out double[][] trainingImages, out double[][] testingImages, out double[][] trainLabels, out double[][] testLabels, int? testDataCount = null, int? trainDataCount = null)
        {

            double[] trainingLabels;
            double[] testingLables;

            LoadTrainData(folderPath, out trainingImages, out trainingLabels, trainDataCount);
            LoadTestData(folderPath, out testingImages, out testingLables, testDataCount);

            testLabels = ConvertLabels(testingLables);
            trainLabels = ConvertLabels(trainingLabels);

        }

        public static double[][] ConvertLabels(double[] labels)
        {
            double[][] convertedLabels = new double[labels.Length][];

            for (int label = 0; label < labels.Length; label++)
            {
                double[] testingOutput = new double[10];
                testingOutput[(int)labels[label]]++;

                convertedLabels[label] = testingOutput;
            }

            return convertedLabels;
        }


        public static void LoadTestData(string folderPath, out double[][] images, out double[] labels, int? count = null)
        {
            string testImages = folderPath + "//t10k-images-idx3-ubyte.mnist.gz";
            string testLabels = folderPath + "/t10k-labels-idx1-ubyte.mnist.gz";

            images = ReadImages(testImages, count);
            labels = ReadLabels(testLabels, count);
        }

        public static void LoadTrainData(string folderPath, out double[][] images, out double[] labels, int? count = null)
        {
            string testImages = folderPath + "//train-images-idx3-ubyte.mnist.gz";
            string testLabels = folderPath + "//train-labels-idx1-ubyte.mnist.gz";

            images = ReadImages(testImages, count);
            labels = ReadLabels(testLabels, count);
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

        public static double[][] ReadImages(string filePath, int? count = null)
        {
            Stream fileStream;

            if (Path.GetExtension(filePath) == ".gz")
                fileStream = FileManager.Decompress(filePath);
            else
                fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read);

            double[][] images;

            try
            {
                using (BinaryReader reader = new BinaryReader(fileStream))
                {
                    reader.ReadBytes(4); // First four bytes are a magic number, we don't need it as we know the structure of the file (3 demensions, ubyte type of data)

                    int imagesCount = BitConverter.ToInt32(reader.ReadBytes(4).Reverse().ToArray(), 0); // As the values incoded in big-endian, we need to reverse bytes
                    imagesCount = count != null ? (int)count : imagesCount;
                    int rowsCount = BitConverter.ToInt32(reader.ReadBytes(4).Reverse().ToArray(), 0);
                    int colsCount = BitConverter.ToInt32(reader.ReadBytes(4).Reverse().ToArray(), 0);

                    images = new double[imagesCount][];

                    for (int img = 0; img < imagesCount; img++)
                    {
                        double[] pixels = new double[colsCount * rowsCount];

                        for (int pixel = 0; pixel < rowsCount * colsCount; pixel++)
                        {
                            pixels[pixel] = reader.ReadByte();
                            pixels[pixel] /= 100;
                        }

                        images[img] = pixels;
                    }
                }

                return images;
            }
            finally
            {
                fileStream.Close();
            }
        }

        public static double[] ReadLabels(string filePath, int? count = null)
        {
            Stream fileStream;

            if (Path.GetExtension(filePath) == ".gz")
                fileStream = FileManager.Decompress(filePath);
            else
                fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read);

            double[] labels;

            try
            {
                using (BinaryReader reader = new BinaryReader(fileStream))
                {
                    reader.ReadBytes(4); // First four bytes are a magic number, we don't need it as we know the structure of the file (3 demensions, ubyte type of data)


                    int labelsCount = BitConverter.ToInt32(reader.ReadBytes(4).Reverse().ToArray(), 0); // As the values incoded in big-endian, we need to reverse bytes
                    labelsCount = count != null ? (int)count : labelsCount;

                    labels = new double[labelsCount];
                    labels = new double[labelsCount];

                    for (int label = 0; label < labelsCount; label++)
                    {
                        labels[label] = reader.ReadByte();
                    }
                }

                return labels;
            }
            finally
            {
                fileStream.Close();
            }
        }
    }
}

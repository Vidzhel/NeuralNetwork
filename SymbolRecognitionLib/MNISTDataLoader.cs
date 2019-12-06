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

            double[] trainingLables;
            double[] testingLables;

            LoadTrainData(folderPath, out trainingImages, out trainingLables, trainDataCount);
            LoadTestData(folderPath, out testingImages, out testingLables, testDataCount);

            testLabels = new double[testingLables.Length][];
            trainLabels = new double[trainingLables.Length][];

            for (int label = 0; label < testingLables.Length; label++)
            {
                double[] testingOutput = new double[10];
                testingOutput[(int)testingLables[label]]++;

                testLabels[label] = testingOutput;
            }

            for (int label = 0; label < trainingLables.Length; label++)
            {
                double[] trainingOutput = new double[10];
                trainingOutput[(int)trainingLables[label]]++;

                trainLabels[label] = trainingOutput;
            }
        }

        public static void LoadTestData(string folderPath, out double[][] images, out double[] labels, int? count = null)
        {
            string testImages = folderPath + "//t10k-images-idx3-ubyte.gz";
            string testLabels = folderPath + "/t10k-labels-idx1-ubyte.gz";

            images = ReadImages(testImages, count);
            labels = ReadLabels(testLabels, count);
        }

        public static void LoadTrainData(string folderPath, out double[][] images, out double[] labels, int? count = null)
        {
            string testImages = folderPath + "//train-images-idx3-ubyte.gz";
            string testLabels = folderPath + "//train-labels-idx1-ubyte.gz";

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

        public static double[][] ReadImages(string filePath, int? count)
        {
            var fileStream = decompress(new FileInfo(filePath));
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

        public static double[] ReadLabels(string filePath, int? count)
        {
            var fileStream = decompress(new FileInfo(filePath));
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

        static MemoryStream decompress(FileInfo fileToDecompress)
        {
            using (FileStream originalFileStream = fileToDecompress.OpenRead())
            {
                string currentFileName = fileToDecompress.FullName;
                string newFileName = currentFileName.Remove(currentFileName.Length - fileToDecompress.Extension.Length);

                MemoryStream decompressedFileStream = new MemoryStream();

                using (GZipStream decompressionStream = new GZipStream(originalFileStream, CompressionMode.Decompress))
                {
                    decompressionStream.CopyTo(decompressedFileStream);
                }

                decompressedFileStream.Seek(0, SeekOrigin.Begin);
                return decompressedFileStream;
            }
        }
    }
}

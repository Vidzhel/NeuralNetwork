using System.Collections.Generic;
using System.IO;
using System.IO.Compression;

namespace SymbolRecognitionLib
{
    public abstract class DataLoader
    {
        internal string filePath;
        DataType type;
        internal MemoryStream file;

        public DataLoader(string filePath, DataType type)
        {
            this.filePath = filePath;
            this.type = type;
            prepareFile(filePath);
        }

        public abstract IEnumerable<double[][]> LoadData(int bathSize, int start = 0, int count = -1);
        public abstract double[][] LoadData(int start = 0);

        void prepareFile(string filePath)
        {
            file = new MemoryStream();

            if (filePath.Contains(".gz"))
            {
                using (var source = new FileStream(filePath, FileMode.Create, FileAccess.Read))
                using (var decompressionStream = new GZipStream(source, CompressionMode.Decompress))
                    decompressionStream.CopyTo(file);
            }
            else
                using (var source = new FileStream(filePath, FileMode.Create, FileAccess.Read))
                    source.CopyTo(file);
        }

    }
}

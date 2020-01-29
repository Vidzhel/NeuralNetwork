using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SymbolRecognitionLib
{
    public class CustomDataLoader : DataLoader
    {
        public CustomDataLoader(string filePath, DataType type) : base(filePath, type)
        {
        }

        public override IEnumerable<double[][]> LoadData(int bathSize, int start = 0, int count = -1)
        {
            throw new NotImplementedException();
        }

        public override double[][] LoadData(int start = 0)
        {
            throw new NotImplementedException();
        }
    }
}

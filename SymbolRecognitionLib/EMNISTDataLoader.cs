using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SymbolRecognitionLib
{
    public class EMNISTDataLoader : MNISTDataLoader
    {
        public EMNISTDataLoader(string filePath, DataType type) : base(filePath, type)
        {
        }

        internal override double[][] convertLabels(double[] labels, int outputsCount = 62)
        {
            return base.convertLabels(labels, outputsCount);
        }

        public static char ConvertEmnistLabelToChar(double label)
        {
            if (label < 0 || label > 61)
                return '0';
            else if (label <= 9)
                return (char)(label + 48);
            else if (label > 9 && label <= 35)
                return (char)(label + 55);
            else
                return (char)(label + 61);
        }

        public static string ConvertEmnistLabelsToString(double[] labels)
        {
            string res = "";

            foreach (var label in labels)
            {
                res += ConvertEmnistLabelToChar(label);
            }

            return res;
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NeuralNetworkLib.Layers
{
    public class SoftmaxLayer : Layer
    {
        public override double[] BackPropagation(double[] errors)
        {
            throw new NotImplementedException();
        }

        public override double[] BackPropagation(double[] nextLayerDeltas, double[][] nextLayerWeights)
        {
            throw new NotImplementedException();
        }

        public override double[] FeedForward(double[] inputs)
        {
            throw new NotImplementedException();
        }

        public override void UpdateDerivatives(double learningRate, double regularizationFactor, double trainingDatasetSize)
        {
            throw new NotImplementedException();
        }
    }
}

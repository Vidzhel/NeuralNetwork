using NeuralNetworkLib.Neurons;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NeuralNetworkLib.Layers
{
    [Serializable]
    struct Shape
    {
        int width;
        int height;
        int 
        public Shape(int width, int height)
        {
            this.width = width;
            this.height = height;
        }

        public static Shape operator -(Shape first, Shape second)
        {
            return new Shape(first.width - second.width, first.height - second.height);
        }

        public static Shape operator +(Shape first, Shape second)
        {
            return new Shape(first.width + second.width, first.height + second.height);
        }

        public static Shape operator -(Shape first, int term)
        {
            return new Shape(first.width - term, first.height - term);
        }

        public static Shape operator +(Shape first, int term)
        {
            return new Shape(first.width + term, first.height + term);
        }
    }

    [Serializable]
    public class ConvolutionalLayer : Layer
    {

        Shape inputShape;
        Shape filterShape;

        public ConvolutionalLayer(Shape inputShape, Shape filterShape, int depth)
        {
            Neurons = new Neuron[NeuronsCount];

            for (int neuron = 0; neuron < Neurons.Length; neuron++)
                Neurons[neuron] = new Neuron(InputsCount, activationFunction);

            OutputsCount = NeuronsCount;
            this.InputsCount = InputsCount;
            setLayerWeights();
        }

        public ConvolutionalLayer(Shape inputShape, Shape outputShape, int depth, int filtersCount)
        {
            this.inputShape = inputShape;
            filterShape = inputShape - outputShape + 1;
        }

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

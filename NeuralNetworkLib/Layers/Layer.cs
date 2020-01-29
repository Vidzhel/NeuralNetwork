using NeuralNetworkLib.Neurons;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NeuralNetworkLib.Layers
{
    [Serializable]
    public enum LayerType
    {
        InputLayer,
        HiddenLayer,
        OutputLayer
    }

    public abstract class Layer
    {
        public Neuron[] Neurons { get; internal set; }
        public int OutputsCount { get; internal set; }
        public int InputsCount { get; internal set; }
        public LayerType Type { get; internal set; }
        public double[][] Weights { get; internal set; }

        public abstract double[] FeedForward(double[] inputs);
        public abstract double[] BackPropagation(double[] errors);
        public abstract double[] BackPropagation(double[] nextLayerDeltas, double[][] nextLayerWeights);
        public abstract void UpdateDerivatives(double learningRate, double regularizationFactor, double trainingDatasetSize);

        internal void setLayerWeights()
        {
            double[][] weights = new double[Neurons.Length][];

            for (int neuron = 0; neuron < Neurons.Length; neuron++)
                weights[neuron] = Neurons[neuron].Weights;

            Weights = weights;
        }

    }
}

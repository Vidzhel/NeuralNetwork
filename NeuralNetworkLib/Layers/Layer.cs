using NeuralNetworkLib.Neurons;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
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

    [Serializable]
    public class LayerTopology<TOutput>
    {
        public BaseNeuron<TOutput>[] Neurons { get; private set; }
        public Shape OutputShape { get; private set; }
        public Shape InputShape { get; private set; }
        public LayerType Type { get; private set; }
        public MultiDimArr[] Weights { get; private set; }

        public LayerTopology(BaseNeuron<TOutput>[] neurons, Shape outputShape, Shape inputShape, LayerType type)
        {
            Neurons = neurons;
            OutputShape = outputShape;
            InputShape = inputShape;
            Type = type;
            Weights = getWeights(neurons);
        }

        public LayerTopology(LayerTopology<TOutput> topology)
        {
            Neurons = new BaseNeuron<TOutput>[topology.Neurons.Length];
            ConstructorInfo copyConstructor = topology.Neurons[0].GetType().GetConstructor(new Type[] { topology.Neurons[0].GetType() });

            for (int i = 0; i < Neurons.Length; i++)
                Neurons[i] = (BaseNeuron<TOutput>)copyConstructor.Invoke(topology.Neurons[i], new object[] { topology.Neurons[i] });

            OutputShape = new Shape(topology.OutputShape);
            InputShape = new Shape(topology.InputShape);
            Type = topology.Type;
            Weights = getWeights(this.Neurons);
        }

        MultiDimArr[] getWeights(BaseNeuron<TOutput>[] neurons)
        {
            MultiDimArr[] weights = new MultiDimArr[neurons.Length];

            for (int i = 0; i < neurons.Length; i++)
                weights[i] = neurons[i].@params.Weights;

            return weights;
        }
    }

    [Serializable]
    public abstract class Layer<TOutput>
    {
        public LayerTopology<TOutput> Topology { get; private set; }

        public Layer(BaseNeuron<TOutput>[] neurons, Shape outputShape, Shape inputShape, LayerType layerType)
        {
            Topology = new LayerTopology<TOutput>(neurons, outputShape, inputShape, layerType);
        }

        public Layer(LayerTopology<TOutput> topology)
        {
            Topology = new LayerTopology<TOutput>(topology);
        }

        public Layer(Layer<TOutput> layer)
        {
            Topology = new LayerTopology<TOutput>(layer.Topology);
        }

        public abstract MultiDimArr FeedForward(MultiDimArr inputs);
        public abstract MultiDimArr BackPropagation(MultiDimArr errors);
        public abstract void UpdateDerivatives(float learningRate, float regularizationFactor, int trainingDatasetSize);

        public abstract override string ToString();
    }
}

using NeuralNetworkLib.Layers;
using System;

namespace NeuralNetworkLib.Neurons
{
    [Serializable]
    public class NeuronParameters<TOutput>
    {
        public MultiDimArr Weights { get; internal set; }
        public float Bias { get; internal set; }
        public LayerType LayerType { get; internal set; }
        public Function<float, float> ActivationFunction { get; internal set; }

        public MultiDimArr weightDeltas { get; internal set; }
        public float biasDelta { get; internal set; }

        public NeuronParameters(LayerType layerType, Function<float, float> activationFunction)
        {
            LayerType = layerType;
            ActivationFunction = activationFunction;
        }

        public NeuronParameters(MultiDimArr weights, float bias, LayerType layerType, Function<float, float> activationFunction)
        {
            Shape weightsMatrixShape = new Shape(new int[] { weights.Length });
            weightDeltas = new MultiDimArr(weightsMatrixShape);
            Weights = weights;
            Bias = bias;

            LayerType = layerType;
            this.ActivationFunction = activationFunction;
        }

        public NeuronParameters(NeuronParameters<TOutput> parameters)
        {
            Weights = new MultiDimArr(parameters.Weights);
            weightDeltas = new MultiDimArr(parameters.weightDeltas);
            Bias = parameters.Bias;
            biasDelta = parameters.biasDelta;

            LayerType = parameters.LayerType;
            ActivationFunction = parameters.ActivationFunction;

            Bias = parameters.Bias;
        }
    }

    [Serializable]
    public abstract class BaseNeuron<TOutput>
    {
        public NeuronParameters<TOutput> @params { get; private set; }
        protected int backPropagationsInRow;
        public TOutput LastOutput { get; protected set; }
        public MultiDimArr LastInputs { get; protected set; }

        #region Constructors

        public BaseNeuron(LayerType layerType, Function<float, float> activationFunction)
        {
            @params = new NeuronParameters<TOutput>(layerType, activationFunction);
        }

        public BaseNeuron(MultiDimArr weights, float bias, LayerType layerType, Function<float, float> activationFunction)
        {
            @params = new NeuronParameters<TOutput>(weights, bias, layerType, activationFunction);
        }

        public BaseNeuron(BaseNeuron<TOutput> neuron)
        {
            @params = new NeuronParameters<TOutput>(neuron.@params);
            backPropagationsInRow = neuron.backPropagationsInRow;
            LastOutput = neuron.LastOutput;
            LastInputs = neuron.LastInputs;
        }

        #endregion

        public abstract TOutput FeedForward(MultiDimArr inputs);
        public abstract TOutput BackPropagation(TOutput error);
        public abstract void UpdateDerivatives(float learningRate, float regularizationFactor, int trainingDatasetSize);

        public abstract override string ToString();
    }
}

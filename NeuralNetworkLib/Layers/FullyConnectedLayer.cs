using NeuralNetworkLib.Neurons;
using System;

namespace NeuralNetworkLib.Layers
{

    [Serializable]
    public class FullyConnectedLayer : Layer<float>
    {

        public FullyConnectedLayer(Shape inputShape, LayerType type, Function<float, float> activationFunction)
            : base(createNeurons(inputShape, type, activationFunction), new Shape(new int[] { inputShape[0] }), inputShape, type)
        {
            if (inputShape.DimensionsCount != 2)
                throw new ArgumentException("Wrong inputShape shape dimensions for fully connected layer. Should be 2 dimensions");
        }

        static Neuron[] createNeurons(Shape inputShape, LayerType type, Function<float, float> activationFunction)
        {
            int neuronsCount = inputShape[0];
            int neuronsInputsCount = inputShape[1];

            Neuron[] neurons = new Neuron[neuronsCount];

            for (int i = 0; i < neuronsCount; i++)
                neurons[i] = new Neuron(neuronsInputsCount, type, activationFunction);

            return neurons;
        }

        #region public Methods

        public override MultiDimArr FeedForward(MultiDimArr inputs)
        {
            inputs = inputs.TryReshape(Topology.InputShape);

            if (Topology.Type == LayerType.InputLayer)
                return inputs;

            float[] outputs = new float[Topology.OutputShape[0]];

            for (int i = 0; i < Topology.Neurons.Length; i++)
                outputs[i] = Topology.Neurons[i].FeedForward(inputs);

            return new MultiDimArr(outputs, Topology.OutputShape);
        }

        public override MultiDimArr BackPropagation(MultiDimArr errors)
        {
            errors = errors.TryReshape(Topology.OutputShape);

            float[] deltas = new float[errors.Length];

            for (int i = 0; i < errors.Length; i++)
                deltas[i] = Topology.Neurons[i].BackPropagation(errors[i]);

            return propagateErrors(deltas);
        }

        public override void UpdateDerivatives(float learningRate, float regularizationFactor, int trainingDatasetSize)
        {
            if (Type == LayerType.InputLayer)
                return;

            foreach (var neuron in Neurons)
                neuron.UpdateDerivatives(learningRate, regularizationFactor, trainingDatasetSize);
        }

        public override string ToString()
        {
            string res = $"Fully connected {Topology.Type}, input shape: {Topology.InputShape}, output shape: {Topology.OutputShape}:\n";

            foreach (var neuron in Topology.Neurons)
            {
                res += "\n\t" + neuron.ToString();
            }

            return res + "\n\n";
        }


        #endregion

        /// <summary>
        /// Back propagate the error
        /// </summary>
        /// <param name="deltas">neurons' deltas from the layer</param>
        /// <returns>appropriate errors for the previous layer</returns>
        double[] propagateErrors(float[] deltas)
        {
            double[] thisLayerErrors = new double[Neurons.Length];

            for (int neuron = 0; neuron < Neurons.Length; neuron++)
                for (int neuronNextLayer = 0; neuronNextLayer < nextLayerErrors.Length; neuronNextLayer++)
                    thisLayerErrors[neuron] += nextLayerErrors[neuronNextLayer] * nextLayerWeights[neuronNextLayer][neuron];

            return thisLayerErrors;
        }

    }
}

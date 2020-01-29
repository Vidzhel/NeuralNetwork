using NeuralNetworkLib.Neurons;
using System;

namespace NeuralNetworkLib.Layers
{

    [Serializable]
    public class FullyConnectedLayer : Layer
    {

        //#region public Members

        //public Neuron[] Neurons { get; private set; }
        //public int OutputsCount { get; private set; }
        //public int InputsCount { get; private set; }
        //public LayerType Type { get; private set; }
        //public double[][] Weights { get; private set; }

        //#endregion


        public FullyConnectedLayer(int NeuronsCount, int InputsCount, LayerType layerType, Function<double, double> activationFunction)
        {
            Neurons = new Neuron[NeuronsCount];
            Type = layerType;

            for (int neuron = 0; neuron < Neurons.Length; neuron++)
                Neurons[neuron] = new Neuron(InputsCount, layerType, activationFunction);

            OutputsCount = NeuronsCount;
            this.InputsCount = InputsCount;
            setLayerWeights();
        }

        public FullyConnectedLayer(FullyConnectedLayer layer)
        {
            Neurons = new Neuron[layer.Neurons.Length];

            for (int neuron = 0; neuron < Neurons.Length; neuron++)
                Neurons[neuron] = new Neuron(layer.Neurons[neuron]);

            Type = layer.Type;

            OutputsCount = Neurons.Length;
            InputsCount = layer.InputsCount;
            setLayerWeights();
        }

        #region public Methods

        public override double[] FeedForward(double[] inputs)
        {
            if (InputsCount != inputs.Length)
                throw new ArgumentException("Neurons of the layer don't accept the number of given inputs");

            if (Type == LayerType.InputLayer)
                return inputs;

            double[] outputs = new double[OutputsCount];

            for (int i = 0; i < Neurons.Length; i++)
                outputs[i] = Neurons[i].FeedForward(inputs);

            return outputs;
        }

        public override double[] BackPropagation(double[] errors)
        {
            if (OutputsCount != errors.Length)
                throw new ArgumentException("Errors vector doesn't match the layer neurons count");

            double[] deltas = new double[errors.Length];

            for (int i = 0; i < Neurons.Length; i++)
                deltas[i] = Neurons[i].BackPropagation(errors[i]);

            return deltas;
        }

        public override double[] BackPropagation(double[] nextLayerDeltas, double[][] nextLayerWeights)
        {
            if (Type == LayerType.InputLayer)
                return nextLayerDeltas;

            if (nextLayerDeltas.Length != nextLayerWeights.Length)
                throw new ArgumentException("Number of errors and neuron's weights doen't match");

            double[] thisLayerErrors = propagateErrors(nextLayerDeltas, nextLayerWeights);
            double[] thisLaterDeltas = new double[thisLayerErrors.Length];

            for (int i = 0; i < Neurons.Length; i++)
                thisLaterDeltas[i] = Neurons[i].BackPropagation(thisLayerErrors[i]);

            return thisLaterDeltas;
        }

        public override void UpdateDerivatives(double learningRate, double regularizationFactor, double trainingDatasetSize)
        {
            if (Type == LayerType.InputLayer)
                return;

            foreach (var neuron in Neurons)
                neuron.UpdateDerivatives(learningRate, regularizationFactor, trainingDatasetSize);
        }

        public override string ToString()
        {
            string res = $"{Type}, inputs {InputsCount}, outputs {OutputsCount}:\n";

            foreach (var neuron in Neurons)
            {
                res += "\n    " + neuron.ToString();
            }

            return res + "\n\n";
        }


        #endregion

        /// <summary>
        /// Back propagate the error
        /// </summary>
        /// <param name="nextLayerErrors">errors from the next layer</param>
        /// <param name="nextLayerWeights">weights from the next layer</param>
        /// <returns>appropriate errors for the layer</returns>
        double[] propagateErrors(double[] nextLayerErrors, double[][] nextLayerWeights)
        {
            double[] thisLayerErrors = new double[Neurons.Length];

            for (int neuron = 0; neuron < Neurons.Length; neuron++)
                for (int neuronNextLayer = 0; neuronNextLayer < nextLayerErrors.Length; neuronNextLayer++)
                    thisLayerErrors[neuron] += nextLayerErrors[neuronNextLayer] * nextLayerWeights[neuronNextLayer][neuron];

            return thisLayerErrors;
        }
    }
}

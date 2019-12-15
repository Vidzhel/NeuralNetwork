using System;
using System.Linq;

namespace NeuralNetworkLib
{
    [Serializable]
    public class Neuron
    {
        #region public Members

        public double[] Weights { get; set; }
        public double Bias { get; set; }
        public LayerType LayerType { get; private set; }
        public double[] LastInputs { get; private set; }
        public double LastOutput { get; private set; }
        public Function<double, double> ActivationFunction { get; private set; }

        #endregion

        #region private Members

        static int neuronsCount = 0;
        Random rnd;

        double[] weightDeltas;
        double biasDelta;
        int backPropagationsCount = 0;

        #endregion

        #region Constructors

        /// <summary>
        /// Randomly fills weights and biases
        /// </summary>
        public Neuron(int inputsCount, LayerType layerType, Function<double, double> ActivationFunction)
        {
            Weights = new double[inputsCount];
            LayerType = layerType;
            this.ActivationFunction = ActivationFunction;
            weightDeltas = new double[Weights.Length];

            neuronsCount++;
            rnd = new Random(neuronsCount);

            Bias = rnd.NextDouble() - 0.5;
            for (int i = 0; i < inputsCount; i++)
                Weights[i] = rnd.NextGaussian() / Math.Sqrt(inputsCount);
        }

        public Neuron(double[] weights, double bias, LayerType layerType, Function<double, double> ActivationFunction)
        {
            Weights = weights;
            Bias = bias;
            LayerType = layerType;
            this.ActivationFunction = ActivationFunction;
            weightDeltas = new double[Weights.Length];

            neuronsCount++;
        }

        public Neuron(Neuron neuron)
        {
            Weights = new double[neuron.Weights.Length];
            LayerType = neuron.LayerType;
            this.ActivationFunction = neuron.ActivationFunction;
            weightDeltas = new double[Weights.Length];

            neuronsCount++;

            Bias = neuron.Bias;

            for (int weight = 0; weight < Weights.Length; weight++)
                Weights[weight] = neuron.Weights[weight];
        }

        #endregion

        #region public methods

        public double FeedForward(double[] inputs)
        {
            if (inputs.Length != Weights.Length)
                throw new ArgumentException($"The neuron inputs count doesn't match the given inputs count, expected {Weights.Length}, got {inputs.Length}");

            LastInputs = inputs;
            double sum = 0;

            for (int i = 0; i < inputs.Length; i++)
                sum += inputs[i] * Weights[i];

            sum += Bias;

            LastOutput = sum;

            return ActivationFunction.Func(sum);
        }

        public double BackPropagation(double error)
        {
            double delta = calculateDelta(error);

            for (int i = 0; i < LastInputs.Length; i++)
            {
                double weightDelta = delta * LastInputs[i];
                weightDeltas[i] += weightDelta;
            }

            biasDelta += delta;

            backPropagationsCount++;

            return delta;
        }

        public void UpdateDerivatives(double learningRate, double regularizationFactor, double trainingDatasetSize)
        {
            backPropagationsCount = backPropagationsCount == 0 ? 1 : backPropagationsCount;

            for (int i = 0; i < LastInputs.Length; i++)
                //Weights[i] = Weights[i] - learningRate*weightDeltas[i]/backPropagationsCount;
                Weights[i] = (1 - learningRate * regularizationFactor / trainingDatasetSize) * Weights[i] - learningRate * weightDeltas[i] / backPropagationsCount;

            Bias -= learningRate * biasDelta / backPropagationsCount;

            if (double.IsNaN(Bias))
                throw new Exception();

            // Clear deltas and counter
            backPropagationsCount = 0;
            weightDeltas = new double[weightDeltas.Length];
            biasDelta = 0;
        }

        public override string ToString()
        {
            return $"{LayerType}, inputs: {Weights.Length}, bias {Bias}";
        }

        #endregion

        double calculateDelta(double error)
        {
            return error * ActivationFunction.DerivativeFunc(LastOutput);
        }
    }
}

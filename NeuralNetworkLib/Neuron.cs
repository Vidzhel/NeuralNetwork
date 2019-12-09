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

        #endregion

        #region private Members

        static int neuronsCount = 0;
        Function<double, double> activationFunction;
        Random rnd;

        double[] weightDeltas;
        double biasDelta;
        int backPropagationsCount = 0;

        #endregion

        #region Constructors

        /// <summary>
        /// Randomly fills weights and biases
        /// </summary>
        public Neuron(int inputsCount, LayerType layerType, Function<double, double> activationFunction)
        {
            Weights = new double[inputsCount];
            LayerType = layerType;
            this.activationFunction = activationFunction;
            weightDeltas = new double[Weights.Length];

            neuronsCount++;
            rnd = new Random(neuronsCount);

            Bias = rnd.NextDouble() - 0.5;
            for (int i = 0; i < inputsCount; i++)
                Weights[i] = rnd.NextGaussian() / Math.Sqrt(inputsCount);
        }

        public Neuron(double[] weights, double bias, LayerType layerType, Function<double, double> activationFunction)
        {
            Weights = weights;
            Bias = bias;
            LayerType = layerType;
            this.activationFunction = activationFunction;
            weightDeltas = new double[Weights.Length];

            neuronsCount++;
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

            return activationFunction.Func(sum);
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
            return error * activationFunction.DerivativeFunc(LastOutput);
        }
    }
}

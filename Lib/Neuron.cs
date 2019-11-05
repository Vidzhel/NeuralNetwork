using System;

namespace SymbolRecognitionLib
{
    public class Neuron
    {
        #region public Members

        public double[] Weights => weights;
        public double[] Biases => biases;

        #endregion

        #region private Members

        double[] weights;
        double[] biases;

        static long neuronsCount = 0;
        Random rnd;


        #endregion

        #region Constructors

        /// <summary>
        /// Randomly fills weights and biases
        /// </summary>
        public Neuron(long inputsCount)
        {
            weights = new double[inputsCount];
            biases = new double[inputsCount];

            neuronsCount++;
            rnd = new Random((int)neuronsCount);

            for (int i = 0; i < inputsCount; i++)
            {
                weights[i] = rnd.NextDouble();
                biases[i] = rnd.NextDouble();
            }
        }

        public Neuron(double[] weights, double[] biases)
        {
            SetComponents(weights, biases);

            neuronsCount++;
        }

        #endregion

        #region public methods

        /// <summary>
        /// Check and sets weights and biases
        /// </summary>
        public void SetComponents(double[] weights, double[] biases)
        {

            if (weights.Length != biases.Length)
                throw new ArgumentException("Arrays of weights and biases should be the same length");

            this.weights = weights;
            this.biases = biases;
        }

        public double FeedForward(double[] inputs)
        {
            if (inputs.Length != weights.Length)
                throw new ArgumentException("The neuron inputs count doesn't match the given inputs count");

            double sum = 0;

            for (long i = 0; i < inputs.Length; i++)
            {
                sum += inputs[i] * Weights[i] - Biases[i];
            }

            return activationFunction(sum);
        }

        #endregion

        double activationFunction(double value)
        {
            return 1 / (1 + Math.Exp(-value));
        }
    }
}

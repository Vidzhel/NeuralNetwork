﻿using System;
using System.Linq;

namespace NeuralNetworkLib
{
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

        static long neuronsCount = 0;
        Function<double, double> activationFunction;
        Random rnd;

        double[] weightDeltas;
        double biasDelta;
        long backPropagationsCount = 0;

        #endregion

        #region Constructors

        /// <summary>
        /// Randomly fills weights and biases
        /// </summary>
        public Neuron(long inputsCount, LayerType layerType, Function<double, double> activationFunction)
        {
            Weights = new double[inputsCount];
            LayerType = layerType;
            this.activationFunction = activationFunction;
            weightDeltas = new double[Weights.Length];

            neuronsCount++;
            rnd = new Random((int)neuronsCount);

            Bias = rnd.NextDouble() - 0.5;
            for (int i = 0; i < inputsCount; i++)
                Weights[i] = rnd.NextDouble() - 0.5;
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

            for (long i = 0; i < inputs.Length; i++)
                sum += inputs[i] * Weights[i];

            sum += Bias;

            LastOutput = sum;

            return activationFunction.Func(sum);
        }

        public double BackPropagation(double error, double learningRate)
        {
            double delta = calculateDelta(error);

            for (long i = 0; i < LastInputs.Length; i++)
            {
                double weightDelta = delta * learningRate * LastInputs[i];
                weightDeltas[i] += weightDelta;
            }

            biasDelta += learningRate * delta;

            backPropagationsCount++;

            return delta;
        }

        public void UpdateDerivatives()
        {
            backPropagationsCount = backPropagationsCount == 0 ? 1 : backPropagationsCount;

            for (long i = 0; i < LastInputs.Length; i++)
                Weights[i] -= weightDeltas[i] / backPropagationsCount;

            Bias -= biasDelta / backPropagationsCount;

            if (Double.IsNaN(Bias))
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
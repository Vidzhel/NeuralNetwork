using System;
using System.Collections.Generic;
using System.Linq;

namespace NeuralNetworkLib
{
    public class NeuralNetwork
    {
        #region pyblic Members

        public List<Layer> Layers;
        public List<Layer> ReversedLayers;
        public Function<double, double, double> CostFunction;

        #endregion

        #region Constructors

        /// <summary>
        /// Initialise a neural network with no layers
        /// </summary>
        public NeuralNetwork(Function<double, double, double> costFunction)
        {
            CostFunction = costFunction;
            Layers = new List<Layer>();
        }

        /// <summary>
        /// Initializes a neural network and generates layers
        /// </summary>
        /// <param name="size">the list contsins a count of neurons on each layer that will be generated</param>
        public NeuralNetwork(int[] size, Function<double, double, double> costFunction, Function<double, double> activationFunction)
        {
            CostFunction = costFunction;
            Layers = new List<Layer>();

            for (int layer = 0; layer < size.Length; layer++)
            {
                Layer newLayer;

                if (layer == 0)
                    newLayer = new Layer(size[layer], size[layer], LayerType.InputLayer, activationFunction);
                else if (layer == size.Length - 1)
                    newLayer = new Layer(size[layer], size[layer - 1], LayerType.OutputLayer, activationFunction);
                else
                    newLayer = new Layer(size[layer], size[layer - 1], LayerType.HiddenLayer, activationFunction);

                Layers.Add(newLayer);
            }
        }

        #endregion

        #region public methods

        /// <summary>
        /// Usses back propagation algorythm to change weigts and biases
        /// </summary>
        /// <param name="learningRate">the speed of learning</param>
        /// <param name="generations">generations count (loops over trainingData)</param>
        /// <param name="miniBatchSize">size of chunks into with the testing data will be split</param>
        /// <param name="testingInputs">if specified will be used to calculate accuracy after each generation</param>
        /// <param name="expectedTestingOutputs">if specified will be used to calculate accuracy after each generation</param>
        /// <returns></returns>
        public double Train(double[][] trainingInputs, double[][] expectedTrainingOutputs, float learningRate, int generations, int miniBatchSize, double[][] testingInputs = null, double[][] expectedTestingOutputs = null)
        {
            long trainingDataCount = trainingInputs.Length;

            Random rnd = new Random(miniBatchSize + generations);
            int[] trainingDataOrder = new int[trainingDataCount];
            trainingDataOrder = trainingDataOrder.Select((x, index) => index).ToArray();

            long miniBatchCount = (long)Math.Ceiling((double)trainingDataCount / miniBatchSize);

            for (int gen = 0; gen < generations; gen++)
            {
                // Shuffle training data
                trainingDataOrder = trainingDataOrder.OrderBy(x => rnd.Next(0, (int)trainingDataCount)).ToArray();


                // Process each train data in a batch and update neuron's weights and biases
                for (long miniBatch = 0; miniBatch < miniBatchCount; miniBatch++)
                {
                    for (int training = 0; training < miniBatchSize; training++)
                    {
                        long trainingSample = training + miniBatch * miniBatchSize;
                        if (trainingSample >= trainingDataCount)
                            break;

                        double[] inputs = trainingInputs[trainingDataOrder[trainingSample]];
                        double[] expectedOutputs = expectedTrainingOutputs[trainingDataOrder[trainingSample]];

                        double[] actualOutputs = Evaluate(inputs);

                        backPropagation(expectedOutputs, actualOutputs, learningRate);

                    }

                    updateWeights();
                }


                if (testingInputs != null && expectedTestingOutputs != null)
                    Console.WriteLine( $"Generation {gen + 1}: accuracy {GetAccuracy(testingInputs, expectedTestingOutputs, 0.01)}" );
                else
                    Console.WriteLine( $"Generation {gen + 1}" );
            }

            return GetAccuracy(testingInputs, expectedTestingOutputs, 0.01);
        }

        public double[] Evaluate(double[] inputs)
        {
            checkLayers();

            if (inputs.Length != Layers[0].InputsCount)
                throw new Exception("Count of given inputs doesn't match the imput layer inputs count");

            foreach (var layer in Layers)
                inputs = layer.FeedForward(inputs);

            return inputs;
        }

        public double GetAccuracy(double[][] inputs, double[][] expectedOutputs, double tolerance = 0.5)
        {
            long loops = inputs.GetLength(0);
            long inputsCount = inputs[0].Length;
            long outputsCount = expectedOutputs[0].Length;

            if (inputsCount != Layers[0].InputsCount)
                throw new Exception("Count of given inputs doesn't match the imput layer inputs count");

            if (outputsCount != Layers.Last().OutputsCount)
                throw new Exception("Count of given expected outputs doesn't match the output layer outputs count");

            if (loops != expectedOutputs.GetLength(0))
                throw new Exception("The size of inputs and expected outputs doesn't match");

            double accuracy = 0;
            double accuracyStep = 100.0 / (outputsCount * loops);

            for (long i = 0; i < loops; i++)
            {
                double[] actualOutputs = Evaluate(inputs[i]);

                for (long output = 0; output < outputsCount; output++)
                    if (Math.Abs(actualOutputs[output] - expectedOutputs[i][output]) < tolerance)
                        accuracy += accuracyStep;
            }

            return accuracy;
        }

        public override string ToString()
        {
            string res = "Neural Network:\n";

            foreach (var layer in Layers)
                res += layer.ToString();

            return res;
        }

        #endregion

        #region private methods

        /// <summary>
        /// Checks if a Neural Network has appropriate structures of layers
        /// </summary>
        void checkLayers()
        {
            if (Layers.Count < 2)
                throw new Exception("There should be at least one layer in a neuron network");

            if (Layers[0].Type != LayerType.InputLayer)
                throw new Exception("First layer should be input layer");

            if (Layers.Last().Type != LayerType.OutputLayer)
                throw new Exception("Last layer should be output layer");

            int inputLayers = Layers.Count(isInputLayer);
            if (inputLayers > 1)
                throw new Exception("There should be only one input layer, actual " + inputLayers);

            int outputLayers = Layers.Count(isOutputLayer);
            if (outputLayers > 1)
                throw new Exception("There should be only one output layer, actual " + outputLayers);

            for (int i = 1; i < Layers.Count; i++)
            {
                Layer previousLayer = Layers[i - 1];
                Layer nextLayer = Layers[i];

                if (previousLayer.OutputsCount != nextLayer.InputsCount)
                    throw new Exception($"The count of outputs on the {i - 1}th layer doesn't match the count of inputs on the {i}th layer\n\nFirst Layer:{previousLayer}Second Layer:{nextLayer}");
            }
        }

        bool isInputLayer(Layer layer)
        {
            if (layer.Type == LayerType.InputLayer)
                return true;

            return false;
        }

        bool isOutputLayer(Layer layer)
        {
            if (layer.Type == LayerType.OutputLayer)
                return true;

            return false;
        }

        double calculateCost(double[] expectedRes, double[] actualRes)
        {
            if (expectedRes.Length != actualRes.Length)
                throw new Exception("Expected and actual values vectors should be the same length");

            double cost = 0;

            for (long i = 0; i < expectedRes.Length; i++)
            {
                cost += CostFunction.Func(actualRes[i], expectedRes[i]);
            }

            return cost;
        }

        void backPropagation(double[] expectedRes, double[] actualRes, double learningRate)
        {
            double[] errors = calculateCostGradient(expectedRes, actualRes);
            double[] deltas = Layers.Last().BackPropagation(errors, learningRate);
            
            for (int layer = Layers.Count - 2; layer > -1; layer--)
            {
                double[][] previousLayerWeights = Layers[layer + 1].Weights;
                deltas = Layers[layer].BackPropagation(deltas, previousLayerWeights, learningRate);
            }
        }

        double[] calculateCostGradient(double[] expectedRes, double[] actualRes)
        {
            double[] gradient = new double[expectedRes.Length];

            for (long i = 0; i < expectedRes.Length; i++)
                gradient[i] = CostFunction.DerivativeFunc(expectedRes[i], actualRes[i]);

            return gradient;
        }

        void updateWeights()
        {
            foreach (var layer in Layers)
                layer.UpdateDerivatives();
        }

        #endregion
    }
}

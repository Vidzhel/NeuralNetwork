using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Threading;
using System.Threading.Tasks;

namespace NeuralNetworkLib
{
    public class TrainingData
    {
        public double[][] trainingInputs { get; private set; }
        public double[][] expectedTrainingOutputs { get; private set; }

        public double[][] testingInputs { get; private set; }
        public double[][] expectedTestingOutputs { get; private set; }

        public float learningRate { get; private set; }
        public int generations { get; private set; }
        public int miniBatchSize { get; private set; }
        public double regularizationFactor { get; private set; }
        public double accuracyTolerance { get; private set; }

        public CancellationToken token { get; private set; }

        public bool monitorTrainingDataCost { get; private set; }
        public bool monitorTrainingDataAccuracy { get; private set; }
        public bool monitorTestingDataCost { get; private set; }
        public bool monitorTestingDataAccuracy { get; private set; }

        public int trainingDataCount { get; private set; }
        public Random rnd { get; private set; }
        public int[] trainingDataOrder { get; private set; }
        public int miniBatchCount { get; private set; }

        public Function<double, double, double> CostFunctions;

        public TrainingData(double[][] trainingInputs, double[][] expectedTrainingOutputs, float learningRate, int generations, int miniBatchSize, CancellationToken token, Function<double, double, double> costFunctions, double[][] testingInputs = null, double[][] expectedTestingOutputs = null, double regularizationFactor = 0.0,
            bool monitorTrainingDataCost = false, bool monitorTrainingDataAccuracy = false, bool monitorTestingDataCost = false, bool monitorTestingDataAccuracy = false, double accuracyTolerance = 0.01)
        {
            this.trainingInputs = trainingInputs;
            this.expectedTrainingOutputs = expectedTrainingOutputs;

            this.testingInputs = testingInputs;
            this.expectedTestingOutputs = expectedTestingOutputs;

            this.learningRate = learningRate;
            this.generations = generations;
            this.miniBatchSize = miniBatchSize;
            this.token = token;
            this.regularizationFactor = regularizationFactor;
            this.accuracyTolerance = accuracyTolerance;

            this.monitorTrainingDataCost = monitorTrainingDataCost;
            this.monitorTrainingDataAccuracy = monitorTrainingDataAccuracy;
            this.monitorTestingDataCost = monitorTestingDataCost;
            this.monitorTestingDataAccuracy = monitorTestingDataAccuracy;

            CostFunctions = costFunctions;

            initData();
        }

        public void initData()
        {
            trainingDataCount = trainingInputs.Length;

            rnd = new Random(miniBatchSize + generations);
            trainingDataOrder = new int[trainingDataCount];
            trainingDataOrder = trainingDataOrder.Select((x, index) => index).ToArray();

            miniBatchCount = (int)Math.Ceiling((double)trainingDataCount / miniBatchSize);
        }

        public void ShaffleTrainingDataOrder()
        {
            trainingDataOrder = trainingDataOrder.OrderBy(x => rnd.Next(0, (int)trainingDataCount)).ToArray();
        }
    }

    [Serializable]
    public class NeuralNetwork
    {
        #region public Members

        public List<Layer> Layers;

        #endregion

        #region Constructors

        /// <summary>
        /// Initialise a neural network with no layers
        /// </summary>
        public NeuralNetwork()
        {
            Layers = new List<Layer>();
        }

        /// <summary>
        /// Initializes a neural network and generates layers
        /// </summary>
        /// <param name="size">the list contsins a count of neurons on each layer that will be generated</param>
        public NeuralNetwork(int[] size, Function<double, double> activationFunction)
        {
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

        public NeuralNetwork(NeuralNetwork net)
        {
            Layers = new List<Layer>();

            foreach (var layer in net.Layers)
                Layers.Add(new Layer(layer));
        }

        #endregion

        #region public methods

        /// <summary>
        /// Uses back propagation algorythm to change weigts and biases.
        /// Each iteration returns monitor data in the array with the size of 5, where values can be null in the case the monitor
        /// flags are set to false. Monitor data in the array follows the next order: generation, trainingDataCost, trainingDataAccuracy, testingDataCost, testingDataAccuracy
        /// </summary>
        /// <param name="learningRate">the speed of learning</param>
        /// <param name="generations">generations count (loops over trainingData)</param>
        /// <param name="miniBatchSize">size of chunks into with the testing data will be split</param>
        /// <param name="testingInputs">if specified will be used to calculate accuracy after each generation</param>
        /// <param name="expectedTestingOutputs">if specified will be used to calculate accuracy after each generation</param>
        /// <param name="regularizationFactor">controls the growth of the weights, the bigger factor, the closer weights will be to zero</param>
        /// <returns>Each iteration returns monitor data in the array with the size of 5, where values can be null in the case the monitor
        /// flags are set to false. Monitor data in the array follows the next order: generation, trainingDataCost, trainingDataAccuracy, testingDataCost, testingDataAccuracy</returns>
        public IEnumerable<Dictionary<string, double>> Train(double[][] trainingInputs, double[][] expectedTrainingOutputs, float learningRate, int generations, int miniBatchSize, CancellationToken token, Function<double, double, double> costFunctions, double[][] testingInputs = null, double[][] expectedTestingOutputs = null, double regularizationFactor = 0.0,
            bool monitorTrainingDataCost = false, bool monitorTrainingDataAccuracy = false, bool monitorTestingDataCost = false, bool monitorTestingDataAccuracy = false, double accuracyTolerance = 0.01)
        {
            checkLayers();

            token.ThrowIfCancellationRequested();
            TrainingData trainingData = new TrainingData(trainingInputs, expectedTrainingOutputs, learningRate, generations, miniBatchSize, token, costFunctions, testingInputs, expectedTestingOutputs, regularizationFactor, monitorTrainingDataCost, monitorTrainingDataAccuracy, monitorTestingDataCost, monitorTestingDataAccuracy, accuracyTolerance);

            for (int gen = 0; gen < generations; gen++)
            {
                var monitorData = trainGenerationAsync(trainingData, token).Result;

                if (monitorData == null)
                    throw new OperationCanceledException();

                monitorData["generation"] = gen;

                yield return monitorData;
            }

        }

        public IEnumerable<Dictionary<string, double>> Train(TrainingData trainingData, CancellationToken token)
        {
            checkLayers();

            token.ThrowIfCancellationRequested();

            for (int gen = 0; gen < trainingData.generations; gen++)
            {
                var monitorData = trainGenerationAsync(trainingData, token).Result;

                if (monitorData == null)
                    throw new OperationCanceledException();

                monitorData["generation"] = gen;

                yield return monitorData;
            }
        }

        #region Train helpers


        Task<Dictionary<string, double>> trainGenerationAsync(TrainingData trainingData, CancellationToken? token = null)
        {
            return Task.Run<Dictionary<string, double>>(() => trainGeneration(trainingData, token));
        }

        Dictionary<string, double> trainGeneration(TrainingData trainingData, CancellationToken? token = null)
        {
            try
            {
                trainingData.ShaffleTrainingDataOrder();

                // Process each train data in a batch and update neuron's weights and biases
                for (int miniBatch = 0; miniBatch < trainingData.miniBatchCount; miniBatch++)
                {
                    for (int training = 0; training < trainingData.miniBatchSize; training++)
                    {
                        int trainingSample = training + miniBatch * trainingData.miniBatchSize;
                        if (trainingSample >= trainingData.trainingDataCount)
                            break;

                        token?.ThrowIfCancellationRequested();

                        feedAndPropagate(trainingData, trainingSample, trainingData.CostFunctions);

                        token?.ThrowIfCancellationRequested();
                    }

                    updateWeights(trainingData.learningRate, trainingData.regularizationFactor, trainingData.trainingDataCount);
                }


                return prepareMonitoringData(trainingData, token);
            }

            catch (OperationCanceledException e)
            {
                return null;
            }
        }

        void feedAndPropagate(TrainingData trainingData, int trainingSample, Function<double, double, double> costFunction)
        {
            double[] inputs = trainingData.trainingInputs[trainingData.trainingDataOrder[trainingSample]];
            double[] expectedOutputs = trainingData.expectedTrainingOutputs[trainingData.trainingDataOrder[trainingSample]];

            double[] actualOutputs = Evaluate(inputs);

            backPropagation(expectedOutputs, actualOutputs, costFunction);
        }

        Dictionary<string, double> prepareMonitoringData(TrainingData trainingData, CancellationToken? token)
        {
            Dictionary<string, double> monitorData = new Dictionary<string, double>();


            token?.ThrowIfCancellationRequested();

            if (trainingData.monitorTrainingDataCost)
                monitorData["trainingDataCost"] = getCost(trainingData.trainingInputs, trainingData.expectedTrainingOutputs, trainingData.CostFunctions);

            token?.ThrowIfCancellationRequested();

            if (trainingData.monitorTrainingDataAccuracy)
                monitorData["trainingDataAccuracy"] = getAccuracy(trainingData.trainingInputs, trainingData.expectedTrainingOutputs, trainingData.accuracyTolerance);

            token?.ThrowIfCancellationRequested();

            if (trainingData.monitorTestingDataCost && trainingData.testingInputs != null && trainingData.expectedTestingOutputs != null)
                monitorData["testingDataCost"] = getCost(trainingData.testingInputs, trainingData.expectedTestingOutputs, trainingData.CostFunctions);

            token?.ThrowIfCancellationRequested();

            if (trainingData.monitorTestingDataAccuracy && trainingData.testingInputs != null && trainingData.expectedTestingOutputs != null)
                monitorData["testingDataAccuracy"] = getAccuracy(trainingData.testingInputs, trainingData.expectedTestingOutputs, trainingData.accuracyTolerance);

            token?.ThrowIfCancellationRequested();

            return monitorData;
        }

        #endregion

        public double[] Evaluate(double[] input)
        {
            checkLayers();

            if (input.Length != Layers[0].InputsCount)
                throw new Exception("Count of given inputs doesn't match the imput layer inputs count");

            return evaluate(input);
        }

        public double[][] Evaluate(double[][] inputs)
        {
            checkLayers();
            if (inputs.First().Length != Layers[0].InputsCount)
                throw new Exception("Count of given inputs doesn't match the imput layer inputs count");

            return evaluate(inputs);
        }

        public double GetAccuracy(double[][] inputs, double[][] expectedOutputs, double tolerance = 0.5)
        {
            int loops = inputs.GetLength(0);
            int inputsCount = inputs[0].Length;
            int outputsCount = expectedOutputs[0].Length;

            if (inputsCount != Layers[0].InputsCount)
                throw new Exception("Count of given inputs doesn't match the imput layer inputs count");

            if (outputsCount != Layers.Last().OutputsCount)
                throw new Exception("Count of given expected outputs doesn't match the output layer outputs count");

            if (loops != expectedOutputs.GetLength(0))
                throw new Exception("The size of inputs and expected outputs doesn't match");

            return getAccuracy(inputs, expectedOutputs, tolerance);
        }

        public double GetCost(double[][] inputs, double[][] expectedOutputs, Function<double, double, double> costFunction)
        {
            int loops = inputs.GetLength(0);
            int inputsCount = inputs[0].Length;
            int outputsCount = expectedOutputs[0].Length;

            if (inputsCount != Layers[0].InputsCount)
                throw new Exception("Count of given inputs doesn't match the imput layer inputs count");

            if (outputsCount != Layers.Last().OutputsCount)
                throw new Exception("Count of given expected outputs doesn't match the output layer outputs count");

            if (loops != expectedOutputs.GetLength(0))
                throw new Exception("The size of inputs and expected outputs doesn't match");

            return getCost(inputs, expectedOutputs, costFunction);
        }

        public void Save(string filePath)
        {
            using (FileStream stream = new FileStream(filePath, FileMode.Create, FileAccess.Write))
            {
                BinaryFormatter formatter = new BinaryFormatter();
                formatter.Serialize(stream, this);
            }
        }

        /// <exception cref="FileNotFoundException">Will be throwen if the file doesn't exist</exception>
        /// <exception cref="System.Runtime.Serialization.SerializationException">Will be throwen if the file data corrupted</exception>
        public static NeuralNetwork Load(string filePath)
        {

            using (FileStream stream = new FileStream(filePath, FileMode.Open, FileAccess.Read))
            {
                BinaryFormatter formatter = new BinaryFormatter();
                return (NeuralNetwork)formatter.Deserialize(stream);
            }
        }

        public override string ToString()
        {
            string res = "Neural Network:\n";

            foreach (var layer in Layers)
                res += layer.ToString();

            return res;
        }

        #endregion

        #region helpers

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

        #endregion

        #region private methods

        double[] evaluate(double[] input)
        {
            foreach (var layer in Layers)
                input = layer.FeedForward(input);

            return input;
        }

        double[][] evaluate(double[][] inputs)
        {
            double[][] outputs = new double[inputs.Length][];

            for (int input = 0; input < inputs.Length; input++)
                outputs[input] = evaluate(inputs[input]);

            return outputs;
        }

        double getAccuracy(double[][] inputs, double[][] expectedOutputs, double tolerance = 0.5)
        {
            int loops = inputs.GetLength(0);
            int outputsCount = expectedOutputs[0].Length;

            double accuracy = 0;
            double accuracyStep = 100.0 / (outputsCount * loops);

            for (int i = 0; i < loops; i++)
            {
                double[] actualOutputs = Evaluate(inputs[i]);

                for (int output = 0; output < outputsCount; output++)
                    if (Math.Abs(actualOutputs[output] - expectedOutputs[i][output]) < tolerance)
                        accuracy += accuracyStep;
            }

            return accuracy;
        }

        double getCost(double[][] inputs, double[][] expectedOutputs, Function<double, double, double> costFunction)
        {
            int loops = inputs.GetLength(0);

            double totalCost = 0;

            for (int i = 0; i < loops; i++)
            {
                double[] actualOutputs = Evaluate(inputs[i]);
                totalCost += calculateCost(expectedOutputs[i], actualOutputs, costFunction);
            }

            return totalCost / loops;
        }

        double calculateCost(double[] expectedRes, double[] actualRes, Function<double, double, double> costFunction)
        {
            if (expectedRes.Length != actualRes.Length)
                throw new Exception("Expected and actual values vectors should be the same length");

            double cost = 0;

            for (int i = 0; i < expectedRes.Length; i++)
            {
                cost += costFunction.Func(expectedRes[i], actualRes[i]);
            }

            return cost;
        }

        void backPropagation(double[] expectedRes, double[] actualRes, Function<double, double, double> costFunction)
        {
            double[] errors = calculateCostGradient(expectedRes, actualRes, costFunction);
            double[] deltas = Layers.Last().BackPropagation(errors);

            for (int layer = Layers.Count - 2; layer > -1; layer--)
            {
                double[][] previousLayerWeights = Layers[layer + 1].Weights;
                deltas = Layers[layer].BackPropagation(deltas, previousLayerWeights);
            }
        }

        double[] calculateCostGradient(double[] expectedRes, double[] actualRes, Function<double, double, double> costFunction)
        {
            double[] gradient = new double[expectedRes.Length];

            for (int i = 0; i < expectedRes.Length; i++)
                gradient[i] = costFunction.DerivativeFunc(expectedRes[i], actualRes[i]);

            return gradient;
        }

        void updateWeights(double learningRate, double regularizationFactor, double trainingDatasetSize)
        {
            foreach (var layer in Layers)
                layer.UpdateDerivatives(learningRate, regularizationFactor, trainingDatasetSize);
        }

        #endregion
    }
}

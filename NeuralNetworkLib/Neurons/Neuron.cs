using NeuralNetworkLib.Layers;
using System;

namespace NeuralNetworkLib.Neurons
{
    [Serializable]
    public class Neuron : BaseNeuron<float>
    {

        #region Constructors

        /// <summary>
        /// Randomly fills weights and biases
        /// </summary>
        public Neuron(int inputsCount, LayerType layerType, Function<float, float> activationFunction) : base(layerType, activationFunction)
        {
            Shape weightsMatrixShape = new Shape(new int[] { inputsCount });
            @params.Weights = new MultiDimArr(weightsMatrixShape);
            @params.Weights.FillMatrixRandomly();
            @params.weightDeltas = new MultiDimArr(weightsMatrixShape);
        }

        public Neuron(MultiDimArr weights, float bias, LayerType layerType, Function<float, float> activationFunction) : base(weights, bias, layerType, activationFunction) { }

        public Neuron(Neuron neuron) : base(neuron) { }

        #endregion

        #region public methods

        /// <summary>
        /// Returns activeted and weighted input
        /// </summary>
        public override float FeedForward(MultiDimArr inputs)
        {
            LastInputs = inputs;
            float sum = (inputs * @params.Weights).Sum() + @params.Bias;
            LastOutput = sum;

            return @params.ActivationFunction.Func(sum);
        }

        public override float BackPropagation(float error)
        {
            float delta = calculateDelta(error);

            @params.weightDeltas = @params.weightDeltas + (LastInputs * delta);
            @params.biasDelta += delta;

            backPropagationsInRow++;

            return delta;
        }

        /// <summary>
        /// Updates parameters, L2 regularization
        /// </summary>
        /// <param name="learningRate">represents speed of learning (the bigger the faster learning and the less precise will be training result)</param>
        /// <param name="regularizationFactor">L2 regularization factor. The bigger factor the bigger will be resulting weights</param>
        public override void UpdateDerivatives(float learningRate, float regularizationFactor, int trainingDatasetSize)
        {
            backPropagationsInRow = backPropagationsInRow == 0 ? 1 : backPropagationsInRow;

            float regularization = 1 - learningRate * regularizationFactor / trainingDatasetSize;
            MultiDimArr meanDelta = @params.weightDeltas * learningRate / backPropagationsInRow;
            @params.Weights = @params.Weights * regularization - meanDelta;

            @params.Bias -= learningRate * @params.biasDelta / backPropagationsInRow;

            // Clear deltas and counter
            backPropagationsInRow = 0;
            @params.weightDeltas = new MultiDimArr(@params.weightDeltas.shape);
            @params.biasDelta = 0;
        }

        public override string ToString()
        {
            return $"Neuron: {@params.LayerType}, inputs: {@params.Weights.Length}, activation: {@params.ActivationFunction.Name}";
        }

        #endregion

        float calculateDelta(float error)
        {
            return error * @params.ActivationFunction.DerivativeFunc(LastOutput);
        }
    }
}

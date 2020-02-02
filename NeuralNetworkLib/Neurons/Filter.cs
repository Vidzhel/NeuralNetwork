using NeuralNetworkLib.Layers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NeuralNetworkLib.Neurons
{
    [Serializable]
    public class Filter : BaseNeuron<MultiDimArr>
    {
        Shape shape;
        Shape step;

        #region Constructors
        /// <summary>
        /// Randomly fills weights and biases
        /// </summary>
        /// <param name="step">stride</param>
        public Filter(Shape shape, Shape step, LayerType layerType, Function<float, float> ActivationFunction) : base(layerType, ActivationFunction)
        {
            if (shape.DimensionsCount > 3)
                throw new ArgumentException("Fileters can't have more than 3 dimensions");
            
            this.step = step;
            this.shape = shape;
            @params.Weights = new MultiDimArr(shape);
            @params.Weights.FillMatrixRandomly();
            @params.weightDeltas = new MultiDimArr(shape);
        }

        public Filter(MultiDimArr weights, float bias, LayerType layerType, Function<float, float> activationFunction) : base(weights, bias, layerType, activationFunction) { }

        public Filter(Filter neuron) : base(neuron) { }

        #endregion

        #region public methods

        public override MultiDimArr FeedForward(MultiDimArr inputs)
        {
            int resRows = shape[0] - inputs.shape[0] + 1;
            int resCols = shape[1] - inputs.shape[1] + 1;

            Shape resShape = new Shape(new int[] { resRows, resCols });
            float[] resData = new float[resShape.ElementsCount];

            int i = 0;
            foreach (var submatrix in inputs.IterateOverSubMatrices(shape, step))
                resData[i] = (submatrix * @params.Weights).Sum() + @params.Bias;

            MultiDimArr output = new MultiDimArr(resData, resShape);

            LastOutput = output;

            return output.Map(@params.ActivationFunction.Func);
        }

        public override MultiDimArr BackPropagation(MultiDimArr errors)
        {
            MultiDimArr deltas = calculateDelta(errors);

            foreach (var submatrix in LastInputs.IterateOverSubMatrices(shape, step))
                @params.weightDeltas = @params.weightDeltas + (submatrix * deltas);
            @params.biasDelta += deltas.Sum();

            backPropagationsInRow++;

            return deltas;
        }

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
            return $"Filter (Neuron): {@params.LayerType}, shape: {@params.Weights.shape}, activation: {@params.ActivationFunction.Name}";
        }

        #endregion

        MultiDimArr calculateDelta(MultiDimArr error)
        {
            return LastOutput.Map(@params.ActivationFunction.DerivativeFunc) * error;
        }
    }
}

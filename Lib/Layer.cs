using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SymbolRecognitionLib
{
    enum LayerTypes
    {
        InputLayer

    }

    class Layer
    {
        #region public Members

        public Neuron[] Neurons => neurons;
        public long OutputsCount => outputsCount;
        public long InputsCount => inputsCount;

        #endregion

        #region private Members

        Neuron[] neurons;
        long outputsCount;
        long inputsCount;

        #endregion

        /// <param name="neuronsCount">count of outputs</param>
        /// <param name="inputsCount">count of neurons on the previous layer</param>
        public Layer(long neuronsCount, long inputsCount)
        {
            neurons = new Neuron[neuronsCount];

            for (long i = 0; i < neurons.Length; i++)
                neurons[i] = new Neuron(inputsCount);

            outputsCount = neuronsCount;
            this.inputsCount = inputsCount;
        }

        #region public Methods

        public double[] FeedForward(double[] inputs)
        {
            if (inputsCount != inputs.Length)
                throw new ArgumentException("Neurons of the layer don't accept the number of given inputs");

            double[] outputs = new double[outputsCount];

            for (long i = 0; i < neurons.Length; i++)
            {
                outputs[i] = neurons[i].FeedForward(inputs);
            }

            return outputs;
        }

        #endregion
    }
}

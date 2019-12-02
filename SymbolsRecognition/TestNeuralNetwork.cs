using SymbolRecognitionLib;
using System;
using NeuralNetworkLib;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SymbolsRecognition
{
    class TestNeuralNetwork
    {
        static void Main(string[] args)
        {
            NeuralNetwork network = new NeuralNetwork( new int[] { 784, 30, 10}, Function<double, double, double>.MeanSquareCost, Function<double, double>.SigmoidFunction);

            double[][] trainingImages;
            double[][] testingImages;

            double[][] trainingLables;
            double[][] testingLabels;

            MNISTDataLoader.PrepeareData("..//..//..//MNISTDataset", out trainingImages, out testingImages, out trainingLables, out testingLabels, 500, 5000);

            network.Train(trainingImages, trainingLables, learningRate: 3F, generations: 30, miniBatchSize: 10, testingImages, testingLabels);

            Console.WriteLine("Done");
            Console.ReadLine();
        }
    }
}

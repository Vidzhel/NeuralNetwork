using NeuralNetworkLib;
using SymbolRecognitionLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SymbolsRecognitionLib
{
    public class TestNeuralNetwork
    {
        public static void Test()
        {
            NeuralNetwork network = new NeuralNetwork(new int[] { 784, 30, 10 }, Function<double, double, double>.MeanSquareCost, Function<double, double>.SigmoidFunction);

            double[][] trainingImages;
            double[][] testingImages;

            double[][] trainingLables;
            double[][] testingLabels;

            MNISTDataLoader.PrepeareData("..//..//..//MNISTDataset", out trainingImages, out testingImages, out trainingLables, out testingLabels);

            network.Train(trainingImages, trainingLables, learningRate: 3F, generations: 15, miniBatchSize: 10, testingImages, testingLabels);
            Console.WriteLine(network.GetAccuracy(testingImages, testingLabels));

            Console.WriteLine("Saveing network");
            network.Save("network.net");
            
            Console.WriteLine("Opening network");
            var newNetwork = NeuralNetwork.Load("network.net");
            
            Console.WriteLine("Testing network");
            Console.WriteLine(newNetwork.GetAccuracy(testingImages, testingLabels));

            Console.WriteLine("Done");
        }

        public static void TestSymbolRecognition(double[][] inputs)
        {
            var network = NeuralNetwork.Load("network.net");

            double[][] outputs = network.Evaluate(inputs);

            foreach (var output in outputs)
            {
                double maxVal = double.MinValue;
                int resultNum = 0;

                for (int value = 0; value < output.Length; value++)
                    if (output[value] > maxVal)
                    {
                        maxVal = output[value];
                        resultNum = value;
                    }

                Console.WriteLine("Res: " + resultNum);
            }
        } 
    }
}

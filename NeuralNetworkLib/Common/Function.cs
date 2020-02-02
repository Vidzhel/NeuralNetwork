using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NeuralNetworkLib
{
    /// <summary>
    /// Stores a normal and derivative version of function
    /// </summary>
    [Serializable]
    public class Function<TArg, TRes>
    {
        #region Default

        static public Function<double, double> SigmoidFunction = new Function<double, double>("Sigmoid activation function", (x) => 1.0 / (1 + Math.Exp(-x)), (x) =>
        {
            double sigm = 1.0 / (1.0 + Math.Exp(-x));

            return sigm * (1 - sigm);
        });
        static public Function<double, double> ReLu = new Function<double, double>("Sigmoid activation function", (x) => Math.Max(0.0, x), (x) => x <= 0 ? 0 : 1);
        static public Function<double, double> Linear = new Function<double, double>("Sigmoid activation function", (x) => x, (x) => 1);

        #endregion

        public string Name { get; }
        public Func<TArg, TRes> Func { get; }
        public Func<TArg, TRes> DerivativeFunc { get; }

        public Function(string name, Func<TArg, TRes> function, Func<TArg, TRes> derivative)
        {
            Name = name;
            Func = function;
            DerivativeFunc = derivative;
        }

    }

    /// <summary>
    /// Stores a normal and derivative version of function
    /// </summary>
    [Serializable]
    public class Function<TArg, TArg2, TRes>
    {
        #region Default

        static public Function<double, double, double> MeanSquareCost = new Function<double, double, double>("Mean square cost function", (expected, actual) => Math.Pow(actual - expected, 2) / 2.0, (expected, actual) => actual - expected);
        static public Function<double, double, double> CrossEntropy = new Function<double, double, double>("Cross entropy cost function", (expected, actual) => -expected * Math.Log10(actual) - (1 - expected) * Math.Log10(1 - actual), (expected, actual) => -expected / actual + (1 - expected) / (1 - actual));

        #endregion

        public string Name { get; }
        public Func<TArg, TArg2, TRes> Func { get; }
        public Func<TArg, TArg2, TRes> DerivativeFunc { get; }

        public Function(string name, Func<TArg, TArg2, TRes> function, Func<TArg, TArg2, TRes> derivative)
        {
            Name = name;
            Func = function;
            DerivativeFunc = derivative;
        }

    }

}

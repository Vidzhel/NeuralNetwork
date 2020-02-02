using System;
using System.Collections.Generic;

namespace NeuralNetworkLib
{
    /// <summary>
    /// The object represents the size of a multi-dimensional array
    /// </summary>
    [Serializable]
    public class Shape
    {
        public IReadOnlyCollection<int> Dimensions { get; private set; }
        private int[] dimensions;
        public int ElementsCount { get; private set; }
        public int DimensionsCount { get; private set; }

        #region Constructors

        public Shape(int dimensionsCount)
        {
            dimensions = new int[dimensionsCount];
            Dimensions = Array.AsReadOnly(dimensions);

            Fill(0);
            ElementsCount = getSize();
            DimensionsCount = Dimensions.Count;
        }

        public Shape(List<int> dimensions)
        {
            this.dimensions = dimensions.ToArray();
            Dimensions = Array.AsReadOnly(this.dimensions);

            ElementsCount = getSize();
            DimensionsCount = Dimensions.Count;
        }

        public Shape(int[] dimensions)
        {
            this.dimensions = dimensions;
            Dimensions = Array.AsReadOnly(this.dimensions);

            ElementsCount = getSize();
            DimensionsCount = Dimensions.Count;
        }

        public Shape(Shape shape)
        {
            dimensions = new int[shape.DimensionsCount];
            Dimensions = Array.AsReadOnly(dimensions);

            for (int i = 0; i < shape.DimensionsCount; i++)
                dimensions[i] = shape[i];
        }

        #endregion

        public void Fill(int number)
        {
            for (int i = 0; i < DimensionsCount; i++)
                dimensions[i] = number;
        }

        public Shape AddDimensionsBack(int[] dimensions)
        {
            int[] newDims = new int[dimensions.Length + this.dimensions.Length];

            dimensions.CopyTo(newDims, 0);
            this.dimensions.CopyTo(newDims, dimensions.Length);

            return new Shape(newDims);
        }

        public Shape AddDimensionsBack(int dimension)
        {
            int[] newDims = new int[1 + dimensions.Length];

            newDims[0] = dimension;
            dimensions.CopyTo(newDims, 1);

            return new Shape(newDims);
        }

        public int this[int index] => dimensions[index];

        public IEnumerable<Shape> IterateShape(Shape start = null, Shape step = null, bool keepNewLineIndentation = false)
        {
            if (keepNewLineIndentation && start == null)
                throw new ArgumentException("Start shape has to be specified in the case if keepNewLineIndentation flag is true");

            if (step == null)
            {
                var dims = new int[DimensionsCount];
                dims[DimensionsCount - 1] = 1;
                step = new Shape(dims);
            }
            else if (step.DimensionsCount != DimensionsCount)
                throw new ArgumentException("Given step's shape dimensions count doesn't equal to the dimensions count");

            Shape shape;
            if (start == null)
            {
                shape = new Shape(DimensionsCount);
            }
            else
            {
                if (start.DimensionsCount != DimensionsCount)
                    throw new ArgumentException("Given starts's shape dimensions count doesn't equal to the dimensions count");

                shape = new Shape(start);
            }

            while (wrapDimensionsIfPossible(ref shape, start, keepNewLineIndentation))
            {
                yield return shape;
                shape += step;
            }
        }

        private bool wrapDimensionsIfPossible(ref Shape shapeToCheck, Shape newLineStart, bool lineBreakFromStart)
        {
            bool isNotOverflow = false;

            for (int i = 0; i < shapeToCheck.DimensionsCount; i++)
            {
                // If we don't need to carry a dimension
                // If there isn't an overflow
                if (shapeToCheck[i] < this[i])
                    continue;

                // If in the first dimension is overflow, then we are done
                if (i == 0)
                    continue;

                if (lineBreakFromStart)
                    // Start from the start of the dim
                    shapeToCheck.dimensions[i] = 0;
                else
                    // Start line from the same position as start
                    // To keep shape
                    shapeToCheck.dimensions[i] = newLineStart[i];

                // Carry a dim
                shapeToCheck.dimensions[i - 1]++;

                isNotOverflow = true;
            }

            return isNotOverflow;
        }

        /// <summary>
        /// Returns true in the case if the shape includes given shape
        /// </summary>
        /// <param name="shape">shape that have the same dimensions count as the object</param>
        /// <returns></returns>
        public bool IsSubmatrix(Shape shape)
        {
            if (shape.DimensionsCount != DimensionsCount)
                throw new ArgumentException("Dimensions count of the given shape doesn't equal to the dimensions count of the object");

            for (int i = 0; i < DimensionsCount; i++)
                if (this[i] < shape[i])
                    return false;

            return true;
        }

        #region Operations

        public static Shape Add(Shape first, Shape second)
        {
            if (first.ElementsCount != second.ElementsCount)
                throw new ArgumentException("Given shape's dimensions count doesn't equal to the shape dimensions count");

            var newDim = new List<int>(first.ElementsCount);

            for (int i = 0; i < first.ElementsCount; i++)
                newDim[i] = first[i] + second[i];

            return new Shape(newDim);
        }

        public static Shape operator +(Shape first, Shape second)
        {
            return Add(first, second);
        }

        public static Shape Sub(Shape first, Shape second)
        {
            if (first.ElementsCount != second.ElementsCount)
                throw new ArgumentException("Given shape's dimensions count doesn't equal to the shape dimensions count");

            var newDim = new List<int>(first.ElementsCount);

            for (int i = 0; i < first.ElementsCount; i++)
                newDim[i] = first[i] - second[i];

            return new Shape(newDim);
        }

        public static Shape operator -(Shape first, Shape second)
        {
            return Sub(first, second);
        }

        bool Compare(Shape other)
        {
            if (Dimensions.Count != other.Dimensions.Count)
                return false;

            for (int i = 0; i < Dimensions.Count; i++)
                if (this[i] != other[i])
                    return false;

            return true;
        }

        public override bool Equals(object obj)
        {
            if (obj == null || !this.GetType().Equals(obj.GetType()))
                return false;

            return Compare((Shape)obj);
        }

        #endregion

        public override string ToString()
        {
            string res = "(";

            foreach (var item in dimensions)
                res += item + " ";

            return res + "\b)";
        }

        private int getSize()
        {
            int size = 1;

            foreach (var dim in Dimensions)
                size *= dim;

            return size;
        }
    }
}

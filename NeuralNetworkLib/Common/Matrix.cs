using System;
using System.Collections.Generic;

namespace NeuralNetworkLib
{
    [Serializable]
    public class MultiDimArr
    {
        private Random rand;
        public Shape shape { get; private set; }
        public int Length { get; private set; }
        public IReadOnlyCollection<float> Data { get; private set; }
        public float[] data;

        #region Constructors

        public MultiDimArr(Shape shape)
        {
            this.shape = shape;
            Length = shape.ElementsCount;

            data = new float[shape.ElementsCount];
            Data = Array.AsReadOnly(data);
            rand = new Random(DateTime.Now.Millisecond);
        }

        public MultiDimArr(List<float> matrixData, Shape shape)
        {
            if (matrixData.Count != shape.ElementsCount)
                throw new ArgumentException("Length of the given matrix data list doesn't equal to the shape's elements count");

            data = matrixData.ToArray();
            Data = Array.AsReadOnly(data);

            this.shape = shape;
            Length = shape.ElementsCount;
            rand = new Random(DateTime.Now.Millisecond);
        }

        public MultiDimArr(float[] matrixData, Shape shape)
        {
            if (matrixData.Length != Length)
                throw new ArgumentException("Length of the given matrix data doesn't equal to the matrix's elements count");

            data = matrixData;
            Data = Array.AsReadOnly(data);

            this.shape = shape;
            Length = shape.ElementsCount;
            rand = new Random(DateTime.Now.Millisecond);
        }

        public MultiDimArr(MultiDimArr other)
        {
            shape = new Shape(other.shape);
            Length = shape.ElementsCount;
            float[] dataCopy = new float[other.Length];
            Data = Array.AsReadOnly(dataCopy);

            for (int i = 0; i < other.Length; i++)
                dataCopy[i] = other.data[i];
        }

        #endregion

        #region Data manipulation
        
        public float GetValue(Shape itemIndex)
        {
            if (itemIndex.ElementsCount != Length)
                throw new ArgumentException("Given index's dimensions don't equal matrix dimensions");

            var absoluteIndex = getAbsoluteIndex(itemIndex);

            if (absoluteIndex < 0 || absoluteIndex > Length)
                throw new IndexOutOfRangeException();

            return data[absoluteIndex];
        }

        public float this[int index] => data[index];

        public void SetData(float[] newData)
        {
            if (Length != newData.Length)
                throw new ArgumentException("Given data's length doesn't equal to the data lenght");

            data = newData;
        }

        public void FillMatrixRandomly()
        {
            double sqrt = Math.Sqrt(Length);

            for (int i = 0; i < Length; i++)
                data[i] = (float)(rand.NextGaussian() / sqrt);
        }

        #endregion

        /// <summary>
        /// Merge a group of multi-dimensional arrays with same shapes into another array with the first dimension the count of a gpoup
        /// </summary>
        /// <param name="arrays"></param>
        /// <returns></returns>
        public static MultiDimArr MergeArrays(MultiDimArr[] arrays)
        {
            Shape membersShape = arrays[0].shape;
            Shape newShape = membersShape.AddDimensionsBack(arrays.Length);
            float[] newData = new float[newShape.ElementsCount];

            for (int i = 0; i < arrays.Length; i++)
            {
                if (arrays[i].shape != membersShape)
                    throw new ArgumentException("All multi-demensional arrays in a group have to have the same shape");

                arrays[i].data.CopyTo(newData, i * membersShape.ElementsCount);
            }

            return new MultiDimArr(newData, newShape);
        }

        /// <summary>
        /// Do not change shape in the case a new shape is equal to old one
        /// </summary>
        public MultiDimArr TryReshape(Shape newShape)
        {
            if (newShape != shape)
                return new MultiDimArr(data, newShape);

            return this;
        }

        /// <summary>
        /// Slides through the matrix with the given step and returns submatrices of the given shape
        /// </summary>
        /// <param name="submatrixShape"></param>
        /// <param name="submatrixStep"></param>
        public IEnumerable<MultiDimArr> IterateOverSubMatrices(Shape submatrixShape, Shape submatrixStep = null, Shape elementsStep = null, bool keepNewLineIndentation = false)
        {
            if (submatrixShape.DimensionsCount != shape.DimensionsCount)
                throw new ArgumentException("Given submatrix's dimensions count doesn't equal to the matrix's dimensions count");

            for (Shape start = new Shape(shape.DimensionsCount); start.ElementsCount <= shape.ElementsCount; start += submatrixStep)
            {
                if (!shape.IsSubmatrix(start + submatrixShape))
                    continue;

                yield return GetSubMatrix(submatrixShape, start, elementsStep, keepNewLineIndentation);
            }
        }

        /// <summary>
        /// Returns submatrix with the first element at the start of the matrix and the given sizes
        /// </summary>
        /// <param name="submatrixShape">new matrix size</param>
        public MultiDimArr GetSubMatrix(Shape submatrixShape)
        {
            if (!shape.IsSubmatrix(submatrixShape))
                throw new ArgumentException("The given shape isn't a submatrix shape of the matrix");

            List<float> subMatrixData = new List<float>(submatrixShape.ElementsCount);

            foreach (var itemIndex in submatrixShape.IterateShape())
                subMatrixData.Add(GetValue(itemIndex));

            return new MultiDimArr(subMatrixData, submatrixShape);
        }

        /// <summary>
        /// Returns submatrix with the first element at the start of the matrix and the given sizes
        /// </summary>
        /// <param name="submatrixShape">new matrix size</param>
        /// <param name="start">Index of the first element in the new matrix</param>
        /// <param name="keepNewLineIndentation">If true will keep the shape when element cursor is wrapped on a new line
        /// It is needed in order to keep shape or prevent getting submatrix as a line rather the block
        public MultiDimArr GetSubMatrix(Shape submatrixShape, Shape start, bool keepNewLineIndentation = false)
        {
            if (!shape.IsSubmatrix(submatrixShape))
                throw new ArgumentException("The given shape isn't a submatrix shape of the matrix");

            List<float> subMatrixData = new List<float>(submatrixShape.ElementsCount);

            foreach (var itemIndex in submatrixShape.IterateShape(start, keepNewLineIndentation: keepNewLineIndentation))
                subMatrixData.Add(GetValue(itemIndex));

            return new MultiDimArr(subMatrixData, submatrixShape);
        }

        /// <summary>
        /// Returns submatrix with the first element at the start and the given size
        /// that contains elements in ascending indexes order
        /// by default index step is next index
        /// </summary>
        /// <param name="submatrixShape">new matrix size</param>
        /// <param name="start">Index of the first element in the new matrix</param>
        /// <param name="step">step between elements</param>
        /// <param name="keepNewLineIndentation">If true will keep the shape when element cursor is wrapped on a new line
        /// It is needed in order to keep shape or prevent getting submatrix as a line rather the block
        /// </param>
        public MultiDimArr GetSubMatrix(Shape submatrixShape, Shape start, Shape step = null, bool keepNewLineIndentation = false)
        {
            if (!shape.IsSubmatrix(submatrixShape + start))
                throw new ArgumentException("The given shape isn't a submatrix shape of the matrix");

            List<float> subMatrixData = new List<float>(submatrixShape.ElementsCount);

            foreach (var itemIndex in submatrixShape.IterateShape(start, step, keepNewLineIndentation))
                subMatrixData.Add(GetValue(itemIndex + start));

            return new MultiDimArr(subMatrixData, submatrixShape);
        }

        public void SetValue(Shape itemIndex, float value)
        {
            if (itemIndex.ElementsCount != Length)
                throw new ArgumentException("Given index's dimensions don't equal matrix dimensions");

            var absoluteIndex = getAbsoluteIndex(itemIndex);

            if (absoluteIndex < 0 || absoluteIndex > Length)
                throw new IndexOutOfRangeException();

            data[absoluteIndex] = value;
        }

        private int getAbsoluteIndex(Shape relativeIndex)
        {
            int elementsInDim = 1;
            int absoluteIndex = 0;

            for (int i = relativeIndex.ElementsCount - 1; i > -1; i--)
            {
                absoluteIndex += relativeIndex[i] * elementsInDim;

                elementsInDim *= relativeIndex[i];
            }

            return absoluteIndex;
        }

        #region Operations

        public MultiDimArr MatrixMultiplication(MultiDimArr otherMatrix)
        {
            if (shape[1] != shape[0])
                throw new ArgumentException("To multiply multi-dimensional arrays first should have the same count of colums as first rows");

             
        }

        public MultiDimArr MultiplyElementwise(MultiDimArr otherMatrix)
        {
            if (shape != otherMatrix.shape)
                throw new ArgumentException("The given matrix's shape doesn't equal to the matrix shape");

            float[] newData = new float[Length];
            MultiDimArr newMatrix = new MultiDimArr(shape);

            for (int i = 0; i < Length; i++)
                newData[i] = data[i] * otherMatrix.data[i];

            newMatrix.SetData(newData);
            return newMatrix;
        }

        public static MultiDimArr operator *(MultiDimArr first, MultiDimArr second)
        {
            return first.MultiplyElementwise(second);
        }

        public MultiDimArr MultiplyElementwise(float number)
        {
            MultiDimArr newMatrix = new MultiDimArr(shape);
            float[] newData = new float[Length];

            for (int i = 0; i < Length; i++)
                newData[i] = data[i] * number;

            return newMatrix;
        }

        public static MultiDimArr operator *(MultiDimArr matrix, float number)
        {
            return matrix.MultiplyElementwise(number);
        }

        public MultiDimArr DivideElementwise(MultiDimArr otherMatrix)
        {
            if (shape != otherMatrix.shape)
                throw new ArgumentException("The given matrix's shape doesn't equal to the matrix shape");

            float[] newData = new float[Length];
            MultiDimArr newMatrix = new MultiDimArr(shape);

            for (int i = 0; i < Length; i++)
                newData[i] = data[i] / otherMatrix.data[i];

            newMatrix.SetData(newData);
            return newMatrix;
        }

        public static MultiDimArr operator /(MultiDimArr first, MultiDimArr second)
        {
            return first.DivideElementwise(second);
        }

        public MultiDimArr DivideElementwise(float number)
        {
            MultiDimArr newMatrix = new MultiDimArr(shape);
            float[] newData = new float[Length];

            for (int i = 0; i < Length; i++)
                newData[i] = data[i] / number;

            return newMatrix;
        }

        public static MultiDimArr operator /(MultiDimArr matrix, float number)
        {
            return matrix.DivideElementwise(number);
        }

        public MultiDimArr AddElementwise(MultiDimArr otherMatrix)
        {
            if (shape != otherMatrix.shape)
                throw new ArgumentException("The given matrix's shape doesn't equal to the matrix shape");

            float[] newData = new float[Length];
            MultiDimArr newMatrix = new MultiDimArr(shape);

            for (int i = 0; i < Length; i++)
                newData[i] = data[i] + otherMatrix.data[i];

            newMatrix.SetData(newData);
            return newMatrix;
        }

        public static MultiDimArr operator +(MultiDimArr first, MultiDimArr second)
        {
            return first.AddElementwise(second);
        }

        public MultiDimArr AddElementwise(float number)
        {
            MultiDimArr newMatrix = new MultiDimArr(shape);
            float[] newData = new float[Length];

            for (int i = 0; i < Length; i++)
                newData[i] = data[i] + number;

            return newMatrix;
        }

        public static MultiDimArr operator +(MultiDimArr matrix, float number)
        {
            return matrix.AddElementwise(number);
        }

        public MultiDimArr SubElementwise(MultiDimArr otherMatrix)
        {
            if (shape != otherMatrix.shape)
                throw new ArgumentException("The given matrix's shape doesn't equal to the matrix shape");

            float[] newData = new float[Length];
            MultiDimArr newMatrix = new MultiDimArr(shape);

            for (int i = 0; i < Length; i++)
                newData[i] = data[i] - otherMatrix.data[i];

            newMatrix.SetData(newData);
            return newMatrix;
        }

        public static MultiDimArr operator -(MultiDimArr first, MultiDimArr second)
        {
            return first.SubElementwise(second);
        }

        public MultiDimArr SubElementwise(float number)
        {
            MultiDimArr newMatrix = new MultiDimArr(shape);
            float[] newData = new float[Length];

            for (int i = 0; i < Length; i++)
                newData[i] = data[i] - number;

            return newMatrix;
        }

        public static MultiDimArr operator -(MultiDimArr matrix, float number)
        {
            return matrix.SubElementwise(number);
        }

        public float Sum()
        {
            float sum = 0;

            for (int i = 0; i < Length; i++)
                sum += data[i];

            return sum;
        }

        public MultiDimArr Map(Func<float, float> func)
        {
            MultiDimArr newMatrix = new MultiDimArr(this);

            for (int i = 0; i < Length; i++)
                newMatrix.data[i] += func(data[i]);

            return newMatrix;
        }

        #endregion

        public void PrintMatrix()
        {
            Console.WriteLine("Matrix: \n");
        }

    }
}

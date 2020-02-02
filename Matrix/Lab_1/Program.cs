using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Lab_1
{
    class Matrix
    {
        #region Public Members

        public int Columns => columns;
        public int Rows => rows;

        #endregion

        #region Private Members

        private int columns;
        private int rows;
        private double[,] matrix;

        private Random random = new Random();

        #endregion

        public Matrix(int rows, int columns)
        {
            this.columns = columns;
            this.rows = rows;

            matrix = new double[rows, columns];
            random = new Random(DateTime.Now.Millisecond);
        }

        #region Public Methods

        public void FillMatrixRandomly()
        {
            for (int i = 0; i < Rows; i++)
            {
                for (int j = 0; j < Columns; j++)
                {
                    SetValue(i, j, getRandomNumber()); 
                }
            }
        }

        public void SetValue(int row, int column, double value)
        {
            if (!isValidMatrixIndex(row, column))
                throw new IndexOutOfRangeException();

            matrix[row, column] = value;
        }

        public double GetValue(int row, int column)
        {
            if (!isValidMatrixIndex(row, column))
                throw new IndexOutOfRangeException();

            return matrix[row, column];
        }

        public void SetMathix(double[,] newMatrix)
        {
            matrix = newMatrix;
        }

        public Matrix Multiply(Matrix otherMatrix)
        {
            if (!isValidMatrexForMultyplying(this, otherMatrix))
                throw new Exception("Inappropriate matrix size");

            Matrix newMatrix = new Matrix(Rows, otherMatrix.Columns);

            for (int i = 0; i < Rows; i++)
            {
                double[] row = GetRow(i);

                for (int j = 0; j < otherMatrix.Columns; j++)
                {
                    double[] column = otherMatrix.GetColumn(j);
                    newMatrix.SetValue(i, j, getSumOfMultipliedArrays(column, row));
                }
            }

            return newMatrix;
        }

        public Matrix Multiply(double number)
        {
            Matrix newMatrix = new Matrix(Rows, Columns);

            for (int i = 0; i < Rows; i++)
            {
                for (int j = 0; j < Columns; j++)
                {
                    newMatrix.SetValue(i, j, GetValue(i, j) * number);
                }
            }

            return newMatrix;
        }

        private double[] GetRow(int index)
        {
            if (index >= rows || index < 0)
                throw new IndexOutOfRangeException();

            double[] row = new double[columns];
            for (int i = 0; i < columns; i++)
            {
                row[i] = GetValue(index, i);
            }

            return row;
        }

        private double[] GetColumn(int index)
        {
            if (index >= columns || index < 0)
                throw new IndexOutOfRangeException();

            double[] column = new double[rows];
            for (int i = 0; i < rows; i++)
            {
                column[i] = GetValue(i, index);
            }

            return column;
        }

        public double GetDeterminant()
        {
            if (!IsSquareMatrix())
                throw new Exception("Should be square matrix");

            double det = 0;
            if (columns == 1)
                det = GetValue(0, 0) * GetValue(0, 0);
            else if (columns == 2)
                det = GetValue(0, 0) * GetValue(1, 1) - GetValue(0, 1) * GetValue(1, 0);
            else
            {
                for(int i = 0; i < columns; i++)
                {
                    Matrix newSubMatrix = excludeRowAndColumn(row: 0, column: i);

                    det += Math.Pow(-1.0, i % 2.0) * GetValue(0, i) * newSubMatrix.GetDeterminant();
                }
            }

            return det;
        }

        public bool IsSquareMatrix()
        {
            if (Columns == Rows)
                return true;

            return false;
        }

        public Matrix GetTransposedMatrix()
        {
            Matrix newMatrix = new Matrix(Columns, Rows);

            for (int i = 0; i < Rows; i++)
            {
                for (int j = 0; j < Columns; j++)
                {
                    newMatrix.SetValue(j, i, GetValue(i, j));
                }
            }

            return newMatrix;
        }

        /// <summary>
        /// Return null if inversed matrix can't be found (determinant == 0)
        /// </summary>
        /// <returns></returns>
        public Matrix GetInversedMatrix()
        {
            double determinant = GetDeterminant();
            if (determinant == 0)
                return null;

            Matrix newMatrix = new Matrix(Rows, Columns);

            for(int i = 0; i < Rows; i++)
            {
                for(int j = 0; j < Columns; j++)
                {
                    double value = Math.Pow(-1.0, (i + j) % 2.0) * (excludeRowAndColumn(i, j)).GetDeterminant();

                    newMatrix.SetValue(j, i, value);
                }
            }

            return newMatrix * (1/determinant);
        }

        public void PrintMatrix()
        {
            Console.WriteLine("Matrix: \n");

            for (int i = 0; i < Rows; i++)
            {
                for (int j = 0; j < Columns; j++)
                {
                    Console.Write($"{Math.Round(GetValue(i, j), 2),6}");
                }
                Console.WriteLine("");

            }
        }

        #endregion

        #region Private Methods

        private int getRandomNumber()
        {
            return random.Next(20);
        }

        private bool isValidMatrexForMultyplying(Matrix oneMatrix, Matrix otherMatrix)
        {
            if (oneMatrix.Columns != otherMatrix.Rows)
                return false;

            return true;
        }

        private double getSumOfMultipliedArrays(double[] firstArray, double[] secondArray)
        {
            if (firstArray.Length != secondArray.Length)
                throw new Exception("Arrays are not the same length");

            double sum = 0;
            for(int i = 0; i < firstArray.Length; i++)
                sum += firstArray[i] * secondArray[i];

            return sum;
        }

        private Matrix excludeRowAndColumn(int row, int column)
        {
            Matrix newMatrix = new Matrix(Rows - 1, Columns - 1);

            for (int i = 0, newI = 0; i < rows; i++)
            {
                if (i == row)
                    continue;

                for (int j = 0, newJ = 0; j < columns; j++)
                {
                    if (j == column)
                        continue;

                    newMatrix.SetValue(newI, newJ, GetValue(i, j));
                    newJ++;
                }

                newI++;
            }

            return newMatrix;
        }

        /// <summary>
        /// Return matrix form the area of two elements
        /// </summary>
        /// <param name="startRow">included</param>
        /// <param name="startColumn">included</param>
        /// <param name="endRow">excluded</param>
        /// <param name="endColumn">excluded</param>
        /// <returns></returns>
        private Matrix getSubMatrix(int startRow = 0, int startColumn = 0, int endRow = 0, int endColumn = 0)
        {
            if (startColumn > endColumn || startRow > endRow)
                throw new Exception("Start column or row can't be greater than end column or row");

            if (!isValidMatrixIndex(startRow, startColumn) || !isValidMatrixIndex(endRow, endColumn))
                throw new IndexOutOfRangeException();

            int newColumns = endColumn - startColumn;
            int newRows = endRow - startRow;

            Matrix newMatrix = new Matrix(newRows, newColumns);

            for (int i = startRow; i < endRow; i++)
                for (int j = startColumn; j < endColumn; j++)
                    newMatrix.SetValue(i - startRow, j - startColumn, GetValue(i, j));

            return newMatrix;
        }

        private bool isValidMatrixIndex(int row, int column)
        {
            if (row < 0 || row > rows - 1 || column < 0 || column > columns - 1)
                return false;

            return true;
        }

        #endregion

        #region Operator overloading

        public static Matrix operator *(Matrix oneMatrix, Matrix otherMatrix)
        {
            return oneMatrix.Multiply(otherMatrix);
        }

        public static Matrix operator *(Matrix oneMatrix, double number)
        {
            return oneMatrix.Multiply(number);
        }
        
        #endregion
    }

    class Program
    {

        static void Main(string[] args)
        {
            Stopwatch timer1 = new Stopwatch();
            Stopwatch timer2 = new Stopwatch();

            timer1.Start();

            Matrix matrix1 = new Matrix(88, 88);

            Matrix matrix2 = new Matrix(88, 88);
            Matrix matrix3 = new Matrix(88, 88);

            matrix1.FillMatrixRandomly();
            matrix2.FillMatrixRandomly();
            matrix3.FillMatrixRandomly();

            timer2.Start();
            Matrix res = matrix1 * matrix2 * matrix3;
            timer2.Stop();
            timer1.Stop();

            Console.WriteLine(timer1.Elapsed.TotalMilliseconds + "ms Outer loop");
            Console.WriteLine(timer2.Elapsed.TotalMilliseconds + "ms Inner loop");
        }
    }
}

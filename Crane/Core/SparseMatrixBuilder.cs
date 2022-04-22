using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MathNet.Numerics.LinearAlgebra;

namespace Crane.Core
{
    public class SparseMatrixBuilder
    {
        public SparseMatrixBuilder(int columns)
        {
            Rows = 0;
            Columns = columns;
            Elements = new List<Tuple<int, int, double>>();
        }
        public int Rows { get; private set; }
        public int Columns { get; private set; }
        public List<Tuple<int, int, double>> Elements { get; private set; }

        public Matrix<double> BuildSparseMatrix()
        {
            return Matrix<double>.Build.SparseOfIndexed(Rows, Columns, Elements);
        }

        public void Append(SparseMatrixBuilder builder)
        {
            Append_(builder.Rows, builder.Columns, builder.Elements);
        }
        public void Append(int rows, int columns, List<Tuple<int, int, double>> elements)
        {
            Append_(rows, columns, elements);
        }
        private void Append_(int rows, int columns, List<Tuple<int, int, double>> elements)
        {
            if (columns != Columns) throw new Exception("Number of columns is different.");
            foreach (var element in elements)
            {
                Elements.Add(new Tuple<int, int, double>(element.Item1+Rows, element.Item2, element.Item3));
            }

            Rows += rows;
        }
    }
}

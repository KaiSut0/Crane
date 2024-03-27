using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using MathNet.Numerics.LinearAlgebra;

namespace Crane.Core
{
    public class SparseMatrixBuilder
    {
        public SparseMatrixBuilder(int rows, int columns)
        {
            Rows = rows;
            Columns = columns;
            Elements = new List<Tuple<int, int, double>>();
            indexMap = new Dictionary<Tuple<int, int>, int>();
        }

        public SparseMatrixBuilder(int rows, int columns, List<Tuple<int, int, double>> elements)
        {
            Rows = rows;
            Columns = columns;
            Elements = elements;
            indexMap = new Dictionary<Tuple<int, int>, int>();
            for(int i = 0; i < elements.Count; i++) 
            {
                var element = elements[i];
                indexMap[new Tuple<int, int>(element.Item1, element.Item2)] = i;
            }
        }
        public int Rows { get; private set; }
        public int Columns { get; private set; }
        public List<Tuple<int, int, double>> Elements { get; private set; }
        private Dictionary<Tuple<int, int>, int> indexMap;


        public Matrix<double> BuildSparseMatrix()
        {
            return Matrix<double>.Build.SparseOfIndexed(Rows, Columns, Elements);
        }

        public void Add(int row, int column, double value)
        {
            if (row >= Rows || column >= Columns) throw new Exception("Index out of range.");
            if(indexMap.ContainsKey(new Tuple<int, int>(row, column)))
            {
                int index = indexMap[new Tuple<int, int>(row, column)];
                Elements[index] = new Tuple<int, int, double>(row, column, value + Elements[index].Item3);
            }
            else
            {
                Elements.Add(new Tuple<int, int, double>(row, column, value));
                indexMap[new Tuple<int, int>(row, column)] = Elements.Count - 1;
            }
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
                indexMap[new Tuple<int, int>(element.Item1+Rows, element.Item2)] = Elements.Count - 1;
            }
            Rows += rows;
        }
    }
}

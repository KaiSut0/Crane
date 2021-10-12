using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Rhino.Geometry;

namespace Crane.Patterns
{
    public class MiuraFoldingOnDoubleSurface : TessellationOnDoubleSurface
    {
        public MiuraFoldingOnDoubleSurface(Surface S1, Surface S2, int U, int V) : base(S1, S2, U, V)
        {
        }

        protected override List<Tuple<int, int, int>> CreateVList()
        {
            var v0 = Tup(0, 0, 0);
            var v1 = Tup(0, 0, 2);
            var v2 = Tup(0, 1, 1);
            var v3 = Tup(0, 1, 3);
            var v4 = Tup(0, 2, 0);
            var v5 = Tup(0, 2, 2);
            var v6 = Tup(1, 0, 1);
            var v7 = Tup(1, 1, 2);
            var v8 = Tup(1, 2, 1);

            return new List<Tuple<int, int, int>> {v0, v1, v2, v3, v4, v5, v6, v7, v8};
        }

        protected override List<int[]> CreateFList1()
        {
            return new List<int[]>();
        }

        protected override List<int[]> CreateFList2()
        {
            var f0 = new[] {0, 2, 7, 6};
            var f1 = new[] {1, 6, 7, 3};
            var f2 = new[] {7, 2, 4, 8};
            var f3 = new[] {3, 7, 8, 5};

            return new List<int[]> {f0, f1, f2, f3};
        }

        protected override List<int[]> CreateFList3()
        {
            return new List<int[]>();
        }

        protected override List<int[]> CreateFList4()
        {
            return new List<int[]>();
        }

        protected override int M()
        {
            return 2;
        }

        protected override int N()
        {
            return 2;
        }
    }
}

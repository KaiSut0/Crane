using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Rhino.Geometry;

namespace Crane.Patterns
{
    public class WaterbombOnDoubleSurface : TessellationOnDoubleSurface
    {
        protected override List<Tuple<int, int, int>> CreateVList()
        {
            var v0 = Tup(0, 0, 0);
            var v1 = Tup(0, 0, 3);
            var v2 = Tup(0, 0, 4);
            var v3 = Tup(0, 1, 1);
            var v4 = Tup(0, 1, 2);
            var v5 = Tup(0, 1, 5);
            var v6 = Tup(0, 2, 0);
            var v7 = Tup(0, 2, 3);
            var v8 = Tup(0, 2, 4);
            var v9 = Tup(1, 0, 3);
            var v10 = Tup(1, 1, 1);
            var v11 = Tup(1, 2, 3);

            return new List<Tuple<int, int, int>> {v0, v1, v2, v3, v4, v5, v6, v7, v8, v9, v10, v11};
        }

        protected override List<int[]> CreateFList1()
        {
            var f0 = new[] {0, 1, 10};
            var f1 = new[] {7, 6, 10};
            var f2 = new[] {0, 10, 3};
            var f3 = new[] {3, 10, 6};
            var f4 = new[] {4, 10, 1};
            var f5 = new[] {7, 10, 4};

            return new List<int[]> {f0, f1, f2, f3, f4, f5};
        }

        protected override List<int[]> CreateFList2()
        {
            var f0 = new[] {1, 9, 4};
            var f1 = new[] {4, 11, 7};
            var f2 = new[] {4, 9, 5};
            var f3 = new[] {4, 5, 11};
            var f4 = new[] {9, 2, 5};
            var f5 = new[] {5, 8, 11};

            return new List<int[]> {f0, f1, f2, f3, f4, f5};
           
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
            return 4;
        }

        public WaterbombOnDoubleSurface(Surface S1, Surface S2, int U, int V) : base(S1, S2, U, V)
        {
        }
    }
}

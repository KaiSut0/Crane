using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Rhino.Geometry;

namespace Crane.Patterns
{
    public class YoshimuraPatternOnSurface : TessellationOnDoubleSurface
    {
        public YoshimuraPatternOnSurface(Surface S1, Surface S2, int U, int V) : base(S1, S2, U, V)
        {
        }

        protected override List<Tuple<int, int, int>> CreateVList()
        {
            var v0 = Tup(0, 0, 1);
            var v1 = Tup(0, 0, 3);
            var v2 = Tup(0, 1, 0);
            var v3 = Tup(0, 1, 2);
            var v4 = Tup(0, 2, 1);
            var v5 = Tup(0, 2, 3);

            return new List<Tuple<int, int, int>> { v0, v1, v2, v3, v4, v5 };
        }

        protected override List<int[]> CreateFList1()
        {
            var f0 = new[] {0, 2, 3};
            var f1 = new[] { 2, 4, 3 };

            return new List<int[]> {f0, f1};
        }

        protected override List<int[]> CreateFList2()
        {
            var f0 = new[] {0, 3, 1};
            var f1 = new[] {4, 5, 3};

            return new List<int[]> {f0, f1};
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

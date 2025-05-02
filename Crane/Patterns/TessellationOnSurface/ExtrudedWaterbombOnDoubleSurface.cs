using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Rhino.Geometry;

namespace Crane.Patterns
{
    public class ExtrudedWaterbombOnDoubleSurface : TessellationOnDoubleSurface
    {
        public ExtrudedWaterbombOnDoubleSurface(Surface S1, Surface S2, int U, int V) : base(S1, S2, U, V)
        {
        }

        protected override List<Tuple<int, int, int>> CreateVList()
        {
            var v0 = Tup(0, 0, 0);
            var v1 = Tup(0, 0, 3);
            var v2 = Tup(0, 1, 1);
            var v3 = Tup(0, 1, 2);
            var v4 = Tup(0, 1, 5);
            var v5 = Tup(0, 3, 1);
            var v6 = Tup(0, 3, 2);
            var v7 = Tup(0, 3, 5);
            var v8 = Tup(0, 4, 0);
            var v9 = Tup(0, 4, 3);
            var v10 = Tup(0, 6, 0);
            var v11 = Tup(0, 6, 3);
            var v12 = Tup(1, 0, 3);
            var v13 = Tup(1, 1, 1);
            var v14 = Tup(1, 3, 1);
            var v15 = Tup(1, 4, 3);
            var v16 = Tup(1, 6, 3);
            var v17 = Tup(0, 0, 4);
            var v18 = Tup(0, 4, 4);
            var v19 = Tup(0, 4, 4);
            var v20 = Tup(0, 6, 4);

            return new List<Tuple<int, int, int>>
                {v0, v1, v2, v3, v4, v5, v6, v7, v8, v9, v10, v11, v12, v13, v14, v15, v16, v17, v18, v19, v20};
        }

        protected override List<int[]> CreateFList1()
        {
            var f0 = new[] {0, 1, 13};
            var f1 = new[] {0, 13, 2};
            var f2 = new[] {1, 3, 13};
            var f3 = new[] {2, 13, 14, 5};
            var f4 = new[] {13, 3, 6, 14};
            var f5 = new[] {5, 14, 8};
            var f6 = new[] {14, 9, 8};
            var f7 = new[] {14, 6, 9};

            return new List<int[]> {f0, f1, f2, f3, f4, f5, f6, f7};
        }

        protected override List<int[]> CreateFList2()
        {
            var f0 = new[] {1, 12, 3};
            var f1 = new[] {3, 12, 4};
            var f2 = new[] {3, 4, 7, 6};
            var f3 = new[] {6, 7, 15};
            var f4 = new[] {6, 15, 9};
            var f5 = new[] {12, 17, 4};
            var f6 = new[] {7, 18, 15};

            return new List<int[]> {f0, f1, f2, f3, f4, f5, f6};
        }

        protected override List<int[]> CreateFList3()
        {
            var f0 = new[] {8, 9, 11, 10};

            return new List<int[]> {f0}; 
            
        }

        protected override List<int[]> CreateFList4()
        {
            var f0 = new[] {9, 15, 16, 11};
            var f1 = new[] {15, 19, 20, 16};
            

            return new List<int[]> {f0, f1};
        }

        protected override int M()
        {
            return 6;
        }

        protected override int N()
        {
            return 4;
        }
    }
}

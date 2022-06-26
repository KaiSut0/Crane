using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using QuickGraph.Collections;
using Rhino.Geometry;

namespace Crane.Patterns.TessellationOnSurface
{
    public class HuffmanWaterbombsOnDoubleSurface : TessellationOnDoubleSurface
    {
        public HuffmanWaterbombsOnDoubleSurface(Surface topSurface, Surface bottomSurface, int uCount, int vCount, TessellationParam param) : base(topSurface, bottomSurface, uCount, vCount, param)
        {
        }

        protected override void SetUnitPointParameterList()
        {
            var p = (HuffmanWaterbomsParam)param;
            UnitPointParameterList = new VertexList<Tuple<int, double, double>>();
            UnitPointParameterList.Add(
                new Tuple<int, double, double>(1, 0, 0));
            UnitPointParameterList.Add(
                new Tuple<int, double, double>(0, 0.2 - p.SlideParam, 0));
            UnitPointParameterList.Add(
                new Tuple<int, double, double>(0, 0.4, p.SlideParam));
            UnitPointParameterList.Add(
                new Tuple<int, double, double>(0, 0.6, - p.SlideParam));
            UnitPointParameterList.Add(
                new Tuple<int, double, double>(0, 0.8 + p.SlideParam, 0));
            UnitPointParameterList.Add(
                new Tuple<int, double, double>(1, 1, 0));

            UnitPointParameterList.Add(
                new Tuple<int, double, double>(0, 0, 0.2 - p.SlideParam));
            UnitPointParameterList.Add(
                new Tuple<int, double, double>(0, 0.2 + p.SlideParam, 0.2));
            UnitPointParameterList.Add(
                new Tuple<int, double, double>(1, 0.4, 0.2));
            UnitPointParameterList.Add(
                new Tuple<int, double, double>(0, 0.6 - p.SlideParam, 0.2));
            UnitPointParameterList.Add(
                new Tuple<int, double, double>(0, 0.8, 0.2 + p.SlideParam));
            UnitPointParameterList.Add(
                new Tuple<int, double, double>(0, 1, 0.2 - p.SlideParam));

            UnitPointParameterList.Add(
                new Tuple<int, double, double>(0, - p.SlideParam, 0.4));
            UnitPointParameterList.Add(
                new Tuple<int, double, double>(0, 0.2, 0.4 + p.SlideParam));
            UnitPointParameterList.Add(
                new Tuple<int, double, double>(0, 0.4, 0.4 - p.SlideParam));
            UnitPointParameterList.Add(
                new Tuple<int, double, double>(0, 0.6 + p.SlideParam, 0.4));
            UnitPointParameterList.Add(
                new Tuple<int, double, double>(1, 0.8, 0.4));
            UnitPointParameterList.Add(
                new Tuple<int, double, double>(0, 1 - p.SlideParam, 0.4));

            UnitPointParameterList.Add(
                new Tuple<int, double, double>(0, p.SlideParam, 0.6));
            UnitPointParameterList.Add(
                new Tuple<int, double, double>(1, 0.2, 0.6));
            UnitPointParameterList.Add(
                new Tuple<int, double, double>(0, 0.4 - p.SlideParam, 0.6));
            UnitPointParameterList.Add(
                new Tuple<int, double, double>(0, 0.6, 0.6 + p.SlideParam));
            UnitPointParameterList.Add(
                new Tuple<int, double, double>(0, 0.8, 0.6 - p.SlideParam));
            UnitPointParameterList.Add(
                new Tuple<int, double, double>(0, 1 + p.SlideParam, 0.6));

            UnitPointParameterList.Add(
                new Tuple<int, double, double>(0, 0, 0.8 + p.SlideParam));
            UnitPointParameterList.Add(
                new Tuple<int, double, double>(0, 0.2, 0.8 - p.SlideParam));
            UnitPointParameterList.Add(
                new Tuple<int, double, double>(0, 0.4 + p.SlideParam, 0.8));
            UnitPointParameterList.Add(
                new Tuple<int, double, double>(1, 0.6, 0.8));
            UnitPointParameterList.Add(
                new Tuple<int, double, double>(0, 0.8 - p.SlideParam, 0.8));
            UnitPointParameterList.Add(
                new Tuple<int, double, double>(0, 1, 0.8 + p.SlideParam));

            UnitPointParameterList.Add(
                new Tuple<int, double, double>(1, 0, 1));
            UnitPointParameterList.Add(
                new Tuple<int, double, double>(0, 0.2 - p.SlideParam, 1));
            UnitPointParameterList.Add(
                new Tuple<int, double, double>(0, 0.4, 1 + p.SlideParam));
            UnitPointParameterList.Add(
                new Tuple<int, double, double>(0, 0.6, 1 -p.SlideParam));
            UnitPointParameterList.Add(
                new Tuple<int, double, double>(0, 0.8 + p.SlideParam, 1));
            UnitPointParameterList.Add(
                new Tuple<int, double, double>(1, 1, 1));
        }

        protected override void SetUnitFaces()
        {
            unitFaces = new List<int[]>();
            unitFaces.Add(new[] { 0, 7, 6 });
            unitFaces.Add(new[] { 0, 1, 7 });
            unitFaces.Add(new[] { 1, 8, 7 });
            unitFaces.Add(new[] { 1, 2, 8 });
            unitFaces.Add(new[] { 2, 3, 8 });
            unitFaces.Add(new[] { 3, 9, 8 });
            unitFaces.Add(new[] { 3, 4, 10, 9 });
            unitFaces.Add(new[] { 4, 5, 10 });
            unitFaces.Add(new[] { 5, 11, 10 });

            unitFaces.Add(new[] { 6, 7, 13, 12 });
            unitFaces.Add(new[] { 7, 8, 13 });
            unitFaces.Add(new[] { 8, 14, 13 });
            unitFaces.Add(new[] { 8, 15, 14 });
            unitFaces.Add(new[] { 8, 9, 15 });
            unitFaces.Add(new[] { 9, 16, 15 });
            unitFaces.Add(new[] { 9, 10, 16 });
            unitFaces.Add(new[] { 10, 11, 16 });
            unitFaces.Add(new[] { 11, 17, 16 });

            unitFaces.Add(new[] { 12, 19, 18 });
            unitFaces.Add(new[] { 12, 13, 19 });
            unitFaces.Add(new[] { 13, 14, 19 });
            unitFaces.Add(new[] { 14, 20, 19 });
            unitFaces.Add(new[] { 14, 15, 21, 20 });
            unitFaces.Add(new[] { 15, 16, 21 });
            unitFaces.Add(new[] { 16, 22, 21 });
            unitFaces.Add(new[] { 16, 23, 22 });
            unitFaces.Add(new[] { 16, 17, 23 });

            unitFaces.Add(new[] { 18, 19, 24 });
            unitFaces.Add(new[] { 19, 24, 25 });
            unitFaces.Add(new[] { 19, 26, 25 });
            unitFaces.Add(new[] { 19, 20, 26 });
            unitFaces.Add(new[] { 20, 27, 26 });
            unitFaces.Add(new[] { 20, 21, 27 });
            unitFaces.Add(new[] { 21, 22, 27 });
            unitFaces.Add(new[] { 22, 28, 27 });
            unitFaces.Add(new[] { 22, 23, 29, 28 });

            unitFaces.Add(new[] { 24, 25, 30 });
            unitFaces.Add(new[] { 25, 31, 30 });
            unitFaces.Add(new[] { 25, 26, 32, 31 });
            unitFaces.Add(new[] { 26, 27, 32 });
            unitFaces.Add(new[] { 27, 33, 32 });
            unitFaces.Add(new[] { 27, 34, 33 });
            unitFaces.Add(new[] { 27, 28, 34 });
            unitFaces.Add(new[] { 28, 35, 34 });
            unitFaces.Add(new[] { 28, 29, 35 });

            throw new NotImplementedException();
        }
    }
}

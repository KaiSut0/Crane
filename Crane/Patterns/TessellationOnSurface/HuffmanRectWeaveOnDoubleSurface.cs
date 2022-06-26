using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using QuickGraph.Collections;
using Rhino.Geometry;

namespace Crane.Patterns.TessellationOnSurface
{
    public class HuffmanRectWeaveOnDoubleSurface : TessellationOnDoubleSurface
    {
        public HuffmanRectWeaveOnDoubleSurface(Surface topSurface, Surface bottomSurface, int uCount, int vCount, TessellationParam param) : base(topSurface, bottomSurface, uCount, vCount, param)
        {
        }

        protected override void SetUnitPointParameterList()
        {
            var p = (HuffmanRectWeaveParam)param;
            UnitPointParameterList = new VertexList<Tuple<int, double, double>>();
            UnitPointParameterList.Add(
                           new Tuple<int, double, double>(1, 0, 0));
            UnitPointParameterList.Add(
                new Tuple<int, double, double>(0, p.SlideParam3, 0));
            UnitPointParameterList.Add(
                new Tuple<int, double, double>(0, p.SlideParam1 - p.SlideParam3, 0));
            UnitPointParameterList.Add(
                new Tuple<int, double, double>(1, p.SlideParam1, 0));
            UnitPointParameterList.Add(
                new Tuple<int, double, double>(1, 0.5 - p.SlideParam2, p.SlideParam2));
            UnitPointParameterList.Add(
                new Tuple<int, double, double>(1, 0.5 + p.SlideParam1 + p.SlideParam2, p.SlideParam2));
            UnitPointParameterList.Add(
                new Tuple<int, double, double>(1, 1, 0));

            UnitPointParameterList.Add(
                new Tuple<int, double, double>(0, 0.5 - p.SlideParam2, p.SlideParam2 + p.SlideParam3));
            UnitPointParameterList.Add(
                new Tuple<int, double, double>(0, 0.5 + p.SlideParam1 + p.SlideParam2, p.SlideParam2 + p.SlideParam3));

            UnitPointParameterList.Add(
                new Tuple<int, double, double>(0, 0.5 - p.SlideParam2, p.SlideParam1 + p.SlideParam2 - p.SlideParam3));
            UnitPointParameterList.Add(
                new Tuple<int, double, double>(0, 0.5 + p.SlideParam1 + p.SlideParam2, p.SlideParam1 + p.SlideParam2 - p.SlideParam3));

            UnitPointParameterList.Add(
                new Tuple<int, double, double>(1, 0, p.SlideParam1 + 2 * p.SlideParam2));
            UnitPointParameterList.Add(
                new Tuple<int, double, double>(0, p.SlideParam3, p.SlideParam1 + 2 * p.SlideParam2));
            UnitPointParameterList.Add(
                new Tuple<int, double, double>(0, p.SlideParam1 - p.SlideParam3, p.SlideParam1 + 2 * p.SlideParam2));
            UnitPointParameterList.Add(
                new Tuple<int, double, double>(1, p.SlideParam1, p.SlideParam1 + 2 * p.SlideParam2));
            UnitPointParameterList.Add(
                new Tuple<int, double, double>(1, 0.5 - p.SlideParam2, p.SlideParam1 + p.SlideParam2));
            UnitPointParameterList.Add(
                new Tuple<int, double, double>(1, 0.5 + p.SlideParam1 + p.SlideParam2, p.SlideParam1 + p.SlideParam2));
            UnitPointParameterList.Add(
                new Tuple<int, double, double>(1, 1, p.SlideParam1 + 2 * p.SlideParam2));

            UnitPointParameterList.Add(
                new Tuple<int, double, double>(1, -p.SlideParam2, 0.5 + p.SlideParam2));
            UnitPointParameterList.Add(
                new Tuple<int, double, double>(1, p.SlideParam1 + p.SlideParam2, 0.5 + p.SlideParam2));
            UnitPointParameterList.Add(
                new Tuple<int, double, double>(1, 0.5, 0.5));
            UnitPointParameterList.Add(
                new Tuple<int, double, double>(0, 0.5 + p.SlideParam3, 0.5));
            UnitPointParameterList.Add(
                new Tuple<int, double, double>(0, 0.5 + p.SlideParam1 - p.SlideParam3, 0.5));
            UnitPointParameterList.Add(
                new Tuple<int, double, double>(1, 0.5 + p.SlideParam1, 0.5));
            UnitPointParameterList.Add(
                new Tuple<int, double, double>(1, 1 - p.SlideParam2, 0.5 + p.SlideParam2));

            UnitPointParameterList.Add(
                new Tuple<int, double, double>(0, -p.SlideParam2, 0.5 + p.SlideParam2 + p.SlideParam3));
            UnitPointParameterList.Add(
                new Tuple<int, double, double>(0, p.SlideParam1 + p.SlideParam2, 0.5 + p.SlideParam2 + p.SlideParam3));
            UnitPointParameterList.Add(
                new Tuple<int, double, double>(0, 1 - p.SlideParam2, 0.5 + p.SlideParam2 + p.SlideParam3));

            UnitPointParameterList.Add(
                new Tuple<int, double, double>(0, -p.SlideParam2, 0.5 + p.SlideParam1 + p.SlideParam2 - p.SlideParam3));
            UnitPointParameterList.Add(
                new Tuple<int, double, double>(0, p.SlideParam1 + p.SlideParam2, 0.5 + p.SlideParam1 + p.SlideParam2 - p.SlideParam3));
            UnitPointParameterList.Add(
                new Tuple<int, double, double>(0, 1 - p.SlideParam2, 0.5 + p.SlideParam1 + p.SlideParam2 - p.SlideParam3));

            UnitPointParameterList.Add(
                new Tuple<int, double, double>(1, -p.SlideParam2, 0.5 + p.SlideParam1 + p.SlideParam2));
            UnitPointParameterList.Add(
                new Tuple<int, double, double>(1, p.SlideParam1 + p.SlideParam2, 0.5 + p.SlideParam1 + p.SlideParam2));
            UnitPointParameterList.Add(
                new Tuple<int, double, double>(1, 0.5, 0.5 + p.SlideParam1 + 2 * p.SlideParam2));
            UnitPointParameterList.Add(
                new Tuple<int, double, double>(0, 0.5 + p.SlideParam3, 0.5 + p.SlideParam1 + 2 * p.SlideParam2));
            UnitPointParameterList.Add(
                new Tuple<int, double, double>(0, 0.5 + p.SlideParam1 - p.SlideParam3, 0.5 + p.SlideParam1 + 2 * p.SlideParam2));
            UnitPointParameterList.Add(
                new Tuple<int, double, double>(1, 0.5 + p.SlideParam1, 0.5 + p.SlideParam1 + 2 * p.SlideParam2));
            UnitPointParameterList.Add(
                new Tuple<int, double, double>(1, 1 - p.SlideParam2, 0.5 + p.SlideParam1 + p.SlideParam2));

            UnitPointParameterList.Add(
                new Tuple<int, double, double>(1, 0, 1));
            UnitPointParameterList.Add(
                new Tuple<int, double, double>(0, p.SlideParam3, 1));
            UnitPointParameterList.Add(
                new Tuple<int, double, double>(0, p.SlideParam1 - p.SlideParam3, 1));
            UnitPointParameterList.Add(
                new Tuple<int, double, double>(1, p.SlideParam1, 1));
            UnitPointParameterList.Add(
                new Tuple<int, double, double>(1, 0.5 - p.SlideParam2, 1 + p.SlideParam2));
            UnitPointParameterList.Add(
                new Tuple<int, double, double>(1, 0.5 + p.SlideParam1 + p.SlideParam2, 1 + p.SlideParam2));
            UnitPointParameterList.Add(
                new Tuple<int, double, double>(1, 1, 1));
        }

        protected override void SetUnitFaces()
        {
            unitFaces = new List<int[]>();
            unitFaces.Add(new[] { 0, 1, 12, 11 });
            unitFaces.Add(new[] { 1, 2, 13, 12 });
            unitFaces.Add(new[] { 2, 3, 14, 13 });
            unitFaces.Add(new[] { 3, 4, 7 });
            unitFaces.Add(new[] { 4, 5, 8, 7 });
            unitFaces.Add(new[] { 5, 6, 8 });
            unitFaces.Add(new[] { 3, 7, 9, 14 });
            unitFaces.Add(new[] { 7, 8, 10, 9 });
            unitFaces.Add(new[] { 8, 6, 17, 10 });
            unitFaces.Add(new[] { 9, 15, 14 });
            unitFaces.Add(new[] { 9, 10, 16, 15 });
            unitFaces.Add(new[] { 10, 17, 16 });

            unitFaces.Add(new[] { 11, 12, 18 });
            unitFaces.Add(new[] { 12, 13, 19, 18 });
            unitFaces.Add(new[] { 13, 14, 19 });
            unitFaces.Add(new[] { 14, 15, 20, 19 });
            unitFaces.Add(new[] { 15, 21, 20 });
            unitFaces.Add(new[] { 15, 16, 22, 21 });
            unitFaces.Add(new[] { 16, 23, 22 });
            unitFaces.Add(new[] { 16, 17, 24, 23 });

            unitFaces.Add(new[] { 18, 19, 26, 25 });
            unitFaces.Add(new[] { 19, 20, 26 });
            unitFaces.Add(new[] { 25, 26, 29, 28 });
            unitFaces.Add(new[] { 26, 20, 33, 29 });
            unitFaces.Add(new[] { 28, 29, 32, 31 });
            unitFaces.Add(new[] { 29, 33, 32 });
            unitFaces.Add(new[] { 20, 21, 34, 33 });
            unitFaces.Add(new[] { 21, 22, 35, 34 });
            unitFaces.Add(new[] { 22, 23, 36, 35 });
            unitFaces.Add(new[] { 23, 24, 27 });
            unitFaces.Add(new[] { 23, 27, 30, 36 });
            unitFaces.Add(new[] { 30, 37, 36 });

            unitFaces.Add(new[] { 31, 37, 38 });
            unitFaces.Add(new[] { 31, 32, 40, 37 });
            unitFaces.Add(new[] { 32, 41, 40 });
            unitFaces.Add(new[] { 32, 33, 42, 41 });
            unitFaces.Add(new[] { 33, 34, 42 });
            unitFaces.Add(new[] { 34, 35, 43, 42 });
            unitFaces.Add(new[] { 35, 36, 43 });
            unitFaces.Add(new[] { 36, 37, 44, 43 });
        }
    }
}

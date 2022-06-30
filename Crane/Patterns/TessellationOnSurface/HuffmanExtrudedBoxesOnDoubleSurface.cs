using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using QuickGraph.Collections;
using Rhino.Geometry;

namespace Crane.Patterns.TessellationOnSurface
{
    public class HuffmanExtrudedBoxesOnDoubleSurface : TessellationOnDoubleSurface
    {
        public HuffmanExtrudedBoxesOnDoubleSurface(Surface topSurface, Surface bottomSurface, int uCount, int vCount, TessellationParam param) : base(topSurface, bottomSurface, uCount, vCount, param)
        {
        }

        protected override void SetUnitPointParameterList()
        {
            var p = (HuffmanExtrudedBoxesParam)param;
            UnitPointParameterList = new VertexList<Tuple<int, double, double>>();
            UnitPointParameterList.Add(
                new Tuple<int, double, double>(0, 0, 0));
            UnitPointParameterList.Add(
                new Tuple<int, double, double>(1, 0.5 * p.SlideParam1, 0));
            UnitPointParameterList.Add(
                new Tuple<int, double, double>(0, p.SlideParam1, 0));
            UnitPointParameterList.Add(
                new Tuple<int, double, double>(0, 0.5 - p.SlideParam2, p.SlideParam2));
            UnitPointParameterList.Add(
                new Tuple<int, double, double>(0, 0.5 + p.SlideParam1 + p.SlideParam2, p.SlideParam2));
            UnitPointParameterList.Add(
                new Tuple<int, double, double>(0, 1, 0));
           
            UnitPointParameterList.Add(
                new Tuple<int, double, double>(1, 0.5 - p.SlideParam2, 0.5 * p.SlideParam1 + p.SlideParam2));
            UnitPointParameterList.Add(
                new Tuple<int, double, double>(1, 0.5 + p.SlideParam1 + p.SlideParam2, 0.5 * p.SlideParam1 + p.SlideParam2));
           
            UnitPointParameterList.Add(
                new Tuple<int, double, double>(0, 0, p.SlideParam1 + 2 * p.SlideParam2));
            UnitPointParameterList.Add(
                new Tuple<int, double, double>(1, 0.5 * p.SlideParam1, p.SlideParam1 + 2 * p.SlideParam2));
            UnitPointParameterList.Add(
                new Tuple<int, double, double>(0, p.SlideParam1, p.SlideParam1 + 2 * p.SlideParam2));
            UnitPointParameterList.Add(
                new Tuple<int, double, double>(0, 0.5 - p.SlideParam2, p.SlideParam1 + p.SlideParam2));
            UnitPointParameterList.Add(
                new Tuple<int, double, double>(0, 0.5 + p.SlideParam1 + p.SlideParam2, p.SlideParam1 + p.SlideParam2));
            UnitPointParameterList.Add(
                new Tuple<int, double, double>(0, 1, p.SlideParam1 + 2 * p.SlideParam2));
           
            UnitPointParameterList.Add(
                new Tuple<int, double, double>(0, - p.SlideParam2, 0.5 + p.SlideParam2));
            UnitPointParameterList.Add(
                new Tuple<int, double, double>(0, p.SlideParam1 + p.SlideParam2, 0.5 + p.SlideParam2));
            UnitPointParameterList.Add(
                new Tuple<int, double, double>(0, 0.5, 0.5));
            UnitPointParameterList.Add(
                new Tuple<int, double, double>(1, 0.5 + 0.5 * p.SlideParam1, 0.5));
            UnitPointParameterList.Add(
                new Tuple<int, double, double>(0, 0.5 + p.SlideParam1, 0.5));
            UnitPointParameterList.Add(
                new Tuple<int, double, double>(0, 1 - p.SlideParam2, 0.5 + p.SlideParam2));
           
            UnitPointParameterList.Add(
                new Tuple<int, double, double>(1, - p.SlideParam2, 0.5 + 0.5 * p.SlideParam1 + p.SlideParam2));
            UnitPointParameterList.Add(
                new Tuple<int, double, double>(1, p.SlideParam1 + p.SlideParam2, 0.5 + 0.5 * p.SlideParam1 + p.SlideParam2));
            UnitPointParameterList.Add(
                new Tuple<int, double, double>(1, 1 - p.SlideParam2, 0.5 + 0.5 * p.SlideParam1 + p.SlideParam2));
           
            UnitPointParameterList.Add(
                new Tuple<int, double, double>(0, -p.SlideParam2, 0.5 + p.SlideParam1 + p.SlideParam2));
            UnitPointParameterList.Add(
                new Tuple<int, double, double>(0, p.SlideParam1 + p.SlideParam2, 0.5 + p.SlideParam1 + p.SlideParam2));
            UnitPointParameterList.Add(
                new Tuple<int, double, double>(0, 0.5, 0.5 + p.SlideParam1 + 2 * p.SlideParam2));
            UnitPointParameterList.Add(
                new Tuple<int, double, double>(1, 0.5 + 0.5 * p.SlideParam1, 0.5 + p.SlideParam1 + 2 * p.SlideParam2));
            UnitPointParameterList.Add(
                new Tuple<int, double, double>(0, 0.5 + p.SlideParam1, 0.5 + p.SlideParam1 + 2 * p.SlideParam2));
            UnitPointParameterList.Add(
                new Tuple<int, double, double>(0, 1 - p.SlideParam2, 0.5 + p.SlideParam1 + p.SlideParam2));
           
            UnitPointParameterList.Add(
                new Tuple<int, double, double>(0, 0, 1));
            UnitPointParameterList.Add(
                new Tuple<int, double, double>(1, 0.5 * p.SlideParam1, 1));
            UnitPointParameterList.Add(
                new Tuple<int, double, double>(0, p.SlideParam1, 1));
            UnitPointParameterList.Add(
                new Tuple<int, double, double>(0, 0.5 - p.SlideParam2, 1 + p.SlideParam2));
            UnitPointParameterList.Add(
                new Tuple<int, double, double>(0, 0.5 + p.SlideParam1 + p.SlideParam2, 1 + p.SlideParam2));
            UnitPointParameterList.Add(
                new Tuple<int, double, double>(0, 1, 1));
        }

        protected override void SetUnitFaces()
        {
            unitFaces = new List<int[]>();
            unitFaces.Add(new[] { 0, 1, 9, 8 });
            unitFaces.Add(new[] { 1, 2, 10, 9 });
            unitFaces.Add(new[] { 2, 6, 10 });
            unitFaces.Add(new[] { 2, 3, 6 });
            unitFaces.Add(new[] { 3, 4, 7, 6 });
            unitFaces.Add(new[] { 4, 5, 7 });
            unitFaces.Add(new[] { 7, 5, 13 });
            unitFaces.Add(new[] { 6, 11, 10 });
            unitFaces.Add(new[] { 6, 7, 12, 11 });
            unitFaces.Add(new[] { 7, 13, 12 });
            unitFaces.Add(new[] { 8, 9, 14 });
            unitFaces.Add(new[] { 9, 15, 14 });
            unitFaces.Add(new[] { 9, 10, 15 });
            unitFaces.Add(new[] { 10, 11, 16, 15 });
            unitFaces.Add(new[] { 11, 17, 16 });
            unitFaces.Add(new[] { 11, 12, 17 });
            unitFaces.Add(new[] { 12, 18, 17 });
            unitFaces.Add(new[] { 12, 13, 19, 18 });
            unitFaces.Add(new[] { 14, 15, 21, 20 });
            unitFaces.Add(new[] { 15, 16, 21 });
            unitFaces.Add(new[] { 20, 21, 24, 23 });
            unitFaces.Add(new[] { 21, 25, 24 });
            unitFaces.Add(new[] { 16, 25, 21 });
            unitFaces.Add(new[] { 16, 17, 26, 25 });
            unitFaces.Add(new[] { 17, 18, 27, 26 });
            unitFaces.Add(new[] { 18, 22, 27 });
            unitFaces.Add(new[] { 18, 19, 22 });
            unitFaces.Add(new[] { 22, 28, 27 });
            unitFaces.Add(new[] { 23, 30, 29 });
            unitFaces.Add(new[] { 23, 24, 30 });
            unitFaces.Add(new[] { 24, 31, 30 });
            unitFaces.Add(new[] { 24, 25, 32, 31 });
            unitFaces.Add(new[] { 25, 26, 32 });
            unitFaces.Add(new[] { 26, 33, 32 });
            unitFaces.Add(new[] { 26, 27, 33 });
            unitFaces.Add(new[] { 27, 28, 34, 33 });
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Rhino.Geometry;

namespace Crane.Patterns.TessellationOnSurface
{
    public class HuffmanStarsTrianglesOnDoubleSurface : TessellationOnDoubleSurface
    {
        public HuffmanStarsTrianglesOnDoubleSurface(Surface topSurface, Surface bottomSurface, int uCount, int vCount, TessellationParam param) 
            : base(topSurface, bottomSurface, uCount, vCount, param)
        {
        }

        protected override void SetUnitPointParameterList()
        {
            var p = (HuffmanStarsTrianglesParam)param;
            UnitPointParameterList = new List<Tuple<int, double, double>>();
            UnitPointParameterList.Add(
                new Tuple<int, double, double>(0, 0, 0));
            UnitPointParameterList.Add(
                new Tuple<int, double, double>(0, 0.5, - p.SlideParam1));
            UnitPointParameterList.Add(
                new Tuple<int, double, double>(0, 1, 0));
            UnitPointParameterList.Add(
                new Tuple<int, double, double>(1, - p.SlideParam2, 0.25 - 0.5 * p.SlideParam1));
            UnitPointParameterList.Add(
                new Tuple<int, double, double>(1, p.SlideParam2, 0.25 - 0.5 * p.SlideParam1));
            UnitPointParameterList.Add(
                new Tuple<int, double, double>(1, 0.5 + p.SlideParam2, 0.25 - 0.5 * p.SlideParam1));
            UnitPointParameterList.Add(
                new Tuple<int, double, double>(0, 0, 0.5 - p.SlideParam1));
            UnitPointParameterList.Add(
                new Tuple<int, double, double>(0, 0.5, 0.5));
            UnitPointParameterList.Add(
                new Tuple<int, double, double>(0, 1, 0.5 - p.SlideParam1));
            UnitPointParameterList.Add(
                new Tuple<int, double, double>(1, 0.5 - p.SlideParam2, 0.75 - 0.5 * p.SlideParam1));
            UnitPointParameterList.Add(
                new Tuple<int, double, double>(1, 0.5 + p.SlideParam2, 0.75 - 0.5 * p.SlideParam1));
            UnitPointParameterList.Add(
                new Tuple<int, double, double>(0, 0, 1));
            UnitPointParameterList.Add(
                new Tuple<int, double, double>(0, 0.5, 1 - p.SlideParam1));
            UnitPointParameterList.Add(
                new Tuple<int, double, double>(0, 1, 1));
        }

        protected override void SetUnitFaces()
        {
            unitFaces = new List<int[]>();
            unitFaces.Add(new[] { 0, 4, 3 });
            unitFaces.Add(new[] { 3, 4, 6 });
            unitFaces.Add(new[] { 0, 1, 4 });
            unitFaces.Add(new[] { 4, 7, 6 });
            unitFaces.Add(new[] { 1, 7, 4 });
            unitFaces.Add(new[] { 1, 5, 7 });
            unitFaces.Add(new[] { 1, 2, 5 });
            unitFaces.Add(new[] { 5, 8, 7 });
            unitFaces.Add(new[] { 6, 9, 11 });
            unitFaces.Add(new[] { 6, 7, 9 });
            unitFaces.Add(new[] { 9, 12, 11 });
            unitFaces.Add(new[] { 7, 10, 9 });
            unitFaces.Add(new[] { 9, 10, 12 });
            unitFaces.Add(new[] { 7, 8, 10 });
            unitFaces.Add(new[] { 10, 13, 12 });
            unitFaces.Add(new[] { 8, 13, 10 });
        }
    }
}

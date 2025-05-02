using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using QuikGraph.Collections;
using Rhino.Geometry;

namespace Crane.Patterns.TessellationOnSurface
{
    public class WaterbombOnDoubleSurface: TessellationOnDoubleSurface

    {
        public WaterbombOnDoubleSurface(Surface topSurface, Surface bottomSurface, int uCount, int vCount, TessellationParam param) : base(topSurface, bottomSurface, uCount, vCount, param)
        {
        }

        protected override void SetUnitPointParameterList()
        {
            var p = (WaterbombParam)param;
            UnitPointParameterList = new VertexList<Tuple<int, double, double>>();
            UnitPointParameterList.Add(
                new Tuple<int, double, double>(0, 0, p.SlideParam));
            UnitPointParameterList.Add(
                new Tuple<int, double, double>(0, 0.5, - p.SlideParam));
            UnitPointParameterList.Add(
                new Tuple<int, double, double>(0, 1, p.SlideParam));
            UnitPointParameterList.Add(
                new Tuple<int, double, double>(1, 0, 0.25));
            UnitPointParameterList.Add(
                new Tuple<int, double, double>(1, 1, 0.25));
            UnitPointParameterList.Add(
                new Tuple<int, double, double>(0, 0, 0.5 - p.SlideParam));
            UnitPointParameterList.Add(
                new Tuple<int, double, double>(0, 0.5, 0.5 + p.SlideParam));
            UnitPointParameterList.Add(
                new Tuple<int, double, double>(0, 1, 0.5 - p.SlideParam));
            UnitPointParameterList.Add(
                new Tuple<int, double, double>(1, 0.5, 0.75));
            UnitPointParameterList.Add(
                new Tuple<int, double, double>(0, 0, 1 + p.SlideParam));
            UnitPointParameterList.Add(
                new Tuple<int, double, double>(0, 0.5, 1 - p.SlideParam));
            UnitPointParameterList.Add(
                new Tuple<int, double, double>(0, 1, 1 + p.SlideParam));
        }

        protected override void SetUnitFaces()
        {
            unitFaces = new List<int[]>();
            unitFaces.Add(new[] { 0, 1, 3 });
            unitFaces.Add(new[] { 1, 2, 4 });
            unitFaces.Add(new[] { 1, 6, 3 });
            unitFaces.Add(new[] { 1, 4, 6 });
            unitFaces.Add(new[] { 3, 6, 5 });
            unitFaces.Add(new[] { 4, 7, 6 });
            unitFaces.Add(new[] { 5, 6, 8 });
            unitFaces.Add(new[] { 6, 7, 8 });
            unitFaces.Add(new[] { 5, 8, 9 });
            unitFaces.Add(new[] { 7, 11, 8 });
            unitFaces.Add(new[] { 8, 10, 9 });
            unitFaces.Add(new[] { 8, 11, 10 });
        }
    }
}

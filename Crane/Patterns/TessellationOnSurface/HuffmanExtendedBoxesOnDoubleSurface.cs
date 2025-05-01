using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using QuikGraph.Collections;
using Rhino.Geometry;

namespace Crane.Patterns.TessellationOnSurface
{
    public class HuffmanExtendedBoxesOnDoubleSurface : TessellationOnDoubleSurface
    {
        public HuffmanExtendedBoxesOnDoubleSurface(Surface topSurface, Surface bottomSurface, int uCount, int vCount, TessellationParam param) : base(topSurface, bottomSurface, uCount, vCount, param)
        {
        }

        protected override void SetUnitPointParameterList()
        {
            var p = (HuffmanExtendedBoxesParam)param;
            double q = 1 / Math.Sqrt(1 + Math.Pow(p.SlideParam1, 2));
            double r = q * p.SlideParam1;

            UnitPointParameterList = new VertexList<Tuple<int, double, double>>();
            UnitPointParameterList.Add(
                new Tuple<int, double, double>(1, r * p.SlideParam2, -q * p.SlideParam2));
            UnitPointParameterList.Add(
                new Tuple<int, double, double>(1, q * (q - r) - r * p.SlideParam2, r * (q - r) + q * p.SlideParam2));
            UnitPointParameterList.Add(
                new Tuple<int, double, double>(1, q * (q + p.SlideParam2), r * (q + p.SlideParam2)));

            UnitPointParameterList.Add(
                new Tuple<int, double, double>(1, -r * r + q * p.SlideParam2, q * r + r * p.SlideParam2));
            UnitPointParameterList.Add(
                new Tuple<int, double, double>(0, -r * r + q * (r + p.SlideParam3), q * r + r * (r + p.SlideParam3)));
            UnitPointParameterList.Add(
                new Tuple<int, double, double>(0, q * (q - r) - r * (r + p.SlideParam3), r * (q - r) + q * (r + p.SlideParam3)));

            UnitPointParameterList.Add(
                new Tuple<int, double, double>(0, -r * (q - r - p.SlideParam3) + q * r, q * (q - r - p.SlideParam3) + r * r));
            UnitPointParameterList.Add(
                new Tuple<int, double, double>(0, -r * (q - r) + q * (q - r - p.SlideParam3), q * (q - r) + r * (q - r - p.SlideParam3)));
            UnitPointParameterList.Add(
                new Tuple<int, double, double>(1, -q * r - q * p.SlideParam2 + 1, q * q - r * p.SlideParam2));
            UnitPointParameterList.Add(
                new Tuple<int, double, double>(1, 1 + r * p.SlideParam2, 1 - q * p.SlideParam2));

            UnitPointParameterList.Add(
                new Tuple<int, double, double>(1, -q * r - q * p.SlideParam2, q * q - r * p.SlideParam2));
            UnitPointParameterList.Add(
                new Tuple<int, double, double>(1, r * p.SlideParam2, 1 - q * p.SlideParam2));
            UnitPointParameterList.Add(
                new Tuple<int, double, double>(1, q * (q - r) - r * p.SlideParam2, r * (q - r) + q * p.SlideParam2 + 1));
            UnitPointParameterList.Add(
                new Tuple<int, double, double>(1, q * (q + p.SlideParam2), r * (q + p.SlideParam2) + 1));

        }

        protected override void SetUnitFaces()
        {
            unitFaces = new List<int[]>();
            unitFaces.Add(new[] { 0, 4, 3 });
            unitFaces.Add(new[] { 0, 1, 5, 4 });
            unitFaces.Add(new[] { 1, 2, 5 });
            unitFaces.Add(new[] { 2, 8, 7, 5 });
            unitFaces.Add(new[] { 4, 5, 7, 6 });
            unitFaces.Add(new[] { 3, 4, 6, 10 });
            unitFaces.Add(new[] { 6, 11, 10 });
            unitFaces.Add(new[] { 6, 7, 12, 11 });
            unitFaces.Add(new[] { 7, 8, 12 });
            unitFaces.Add(new[] { 8, 9, 13, 12 });
        }
    }
}

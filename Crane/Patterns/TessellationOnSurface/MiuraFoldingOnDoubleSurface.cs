using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Rhino.Geometry;

namespace Crane.Patterns.TessellationOnSurface
{
    public class MiuraFoldingOnDoubleSurface : TessellationOnDoubleSurface
    {
        public MiuraFoldingOnDoubleSurface(Surface topSurface, Surface bottomSurface, int uCount, int vCount, MiuraFoldingParam param) 
            : base(topSurface, bottomSurface, uCount, vCount, param)
        {
        }

        protected override void SetUnitPointParameterList()
        {
            var p = (MiuraFoldingParam)param;
            UnitPointParameterList = new List<Tuple<int, double, double>>();
            UnitPointParameterList.Add(
                new Tuple<int,double,double>(1, p.SlideParam, 0));
            UnitPointParameterList.Add(
                new Tuple<int,double,double>(0, p.SlideParam + 0.5, 0));
            UnitPointParameterList.Add(
                new Tuple<int,double,double>(1, p.SlideParam + 1.0, 0));
            UnitPointParameterList.Add(
                new Tuple<int,double,double>(1, 0, 0.5));
            UnitPointParameterList.Add(
                new Tuple<int,double,double>(0, 0.5, 0.5));
            UnitPointParameterList.Add(
                new Tuple<int,double,double>(1, 1.0, 0.5));
            UnitPointParameterList.Add(
                new Tuple<int,double,double>(1, p.SlideParam, 1.0));
            UnitPointParameterList.Add(
                new Tuple<int,double,double>(0, p.SlideParam + 0.5, 1.0));
            UnitPointParameterList.Add(
                new Tuple<int,double,double>(1, p.SlideParam + 1.0, 1.0));
        }

        protected override void SetUnitFaces()
        {
            unitFaces = new List<int[]>();
            unitFaces.Add(new[] { 0, 1, 4, 3 });
            unitFaces.Add(new[] { 1, 2, 5, 4 });
            unitFaces.Add(new[] { 3, 4, 7, 6 });
            unitFaces.Add(new[] { 4, 5, 8, 7 });
        }
    }
}

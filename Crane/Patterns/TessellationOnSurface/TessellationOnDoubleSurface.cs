using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Rhino.Geometry;
using Rhino.Geometry.Collections;

namespace Crane.Patterns.TessellationOnSurface
{
    public abstract class TessellationOnDoubleSurface
    {
        public TessellationOnDoubleSurface(Surface topSurface, Surface bottomSurface, int uCount, int vCount, TessellationParam param)
        {
            TopSurface = topSurface;
            BottomSurface = bottomSurface;
            UCount = uCount;
            VCount = vCount;
            this.param = param;
            SetUnitParameterLength();
            SetUnitPointParameterList();
            UnitPointCount = UnitPointParameterList.Count;
            SetUnitFaces();
            SetTessellation();
        }

        protected TessellationParam param;
        /// <summary>
        /// Describe unit point parameter such as
        /// [top or bottom (0 or 1), U parameter in [0,1], V parameter in [0,1]]
        /// </summary>
        protected List<Tuple<int, double, double>> UnitPointParameterList;

        protected List<int[]> unitFaces;

        private double unitUParameterLength;
        private double unitVParameterLength;
        private double uParamStart;
        private double vParamStart;

        /// <summary>
        /// Number of units along U direction.
        /// </summary>
        public int UCount { get; private set; }

        /// <summary>
        /// Number of units along V direction.
        /// </summary>
        public int VCount { get; private set; }

        public int UnitPointCount { get; private set; }

        public Surface TopSurface { get; private set; }
        public Surface BottomSurface { get; private set; }
        public Mesh Tessellation { get; private set; }

        protected abstract void SetUnitPointParameterList();

        protected abstract void SetUnitFaces();

        private void SetUnitParameterLength()
        {
            var uInterval = TopSurface.Domain(0);
            var vInterval = TopSurface.Domain(1);
            unitUParameterLength = uInterval.Length / UCount;
            unitVParameterLength = vInterval.Length / VCount;
            uParamStart = uInterval.Min;
            vParamStart = vInterval.Min;
        }
        private void SetTessellation()
        {
            var pts = new List<Point3d>();
            for (int u = 0; u < UCount; u++)
            {
                for (int v = 0; v < VCount; v++)
                {
                    for (int i = 0; i < UnitPointCount; i++)
                    {
                        pts.Add(GetSurfacePoint(u, v, i));
                    }
                }
            }

            var mesh = new Mesh();
            mesh.Vertices.AddVertices(pts);

            for (int u = 0; u < UCount; u++)
            {
                for (int v = 0; v < VCount; v++)
                {
                    foreach (var face in unitFaces)
                    {
                        int offset = UnitPointCount * (u + UCount * v);
                        if (face.Length == 3)
                        {
                            mesh.Faces.AddFace(offset + face[0], offset + face[1], offset + face[2]);
                        }
                        else if (face.Length == 4)
                        {
                            mesh.Faces.AddFace(offset + face[0], offset + face[1], offset + face[2], offset + face[3]);
                        }
                    }
                }
            } 
            mesh.Normals.ComputeNormals();
            mesh.FaceNormals.ComputeFaceNormals();
            mesh.Weld(Math.PI);
            Tessellation = mesh;

        }

        private Point3d GetSurfacePoint(int uIndex, int vIndex, int unitPointIndex)
        {
            var unitPointParameter = UnitPointParameterList[unitPointIndex];
            var uParameter = (uIndex + unitPointParameter.Item2) * unitUParameterLength + uParamStart;
            var vParameter = (vIndex + unitPointParameter.Item3) * unitVParameterLength + vParamStart;
            var topOrBottom = unitPointParameter.Item1;
            if (topOrBottom == 0)
            {
                return TopSurface.PointAt(uParameter, vParameter);
            }
            else
            {
                return BottomSurface.PointAt(uParameter, vParameter);
            }
        }

    }
}

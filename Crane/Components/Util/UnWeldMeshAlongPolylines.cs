using System;
using System.Collections.Generic;
using System.Linq;
using Crane.Core;
using Grasshopper.Kernel;
using Rhino.Geometry;

namespace Crane.Components.Util
{
    public class UnWeldMeshAlongPolylines : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the UnWeldMeshAlongPolylines class.
        /// </summary>
        public UnWeldMeshAlongPolylines()
          : base("UnWeld Mesh Along Polylines", "UnWeld Mesh",
              "Un weld a mesh along with polylines.",
              "Crane", "Util")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddMeshParameter("Mesh", "Mesh", "A mesh to un weld.", GH_ParamAccess.item);
            pManager.AddCurveParameter("Polylines", "Polylines", "Un welding polylines.", GH_ParamAccess.list);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddMeshParameter("UnWelded", "UnWelded", "Un welded mesh.", GH_ParamAccess.item);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            Mesh mesh = null;
            List<Curve> curves = new List<Curve>();
            DA.GetData(0, ref mesh);
            DA.GetDataList(1, curves);

            List<PolylineCurve> polylines = curves.Select(c => (PolylineCurve)c).ToList();
            PointCloud pc = new PointCloud(mesh.Vertices.ToPoint3dArray());
            List<int> edgeIds = new List<int>();
            foreach (PolylineCurve plc in polylines)
            {
                var pl = plc.ToPolyline();
                var segments = pl.GetSegments();
                foreach (Line seg in segments)
                {
                    int vst = pc.ClosestPoint(seg.From);
                    int ven = pc.ClosestPoint(seg.To);
                    int eid = mesh.TopologyEdges.GetEdgeIndex(vst, ven);
                    edgeIds.Add(eid);
                }
            }

            mesh.UnweldEdge(edgeIds, true);
            List<Vector3d> moveVecs = new List<Vector3d>();
            Random random = new Random();
            for (int i = 0; i < mesh.Vertices.Count; i++)
            {
                double phi = 2 * Math.PI * random.NextDouble();
                double rho = Math.PI * (random.NextDouble() - 0.5);
                Vector3d vec = Vector3d.XAxis;
                vec.Rotate(rho, Vector3d.YAxis);
                vec.Rotate(phi, Vector3d.ZAxis);
                vec /= 1e4;
                Vector3f move = new Vector3f((float)vec.X, (float)vec.Y, (float)vec.Z);
                mesh.Vertices[i] += move;
            }

            DA.SetData(0, mesh);
        }

        /// <summary>
        /// Provides an Icon for the component.
        /// </summary>
        protected override System.Drawing.Bitmap Icon
        {
            get
            {
                //You can add image files to your project resources and access them like this:
                // return Resources.IconForThisComponent;
                return Properties.Resource.icons_unweld_polyline;
            }
        }

        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid
        {
            get { return new Guid("D3D0E5B8-D7F6-4DDE-9DDC-A174FF02ED2A"); }
        }
    }
}
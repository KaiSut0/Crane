using System;
using System.Collections.Generic;

using Grasshopper.Kernel;
using MathNet.Numerics.LinearAlgebra;
using Rhino.Geometry;

namespace Crane.Components.Util
{
    public class DeformationGradientComponent : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the ComputeDeformationGradient class.
        /// </summary>
        public DeformationGradientComponent()
          : base("Deformation Gradient", "Def Grad",
              "Calculate deformation gradient between two topologically identical meshes.",
              "Crane", "Util")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddMeshParameter("From", "From", "From", GH_ParamAccess.item);
            pManager.AddMeshParameter("To", "To", "To", GH_ParamAccess.item);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddNumberParameter("Sigma1", "Sigma1", "Sigma1", GH_ParamAccess.list);
            pManager.AddNumberParameter("Sigma2", "Sigma2", "Sigma2", GH_ParamAccess.list);
            pManager.AddNumberParameter("ThetaV", "ThetaV", "ThetaV", GH_ParamAccess.list);
            pManager.AddNumberParameter("ThetaU", "ThetaU", "ThetaU", GH_ParamAccess.list);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            Mesh from = null;
            Mesh to = null;

            DA.GetData(0, ref from);
            DA.GetData(1, ref to);

            if (from.Vertices.Count != to.Vertices.Count) throw new Exception("Vertex counts do not match.");
            if (from.Faces.Count != to.Faces.Count) throw new Exception("Face counts do not match.");
            if (from.TopologyEdges.Count != to.TopologyEdges.Count) throw new Exception("Edge counts do not match.");


            var vartsFrom = from.Vertices.ToPoint3dArray();
            var vartsTo = to.Vertices.ToPoint3dArray();

            var sigma1 = new double[from.Faces.Count];
            var sigma2 = new double[from.Faces.Count];
            var thetaV = new double[from.Faces.Count];
            var thetaU = new double[from.Faces.Count];

            for (int fid = 0; fid < from.Faces.Count; fid++)
            {
                var ff = from.Faces[fid];
                var ft = to.Faces[fid];
                var vf0 = vartsFrom[ff.A];
                var vf1 = vartsFrom[ff.B];
                var vf2 = vartsFrom[ff.C];
                var vt0 = vartsTo[ft.A];
                var vt1 = vartsTo[ft.B];
                var vt2 = vartsTo[ft.C];

                var vf01 = vf1 - vf0;
                var vf02 = vf2 - vf0;
                var vt01 = vt1 - vt0;
                var vt02 = vt2 - vt0;
                var nf = Vector3d.CrossProduct(vf01, vf02);
                nf.Unitize();
                var nt = Vector3d.CrossProduct(vt01, vt02);
                nt.Unitize();

                var xf = vf01 / vf01.Length;
                var yf = Vector3d.CrossProduct(nf, xf);

                var xt = vt01 / vt01.Length;
                var yt = Vector3d.CrossProduct(nt, xt);

                var localCoordinateFrom = new Plane(vf0, xf, yf);
                var localCoordinateTo = new Plane(vt0, xt, yt);

                double uxf, vxf, uyf, vyf;
                double uxt, vxt, uyt, vyt;

                localCoordinateFrom.ClosestParameter(vf1, out uxf, out vxf);
                localCoordinateFrom.ClosestParameter(vf2, out uyf, out vyf);
                localCoordinateTo.ClosestParameter(vt1, out uxt, out vxt);
                localCoordinateTo.ClosestParameter(vt2, out uyt, out vyt);

                Matrix<double> dxf = Matrix<double>.Build.DenseOfArray(new double[,]
                {
                    { uxf, uyf },
                    { vxf, vyf }
                });

                Matrix<double> dxt = Matrix<double>.Build.DenseOfArray(new double[,]
                {
                    { uxt, uyt },
                    { vxt, vyt }
                });

                Matrix<double> F = dxt * dxf.Inverse();

                var FSVD = F.Svd();
                sigma1[fid] = FSVD.S[0];
                sigma2[fid] = FSVD.S[1];

                

                var cosThetaV = FSVD.VT[0, 0];
                var sinThetaV = FSVD.VT[1, 0];
                thetaV[fid] = Math.Atan2(sinThetaV, cosThetaV);
                
                var cosThetaU = FSVD.U[0, 0];
                var sinThetaU = FSVD.U[1, 0];
                thetaU[fid] = Math.Atan2(sinThetaU, cosThetaU);
            }

            DA.SetDataList(0, sigma1);
            DA.SetDataList(1, sigma2);
            DA.SetDataList(2, thetaV);
            DA.SetDataList(3, thetaU);

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
                return Properties.Resource.icons_deformation_grad;
            }
        }

        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid
        {
            get { return new Guid("A7241039-76F5-4276-AFD1-D3C77FCC618C"); }
        }
    }
}
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
            pManager.AddPlaneParameter("PD", "PD", "Principal Direction", GH_ParamAccess.list);
            pManager.AddPlaneParameter("R", "R", "Rotation Plane", GH_ParamAccess.list);
            pManager.AddVectorParameter("S", "S", "S", GH_ParamAccess.list);
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
            var planes = new Plane[from.Faces.Count];
            var rots = new Plane[from.Faces.Count];
            var scales = new Vector3d[from.Faces.Count];

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
                var ffc = from.Faces.GetFaceCenter(fid);
                var ftc = to.Faces.GetFaceCenter(fid);

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

                var localCoordinateFrom = new Plane(ffc, xf, yf);
                var localCoordinateTo = new Plane(ftc, xt, yt);

                double uxf, vxf, uyf, vyf;
                double uxt, vxt, uyt, vyt;                

                localCoordinateFrom.ClosestParameter(vf1, out uxf, out vxf);
                localCoordinateFrom.ClosestParameter(vf2, out uyf, out vyf);
                localCoordinateTo.ClosestParameter(vt1, out uxt, out vxt);
                localCoordinateTo.ClosestParameter(vt2, out uyt, out vyt);

                var dxf_ = new Transform();
                dxf_[0, 0] = vf01[0]; dxf_[0, 1] = vf02[0]; dxf_[0, 2] = nf[0];
                dxf_[1, 0] = vf01[1]; dxf_[1, 1] = vf02[1]; dxf_[1, 2] = nf[1];
                dxf_[2, 0] = vf01[2]; dxf_[2, 1] = vf02[2]; dxf_[2, 2] = nf[2];
                dxf_[3, 3] = 1;

                var dxt_ = new Transform();
                dxt_[0, 0] = vt01[0]; dxt_[0, 1] = vt02[0]; dxt_[0, 2] = nt[0];
                dxt_[1, 0] = vt01[1]; dxt_[1, 1] = vt02[1]; dxt_[1, 2] = nt[1];
                dxt_[2, 0] = vt01[2]; dxt_[2, 1] = vt02[2]; dxt_[2, 2] = nt[2];
                dxt_[3, 3] = 1;


                var dxf_inv = new Transform();
                dxf_.TryGetInverse(out dxf_inv);
                var F_ = dxt_ * dxf_inv;


                

 
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

                var U = Matrix<double>.Build.Dense(2, 2);
                var u = Matrix<double>.Build.Dense(2, 2);

                var Theta = Vector3d.VectorAngle(vf01, vf02);
                U[0, 0] = vf01.Length;
                U[0, 1] = vf02.Length * Math.Cos(Theta);
                U[1, 1] = vf02.Length * Math.Sin(Theta);
                
                var theta = Vector3d.VectorAngle(vt01, vt02);
                u[0, 0] = vt01.Length;
                u[0, 1] = vt02.Length * Math.Cos(theta);
                u[1, 1] = vt02.Length * Math.Sin(theta);

                F = u * U.Inverse();

                var FSVD = F.Svd();
                sigma1[fid] = FSVD.S[0];
                sigma2[fid] = FSVD.S[1];

                //var F_ = new Transform();
                //F_[0, 0] = F[0, 0]; F_[0, 1] = F[0, 1];
                //F_[1, 0] = F[1, 0]; F_[1, 1] = F[1, 1];
                //F_[2, 2] = 1; F_[3, 3] = 1;
                var ortho = new Transform();
                var rot = new Transform();
                var scale = new Vector3d();
                var trans = new Vector3d();
                F_.DecomposeAffine(out trans, out rot, out ortho, out scale);

                Vector3d x = new Vector3d(ortho[0, 0], ortho[1, 0], 0);
                Vector3d y = new Vector3d(ortho[0, 1], ortho[1, 1], 0);
                Vector3d rotX = new Vector3d(rot[0, 0], rot[1, 0], 0);
                Vector3d rotY = new Vector3d(rot[0, 1], rot[1, 1], 0);
                var pln = new Plane(Point3d.Origin, x, y);
                var rotPln = new Plane(Point3d.Origin, rotX, rotY);
                planes[fid] = localCoordinateFrom;
                rots[fid] = localCoordinateTo;
                scales[fid] = scale;


                var cosThetaV = FSVD.VT[0, 0];
                var sinThetaV = FSVD.VT[0, 1];
                thetaV[fid] = Math.Atan2(sinThetaV, cosThetaV);
                
                var cosThetaU = FSVD.U[0, 0];
                var sinThetaU = FSVD.U[1, 0];
                thetaU[fid] = Math.Atan2(sinThetaU, cosThetaU);
            }

            DA.SetDataList(0, sigma1);
            DA.SetDataList(1, sigma2);
            DA.SetDataList(2, thetaV);
            DA.SetDataList(3, thetaU);
            DA.SetDataList(4, planes);
            DA.SetDataList(5, rots);
            DA.SetDataList(6, scales);

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
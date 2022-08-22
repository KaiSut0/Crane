using Grasshopper.Kernel;
using Rhino.Geometry;
using System;
using System.Collections.Generic;
using Crane.Core;
using MathNet.Numerics.LinearAlgebra.Factorization;

namespace Crane.Components.Solver
{
    public class SolutionSpaceDOF : GH_Component
    {
        private RigidOrigami rigidOrigami = null;
        //private Svd<double> svd = null;
        private double[] s = null;
        private int dof = 0;
        private double[] rv = null;
        /// <summary>
        /// Initializes a new instance of the SolutionSpaceDOF class.
        /// </summary>
        public SolutionSpaceDOF()
          : base("Solution Space DOF", "Solution Space DOF",
              "Calculate the solution space DOF",
              "Crane", "Solver")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddGenericParameter("Rigid Origami", "Rigid Origami", "Rigid Origami", GH_ParamAccess.item);
            pManager.AddBooleanParameter("Compute", "Compute", "Compute", GH_ParamAccess.item, false);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddIntegerParameter("DOF", "DOF", "DOF", GH_ParamAccess.item);
            pManager.AddNumberParameter("SV", "SV", "Singular values", GH_ParamAccess.list);
            pManager.AddNumberParameter("RV", "RV", "Residual vector", GH_ParamAccess.list);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            RigidOrigami rigidOrigamiSI = new RigidOrigami();
            bool compute = false;

            DA.GetData(0, ref rigidOrigamiSI);
            DA.GetData(1, ref compute);

            if (compute)
            {
                rigidOrigami = rigidOrigamiSI;
            }

            if (rigidOrigami != null && compute)
            {
                //bool useMKL = MathNet.Numerics.Providers.LinearAlgebra.LinearAlgebraControl.TryUseNativeMKL();
                //MathNet.Numerics.Providers.LinearAlgebra.LinearAlgebraControl.UseManaged();
                //MathNet.Numerics.Providers.LinearAlgebra.LinearAlgebraControl.UseNativeMKL();
                //var provider = MathNet.Numerics.Providers.LinearAlgebra.LinearAlgebraControl.Provider; 
                
                s = rigidOrigami.ComputeSvdOfJacobian();

                dof = 3 * rigidOrigami.CMesh.NumberOfVertices - Core.Util.SvdRank(s);
                rigidOrigami.ComputeResidual();
                rv = rigidOrigami.Error.ToArray();
            }

            DA.SetData(0, dof);
            DA.SetDataList(1, s);
            DA.SetDataList(2, rv);


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
                return Properties.Resource.icons_compute_solution_space_dof;
            }
        }

        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid
        {
            get { return new Guid("047d74be-ecd7-40dd-bcd9-f85829796e21"); }
        }
    }
}
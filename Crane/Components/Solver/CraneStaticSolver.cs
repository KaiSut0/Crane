using Grasshopper.Kernel;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Crane.Core;
using MathNet.Numerics.LinearAlgebra;

namespace Crane.Components.Solver
{
    public class CraneStaticSolver : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the CraneStaticSolver class.
        /// </summary>
        public CraneStaticSolver()
          : base("Crane Static Solver", "Static Solver",
              "Static solver for a folding simulation and a form finding.",
              "Crane", "Solver")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddGenericParameter("CMesh", "CMesh", "Input a CMesh.", GH_ParamAccess.item);
            pManager.AddGenericParameter("Constraints", "Constraints", "Input constraints", GH_ParamAccess.list);
            pManager.AddIntegerParameter("NR Iteration", "NR Iteration", "Each iteration of Newton Raphson method.", GH_ParamAccess.item, 50);
            pManager.AddIntegerParameter("CGNR Iteration", "CGNR Iteration",
                "Each iteration of CGNR method for solving linear equation in Newton Raphson method.",
                GH_ParamAccess.item, 100);
            pManager.AddNumberParameter("Threshold", "Threshold", "Threshold", GH_ParamAccess.item, 1e-13);
            pManager.AddBooleanParameter("Solve", "Solve", "If true, run to solve constraints.", GH_ParamAccess.item,
                false);
            pManager.AddBooleanParameter("Is Rigid Edge", "Is Rigid Edge",
                "if true, this enforce rigid edge constraint.", GH_ParamAccess.item, false);
            pManager.AddBooleanParameter("Is Panel Flat", "Is Panel Flat",
                "If true, this enforce panel flat constraint.", GH_ParamAccess.item, true);
            pManager.AddBooleanParameter("Is Fold Block", "Is Fold Block",
                "If true, this enforce 180° folding angle constarint", GH_ParamAccess.item, true);
            pManager.AddBooleanParameter("Is Constraint", "Is Constraint",
                "If true, this enforce additional constraints", GH_ParamAccess.item, true);

            pManager[1].Optional = true;
            pManager[2].Optional = true;
            pManager[3].Optional = true;
            pManager[4].Optional = true;
            pManager[5].Optional = true;
            pManager[6].Optional = true;
            pManager[7].Optional = true;
            pManager[8].Optional = true;
            pManager[9].Optional = true;
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddGenericParameter("CMesh", "CMesh", "Output the CMesh", GH_ParamAccess.item);
            pManager.AddGenericParameter("RigidOrigami", "RigidOrigami", "Output the RigidOrigami", GH_ParamAccess.item);
            pManager.AddNumberParameter("Residual", "Residual", "Residual", GH_ParamAccess.item);
            pManager.AddNumberParameter("RVec", "RVec", "Residual vector.", GH_ParamAccess.list);
            pManager.AddNumberParameter("Jacobian", "Jacobian", "Jacobian", GH_ParamAccess.list);

        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            bool useMKL = true;
            string message = "";
            message += useMKL.ToString();
            CMesh cMesh = new CMesh();
            List<Constraint> constraints = new List<Constraint>();
            int nrIteration = 10;
            int cgnrIteration = 100;
            double threshold = 1e-14;
            bool solve = false;
            bool isRigid = false;
            bool isPanelFlat = true;
            bool isFoldBlock = true;
            bool isConstraint = true;
            double residual = 0;

            if(!DA.GetData(0, ref cMesh)) { return; }
            DA.GetDataList(1, constraints);
            DA.GetData(2, ref nrIteration);
            DA.GetData(3, ref cgnrIteration);
            DA.GetData(4, ref threshold);
            DA.GetData(5, ref solve);
            DA.GetData(6, ref isRigid);
            DA.GetData(7, ref isPanelFlat);
            DA.GetData(8, ref isFoldBlock);
            DA.GetData(9, ref isConstraint);

            RigidOrigami rigidOrigami = new RigidOrigami(cMesh, constraints);

            rigidOrigami.SaveModes(isRigid, isPanelFlat, isFoldBlock, isConstraint);


            if (solve)
            {
                var moveVector = Vector<double>.Build.Dense(rigidOrigami.CMesh.DOF);
                residual = rigidOrigami.NRSolve(moveVector, threshold, nrIteration, cgnrIteration);

            }

            DA.SetData(0, rigidOrigami.CMesh);
            DA.SetData(1, rigidOrigami);
            DA.SetData(2, residual);
            DA.SetDataList(3, rigidOrigami.Error.ToArray());
            DA.SetDataList(4, rigidOrigami.Jacobian.ToArray());


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
                return Properties.Resource.icons_static_solver;
            }
        }


        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid
        {
            get { return new Guid("a0b16270-f6ff-445f-9baa-e42fee38c940"); }
        }
    }
}
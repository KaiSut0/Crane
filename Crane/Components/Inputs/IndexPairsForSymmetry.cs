using Grasshopper.Kernel;
using Rhino.Geometry;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Crane.Core;
using Rhino;

namespace Crane.Components.Inputs
{
    public class IndexPairsForSymmetry : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the IndexPairsForSymmetry class.
        /// </summary>
        public IndexPairsForSymmetry()
          : base("IndexPairs For Symmetry", "IndexPairs For Symmetry",
              "Create index pairs for symmetric constraint.",
              "Crane", "Inputs")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddGenericParameter("CMesh", "CMesh", "CMesh", GH_ParamAccess.item);
            pManager.AddTransformParameter("Transform", "Transform", "Transform", GH_ParamAccess.item);
            pManager.AddNumberParameter("Tolerance", "Tolerance", "Tolerance", GH_ParamAccess.item, 1e-3);
            pManager[2].Optional = true;
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddIntegerParameter("FirstIndex", "FirstIndex", "FirstIndex", GH_ParamAccess.list);
            pManager.AddIntegerParameter("SecondIndex", "SecondIndex", "SecondIndex", GH_ParamAccess.list);
            pManager.AddIntegerParameter("FixIndex", "FixIndex", "FixIndex", GH_ParamAccess.item);
            pManager.AddIntegerParameter("Development FirstIndex", "Dev FirstIndex", "Dev FirstIndex", GH_ParamAccess.list);
            pManager.AddIntegerParameter("Development SecondIndex", "Dev SecondIndex", "Dev SecondIndex", GH_ParamAccess.list);
            pManager.AddIntegerParameter("Development FixIndex", "Dev FixIndex", "Dev FixIndex", GH_ParamAccess.item);

        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            CMesh cMesh = new CMesh();
            Transform transform = new Transform();
            double tolerance = 1e-3;
            DA.GetData(0, ref cMesh);
            DA.GetData(1, ref transform);
            DA.GetData(2, ref tolerance);

            var pts = cMesh.Mesh.Vertices.ToPoint3dArray().ToList();
            List<IndexPair> indexPairs = new List<IndexPair>();
            List<int> indices = new List<int>();
            Utils.CreateTransformIndexPairs(pts, transform, tolerance, out indexPairs, out indices);
            List<int> firstIndices = new List<int>();
            List<int> secondIndices = new List<int>();
            Utils.SplitIndexPairs(indexPairs, out firstIndices, out secondIndices);

            DA.SetDataList(0, firstIndices);
            DA.SetDataList(1, secondIndices);
            DA.SetDataList(2, indices);
            if (cMesh.HasDevelopment)
            {
                List<int> devFirstIndices = new List<int>();
                List<int> devSecondIndices = new List<int>();
                List<int> devFixIndices = new List<int>();
                foreach(var id in firstIndices) devFirstIndices.Add(id + cMesh.NumberOfFoldingVertices);
                foreach(var id in secondIndices) devSecondIndices.Add(id + cMesh.NumberOfFoldingVertices);
                foreach(var id in indices) devFixIndices.Add(id + cMesh.NumberOfFoldingVertices);
                DA.SetDataList(3, devFirstIndices);
                DA.SetDataList(4, devSecondIndices);
                DA.SetDataList(5, devFixIndices);
            }

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
                return null;
            }
        }

        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid
        {
            get { return new Guid("79366125-e624-435d-acac-6a1156148574"); }
        }
    }
}
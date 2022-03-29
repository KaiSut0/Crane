using Grasshopper.Kernel;
using Rhino.Geometry;
using System;
using System.Collections.Generic;
using System.IO;
using Crane.Core;
using Grasshopper.Kernel.Data;

namespace Crane.Components.Util
{
    public class WriteFoldFormat : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the WriteFoldFormat class.
        /// </summary>
        public WriteFoldFormat()
          : base("Write Fold Format", "Write Fold Format",
              "Write CMesh to fold format file.",
              "Crane", "Outputs")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddGenericParameter("CMesh", "CMesh", "CMesh to write.", GH_ParamAccess.item);
            pManager.AddTextParameter("Path", "Path", "Path", GH_ParamAccess.item);
            pManager.AddBooleanParameter("Write", "Write", "Write or not.", GH_ParamAccess.item);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            CMesh cMesh = new CMesh();
            string path = "";
            bool write = false;

            DA.GetData(0, ref cMesh);
            DA.GetData(1, ref path);
            DA.GetData(2, ref write);

            if (write)
            {
                var foldFormat = cMesh.ToFoldFormat();
                File.WriteAllText(path, foldFormat);
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
                return Properties.Resource.icons_write_fold_format;
            }
        }

        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid
        {
            get { return new Guid("f9c22580-3741-42ac-b21e-4f6af34c0e5e"); }
        }
    }
}
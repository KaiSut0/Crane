using System;
using System.Collections.Generic;
using Crane.Core;
using Grasshopper.Kernel;
using Rhino.Geometry;

namespace Crane.Components.Outputs
{
    public class GetPeriodicProperties : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the GetPeriodicProperies class.
        /// </summary>
        public GetPeriodicProperties()
          : base("Get Periodic Properties", "Get Periodic properties",
              "Get Periodic Properties",
              "Crane", "Outputs")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddGenericParameter("CMesh", "CMesh", "CMesh", GH_ParamAccess.item);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddNumberParameter("theta1", "theta1", "theta1", GH_ParamAccess.item);
            pManager.AddNumberParameter("theta2", "theta2", "theta2", GH_ParamAccess.item);
            pManager.AddNumberParameter("a1", "a1", "a1", GH_ParamAccess.item);
            pManager.AddNumberParameter("a2", "a2", "a2", GH_ParamAccess.item);
            pManager.AddVectorParameter("Axis", "Axis", "Axis", GH_ParamAccess.item);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            CMesh cMesh = new CMesh();
            DA.GetData(0, ref cMesh);
            double theta1 = cMesh.CylindricallyRotationAngles[0];
            double theta2 = cMesh.CylindricallyRotationAngles[1];
            double a1 = cMesh.CylindricallyTranslationCoefficients[0];
            double a2 = cMesh.CylindricallyTranslationCoefficients[1];
            Vector3d axis = cMesh.CylinderAxis;
            DA.SetData(0, theta1);
            DA.SetData(1, theta2);
            DA.SetData(2, a1);
            DA.SetData(3, a2);
            DA.SetData(4, axis);

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
            get { return new Guid("f056379e-e107-48a7-bde2-15435ac138fa"); }
        }
    }
}
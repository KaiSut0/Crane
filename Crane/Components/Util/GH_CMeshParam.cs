using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Special;
using Microsoft.Win32.SafeHandles;
using Rhino;
using Rhino.DocObjects;
using Rhino.Geometry;
using Rhino.Input.Custom;

namespace Crane.Components.Util
{
    public class GH_CMeshParam : GH_PersistentGeometryParam<GH_CMesh>, IGH_PreviewObject, IGH_BakeAwareObject
    {
        public GH_CMeshParam()
            : base(new GH_InstanceDescription("CraneMesh", "CMesh", "Represents a origami geometry as a mesh.", "Params", "Geometry")) 
        { }

        protected override GH_CMesh InstantiateT()
        {
            return new GH_CMesh();
        }

        public override Guid ComponentGuid
        {
            get { return new Guid("86adae22-bb57-4491-9af7-bf1bdab08915"); }
        }
        protected override GH_GetterResult Prompt_Singular(ref GH_CMesh value)
        {
            throw new NotImplementedException();
        }

        protected override GH_GetterResult Prompt_Plural(ref List<GH_CMesh> values)
        {
            //GetObject go = new GetObject();
            //go.GeometryFilter = Rhino.DocObjects.ObjectType.Mesh;
            //if (go.GetMultiple(1, 0) != Rhino.Input.GetResult.Object)
            //    return GH_GetterResult.cancel;
            //if (values == null) values = new List<GH_CMesh>();

            //for (int i = 0; i < go.ObjectCount; i++)
            //    values.Add();
            throw new NotImplementedException();
        }

        

        public void DrawViewportWires(IGH_PreviewArgs args)
        {
            throw new NotImplementedException();
        }

        public void DrawViewportMeshes(IGH_PreviewArgs args)
        {
            throw new NotImplementedException();
        }

        public bool Hidden { get; set; }
        public bool IsPreviewCapable { get; }
        public BoundingBox ClippingBox { get; }
        public void BakeGeometry(RhinoDoc doc, List<Guid> obj_ids)
        {
            throw new NotImplementedException();
        }

        public void BakeGeometry(RhinoDoc doc, ObjectAttributes att, List<Guid> obj_ids)
        {
            throw new NotImplementedException();
        }

        public bool IsBakeCapable { get; }
    }
}

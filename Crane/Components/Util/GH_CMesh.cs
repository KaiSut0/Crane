using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using Crane.Core;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Types;
using Grasshopper.Kernel.Utility;
using Rhino;
using Rhino.Geometry;
using Rhino.DocObjects;

namespace Crane.Components.Util
{
    public class GH_CMesh : GH_GeometricGoo<CMesh>,
        IGH_BakeAwareData, IGH_PreviewData, IGH_PreviewMeshData
    {
        private Guid reference;
        BoundingBox _b = Rhino.Geometry.BoundingBox.Unset;
        private Line[] _mountainLines;
        private Line[] _valleyLines;
        private Line[] _triangulatedLines;
        private Line[] _unassignedLines;
        private Line[] _boundaryLines;
        private Mesh _mesh;

        public GH_CMesh() :this(null)
        { }

        public GH_CMesh(CMesh cMesh)
        {
            m_value = cMesh;
            ClearCaches();
        }

        public override CMesh Value
        {
            get { return base.Value; }
            set { base.Value = value; ClearCaches(); }
        }

        public override Guid ReferenceID
        {
            get { return reference; }
            set { reference = value; }
        }

        public override string ToString()
        {
            if (m_value == null)
                return "<Null mesh>";
            return m_value.ToString();
        }


        public bool BakeGeometry(RhinoDoc doc, ObjectAttributes att, out Guid obj_guid)
        {
            obj_guid = Guid.Empty;
            if (m_value == null)
                return false;

            doc.Objects.AddMesh(m_value.Mesh);
            return true;
        }

        public override string TypeName
        {
            get { return "CMesh"; }
        }
        public override string TypeDescription
        {
            get { return "Container for CMesh"; }
        }
        public override IGH_GeometricGoo DuplicateGeometry()
        {
            if (m_value == null) return null;
            return new GH_CMesh(m_value == null ? null : new CMesh(m_value)) {ReferenceID = ReferenceID};
        }

        public override Rhino.Geometry.BoundingBox Boundingbox
        {
            get
            {
                if (m_value != null && !_b.IsValid)
                {
                    _b = new BoundingBox(m_value.Mesh.Vertices.ToPoint3dArray());
                }
                return _b;
            }

        }
        public override BoundingBox GetBoundingBox(Transform xform)
        {
            var b = Boundingbox;
            b.Transform(xform);
            return b;
        }

        public override IGH_GeometricGoo Transform(Transform xform)
        {
            if (m_value != null)
            {
                var m = new CMesh(m_value);
                Point3d[] verts = m_value.Mesh.Vertices.ToPoint3dArray();
                List<Point3d> vertsTransed = new List<Point3d>();

                foreach (var v in verts)
                {
                    Point3d p = new Point3d(v);
                    p.Transform(xform);

                    vertsTransed.Add(p);
                }

                m.UpdateMesh(vertsTransed.ToArray());
                return new GH_CMesh(m);
            }

            return new GH_CMesh(null);
        }

        public override IGH_GeometricGoo Morph(SpaceMorph xmorph)
        {
            if (m_value != null)
            {
                var m = new CMesh(m_value);
                Point3d[] verts = m_value.Mesh.Vertices.ToPoint3dArray();
                List<Point3d> vertsTransed = new List<Point3d>();

                foreach (var v in verts)
                {
                    Point3d p = new Point3d(v);
                    p = xmorph.MorphPoint(p);

                    vertsTransed.Add(p);
                }

                m.UpdateMesh(vertsTransed.ToArray());
                return new GH_CMesh(m);
            }

            return new GH_CMesh(null);
        }

        public void DrawViewportWires(GH_PreviewWireArgs args)
        {
            if (this.m_value == null) return;
            for (int i = 0; i < m_value.Mesh.TopologyEdges.Count; i++)
            {
                args.Pipeline.DrawLine(m_value.Mesh.TopologyEdges.EdgeLine(i), args.Color, args.Thickness);
            }
        }

        public void DrawViewportMeshes(GH_PreviewMeshArgs args)
        {
            if (m_value == null) return;
            if (args.Pipeline.SupportsShading)
            {
                var c = args.Material.Diffuse;
                c = System.Drawing.Color.FromArgb((int) (args.Material.Transparency * 255), c);

                args.Pipeline.DrawMeshShaded(m_value.Mesh, args.Material);
            }
        }

        public BoundingBox ClippingBox
        {
            get { return Boundingbox; }
        }
        public Mesh[] GetPreviewMeshes()
        {
            if (m_value == null) return null;
            return new Mesh[] {m_value.Mesh};
        }

        public void DestroyPreviewMeshes()
        {
            m_value = null;
        }

        public override bool CastFrom(object source)
        {
            if (source == null)
            {
                m_value = null;
                ClearCaches();
                return true;
            }

            if (source is GH_GeometricGoo<Mesh>)
            {
                source = ((GH_GeometricGoo<Mesh>) source).Value;
            }

            if (source is CMesh)
            {
                m_value = source as CMesh;
                ClearCaches();
                return true;
            }
            else if (source is Mesh)
            {
                m_value = new CMesh(source as Mesh);
                ClearCaches();
                return true;
            }

            return base.CastFrom(source);
        }

        public override bool CastTo<Q>(out Q target)
        {
            if (typeof(Q) == typeof(Mesh) || typeof(Q) == typeof(GeometryBase))
            {
                target = (Q)(object)m_value.Mesh;
                return true;
            }
            if (typeof(Q) == typeof(CMesh))
            {
                target = (Q)(object)m_value;
                return true;
            }

            target = default(Q);
            return false;
        }
    }
}
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Crane.Constraints;
using FSharpx.Collections.Experimental;
using Grasshopper.Kernel.Geometry.Delaunay;
using QuickGraph;
using Rhino;
using Rhino.DocObjects.Custom;
using Rhino.Geometry;
using Rhino.Geometry.Intersect;
using Point2d = Rhino.Geometry.Point2d;
using Point3d = Rhino.Geometry.Point3d;

namespace Crane.Core
{
    public enum OffsetMethod
    {
        ThickPanel,
        Inkjet4DPrint
    }
    public class FabMesh
    {
        public CMesh CMesh { get; private set; }
        public CMesh DevCMesh { get; private set; }
        public int FaceCount { get; private set; }
        public List<Polyline> FacePolylines { get; private set; }
        public List<IndexPair> NgonConnectedEdges { get; private set; }
        public List<IndexPair> EdgeConnectedFacePair { get; private set; }
        public List<IndexPair> EdgeConnectedFacePolylinesIndexPair { get; private set; }
        public List<int> NgonConnectedEdgeIndex2InnerEdgeIndex { get; private set; }
        public List<double> FoldAngles { get; private set; }
        /// <summary>
        /// ngonEdgeFoldAngle[ngonIndex][ngonPolylineIndex]
        /// </summary>
        public Dictionary<IndexPair, double> NgonEdgeFoldAngle { get; private set; }
        public Dictionary<IndexPair, int> NgonEdgeIndex2NgonConnectedEdgeIndex { get; private set; }
        public List<Vector3d> NgonNormals { get; private set; }

        public FabMesh(CMesh cMesh, Point2d origin, double rotAngle, double tolerance)
        {
            CMesh = new CMesh(cMesh);
            DevCMesh = new CMesh(CMesh.GetDevelopment(new Point3d(origin.X, origin.Y, 0), rotAngle),
                CMesh.MountainEdges, CMesh.ValleyEdges);
            FaceCount = CMesh.Mesh.Ngons.Count;
            SetNgonNormals(DevCMesh);
            SetFacePolylines(DevCMesh);
            SetEdgeConnectedFacePair(DevCMesh);
            SetEdgeConnectedFacePolylinesIndexPair(DevCMesh, tolerance);
            SetFoldAngle(CMesh);
        }

        public FabMesh(CMesh cMesh, double tolerance)
        {
            CMesh = new CMesh(cMesh);
            FaceCount = CMesh.Mesh.Ngons.Count;
            SetNgonNormals(CMesh);
            SetFacePolylines(CMesh);
            SetEdgeConnectedFacePair(DevCMesh);
            SetEdgeConnectedFacePolylinesIndexPair(DevCMesh, tolerance);
            SetFoldAngle(CMesh);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="thickness"></param>
        /// <param name="topOrBootom">
        /// Top : true
        /// Bottom : false
        /// </param>
        /// <returns></returns>
        public virtual List<Polyline> OffsetFace(double thickness, bool topOrBootom)
        {
            List<Polyline> offsetFaces = new List<Polyline>();
            for (int i = 0; i < FaceCount; i++)
            {
                var facePolyline = FacePolylines[i];
                List<double> offsets = new List<double>();
                for (int j = 0; j < facePolyline.SegmentCount; j++)
                {
                    double foldAng = NgonEdgeFoldAngle[new IndexPair(i, j)];
                    if (topOrBootom)
                    {
                        if (foldAng < 0) foldAng = 0;
                    }
                    else
                    {
                        if (foldAng > 0) foldAng = 0;
                    }

                    double rho = Math.PI - Math.Abs(foldAng); 
                    double offset = thickness / Math.Tan(rho / 2);
                    offsets.Add(offset);
                }

                var normal = NgonNormals[i];
                offsetFaces.Add(OffsetPolyline(facePolyline, normal, offsets));
            }
            return offsetFaces;
        }

        public List<Polyline> OffsetFaceInkjet4D(double panelThickness, double sheetThickness, double shrinkRatio, bool topOrBottom)
        {
            List<Polyline> offsetFaces = new List<Polyline>();
            for (int i = 0; i < FaceCount; i++)
            {
                var facePolyline = FacePolylines[i];
                List<double> offsets = new List<double>();
                for (int j = 0; j < facePolyline.SegmentCount; j++)
                {
                    double foldAng = NgonEdgeFoldAngle[new IndexPair(i, j)];
                    double rho = Math.PI - Math.Abs(foldAng);

                    if (topOrBottom)
                    {
                        if (foldAng < 0)
                        {
                            rho = Math.PI;
                            foldAng = 0;
                        }
                    }
                    else
                    {
                        if (foldAng > 0)
                        {
                            rho = Math.PI;
                            foldAng = 0;
                        }
                    }

                    double offset_panel = (panelThickness + sheetThickness / 2) / Math.Tan(rho / 2);
                    double offset_ink = (panelThickness / 2 + sheetThickness) * Math.Abs(foldAng) / (2 * shrinkRatio);
                    //double offset = Math.Max(offset_panel, offset_ink);
                    double offset = offset_ink;
                    offsets.Add(offset);
                }

                var normal = NgonNormals[i];
                offsetFaces.Add(OffsetPolyline(facePolyline, normal, offsets));
            }
            return offsetFaces;

        }

        private Polyline OffsetPolyline(Polyline polyline, Vector3d normal, List<double> offsets)
        {
            List<Line> offsetLines = new List<Line>();
            for (int i = 0; i < polyline.SegmentCount; i++)
            {
                Line segment = polyline.SegmentAt(i);
                Vector3d offsetVec = Vector3d.CrossProduct(normal, segment.To - segment.From);
                offsetVec.Unitize();
                offsetVec *= offsets[i];
                segment.Transform(Transform.Translation(offsetVec));
                offsetLines.Add(segment);
            }

            List<Point3d> pts = new List<Point3d>();
            for (int i = 0; i < offsetLines.Count; i++)
            {
                Line s1 = offsetLines[i];
                Line s2 = offsetLines[(i+1) % offsetLines.Count];
                double a = 0, b = 0;
                Intersection.LineLine(s1, s2, out a, out b, 0, false);
                Point3d intersect = (s1.PointAt(a) + s2.PointAt(b)) / 2;
                pts.Add(intersect);
            }

            pts.Insert(0, pts[pts.Count - 1]);

            return new Polyline(pts);
        }

        private void SetNgonNormals(CMesh cMesh)
        {
            NgonNormals = new List<Vector3d>();
            for (int i = 0; i < cMesh.Mesh.Ngons.Count; i++)
            {
                var ngon = cMesh.Mesh.Ngons[i];
                Vector3d normal = cMesh.Mesh.FaceNormals[(int)(ngon.FaceIndexList()[0])];
                NgonNormals.Add(normal);
            }
        }
        private void SetFacePolylines(CMesh cMesh)
        {
            FacePolylines = new List<Polyline>();
            for (int i = 0; i < cMesh.Mesh.Ngons.Count; i++)
            {
                var ngon = cMesh.Mesh.Ngons[i];
                Point3d[] pts = cMesh.Mesh.Ngons.NgonBoundaryVertexList(ngon, true);
                Polyline polyline = new Polyline(pts);
                FacePolylines.Add(polyline);
            }
        }

        public List<Polyline> GetFacePolylines(CMesh cMesh)
        {
            var facePolylines = new List<Polyline>();
            for (int i = 0; i < cMesh.Mesh.Ngons.Count; i++)
            {
                var ngon = cMesh.Mesh.Ngons[i];
                Point3d[] pts = cMesh.Mesh.Ngons.NgonBoundaryVertexList(ngon, true);
                Polyline polyline = new Polyline(pts);
                facePolylines.Add(polyline);
            }
            return facePolylines;

        }

        public List<Transform> GetDev2FoldTransforms()
        {
            var transforms = new List<Transform>();
            for (int i = 0; i < CMesh.Mesh.Ngons.Count; i++)
            {
                var ngonF = CMesh.Mesh.Ngons[i];
                var ngonD = DevCMesh.Mesh.Ngons[i];
                Point3d[] ptsF = CMesh.Mesh.Ngons.NgonBoundaryVertexList(ngonF, true);
                Point3d[] ptsD = DevCMesh.Mesh.Ngons.NgonBoundaryVertexList(ngonD, true);
                Plane plnF = new Plane(ptsF[0], ptsF[1], ptsF[2]);
                Plane plnD = new Plane(ptsD[0], ptsD[1], ptsD[2]);
                transforms.Add(Transform.PlaneToPlane(plnD, plnF));
            }

            return transforms;
        }

        public List<Mesh> ThickPanels(List<Polyline> panelPolylines, double thickness)
        {
            var thickPanels = new List<Mesh>();

            foreach (var panelPolyline in panelPolylines)
            {
                Mesh baseMesh = Mesh.CreateFromClosedPolyline(panelPolyline);
                List<int> verts = new List<int>();
                List<int> faces = new List<int>();
                for(int i = 0; i < baseMesh.Vertices.Count; i++) verts.Add(i);
                for (int i = 0; i < baseMesh.Faces.Count; i++) faces.Add(i);
                baseMesh.Ngons.AddNgon(MeshNgon.Create(verts, faces));
                baseMesh.FaceNormals.ComputeFaceNormals();
                if (baseMesh.FaceNormals[0] * Vector3d.ZAxis > 0)
                {
                    baseMesh.Flip(true, true, true);
                }

                Mesh offsetMesh = baseMesh.Offset(thickness);
                offsetMesh.Flip(true, true, true);
                baseMesh.Append(offsetMesh);
                for (int i = 0; i < panelPolyline.SegmentCount; i++)
                {
                    Line segment = panelPolyline.SegmentAt(i);
                    Line offsetSegment = new Line(segment.From, segment.To);
                    offsetSegment.Transform(Transform.Translation(thickness * Vector3d.ZAxis));
                    Mesh sideMesh = new Mesh();
                    sideMesh.Vertices.Add(segment.From);
                    sideMesh.Vertices.Add(segment.To);
                    sideMesh.Vertices.Add(offsetSegment.To);
                    sideMesh.Vertices.Add(offsetSegment.From);
                    sideMesh.Faces.AddFace(0, 1, 2, 3);
                    baseMesh.Append(sideMesh);
                }

                baseMesh.FaceNormals.ComputeFaceNormals();
                baseMesh.Normals.ComputeNormals();
                thickPanels.Add(baseMesh);
            }
            return thickPanels;
        }
        private void SetEdgeConnectedFacePair(CMesh cMesh)
        {
            EdgeConnectedFacePair = new List<IndexPair>();
            NgonConnectedEdgeIndex2InnerEdgeIndex = new List<int>();
            NgonConnectedEdges = new List<IndexPair>();
            for (int i = 0; i < cMesh.InnerEdges.Count; i++)
            {
                if (cMesh.InnerEdgeAssignment[i] != 'T')
                {
                    var facePair = cMesh.FacePairs[i];
                    int ngonIndexI = cMesh.Mesh.Ngons.NgonIndexFromFaceIndex(facePair.I);
                    int ngonIndexJ = cMesh.Mesh.Ngons.NgonIndexFromFaceIndex(facePair.J);
                    EdgeConnectedFacePair.Add(new IndexPair(ngonIndexI, ngonIndexJ));
                    NgonConnectedEdgeIndex2InnerEdgeIndex.Add(i);
                    NgonConnectedEdges.Add(cMesh.InnerEdges[i]);
                }
            }
        }

        private void SetEdgeConnectedFacePolylinesIndexPair(CMesh cMesh, double tolerance)
        {
            EdgeConnectedFacePolylinesIndexPair = new List<IndexPair>();
            NgonEdgeIndex2NgonConnectedEdgeIndex = new Dictionary<IndexPair, int>();
            for (int i = 0; i < EdgeConnectedFacePair.Count; i++)
            {
                var e = cMesh.InnerEdges[NgonConnectedEdgeIndex2InnerEdgeIndex[i]];

                int vi = e.I;
                int vj = e.J;

                var fPair = EdgeConnectedFacePair[i];
                int fi = fPair.I;
                int fj = fPair.J;

                var pli = FacePolylines[fi];
                var plj = FacePolylines[fj];

                int ei = 0;
                int ej = 0;

                for (int ni = 0; ni < pli.SegmentCount; ni++)
                {
                    var line = pli.SegmentAt(ni);
                    var ptI = cMesh.Mesh.Vertices[vi];
                    var ptJ = cMesh.Mesh.Vertices[vj];
                    if ((line.From.DistanceTo(ptI) + line.To.DistanceTo(ptJ)) < tolerance
                        || (line.From.DistanceTo(ptJ) + line.To.DistanceTo(ptI)) < tolerance)
                    {
                        ei = ni;
                    }
                }
                for (int nj = 0; nj < plj.SegmentCount; nj++)
                {
                    var line = plj.SegmentAt(nj);
                    var ptI = cMesh.Mesh.Vertices[vi];
                    var ptJ = cMesh.Mesh.Vertices[vj];
                    if ((line.From.DistanceTo(ptI) + line.To.DistanceTo(ptJ)) < tolerance
                        || (line.From.DistanceTo(ptJ) + line.To.DistanceTo(ptI)) < tolerance)
                    {
                        ej = nj;
                    }
                }
                EdgeConnectedFacePolylinesIndexPair.Add(new IndexPair(ei, ej));
                NgonEdgeIndex2NgonConnectedEdgeIndex.Add(new IndexPair(fi, ei), i);
                NgonEdgeIndex2NgonConnectedEdgeIndex.Add(new IndexPair(fj, ej), i);
            }
        }

        private void SetFoldAngle(CMesh cMesh)
        {
            var origFoldAng = cMesh.GetFoldAngles();
            FoldAngles = new List<double>();
            for (int i = 0; i < NgonConnectedEdges.Count; i++)
            {
                FoldAngles.Add(origFoldAng[NgonConnectedEdgeIndex2InnerEdgeIndex[i]]);
            }

            NgonEdgeFoldAngle = new Dictionary<IndexPair, double>();
            for (int i = 0; i < FacePolylines.Count; i++)
            {
                var facePolyLine = FacePolylines[i];
                for (int j = 0; j < facePolyLine.SegmentCount; j++)
                {
                    var indexPair = new IndexPair(i, j);
                    if (NgonEdgeIndex2NgonConnectedEdgeIndex.ContainsKey(indexPair))
                    {
                        int ngonConnectedEdgeId = NgonEdgeIndex2NgonConnectedEdgeIndex[indexPair];
                        NgonEdgeFoldAngle[indexPair] = FoldAngles[ngonConnectedEdgeId];
                    }
                    else
                    {
                        NgonEdgeFoldAngle[indexPair] = 0;
                    }
                }
            }
        }
    }
}

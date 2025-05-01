using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using Rhino.Geometry;

namespace Crane.Misc
{
    public class ScaleRotation
    {
        public ScaleRotation(double scaleFactor, double rotationAngle, Vector3d rotationAxis, Point3d origin)
        {
            this.scaleFactor = scaleFactor;
            this.rotationAngle = rotationAngle;
            this.rotationAxis = rotationAxis;
            this.origin = origin;
            this.transform = this.Parametrized(1);
            this.scale = Transform.Scale(origin, scaleFactor);
            this.rotation = Transform.Rotation(rotationAngle, rotationAxis, origin);
        }

        public Transform Transform { get => transform; }
        public Transform Scale { get => scale; }
        public Transform Rotation { get => rotation; }
        public Point3d Origin { get => origin; }
        public Vector3d RotationAxis { get => rotationAxis; }
        public double RotationAngle { get => rotationAngle; }
        public double ScaleFactor { get => scaleFactor; }

        private Transform transform;
        private Transform rotation;
        private Transform scale;

        private Point3d origin;
        private Vector3d rotationAxis;
        private double rotationAngle;
        private double scaleFactor;


        public static Transform operator* (ScaleRotation scaleRotation, Transform transform)
        {
            return scaleRotation.transform * transform;
        }

        public static Transform operator *(Transform transform, ScaleRotation scaleRotation)
        {
            return transform * scaleRotation.transform;
        }

        public static Vector3d operator* (ScaleRotation scaleRotation, Vector3d vector)
        {
            return scaleRotation.transform * vector;
        }

        public Transform Parametrized(double t)
        {
            var pRotation = Transform.Rotation(t * rotationAngle, rotationAxis, origin);
            var pScale = Transform.Scale(origin, Math.Pow(scaleFactor, t));
            return pScale * pRotation; // This is equal to pRotation * pScale because of commutativity of the scale and the rotation with same origin.
        }

        public Transform Derivative(double t)
        {
            // Compute the derivative of the scale rotation as below.
            // [ I r1 ] [ d(s*t * R(theta * t)) / dt 0 ] [ I -r1 ]
            // [ 0  1 ] [                          0 0 ] [ 0   1 ]

            // Compute
            //   d(s*t * R(theta * t)) / dt
            // = s * R(theta * t) + s*t * [n]
            // = term1 + term2
            // where
            // n = [   0  -nz  ny ]
            //     [  nz    0 -nx ]
            //     [ -ny   nx   0 ].

            // Compute term1.
            var term1 = 
                  Transform.Scale(origin, Math.Log(scaleFactor) * Math.Pow(scaleFactor, t))
                * Transform.Rotation(t * rotationAngle, rotationAxis, origin);
            for (int i = 0; i < 4; i++) term1[i, 3] = 0;
            
            // Compute term2.
            var n = Transform.ZeroTransformation;
            n[3, 3] = 0;
            n[0, 1] = -rotationAxis.Z;
            n[1, 0] =  rotationAxis.Z;
            n[0, 2] =  rotationAxis.Y;
            n[2, 0] = -rotationAxis.Y;
            n[1, 2] = -rotationAxis.X;
            n[2, 1] =  rotationAxis.X;

            var nn = n * n;

            var term2 = Transform.ZeroTransformation;
            term2[3, 3] = 0;
            for(int i = 0; i < 3; i++)
            for (int j = 0; j < 3; j++)
                term2[i, j] = Math.Pow(scaleFactor, t) * rotationAngle * (Math.Cos(t) * n[i, j] + Math.Sin(t) * nn[i, j]);

            // Compute derivative.
            var term = Transform.ZeroTransformation;
            term[3, 3] = 0;
            for(int i = 0; i < 3; i++)
            for (int j = 0; j < 3; j++)
                term[i, j] = term1[i, j] + term2[i, j];

            var left = Transform.Translation(new Vector3d(origin));
            var right = Transform.Translation(-new Vector3d(origin));

            var value = left * term * right;
            value[3, 3] = 1;
            return value;
        }



    }
}

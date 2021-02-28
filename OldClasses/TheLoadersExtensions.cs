using System;
using System.IO;                //Required by Assimp-net
using System.Reflection;        //Required by Assimp-net
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Content;
using Assimp;                               // Note: install AssimpNET 5.01 via nuget
using Assimp.Configs;                    // Required by Assimp-net

namespace AssimpLoaderPbrDx
{
    public static class TheLoadersExtensions
    {
        public static float CheckVal(float n)
        {
            if (float.IsNaN(n) || n == float.NaN || float.IsInfinity(n))
                return 0.0f;
            else
                return n;
        }

        public static Matrix ToMg(this Assimp.Matrix4x4 ma)
        {
            Matrix m = Matrix.Identity;
            m.M11 = CheckVal(ma.A1); m.M12 = CheckVal(ma.A2); m.M13 = CheckVal(ma.A3); m.M14 = CheckVal(ma.A4);
            m.M21 = CheckVal(ma.B1); m.M22 = CheckVal(ma.B2); m.M23 = CheckVal(ma.B3); m.M24 = CheckVal(ma.B4);
            m.M31 = CheckVal(ma.C1); m.M32 = CheckVal(ma.C2); m.M33 = CheckVal(ma.C3); m.M34 = CheckVal(ma.C4);
            m.M41 = CheckVal(ma.D1); m.M42 = CheckVal(ma.D2); m.M43 = CheckVal(ma.D3); m.M44 = CheckVal(ma.D4);
            return m;
        }
        public static Matrix ToMgTransposed(this Assimp.Matrix4x4 ma)
        {
            Matrix m = Matrix.Identity;
            m.M11 = CheckVal(ma.A1); m.M12 = CheckVal(ma.A2); m.M13 = CheckVal(ma.A3); m.M14 = CheckVal(ma.A4);
            m.M21 = CheckVal(ma.B1); m.M22 = CheckVal(ma.B2); m.M23 = CheckVal(ma.B3); m.M24 = CheckVal(ma.B4);
            m.M31 = CheckVal(ma.C1); m.M32 = CheckVal(ma.C2); m.M33 = CheckVal(ma.C3); m.M34 = CheckVal(ma.C4);
            m.M41 = CheckVal(ma.D1); m.M42 = CheckVal(ma.D2); m.M43 = CheckVal(ma.D3); m.M44 = CheckVal(ma.D4);
            m = Matrix.Transpose(m);
            return m;
        }
        public static Matrix ToMgTransposed(this Assimp.Matrix3x3 ma)
        {
            Matrix m = Matrix.Identity;
            ma.Transpose();
            m.M11 = CheckVal(ma.A1); m.M12 = CheckVal(ma.A2); m.M13 = CheckVal(ma.A3); m.M14 = 0;
            m.M21 = CheckVal(ma.B1); m.M22 = CheckVal(ma.B2); m.M23 = CheckVal(ma.B3); m.M24 = 0;
            m.M31 = CheckVal(ma.C1); m.M32 = CheckVal(ma.C2); m.M33 = CheckVal(ma.C3); m.M34 = 0;
            m.M41 = 0; m.M42 = 0; m.M43 = 0; m.M44 = 1;
            return m;
        }

        public static Vector3 ToMg(this Assimp.Vector3D v)
        {
            return new Vector3(v.X, v.Y, v.Z);
        }

        public static Vector4 ToMg(this Assimp.Color4D v)
        {
            return new Vector4(v.R, v.G, v.B, v.A);
        }


        public static Microsoft.Xna.Framework.Quaternion ToMg(this Assimp.Quaternion aq)
        {
            //return new Microsoft.Xna.Framework.Quaternion(aq.X, aq.Y, aq.Z, aq.W);
            var m = aq.GetMatrix();
            var n = m.ToMgTransposed();
            var q = Microsoft.Xna.Framework.Quaternion.CreateFromRotationMatrix(n);
            return q;
        }


        public static string ToStringTrimed(this Assimp.Vector3D v)
        {
            string d = "+0.000;-0.000"; // "0.00";
            int pamt = 8;
            return (v.X.ToString(d).PadRight(pamt) + ", " + v.Y.ToString(d).PadRight(pamt) + ", " + v.Z.ToString(d).PadRight(pamt));
        }
        public static string ToStringTrimed(this Assimp.Quaternion q)
        {
            string d = "+0.000;-0.000"; // "0.00";
            int pamt = 8;
            return (q.X.ToString(d).PadRight(pamt) + ", " + q.Y.ToString(d).PadRight(pamt) + ", " + q.Z.ToString(d).PadRight(pamt) + "w " + q.W.ToString(d).PadRight(pamt));
        }

        // ______________________

        /// <summary>
        /// just use the assimp version to get the info;
        /// </summary>
        public static string SrtInfoToString(this Matrix mat, string tabspaces, bool showQuaternions)
        {
            Assimp.Matrix4x4 m = mat.ToAssimpTransposed();
            return QsrtInfoToString(m, tabspaces, showQuaternions);
        }
        /// <summary>
        /// just use the assimp version to get the info;
        /// </summary>
        public static string SrtInfoToString(this Matrix mat, string tabspaces)
        {
            Assimp.Matrix4x4 m = mat.ToAssimpTransposed();
            return QsrtInfoToString(m, tabspaces, true);
        }
        public static string SrtInfoToString(this Assimp.Matrix4x4 m, string tabspaces)
        {
            return QsrtInfoToString(m, tabspaces, true);
        }

        private static string QsrtInfoToString(this Assimp.Matrix4x4 m, string tabspaces, bool showQuaternions)
        {
            var checkdeterminatevalid = Math.Abs(m.Determinant()) < 1e-5;
            string str = "";
            // this can fail if the determinante is invalid.
            if (checkdeterminatevalid == false)
            {
                Vector3D scale;
                Assimp.Quaternion rot;
                Vector3D rotAngles;
                Vector3D trans;
                m.Decompose(out scale, out rot, out trans);
                QuatToEulerXyz(ref rot, out rotAngles);
                var rotDeg = rotAngles * (float)(180d / Math.PI);
                int padamt = 20;
                if (showQuaternions)
                    str += "\n" + tabspaces + "    " + "As Quaternion     ".PadRight(padamt) + rot.ToStringTrimed();
                str += "\n" + tabspaces + "    " + "Translation          ".PadRight(padamt) + trans.ToStringTrimed();
                if (scale.X != scale.Y || scale.Y != scale.Z || scale.Z != scale.X)
                    str += "\n" + tabspaces + "    " + "Scale                  ".PadRight(padamt) + scale.ToStringTrimed();
                else
                    str += "\n" + tabspaces + "    " + "Scale                  ".PadRight(padamt) + scale.X.ToStringTrimed();
                str += "\n" + tabspaces + "    " + "Rotation degrees  ".PadRight(padamt) + rotDeg.ToStringTrimed();// + "   radians: " + rotAngles.ToStringTrimed();
                str += "\n";
            }
            return str;
        }
        public static string GetSrtFromMatrix(Assimp.Matrix4x4 m, string tabspaces)
        {
            var checkdeterminatevalid = Math.Abs(m.Determinant()) < 1e-5;
            string str = "";
            int pamt = 12;
            // this can fail if the determinante is invalid.
            if (checkdeterminatevalid == false)
            {
                Vector3D scale;
                Assimp.Quaternion rot;
                Vector3D rotAngles;
                Vector3D trans;
                m.Decompose(out scale, out rot, out trans);
                QuatToEulerXyz(ref rot, out rotAngles);
                var rotDeg = rotAngles * (float)(180d / Math.PI);
                str += "\n" + tabspaces + " Rot (deg)".PadRight(pamt) + ":" + rotDeg.ToStringTrimed();// + "   radians: " + rotAngles.ToStringTrimed();
                if (scale.X != scale.Y || scale.Y != scale.Z || scale.Z != scale.X)
                    str += "\n" + tabspaces + " Scale ".PadRight(pamt) + ":" + scale.ToStringTrimed();
                else
                    str += "\n" + tabspaces + " Scale".PadRight(pamt) + ":" + scale.X.ToStringTrimed();
                str += "\n" + tabspaces + " Position".PadRight(pamt) + ":" + trans.ToStringTrimed();
                str += "\n";
            }
            return str;
        }
        /// <summary>
        /// returns true if decomposed failed.
        /// </summary>
        public static bool GetSrtFromMatrix(Matrix mat, string tabspaces, out Vector3 scale, out Vector3 trans, out Vector3 degRot)
        {
            var m = mat.ToAssimpTransposed();
            var checkdeterminatevalid = Math.Abs(m.Determinant()) < 1e-5;
            string str = "";
            int pamt = 12;
            // this can fail if the determinante is invalid.
            if (checkdeterminatevalid == false)
            {
                Vector3D _scale = new Vector3D();
                Assimp.Quaternion _rot = new Assimp.Quaternion();
                Vector3D _rotAngles = new Vector3D();
                Vector3D _trans = new Vector3D();
                m.Decompose(out _scale, out _rot, out _trans);
                QuatToEulerXyz(ref _rot, out _rotAngles);
                var rotDeg = _rotAngles * (float)(180d / Math.PI);
                scale = _scale.ToMg();
                degRot = rotDeg.ToMg();
                trans = _trans.ToMg();
            }
            else
            {
                Vector3D _scale = new Vector3D();
                Assimp.Quaternion _rot = new Assimp.Quaternion();
                Vector3D _rotAngles = new Vector3D();
                Vector3D _trans = new Vector3D();
                var rotDeg = _rotAngles * (float)(180d / Math.PI);
                scale = _scale.ToMg();
                degRot = _rotAngles.ToMg();
                trans = _trans.ToMg();
            }
            return checkdeterminatevalid;
        }
        // quat4 -> (roll, pitch, yaw)
        private static void QuatToEulerXyz(ref Assimp.Quaternion q1, out Vector3D outVector)
        {
            // http://www.euclideanspace.com/maths/geometry/rotations/conversions/quaternionToEuler/
            double sqw = q1.W * q1.W;
            double sqx = q1.X * q1.X;
            double sqy = q1.Y * q1.Y;
            double sqz = q1.Z * q1.Z;
            double unit = sqx + sqy + sqz + sqw; // if normalised is one, otherwise is correction factor
            double test = q1.X * q1.Y + q1.Z * q1.W;
            if (test > 0.499 * unit)
            { // singularity at north pole
                outVector.Z = (float)(2 * Math.Atan2(q1.X, q1.W));
                outVector.Y = (float)(Math.PI / 2);
                outVector.X = 0;
                return;
            }
            if (test < -0.499 * unit)
            { // singularity at south pole
                outVector.Z = (float)(-2 * Math.Atan2(q1.X, q1.W));
                outVector.Y = (float)(-Math.PI / 2);
                outVector.X = 0;
                return;
            }
            outVector.Z = (float)Math.Atan2(2 * q1.Y * q1.W - 2 * q1.X * q1.Z, sqx - sqy - sqz + sqw);
            outVector.Y = (float)Math.Asin(2 * test / unit);
            outVector.X = (float)Math.Atan2(2 * q1.X * q1.W - 2 * q1.Y * q1.Z, -sqx + sqy - sqz + sqw);
        }
        public static Assimp.Matrix4x4 ToAssimpTransposed(this Matrix m)
        {
            Assimp.Matrix4x4 ma = Matrix4x4.Identity;
            ma.A1 = m.M11; ma.A2 = m.M12; ma.A3 = m.M13; ma.A4 = m.M14;
            ma.B1 = m.M21; ma.B2 = m.M22; ma.B3 = m.M23; ma.B4 = m.M24;
            ma.C1 = m.M31; ma.C2 = m.M32; ma.C3 = m.M33; ma.C4 = m.M34;
            ma.D1 = m.M41; ma.D2 = m.M42; ma.D3 = m.M43; ma.D4 = m.M44;
            ma.Transpose();
            return ma;
        }

    }
}

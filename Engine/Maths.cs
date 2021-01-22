using System;

namespace Engine
{
    public struct Vector2
    {
        public float X { get; set; }
        public float Y { get; set; }
    }

    public struct Color
    {
        public static readonly Color Black = new Color(0, 0, 0, 1);
        public static readonly Color White = new Color(1, 1, 1, 1);

        public Color(Vector v, float w)
        {
            R = v.X;
            G = v.Y;
            B = v.Z;
            A = w;
        }
        public Color(float r, float g, float b, float a)
        {
            R = r;
            G = g;
            B = b;
            A = a;
        }
        public Color(float r, float g, float b) : this(r, g, b, 1)
        {

        }

        public float R { get; set; }
        public float G { get; set; }
        public float B { get; set; }
        public float A { get; set; }

        public static Color operator -(Color p)
        {
            return new Color(-p.R, -p.G, -p.B, -p.A);
        }
        public static Color operator +(Color p, Color q)
        {
            return new Color(p.R + q.R, p.G + q.G, p.B + q.B, p.A + q.A);
        }
        public static Color operator -(Color p, Color q)
        {
            return new Color(p.R - q.R, p.G - q.G, p.B - q.B, p.A - q.A);
        }
        public static Color operator *(Color p, Color q)
        {
            return new Color(p.R * q.R, p.G * q.G, p.B * q.B, p.A * q.A);
        }

        public static Color operator *(float p, Color q)
        {
            return new Color(p * q.R, p * q.G, p * q.B, p * q.A);
        }
        public static Color operator /(float p, Color q)
        {
            return new Color(p / q.R, p / q.G, p / q.B, p / q.A);
        }

        public static Color operator *(Color p, float q)
        {
            return new Color(p.R * q, p.G * q, p.B * q, p.A * q);
        }
        public static Color operator /(Color p, float q)
        {
            return new Color(p.R / q, p.G / q, p.B / q, p.A / q);
        }

        public override string ToString()
        {
            return $"{R}, {G}, {B}, {A}";
        }
    }

    public struct Vector
    {
        public static readonly Vector Zero = new Vector(0, 0, 0);

        public Vector(float x, float y, float z)
        {
            X = x;
            Y = y;
            Z = z;
        }
        public Vector(float thera, float phi)
        {
            X = (float)(Math.Sin(thera) * Math.Cos(phi));
            Y = (float)(Math.Sin(thera) * Math.Sin(phi));
            Z = (float)Math.Cos(thera);
        }

        public float X { get; set; }
        public float Y { get; set; }
        public float Z { get; set; }

        public float Length
        {
            get => (float)Math.Sqrt(this | this);
        }

        public Vector Normalize()
        {
            return this / Length;
        }

        public static float operator |(Vector p, Vector q)
        {
            return p.X * q.X + p.Y * q.Y + p.Z * q.Z;
        }
        public static Vector operator ^(Vector p, Vector q)
        {
            return new Vector(p.Y * q.Z - p.Z * q.Y, p.Z * q.X - p.X * q.Z, p.X * q.Y - p.Y * q.X);
        }

        public static Vector operator -(Vector p)
        {
            return new Vector(-p.X, -p.Y, -p.Z);
        }
        public static Vector operator +(Vector p, Vector q)
        {
            return new Vector(p.X + q.X, p.Y + q.Y, p.Z + q.Z);
        }
        public static Vector operator -(Vector p, Vector q)
        {
            return new Vector(p.X - q.X, p.Y - q.Y, p.Z - q.Z);
        }
        public static Vector operator *(Vector p, Vector q)
        {
            return new Vector(p.X * q.X, p.Y * q.Y, p.Z * q.Z);
        }

        public static Vector operator *(float p, Vector q)
        {
            return new Vector(p * q.X, p * q.Y, p * q.Z);
        }
        public static Vector operator /(float p, Vector q)
        {
            return new Vector(p / q.X, p / q.Y, p / q.Z);
        }

        public static Vector operator *(Vector p, float q)
        {
            return new Vector(p.X * q, p.Y * q, p.Z * q);
        }
        public static Vector operator /(Vector p, float q)
        {
            return new Vector(p.X / q, p.Y / q, p.Z / q);
        }

        public override string ToString()
        {
            return $"{X}, {Y}, {Z}";
        }
    }

    public struct Matrix
    {
        public static readonly Matrix One = new Matrix(1, 0, 0, 0, 0, 1, 0, 0, 0, 0, 1, 0, 0, 0, 0, 1);
        public static readonly Matrix Zero = new Matrix();

        private readonly float m00, m10, m20, m30;
        private readonly float m01, m11, m21, m31;
        private readonly float m02, m12, m22, m32;
        private readonly float m03, m13, m23, m33;

        private Matrix(float m00, float m01, float m02, float m03, float m10, float m11, float m12, float m13, float m20, float m21, float m22, float m23, float m30, float m31, float m32, float m33)
        {
            this.m00 = m00;
            this.m01 = m01;
            this.m02 = m02;
            this.m03 = m03;
            this.m10 = m10;
            this.m11 = m11;
            this.m12 = m12;
            this.m13 = m13;
            this.m20 = m20;
            this.m21 = m21;
            this.m22 = m22;
            this.m23 = m23;
            this.m30 = m30;
            this.m31 = m31;
            this.m32 = m32;
            this.m33 = m33;
        }

        public Vector Position
        {
            get => new Vector(m03, m13, m23);
        }

        public Vector X
        {
            get => new Vector(m00, m10, m20);
        }
        public Vector Y
        {
            get => new Vector(m01, m11, m21);
        }
        public Vector Z
        {
            get => new Vector(m02, m12, m22);
        }

        public static Matrix CreateLook(Vector pos, Vector target, Vector up)
        {
            return CreateView(pos, (target - pos).Normalize(), up);
        }
        public static Matrix CreateView(Vector pos, Vector direction, Vector up)
        {
            var z = direction;
            var x = z ^ up;
            var y = x ^ z;

            return new Matrix
            (
                x.X,  x.Y,  x.Z,  -(x | pos),
                y.X,  y.Y,  y.Z,  -(y | pos),
                z.X,  z.Y,  z.Z,  -(z | pos),
                  0,    0,    0,           1
            );
        }

        public static Matrix CreateProjection(float fov, float aspectInv, float near, float far)
        {
            fov *= 0.5f;

            var x = 1 / (float)Math.Tan(fov);
            var y = x * aspectInv;
            var z = (far + near) / (far - near);
            var w = 2 * near * far / (near - far);

            return new Matrix
            (
                y, 0, 0, 0,
                0, x, 0, 0,
                0, 0, z, w,
                0, 0, 1, 0
            );
        }

        public static Matrix CreateRotation(Vector velocity)
        {
            var angle = velocity.Length;

            if (angle == 0) return Matrix.One;

            return CreateRotation(velocity / angle, angle);
        }
        public static Matrix CreateRotation(Vector axis, float angle)
        {
            var x = axis.X;
            var y = axis.Y;
            var z = axis.Z;
            var sin = (float)Math.Sin(angle);
            var cos = (float)Math.Cos(angle);
            var x2 = x * x;
            var y2 = y * y;
            var z2 = z * z;
            var xy = x * y;
            var xz = x * z;
            var yz = y * z;

            return new Matrix
            (
                x2 + (cos * (1 - x2)),        xy - (cos * xy) + (sin * z),  xz - (cos * xz) - (sin * y),  0,
                xy - (cos * xy) - (sin * z),        y2 + (cos * (1 - y2)),  yz - (cos * yz) + (sin * x),  0,
                xz - (cos * xz) + (sin * y),  yz - (cos * yz) - (sin * x),        z2 + (cos * (1 - z2)),  0,
                                          0,                            0,                            0,  1
            );
        }

        public static Matrix CreateRotationX(float radians)
        {
            var c = (float)Math.Cos(radians);
            var s = (float)Math.Sin(radians);

            return new Matrix
            (
                1,  0,  0,  0,
                0,  c,  s,  0,
                0, -s,  c,  0,
                0,  0,  0,  1
            );
        }
        public static Matrix CreateRotationY(float radians)
        {
            var c = (float)Math.Cos(radians);
            var s = (float)Math.Sin(radians);

            return new Matrix
            (
                 c,  0,  s,  0,
                 0,  1,  0,  0,
                -s,  0,  s,  0,
                 0,  0,  0,  1
            );
        }
        public static Matrix CreateRotationZ(float radians)
        {
            var c = (float)Math.Cos(radians);
            var s = (float)Math.Sin(radians);

            return new Matrix
            (
                 c,  s,  0,  0,
                -s,  c,  0,  0,
                 0,  0,  1,  0,
                 0,  0,  0,  1
            );
        }

        public static Matrix CreateScale(Vector s)
        {
            return new Matrix
            (
                s.X,   0,   0,   0,
                  0, s.Y,   0,   0,
                  0,   0, s.Z,   0,
                  0,   0,   0,   1
            );
        }
        public static Matrix CreateScale(float x, float y, float z)
        {
            return new Matrix
            (
                x, 0, 0, 0,
                0, y, 0, 0,
                0, 0, z, 0,
                0, 0, 0, 1
            );
        }

        public static Matrix CreateTranslation(float x, float y, float z)
        {
            return new Matrix
            (
                1, 0, 0, x,
                0, 1, 0, y,
                0, 0, 1, z,
                0, 0, 0, 1
            );
        }
        public static Matrix CreateTranslation(Vector t)
        {
            return new Matrix
            (
                1,  0,  0,  t.X,
                0,  1,  0,  t.Y,
                0,  0,  1,  t.Z,
                0,  0,  0,    1
            );
        }

        public static Matrix operator ~(Matrix p)
        {
            return new Matrix
            (
                p.m00, p.m10, p.m20, p.m30,
                p.m01, p.m11, p.m21, p.m31,
                p.m02, p.m12, p.m22, p.m32,
                p.m03, p.m13, p.m23, p.m33
            );
        }
        public static Matrix operator -(Matrix p)
        {
            return new Matrix
            (
                -p.m00, -p.m01, -p.m02, -p.m03,
                -p.m10, -p.m11, -p.m12, -p.m13,
                -p.m20, -p.m21, -p.m22, -p.m23,
                -p.m30, -p.m31, -p.m32, -p.m33
            );
        }

        public static Matrix operator +(Matrix p, Matrix q)
        {
            return new Matrix
            (
                p.m00 + q.m00, p.m01 + q.m01, p.m02 + q.m02, p.m03 + q.m03,
                p.m10 + q.m10, p.m11 + q.m11, p.m12 + q.m12, p.m13 + q.m13,
                p.m20 + q.m20, p.m21 + q.m21, p.m22 + q.m22, p.m23 + q.m23,
                p.m30 + q.m30, p.m31 + q.m31, p.m32 + q.m32, p.m33 + q.m33
            );
        }
        public static Matrix operator -(Matrix p, Matrix q)
        {
            return new Matrix
            (
                p.m00 - q.m00, p.m01 - q.m01, p.m02 - q.m02, p.m03 - q.m03,
                p.m10 - q.m10, p.m11 - q.m11, p.m12 - q.m12, p.m13 - q.m13,
                p.m20 - q.m20, p.m21 - q.m21, p.m22 - q.m22, p.m23 - q.m23,
                p.m30 - q.m30, p.m31 - q.m31, p.m32 - q.m32, p.m33 - q.m33
            );
        }
        public static Matrix operator *(Matrix p, Matrix q)
        {
            return new Matrix
            (
                p.m00 * q.m00 + p.m01 * q.m10 + p.m02 * q.m20 + p.m03 * q.m30,
                p.m00 * q.m01 + p.m01 * q.m11 + p.m02 * q.m21 + p.m03 * q.m31,
                p.m00 * q.m02 + p.m01 * q.m12 + p.m02 * q.m22 + p.m03 * q.m32,
                p.m00 * q.m03 + p.m01 * q.m13 + p.m02 * q.m23 + p.m03 * q.m33,

                p.m10 * q.m00 + p.m11 * q.m10 + p.m12 * q.m20 + p.m13 * q.m30,
                p.m10 * q.m01 + p.m11 * q.m11 + p.m12 * q.m21 + p.m13 * q.m31,
                p.m10 * q.m02 + p.m11 * q.m12 + p.m12 * q.m22 + p.m13 * q.m32,
                p.m10 * q.m03 + p.m11 * q.m13 + p.m12 * q.m23 + p.m13 * q.m33,

                p.m20 * q.m00 + p.m21 * q.m10 + p.m22 * q.m20 + p.m23 * q.m30,
                p.m20 * q.m01 + p.m21 * q.m11 + p.m22 * q.m21 + p.m23 * q.m31,
                p.m20 * q.m02 + p.m21 * q.m12 + p.m22 * q.m22 + p.m23 * q.m32,
                p.m20 * q.m03 + p.m21 * q.m13 + p.m22 * q.m23 + p.m23 * q.m33,

                p.m30 * q.m00 + p.m31 * q.m10 + p.m32 * q.m20 + p.m33 * q.m30,
                p.m30 * q.m01 + p.m31 * q.m11 + p.m32 * q.m21 + p.m33 * q.m31,
                p.m30 * q.m02 + p.m31 * q.m12 + p.m32 * q.m22 + p.m33 * q.m32,
                p.m30 * q.m03 + p.m31 * q.m13 + p.m32 * q.m23 + p.m33 * q.m33
            );
        }

        public static Matrix operator *(float p, Matrix q)
        {
            return new Matrix
            (
                p * q.m00, p * q.m01, p * q.m02, p * q.m03,
                p * q.m10, p * q.m11, p * q.m12, p * q.m13,
                p * q.m20, p * q.m21, p * q.m22, p * q.m23,
                p * q.m30, p * q.m31, p * q.m32, p * q.m33
            );
        }

        public static Matrix operator *(Matrix p, float q)
        {
            return new Matrix
            (
                p.m00 * q, p.m01 * q, p.m02 * q, p.m03 * q,
                p.m10 * q, p.m11 * q, p.m12 * q, p.m13 * q,
                p.m20 * q, p.m21 * q, p.m22 * q, p.m23 * q,
                p.m30 * q, p.m31 * q, p.m32 * q, p.m33 * q
            );
        }
        public static Matrix operator /(Matrix p, float q)
        {
            return new Matrix
            (
                p.m00 / q, p.m01 / q, p.m02 / q, p.m03 / q,
                p.m10 / q, p.m11 / q, p.m12 / q, p.m13 / q,
                p.m20 / q, p.m21 / q, p.m22 / q, p.m23 / q,
                p.m30 / q, p.m31 / q, p.m32 / q, p.m33 / q
            );
        }

        public static Vector operator *(Matrix p, Vector q)
        {
            var x = p.m00 * q.X + p.m01 * q.Y + p.m02 * q.Z + p.m03;
            var y = p.m10 * q.X + p.m11 * q.Y + p.m12 * q.Z + p.m13;
            var z = p.m20 * q.X + p.m21 * q.Y + p.m22 * q.Z + p.m23;

            return new Vector(x, y, z);
        }
        public static Vector operator &(Matrix p, Vector q)
        {
            var x = p.m00 * q.X + p.m01 * q.Y + p.m02 * q.Z;
            var y = p.m10 * q.X + p.m11 * q.Y + p.m12 * q.Z;
            var z = p.m20 * q.X + p.m21 * q.Y + p.m22 * q.Z;

            return new Vector(x, y, z);
        }
        public static Color operator *(Matrix p, Color q)
        {
            var r = p.m00 * q.R + p.m01 * q.G + p.m02 * q.B + p.m03 * q.A;
            var g = p.m10 * q.R + p.m11 * q.G + p.m12 * q.B + p.m13 * q.A;
            var b = p.m20 * q.R + p.m21 * q.G + p.m22 * q.B + p.m23 * q.A;
            var a = p.m30 * q.R + p.m31 * q.G + p.m32 * q.B + p.m33 * q.A;
                                                          
            return new Color(r, g, b, a);
        }

        public override string ToString()
        {
            return $"{m00}, {m01}, {m02}, {m03} | {m10}, {m11}, {m12}, {m13} | {m20}, {m21}, {m22}, {m23} | {m30}, {m31}, {m32}, {m33}";
        }
    }
}

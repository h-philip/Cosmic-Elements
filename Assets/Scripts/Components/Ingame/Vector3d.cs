using System.Collections;
using System.Collections.Generic;

namespace Components
{
    public class Vector3d
    {
        public double x, y, z;


        public static Vector3d Zero => new Vector3d(0, 0, 0);
        public static Vector3d One => new Vector3d(1, 1, 1);
        public double Magnitude => System.Math.Sqrt(x * x + y * y + z * z);
        public Vector3d Normalized => this.Magnitude != 0 ? this / this.Magnitude : this;

        public Vector3d() : this(0, 0, 0)
        { }

        public Vector3d(double x, double y, double z)
        {
            this.x = x;
            this.y = y;
            this.z = z;
        }

        public static Vector3d operator *(Vector3d a, double b)
        {
            return new Vector3d(a.x * b, a.y * b, a.z * b);
        }

        public static Vector3d operator +(Vector3d a, Vector3d b)
        {
            return new Vector3d(a.x + b.x, a.y + b.y, a.z + b.z);
        }

        public static Vector3d operator -(Vector3d a, Vector3d b)
        {
            return new Vector3d(a.x - b.x, a.y - b.y, a.z - b.z);
        }

        public static Vector3d operator /(Vector3d a, double b)
        {
            return new Vector3d(a.x / b, a.y / b, a.z / b);
        }

        public override string ToString()
        {
            return "(" + x + "; " + y + "; " + z + ")";
        }

        public string ToString(string format)
        {
            return "(" + x.ToString(format) + "; " + y.ToString(format) + "; " + z.ToString(format) + ")";
        }
    }
}
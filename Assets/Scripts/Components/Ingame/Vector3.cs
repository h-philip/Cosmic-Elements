using System.Collections;
using System.Collections.Generic;

namespace Components
{
    public class Vector3
    {
        public double x, y, z;
        //public static Vector3 forward => new Vector3()

        public double magnitude => System.Math.Sqrt(x * x + y * y + z * z);
        public Vector3 normalized => this.magnitude != 0 ? this / this.magnitude : this;

        public Vector3() : this(0, 0, 0)
        { }

        public Vector3(double x, double y, double z)
        {
            this.x = x;
            this.y = y;
            this.z = z;
        }

        public static Vector3 operator *(Vector3 a, double b)
        {
            return new Vector3(a.x * b, a.y * b, a.z * b);
        }

        public static Vector3 operator +(Vector3 a, Vector3 b)
        {
            return new Vector3(a.x + b.x, a.y + b.y, a.z + b.z);
        }

        public static Vector3 operator -(Vector3 a, Vector3 b)
        {
            return new Vector3(a.x - b.x, a.y - b.y, a.z - b.z);
        }

        public static Vector3 operator /(Vector3 a, double b)
        {
            return new Vector3(a.x / b, a.y / b, a.z / b);
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
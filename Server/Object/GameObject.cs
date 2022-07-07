using System;
using System.Collections.Generic;
using System.Text;

namespace Server.Object
{
    public struct Vector3
    {
        public float x, y, z;

        public Vector3(float x, float y, float z)
        {
            this.x = x;
            this.y = y;
            this.z = z;
        }

        public static float Distance(Vector3 a, Vector3 b)
        {
            float num = a.x - b.x;
            float num2 = a.y - b.y;
            float num3 = a.z - b.z;
            return (float)Math.Sqrt(num * num + num2 * num2 + num3 * num3);
        }

        public static Vector3 operator -(Vector3 a, Vector3 b)
        {
            return new Vector3(a.x - b.x, a.y - b.y, a.z - b.z);
        }

        public static Vector3 operator *(Vector3 a, float d)
        {
            return new Vector3(a.x * d, a.y * d, a.z * d);
        }


        public static Vector3 operator /(Vector3 a, float d)
        {
            return new Vector3(a.x / d, a.y / d, a.z / d);
        }

        public Vector3 Normalize()
        {
            float num = Magnitude(this);
            if (num > 1E-05f)
            {
                this /= num;
            }
            else
            {
                this = new Vector3(0f, 0f, 0f);
            }

            return this;
        }

        public static float Magnitude(Vector3 vector)
        {
            return (float)Math.Sqrt(vector.x * vector.x + vector.y * vector.y + vector.z * vector.z);
        }
    }

    public class GameObject
    {
        public Define.GameObjectType ObjectType { get; protected set; } = Define.GameObjectType.None;

        public GameRoom Room { get; set; }

        public Define.CreatureState State
        {
            get;
            set;
        }

        int _objectId;

        public int Id
        {
            get { return _objectId; }
            set { _objectId = value; }
        }

        public float PosX { get; set; }
        public float PosY { get; set; }
        public float PosZ { get; set; }
    }
}

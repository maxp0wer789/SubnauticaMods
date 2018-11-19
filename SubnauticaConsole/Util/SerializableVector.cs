using UnityEngine;

namespace pp.SubnauticaMods.dbg
{
    [System.Serializable]
    public class SerializableVector
    {
        public float X;
        public float Y;

        public SerializableVector() { }
        public SerializableVector(float _X, float _Y)
        {
            X = _X;
            Y = _Y;
        }

        public static implicit operator Vector2(SerializableVector _vector)
        {
            return new Vector2(_vector.X, _vector.Y);
        }

        public static implicit operator SerializableVector(Vector2 _vector)
        {
            return new SerializableVector(_vector.x, _vector.y);
        }
    }
}

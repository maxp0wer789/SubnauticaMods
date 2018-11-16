using UnityEngine;

namespace pp.subnauticamods.dbg.Drawer
{
    public static class PrimitiveDrawer
    {

        [TypeDrawer(typeof(Quaternion))]
        private static Quaternion QuaternionDraw(Quaternion _rotation, string _label)
        {
            try
            {
                var x = _rotation.eulerAngles.x.ToString() ?? "0";
                var y = _rotation.eulerAngles.y.ToString() ?? "0";
                var z = _rotation.eulerAngles.z.ToString() ?? "0";
                GUILayout.BeginHorizontal();
                GUILayout.Label(_label);
                GUILayout.FlexibleSpace();
                return Quaternion.Euler(
                    float.Parse(GUILayout.TextField(x, GUILayout.Width(35f))),
                    float.Parse(GUILayout.TextField(y, GUILayout.Width(35f))),
                    float.Parse(GUILayout.TextField(z, GUILayout.Width(35f))));
            }
            finally
            {
                GUILayout.EndHorizontal();
            }
        }

        [TypeDrawer(typeof(Vector3))]
        private static Vector3 Vector3Draw(Vector3 _vector3, string _label)
        {
            try
            {
                var x = _vector3.x.ToString() ?? "0";
                var y = _vector3.y.ToString() ?? "0";
                var z = _vector3.z.ToString() ?? "0";
                GUILayout.BeginHorizontal();
                GUILayout.Label(_label);
                GUILayout.FlexibleSpace();
                return new Vector3(
                    float.Parse(GUILayout.TextField(x, GUILayout.Width(35f))),
                    float.Parse(GUILayout.TextField(y, GUILayout.Width(35f))),
                    float.Parse(GUILayout.TextField(z, GUILayout.Width(35f))));
            }
            finally
            {
                GUILayout.EndHorizontal();
            }
        }

        private static string StringDraw(string _string, string _label)     => GUILayout.TextField(_string, GUILayout.MinWidth(75f));
        private static int IntDraw(int _int, string _label)                 => int.Parse(GUILayout.TextField($"{_int}", GUILayout.MinWidth(50f)));
        private static long LongDraw(long _long, string _label)             => long.Parse(GUILayout.TextField($"{_long}", GUILayout.MinWidth(50f)));
        private static float FloatDraw(float _float, string _label)         => float.Parse(GUILayout.TextField($"{_float}", GUILayout.MinWidth(75f)));
        private static double DoubleDraw(double _double, string _label)     => double.Parse(GUILayout.TextField($"{_double}", GUILayout.MinWidth(75f)));
        private static short ShortDraw(short _short, string _label)         => short.Parse(GUILayout.TextField($"{_short}", GUILayout.MinWidth(75f)));
        private static bool BoolDraw(bool _bool, string _label)             => bool.Parse(GUILayout.TextField($"{_bool}", GUILayout.MinWidth(75f)));
    }
}

using UnityEngine;

namespace pp.SubnauticaMods.dbg
{
    public class DrawerHelper
    {
        public static float DrawCheckFloatField(string _label, float _value)
        {
            float val;
            if(!string.IsNullOrEmpty(_label))
                GUILayout.Label(_label);
            if (DebugPanel.Get.PanelConfig.BrowserShowValueChangeButtons && GUILayout.RepeatButton("<"))
            {
                _value -= 0.01f;
            }
            if (!float.TryParse(GUILayout.TextField($"{_value}", GUILayout.Width(DebugPanel.DEFAULT_TXT_FIELD_WIDTH)), out val))
                return _value;
            if (DebugPanel.Get.PanelConfig.BrowserShowValueChangeButtons && GUILayout.RepeatButton(">"))
            {
                val += 0.01f;
            }
            return val;
        }
    }

    public class ObjectDrawer : ATypeDrawer<Object>
    {
        protected override Object Draw(string _label, Object _unityObject)
        {
            GUILayout.BeginHorizontal();
            GUILayout.Label(_label);
            GUILayout.FlexibleSpace();
            GUILayout.Label(!_unityObject ? "null" : _unityObject.name);
            GUILayout.EndHorizontal();
            return _unityObject;
        }
    }

    public class GameObjectDrawer : ATypeDrawer<GameObject>
    {
        protected override GameObject Draw(string _label, GameObject _gameObject)
        {
            GUILayout.BeginHorizontal();
            GUILayout.Label(_label);
            GUILayout.FlexibleSpace();
            GUILayout.Label(!_gameObject ? "null" : _gameObject.GetHierarchyPath());
            GUILayout.EndHorizontal();
            return _gameObject;
        }
    }

    public class ColorDrawer : ATypeDrawer<Color>
    {
        protected override Color Draw(string _label, Color _color)
        {
            try
            {
                GUILayout.BeginHorizontal();
                GUILayout.Label(_label);
                GUILayout.FlexibleSpace();
                return new Color(
                    DrawerHelper.DrawCheckFloatField("R", _color.r), 
                    DrawerHelper.DrawCheckFloatField("G", _color.g),
                    DrawerHelper.DrawCheckFloatField("B", _color.b), 
                    DrawerHelper.DrawCheckFloatField("A", _color.a));
            }
            finally
            {
                GUILayout.EndHorizontal();
            }
        }
    }

    public class QuaternionDrawer : ATypeDrawer<Quaternion>
    {
        protected override Quaternion Draw(string _label, Quaternion _rotation)
        {
            try
            {
                GUILayout.BeginHorizontal();
                GUILayout.Label(_label);
                GUILayout.FlexibleSpace();
                return Quaternion.Euler(
                    DrawerHelper.DrawCheckFloatField("X", _rotation.eulerAngles.x), 
                    DrawerHelper.DrawCheckFloatField("Y", _rotation.eulerAngles.y), 
                    DrawerHelper.DrawCheckFloatField("Z", _rotation.eulerAngles.z));
            }
            finally
            {
                GUILayout.EndHorizontal();
            }
        }
    }

    public class Vector2Drawer : ATypeDrawer<Vector2>
    {
        protected override Vector2 Draw(string _label, Vector2 _vector2)
        {
            try
            {
                GUILayout.BeginHorizontal();
                GUILayout.Label(_label);
                GUILayout.FlexibleSpace();
                return new Vector2(
                    DrawerHelper.DrawCheckFloatField("X", _vector2.x), 
                    DrawerHelper.DrawCheckFloatField("Y", _vector2.y));
            }
            finally
            {
                GUILayout.EndHorizontal();
            }
        }
    }

    public class Vector3Drawer : ATypeDrawer<Vector3>
    {
        protected override Vector3 Draw(string _label, Vector3 _vector3)
        {
            try
            {
                GUILayout.BeginHorizontal();
                GUILayout.Label(_label);
                GUILayout.FlexibleSpace();
                return new Vector3(
                    DrawerHelper.DrawCheckFloatField("X", _vector3.x), 
                    DrawerHelper.DrawCheckFloatField("Y", _vector3.y), 
                    DrawerHelper.DrawCheckFloatField("Z", _vector3.z));
            }
            finally
            {
                GUILayout.EndHorizontal();
            }
        }
    }

    public class StringDrawer : ATypeDrawer<string>
    {
        protected override string Draw(string _label, string _string)
        {
            try
            {
                GUILayout.BeginHorizontal();
                GUILayout.Label(_label);
                GUILayout.FlexibleSpace();
                return GUILayout.TextField(_string ?? "", GUILayout.MinWidth(75f));
            }
            finally
            {
                GUILayout.EndHorizontal();
            }
        }
    }

    public class IntDrawer : ATypeDrawer<int>
    {
        protected override int Draw(string _label, int _int)
        {
            try
            {
                GUILayout.BeginHorizontal();
                GUILayout.Label(_label);
                GUILayout.FlexibleSpace();
                int result;
                if (!int.TryParse(GUILayout.TextField($"{_int}", GUILayout.MinWidth(50f)), out result))
                    return 0;
                return result;
            }
            finally
            {
                GUILayout.EndHorizontal();
            }
        }
    }

    public class LongDrawer : ATypeDrawer<long>
    {
        protected override long Draw(string _label, long _long)
        {
            try
            {
                GUILayout.BeginHorizontal();
                GUILayout.Label(_label);
                GUILayout.FlexibleSpace();
                long result;
                if (!long.TryParse(GUILayout.TextField($"{_long}", GUILayout.MinWidth(50f)), out result))
                    return 0;
                return result;
            }
            finally
            {
                GUILayout.EndHorizontal();
            }
        }
    }

    public class FloatDrawer : ATypeDrawer<float>
    {
        protected override float Draw(string _label, float _float)
        {
            try
            {
                GUILayout.BeginHorizontal();
                GUILayout.Label(_label);
                GUILayout.FlexibleSpace();
                return DrawerHelper.DrawCheckFloatField("", _float);
            }
            finally
            {
                GUILayout.EndHorizontal();
            }
        }
    }

    public class DoubleDrawer : ATypeDrawer<double>
    {
        protected override double Draw(string _label, double _double)
        {
            try
            {
                GUILayout.BeginHorizontal();
                GUILayout.Label(_label);
                GUILayout.FlexibleSpace();
                double result;
                if (!double.TryParse(GUILayout.TextField($"{_double}", GUILayout.MinWidth(50f)), out result))
                    return 0;
                return result;
            }
            finally
            {
                GUILayout.EndHorizontal();
            }
        }
    }

    public class ShortDrawer : ATypeDrawer<short>
    {
        protected override short Draw(string _label, short _short)
        {
            try
            {
                GUILayout.BeginHorizontal();
                GUILayout.Label(_label);
                GUILayout.FlexibleSpace();
                short result;
                if (!short.TryParse(GUILayout.TextField($"{_short}", GUILayout.MinWidth(50f)), out result))
                    return 0;
                return result;
            }
            finally
            {
                GUILayout.EndHorizontal();
            }
        }
    }

    public class BoolDrawer : ATypeDrawer<bool>
    {
        protected override  bool Draw(string _label, bool _bool)
        {
            try
            {
                GUILayout.BeginHorizontal();
                GUILayout.Label(_label);
                GUILayout.FlexibleSpace();
                return GUILayout.Toggle(_bool, "", GUILayout.MinWidth(50f));
            }
            finally
            {
                GUILayout.EndHorizontal();
            }
        }
    }
}

namespace pp.subnauticamods.dbg
{
    [System.AttributeUsage(System.AttributeTargets.Method, AllowMultiple = true)]
    public class TypeDrawer : System.Attribute
    {
        public System.Type DrawObjectType;

        public TypeDrawer(System.Type _objectType)
        {
            DrawObjectType = _objectType;
        }
    }
}

namespace pp.SubnauticaMods.dbg
{
    public abstract class ATypeDrawer<T> : ITypeDrawer
    {
        public object Draw(string _label, object _object)
        {
            if (!(_object is T))
                return Draw(_label, default(T));
            return Draw(_label, (T) _object);
        }

        protected abstract T Draw(string _label, T _object);
    }

    public interface ITypeDrawer
    {
        object Draw(string _label, object _object);
    }
}

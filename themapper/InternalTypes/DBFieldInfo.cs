using themapper.Attributes;
using System.Reflection;

namespace themapper.InternalTypes
{
    internal class DBFieldInfo
    {
        public PropertyInfo PropertyInfo { get; set; }

        public DBField FieldInfo { get; set; }
    }
}

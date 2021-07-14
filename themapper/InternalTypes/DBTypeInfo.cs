using themapper.Attributes;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;

namespace themapper.InternalTypes
{
    internal class DBTypeInfo
    {
        Dictionary<string, DBFieldInfo> _map;

        internal ConstructorInfo ConstructorInfo { get; private set; }

        internal DBTypeInfo(ConstructorInfo ci)
        {
            _map = new Dictionary<string, DBFieldInfo>();
            ConstructorInfo = ci;
        }

        internal void AddFieldInfo(PropertyInfo pi, DBField attr)
        {
            string key = string.IsNullOrEmpty(attr.FieldName) ? pi.Name : attr.FieldName;
            _map.Add(key, new DBFieldInfo { PropertyInfo = pi, FieldInfo = attr });
        }

        internal T CreateInstance<T>() where T : class
        {
            return ConstructorInfo.Invoke(null) as T;
        }

        internal void SetProperties<T>(T entity, IDataReader reader) where T : class
        {
            foreach (KeyValuePair<string, DBFieldInfo> pair in _map)
                pair.Value.PropertyInfo.SetValue(entity, reader[pair.Key]);
        }

        internal DataTable GetTable()
        {
            DataTable tbl = new DataTable();

            var ordered = from p in _map
                          orderby p.Value.FieldInfo.Order
                          select p;

            foreach (var p in ordered)
                tbl.Columns.Add(p.Key, p.Value.PropertyInfo.PropertyType);

            return tbl;
        }

        internal void AddRow<T>(T entity, DataTable tbl)
        {
            DataRow row = tbl.NewRow();

            foreach (var pair in _map)
                row[pair.Key] = pair.Value.PropertyInfo.GetValue(entity);

            tbl.Rows.Add(row);
        }
    }
}

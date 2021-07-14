using themapper.Attributes;
using themapper.Exceptions;
using themapper.InternalTypes;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Reflection;

namespace themapper
{
    public class DBMapper
    {
        #region Fields

        static readonly object _syncRoot = new object();
        static DBMapper _instance = null;

        Dictionary<Type, DBTypeInfo> _map;

        #endregion

        #region Properties

        public static DBMapper Instance
        {
            get
            {
                if (_instance == null)
                    lock (_syncRoot)
                        if (_instance == null)
                            _instance = new DBMapper();

                return _instance;
            }
        }

        public int TypesCount => _map.Count;

        #endregion

        #region Ctor

        protected DBMapper()
        {
            _map = new Dictionary<Type, DBTypeInfo>();

            Initialize();
        }

        #endregion

        #region Private methods

        private List<Assembly> GetAssembliesRecursively(Assembly asm)
        {
            List<Assembly> assemblies = new List<Assembly>();

            foreach (AssemblyName an in asm.GetReferencedAssemblies())
            {
                Assembly a = AppDomain.CurrentDomain.Load(an);
                assemblies.Add(a);

                assemblies.AddRange(GetAssembliesRecursively(a));
            }

            return assemblies;
        }

        private void TraverseAssembly(Assembly asm)
        {
            foreach (Type type in asm.GetTypes())
            {
                DBEntity attr = type.GetCustomAttribute<DBEntity>();

                if (attr != null)
                    _map.TryAdd(type, TraverseType(type));
            }
        }

        private DBTypeInfo TraverseType(Type type)
        {
            ConstructorInfo ci = type.GetConstructor(Type.EmptyTypes);

            if (ci == null)
                throw new ConstructorNotFoundException(type);

            DBTypeInfo typeInfo = new DBTypeInfo(ci);

            PropertyInfo[] props = type.GetProperties(BindingFlags.Public | BindingFlags.DeclaredOnly | BindingFlags.Instance);

            foreach (PropertyInfo pi in props)
            {
                DBField attr = pi.GetCustomAttribute<DBField>();

                if (attr != null)
                    typeInfo.AddFieldInfo(pi, attr);
            }

            return typeInfo;
        }

        private T CreateEntity<T>(IDataReader reader) where T : class
        {
            DBTypeInfo typeInfo = _map[typeof(T)];

            T entity = typeInfo.CreateInstance<T>();
            typeInfo.SetProperties<T>(entity, reader);

            return entity;
        }

        private DataTable ToTableInternal<T>(IEnumerable<T> entities) where T : class
        {
            Type type = typeof(T);

            if (!_map.ContainsKey(type))
                throw new MappingNotFoundException(type);

            DBTypeInfo typeInfo = _map[type];
            DataTable tbl = typeInfo.GetTable();

            foreach (T e in entities)
                typeInfo.AddRow<T>(e, tbl);

            tbl.AcceptChanges();

            return tbl;
        }

        private void Initialize()
        {
            HashSet<Assembly> assemblies = new HashSet<Assembly>(GetAssembliesRecursively(Assembly.GetEntryAssembly()));

            foreach (Assembly asm in assemblies)
                TraverseAssembly(asm);
        }

        #endregion

        #region Public methods

        public T Map<T>(IDataReader reader) where T : class
        {
            Type type = typeof(T);

            if (!_map.ContainsKey(type))
                throw new MappingNotFoundException(type);

            return CreateEntity<T>(reader);
        }

        public List<T> Map2List<T>(IDataReader reader) where T : class
        {
            Type type = typeof(T);

            if (!_map.ContainsKey(type))
                throw new MappingNotFoundException(type);

            List<T> res = new List<T>();

            while (reader.Read())
                res.Add(CreateEntity<T>(reader));

            return res;
        }

        public T[] Map2Array<T>(IDataReader reader) where T : class
        {
            return Map2List<T>(reader).ToArray();
        }

        public ObservableCollection<T> Map2Observable<T>(IDataReader reader) where T : class
        {
            Type type = typeof(T);

            if (!_map.ContainsKey(type))
                throw new MappingNotFoundException(type);

            ObservableCollection<T> res = new ObservableCollection<T>();

            while (reader.Read())
                res.Add(CreateEntity<T>(reader));

            return res;
        }

        public DataTable ToTable<T>(List<T> entities) where T : class
        {
            return ToTableInternal<T>(entities);
        }

        public DataTable ToTable<T>(T[] entities) where T : class
        {
            return ToTableInternal<T>(entities);
        }

        public DataTable ToTable<T>(ObservableCollection<T> entities) where T : class
        {
            return ToTableInternal<T>(entities);
        }

        public void ReInitialize()
        {
            lock (_syncRoot)
            {
                _map.Clear();
                Initialize();
            }
        }

        #endregion
    }
}

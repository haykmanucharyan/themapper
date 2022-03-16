using themapper.Attributes;
using themapper.Exceptions;
using themapper.InternalTypes;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Reflection;
using System.Collections.Concurrent;

namespace themapper
{
    public class DBMapper : IDBMapper
    {
        #region Fields

        static readonly object _syncRoot = new object();
        static IDBMapper _instance = null;

        ConcurrentDictionary<Type, DBTypeInfo> _map;

        #endregion

        #region Properties

        public static IDBMapper Instance
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

        /// <summary>
        /// Gets quantity of mapping types.
        /// </summary>
        public int TypesCount => _map.Count;

        #endregion

        #region Ctor

        protected DBMapper()
        {
            _map = new ConcurrentDictionary<Type, DBTypeInfo>();
        }

        #endregion

        #region Private methods        

        private DBTypeInfo TraverseType(Type type)
        {
            DBEntity objectAttribute = type.GetCustomAttribute<DBEntity>();

            if (objectAttribute == null)
                throw new MissingDBEntityAttribute(type);

            ConstructorInfo ci = type.GetConstructor(Type.EmptyTypes);

            if (ci == null)
                throw new ConstructorNotFoundException(type);

            DBTypeInfo typeInfo = new DBTypeInfo(ci);

            PropertyInfo[] props = type.GetProperties(BindingFlags.Public | BindingFlags.DeclaredOnly | BindingFlags.Instance);

            foreach (PropertyInfo pi in props)
            {
                DBField propertyAttribute = pi.GetCustomAttribute<DBField>();

                if (propertyAttribute != null)
                    typeInfo.AddFieldInfo(pi, propertyAttribute);
            }

            return typeInfo;
        }

        private DBTypeInfo GetDBTypeInfo<T>() where T : class
        {
            Type type = typeof(T);

            return _map.GetOrAdd(type, _ =>
            {
                return TraverseType(type);
            });
        }

        private T CreateEntity<T>(DBTypeInfo dBTypeInfo, IDataReader reader) where T : class
        {
            T entity = dBTypeInfo.CreateInstance<T>();
            dBTypeInfo.SetProperties<T>(entity, reader);

            return entity;
        }

        private List<T> Map2ListInternal<T>(IDataReader reader, int knownSize) where T : class
        {
            DBTypeInfo dbTypeInfo = GetDBTypeInfo<T>();

            List<T> res = new List<T>(knownSize > 0 ? 4 : knownSize);

            while (reader.Read())
                res.Add(CreateEntity<T>(dbTypeInfo, reader));

            return res;
        }

        #endregion

        #region Public methods

        /// <summary>
        /// Maps single entity to IDataReader's current record.
        /// So make sure that IDataReader Read() method is called for at least once.
        /// </summary>
        /// <typeparam name="T">Type to be mapped to.</typeparam>
        /// <param name="reader">IDataReader object.</param>
        /// <returns>Mapped entity.</returns>
        public T Map<T>(IDataReader reader) where T : class
        {
            DBTypeInfo dbTypeInfo = GetDBTypeInfo<T>();

            return CreateEntity<T>(dbTypeInfo, reader);
        }

        /// <summary>
        /// Reads and maps all records of IDataReader object to List<T>.
        /// Resizing of List<T> will occur.
        /// The logic is while(reader.Read()) DoMapping();
        /// </summary>
        /// <typeparam name="T">Type to be mapped to.</typeparam>
        /// <param name="reader">IDataReader object.</param>
        /// <returns>List of mapped entities.</returns>
        public List<T> Map2List<T>(IDataReader reader) where T : class
        {
            return Map2ListInternal<T>(reader, 0);
        }

        /// <summary>
        /// Reads and maps all records of IDataReader object to List<T> by passing knownSize argument to List<T>'s Ctor (avoiding List<T> resize).
        /// The logic is while(reader.Read()) DoMapping();
        /// </summary>
        /// <typeparam name="T">Type to be mapped to.</typeparam>
        /// <param name="reader">IDataReader object.</param>
        /// <param name="knownSize">The size of result list to avoid List<T> resizing.</param>
        /// <returns>List of mapped entities.</returns>
        public List<T> Map2List<T>(IDataReader reader, int knownSize) where T : class
        {
            return Map2ListInternal<T>(reader, knownSize);
        }

        /// <summary>
        /// Reads and maps all records of IDataReader object to T[] (array).
        /// Because the size of array is not known, List<T> is used (resizing will occur). 
        /// The result is List<T>'s ToArray() method call.
        /// </summary>
        /// <typeparam name="T">Type to be mapped to.</typeparam>
        /// <param name="reader">IDataReader object.</param>
        /// <returns>Array of mapped entities.</returns>
        public T[] Map2Array<T>(IDataReader reader) where T : class
        {
            return Map2List<T>(reader).ToArray();
        }

        /// <summary>
        /// Reads and maps all records of IDataReader object to T[] (array).
        /// Because there is known size, the result array will be of that size.
        /// The loop will iterate, untill can read from IDataReader object or the array size is reached.
        /// The logic is while(reader.Read() && index < array.Length) DoMapping();
        /// </summary>
        /// <typeparam name="T">Type to be mapped to.</typeparam>
        /// <param name="reader">IDataReader object.</param>
        /// <param name="knownSize">The size of array.</param>
        /// <returns>Array of mapped entities.</returns>
        public T[] Map2Array<T>(IDataReader reader, int knownSize) where T : class
        {
            DBTypeInfo dbTypeInfo = GetDBTypeInfo<T>();
            T[] array = new T[knownSize];

            int index = 0;
            while (reader.Read() && index < array.Length)
                array[index++] = CreateEntity<T>(dbTypeInfo, reader);

            return array;
        }

        /// <summary>
        /// Reads and maps all records of IDataReader object to ObservableCollection<T>.
        /// Resizing of ObservableCollection<T> will occur.
        /// </summary>
        /// <typeparam name="T">Type to be mapped to.</typeparam>
        /// <param name="reader">IDataReader object.</param>
        /// <returns>ObservableCollection of mapped entities.</returns>
        public ObservableCollection<T> Map2Observable<T>(IDataReader reader) where T : class
        {
            DBTypeInfo dbTypeInfo = GetDBTypeInfo<T>();

            ObservableCollection<T> res = new ObservableCollection<T>();

            while (reader.Read())
                res.Add(CreateEntity<T>(dbTypeInfo, reader));

            return res;
        }

        /// <summary>
        /// Will map entities to DataTable object.
        /// AcceptChanges() method is called after mapping.
        /// </summary>
        /// <typeparam name="T">Type to be mapped from.</typeparam>
        /// <param name="entities">Enumerable entities.</param>
        /// <returns>DataTable filled with mapped entities.</returns>
        public DataTable ToTable<T>(IEnumerable<T> entities) where T : class
        {
            DBTypeInfo typeInfo = GetDBTypeInfo<T>();
            DataTable tbl = typeInfo.GetTable();

            foreach (T e in entities)
                typeInfo.AddRow<T>(e, tbl);

            tbl.AcceptChanges();

            return tbl;
        }

        #endregion
    }
}

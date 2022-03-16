using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;

namespace themapper
{
    public interface IDBMapper
    {
        /// <summary>
        /// Gets quantity of mapping types.
        /// </summary>
        int TypesCount { get; }

        /// <summary>
        /// Maps single entity to IDataReader's current record.
        /// So make sure that IDataReader Read() method is called for at least once.
        /// </summary>
        /// <typeparam name="T">Type to be mapped to.</typeparam>
        /// <param name="reader">IDataReader object.</param>
        /// <returns>Mapped entity.</returns>
        T Map<T>(IDataReader reader) where T : class;

        /// <summary>
        /// Reads and maps all records of IDataReader object to List<T>.
        /// Resizing of List<T> will occur.
        /// The logic is while(reader.Read()) DoMapping();
        /// </summary>
        /// <typeparam name="T">Type to be mapped to.</typeparam>
        /// <param name="reader">IDataReader object.</param>
        /// <returns>List of mapped entities.</returns>
        List<T> Map2List<T>(IDataReader reader) where T : class;

        /// <summary>
        /// Reads and maps all records of IDataReader object to List<T> by passing knownSize argument to List<T>'s Ctor (avoiding List<T> resize).
        /// The logic is while(reader.Read()) DoMapping();
        /// </summary>
        /// <typeparam name="T">Type to be mapped to.</typeparam>
        /// <param name="reader">IDataReader object.</param>
        /// <param name="knownSize">The size of result list to avoid List<T> resizing.</param>
        /// <returns>List of mapped entities.</returns>
        List<T> Map2List<T>(IDataReader reader, int knownSize) where T : class;

        /// <summary>
        /// Reads and maps all records of IDataReader object to T[] (array).
        /// Because the size of array is not known, List<T> is used (resizing will occur). 
        /// The result is List<T>'s ToArray() method call.
        /// </summary>
        /// <typeparam name="T">Type to be mapped to.</typeparam>
        /// <param name="reader">IDataReader object.</param>
        /// <returns>Array of mapped entities.</returns>
        T[] Map2Array<T>(IDataReader reader) where T : class;

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
        T[] Map2Array<T>(IDataReader reader, int knownSize) where T : class;

        /// <summary>
        /// Reads and maps all records of IDataReader object to ObservableCollection<T>.
        /// Resizing of ObservableCollection<T> will occur.
        /// </summary>
        /// <typeparam name="T">Type to be mapped to.</typeparam>
        /// <param name="reader">IDataReader object.</param>
        /// <returns>ObservableCollection of mapped entities.</returns>
        ObservableCollection<T> Map2Observable<T>(IDataReader reader) where T : class;

        /// <summary>
        /// Will map entities to DataTable object.
        /// AcceptChanges() method is called after mapping.
        /// </summary>
        /// <typeparam name="T">Type to be mapped from.</typeparam>
        /// <param name="entities">Enumerable entities.</param>
        /// <returns>DataTable filled with mapped entities.</returns>
        DataTable ToTable<T>(IEnumerable<T> entities) where T : class;
    }
}

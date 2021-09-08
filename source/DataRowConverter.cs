using System.Data;

namespace Shared.Common.Data
{
    /// <summary>
    /// Converts a DataRow to another class instance, primarily used to convert derived DataRow to Plain Old CRL Object
    /// </summary>
    /// <example>
    /// <![CDATA[
    /// adapter.GetData().Select(DataRowConverter<YourClass>.Cast).ToList()
    /// ]]>
    /// </example>
    /// <typeparam name="TResult"></typeparam>
    public static class DataRowConverter<TResult>
        where TResult : class
    {
        /// <summary>
        /// Converts the row to the given type
        /// </summary>
        /// <typeparam name="TRow"></typeparam>
        /// <param name="source"></param>
        /// <returns></returns>
        public static TResult Cast<TRow>(TRow source)
            where TRow : DataRow
        {
            return DataRowConvererImpl<TRow, TResult>.Cast(source);
        }
    }
}

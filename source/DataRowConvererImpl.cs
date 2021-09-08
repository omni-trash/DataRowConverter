using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;

namespace Shared.Common.Data
{
    /// <summary>
    /// Converts a DataRow to another class instance, cache some informations about TResult in static properties
    /// </summary>
    /// <typeparam name="TRow"></typeparam>
    /// <typeparam name="TResult"></typeparam>
    internal static class DataRowConvererImpl<TRow, TResult>
        where TRow : DataRow
        where TResult : class
    {
        /// <summary>
        /// Some flat informations from PropertyInfo
        /// </summary>
        private class PropInfo
        {
            public string Name { get; set; }
            public PropertyInfo PropertyInfo { get; set; }
            public Type DataType { get; set; }
            public bool SupportsNull { get; set; }
        }

        /// <summary>
        /// Writable properties
        /// </summary>
        static readonly List<PropInfo>
            targetProps = typeof(TResult).GetProperties(BindingFlags.Public | BindingFlags.Instance)
                            .Where(prop => prop.CanWrite)
                            .Select(prop => new PropInfo
                            {
                                Name = prop.Name,
                                PropertyInfo = prop,
                                DataType = Nullable.GetUnderlyingType(prop.PropertyType) ?? prop.PropertyType,
                                SupportsNull = (Nullable.GetUnderlyingType(prop.PropertyType) != null || !prop.PropertyType.IsValueType)
                            })
                            .ToList();

        /// <summary>
        /// Check we have a constructor(TRow) or constructor(DataRow)
        /// </summary>
        static readonly ConstructorInfo
            targetCtor = typeof(TResult).GetConstructor(new[] { typeof(TRow) }) ?? typeof(TResult).GetConstructor(new[] { typeof(DataRow) });

        /// <summary>
        /// Converts the row to the given type
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        public static TResult Cast(TRow source)
        {
            // if we have paramterized constructor let him do the work
            if (targetCtor != null)
            {
                return (TResult)targetCtor.Invoke(new[] { source });
            }

            // direct access to DataRow
            DataRow row = source as DataRow;

            // columns definition
            DataColumnCollection columns = row.Table.Columns;

            // parameterless constructor
            TResult instance = Activator.CreateInstance<TResult>();

            targetProps.ForEach(prop =>
            {
                // column definition
                DataColumn column = columns[prop.Name];

                // must exist with same name
                // must have the same type as defined in column definition
                // we dont check the real value like row[name].GetType()
                if (column != null && prop.DataType == column.DataType)
                {
                    object value = row[prop.Name] is DBNull ? null : row[prop.Name];

                    // we can write null if it is not a value type or it is nullable
                    if (value == null && !prop.SupportsNull)
                    {
                        // unable to assign
                        return;
                    }

                    // copy to result
                    prop.PropertyInfo.SetValue(instance, value);
                }
            });

            return instance;
        }
    }
}

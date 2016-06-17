using System;
using System.Data;

namespace EventCentric.Database
{
    /// <summary>
    /// Provides usability overloads for <see cref="SqlDataReader"/>.
    /// </summary>
    /// <remarks>
    /// Based on: http://stackoverflow.com/questions/1772025/sql-data-reader-handling-null-column-values
    /// </remarks>
    public static class DataReaderExtensions
    {
        #region Decimal

        public static decimal GetDecimal(this IDataReader reader, string name) => (decimal)reader[name];

        public static decimal SafeGetDecimal(this IDataReader reader, int i)
            => reader.IsDBNull(i) ? default(decimal)
                                  : reader.GetDecimal(i);

        public static decimal SafeGetDecimal(this IDataReader reader, string name)
            => reader.SafeGetDecimal(reader.GetOrdinal(name));

        public static decimal? SafeGetDecimalOrNull(this IDataReader reader, int i) => reader[reader.GetName(i)] as decimal?;

        public static decimal? SafeGetDecimalOrNull(this IDataReader reader, string name) => reader[name] as decimal?;

        #endregion

        #region String

        public static string GetString(this IDataReader reader, string name) => reader[name] as string;

        public static string SafeGetString(this IDataReader reader, int i)
            => reader.IsDBNull(i) ? string.Empty
                                   : reader.GetString(i);

        public static string SafeGetString(this IDataReader reader, string name)
            => reader.SafeGetString(reader.GetOrdinal(name));

        public static string SafeGetStringAndTrim(this IDataReader reader, int i)
            => reader.SafeGetString(i).Trim();

        public static string SafeGetStringAndTrim(this IDataReader reader, string name)
            => reader.SafeGetString(name).Trim();

        #endregion

        #region Int32

        public static int GetInt32(this IDataReader reader, string name)
            => (int)reader[name];

        public static int SafeGetInt32(this IDataReader reader, int i)
            => reader.IsDBNull(i) ? default(int)
                                  : reader.GetInt32(i);

        public static int SafeGetInt32(this IDataReader reader, string name)
            => reader.SafeGetInt32(reader.GetOrdinal(name));

        public static int? SafeGetInt32OrNull(this IDataReader reader, int i) => reader[reader.GetName(i)] as int?;

        public static int? SafeGetInt32OrNull(this IDataReader reader, string name) => reader[name] as int?;

        #endregion

        #region Int64

        public static long GetInt64(this IDataReader reader, string name) => (long)reader[name];

        public static long SafeGetInt64(this IDataReader reader, int i)
            => reader.IsDBNull(i) ? default(long)
                                  : reader.GetInt64(i);

        public static long SafeGetInt64(this IDataReader reader, string name)
            => reader.SafeGetInt64(reader.GetOrdinal(name));

        public static long? SafeGetInt64OrNull(this IDataReader reader, int i) => reader[reader.GetName(i)] as long?;

        public static long? SafeGetInt64OrNull(this IDataReader reader, string name) => reader[name] as long?;

        #endregion

        #region Float

        public static float GetFloat(this IDataReader reader, string name) => (float)reader[name];

        public static float SafeGetFloat(this IDataReader reader, int i)
            => reader.IsDBNull(i) ? default(float)
                                  : reader.GetFloat(i);

        public static float SafeGetFloat(this IDataReader reader, string name)
            => reader.SafeGetFloat(reader.GetOrdinal(name));

        public static float? SafeGetFloatOrNull(this IDataReader reader, int i) => reader[reader.GetName(i)] as float?;

        public static float? SafeGetFloatOrNull(this IDataReader reader, string name) => reader[name] as float?;

        #endregion

        #region Double

        public static double GetDouble(this IDataReader reader, string name) => (double)reader[name];

        public static double SafeGetDouble(this IDataReader reader, int i)
            => reader.IsDBNull(i) ? default(double)
                                  : reader.GetDouble(i);

        public static double SafeGetDouble(this IDataReader reader, string name)
            => reader.SafeGetDouble(reader.GetOrdinal(name));

        public static double? SafeGetDoubleOrNull(this IDataReader reader, int i) => reader[reader.GetName(i)] as double?;

        public static double? SafeGetDoubleOrNull(this IDataReader reader, string name) => reader[name] as double?;

        #endregion

        #region Guid
        public static Guid GetGuid(this IDataReader reader, string name) => (Guid)reader[name];

        public static Guid SafeGetGuid(this IDataReader reader, int i)
            => reader.IsDBNull(i) ? default(Guid)
                                  : reader.GetGuid(i);

        public static Guid SafeGetGuid(this IDataReader reader, string name)
            => reader.SafeGetGuid(reader.GetOrdinal(name));

        public static Guid? SafeGetGuidOrNull(this IDataReader reader, int i) => reader[reader.GetName(i)] as Guid?;

        public static Guid? SafeGetGuidOrNull(this IDataReader reader, string name) => reader[name] as Guid?;

        #endregion

        #region DateTime

        public static DateTime GetDateTime(this IDataReader reader, string name) => (DateTime)reader[name];

        public static DateTime SafeGetDateTime(this IDataReader reader, int i)
            => reader.IsDBNull(i) ? default(DateTime)
                                  : reader.GetDateTime(i);

        public static DateTime SafeGetDateTime(this IDataReader reader, string name)
            => reader.SafeGetDateTime(reader.GetOrdinal(name));

        public static DateTime? SafeGetDateTimeOrNull(this IDataReader reader, int i) => reader[reader.GetName(i)] as DateTime?;

        public static DateTime? SafeGetDateTimeOrNull(this IDataReader reader, string name) => reader[name] as DateTime?;

        #endregion

        #region Bool

        public static bool GetBoolean(this IDataReader reader, string name) => (bool)reader[name];

        public static bool SafeGetBoolean(this IDataReader reader, int i)
            => reader.IsDBNull(i) ? default(bool)
                                  : reader.GetBoolean(i);

        public static bool SafeGetBoolean(this IDataReader reader, string name)
            => reader.SafeGetBoolean(reader.GetOrdinal(name));

        public static bool? SafeGetBooleanOrNull(this IDataReader reader, int i) => reader[reader.GetName(i)] as bool?;

        public static bool? SafeGetBooleanOrNull(this IDataReader reader, string name) => reader[name] as bool?;

        #endregion

        #region Object

        public static object GetValue(this IDataReader reader, string column)
        {
            return reader[column];
        }

        public static object SafeGetValue(this IDataReader reader, string column)
        {
            return reader.SafeGetValue(reader.GetOrdinal(column));
        }

        public static object SafeGetValue(this IDataReader reader, int i)
        {
            return reader.IsDBNull(i) ? null : reader.GetValue(i);
        }

        #endregion
    }
}

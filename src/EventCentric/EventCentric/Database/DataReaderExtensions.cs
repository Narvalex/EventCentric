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
        /// <summary>
        /// Gets the value of the specified column as a string in Null-Safe mode.
        /// </summary>
        /// <param name="reader">The <see cref="SqlDataReader"/> instance.</param>
        /// <param name="i">The zero-based column ordinal.</param>
        public static string SafeGetString(this IDataReader reader, int i)
        {
            if (!reader.IsDBNull(i))
                return reader.GetString(i);
            else
                return string.Empty;
        }

        /// <summary>
        /// Gets the value of the specified column as a string in Null-Safe mode.
        /// </summary>
        /// <param name="reader">The <see cref="SqlDataReader"/> instance.</param>
        /// <param name="name">The column name.</param>
        public static string SafeGetString(this IDataReader reader, string name)
        {
            if (reader[name] != null)
                return reader[name].ToString();
            else
                return string.Empty;
        }

        public static string GetString(this IDataReader reader, string name)
        {
            return reader[name].ToString();
        }

        /// <summary>
        /// Gets the value of the specified column as a string in Null-Safe mode. Also trims the string to eliminate white spaces.
        /// </summary>
        /// <param name="reader">The <see cref="SqlDataReader"/> instance.</param>
        /// <param name="i">The zero-based column ordinal.</param>
        public static string SafeGetAndTrimString(this IDataReader reader, int i)
        {
            if (!reader.IsDBNull(i))
                return reader.GetString(i).Trim();
            else
                return string.Empty;
        }

        /// <summary>
        /// Gets the value of the specified column as a string in Null-Safe mode. Also trims the string to eliminate white spaces.
        /// </summary>
        /// <param name="reader">The <see cref="SqlDataReader"/> instance.</param>
        /// <param name="name">The zero-based column ordinal.</param>
        public static string SafeGetAndTrimString(this IDataReader reader, string name)
        {
            if (reader[name] != null)
                return ((string)reader[name]).Trim();
            else
                return string.Empty;
        }

        /// <summary>
        /// Gets the value of the specified column as a an int in Null-Safe mode.
        /// </summary>
        /// <param name="reader">The <see cref="SqlDataReader"/> instance.</param>
        /// <param name="i">The zero-based column ordinal.</param>
        public static int SafeGetInt32(this IDataReader reader, int i)
        {
            if (!reader.IsDBNull(i))
                return reader.GetInt32(i);
            else
                return default(int);
        }

        /// <summary>
        /// Gets the value of the specified column as a an int in Null-Safe mode.
        /// </summary>
        /// <param name="reader">The <see cref="SqlDataReader"/> instance.</param>
        /// <param name="name">The column name.</param>
        public static int SafeGetInt32(this IDataReader reader, string name)
        {
            int number;
            int.TryParse(reader[name].ToString(), out number);
            return number;
        }

        /// <summary>
        /// Gets the value of the specified column as a long in Null-Safe mode.
        /// </summary>
        /// <param name="reader">The <see cref="SqlDataReader"/> instance.</param>
        /// <param name="i">The zero-based column ordinal.</param>
        public static long SafeGetInt64(this IDataReader reader, int i)
        {
            if (!reader.IsDBNull(i))
                return reader.GetInt64(i);
            else
                return default(int);
        }

        /// <summary>
        /// Gets the value of the specified column as a long in Null-Safe mode.
        /// </summary>
        /// <param name="reader">The <see cref="SqlDataReader"/> instance.</param>
        /// <param name="i">The zero-based column ordinal.</param>
        public static long SafeGetInt64(this IDataReader reader, string name)
        {
            long number;
            long.TryParse(reader[name].ToString(), out number);
            return number;
        }

        public static long GetInt64(this IDataReader reader, string name)
        {
            return long.Parse(reader[name].ToString());
        }

        /// <summary>
        /// Gets the value of the specified column as a decimal in Null-Safe mode.
        /// </summary>
        /// <param name="reader">The <see cref="SqlDataReader"/> instance.</param>
        /// <param name="i">The zero-based column ordinal.</param>
        public static decimal SafeGetDecimal(this IDataReader reader, int i)
        {
            if (!reader.IsDBNull(i))
                return reader.GetDecimal(i);
            else
                return default(decimal);
        }

        /// <summary>
        /// Gets the value of the specified column as a float in Null-Safe mode.
        /// </summary>
        /// <param name="reader">The <see cref="SqlDataReader"/> instance.</param>
        /// <param name="i">The zero-based column ordinal.</param>
        public static float SafeGetFloat(this IDataReader reader, int i)
        {
            if (!reader.IsDBNull(i))
                return reader.GetFloat(i);
            else
                return default(float);
        }

        /// <summary>
        /// Gets the value of the specified column as a double in Null-Safe mode.
        /// </summary>
        /// <param name="reader">The <see cref="SqlDataReader"/> instance.</param>
        /// <param name="i">The zero-based column ordinal.</param>
        public static double SafeGetDouble(this IDataReader reader, int i)
        {
            if (!reader.IsDBNull(i))
                return reader.GetDouble(i);
            else
                return default(float);
        }

        /// <summary>
        /// Gets the value of the specified column as a GUID in Null-Safe mode.
        /// </summary>
        /// <param name="reader">The <see cref="SqlDataReader"/> instance.</param>
        /// <param name="i">The zero-based column ordinal.</param>
        public static Guid SafeGetGuid(this IDataReader reader, int i)
        {
            if (!reader.IsDBNull(i))
                return reader.GetGuid(i);
            else
                return default(Guid);
        }

        public static Guid GetGuid(this IDataReader reader, string name)
        {
            return new Guid(reader[name].ToString());
        }

        /// <summary>
        /// Gets the value of the specified column as a DateTime in Null-Safe mode.
        /// </summary>
        /// <param name="reader">The <see cref="SqlDataReader"/> instance.</param>
        /// <param name="i">The zero-based column ordinal.</param>
        public static DateTime? SafeGetNullableDateTime(this IDataReader reader, int i)
        {
            if (!reader.IsDBNull(i))
                return reader.GetDateTime(i);
            else
                return null;
        }

        /// <summary>
        /// Gets the value of the specified column as a DateTime in Null-Safe mode.
        /// </summary>
        /// <param name="reader">The <see cref="SqlDataReader"/> instance.</param>
        /// <param name="i">The zero-based column ordinal.</param>
        public static DateTime? SafeGetDateTime(this IDataReader reader, string name)
        {
            DateTime date;
            DateTime.TryParse(reader[name].ToString(), out date);
            if (date != DateTime.MinValue)
                return date;
            else
                return null;
        }

        public static DateTime GetDateTime(this IDataReader reader, string name)
        {
            return DateTime.Parse(reader[name].ToString());
        }

        /// <summary>
        /// Gets the value of the specified column as a DateTime in Null-Safe mode.
        /// </summary>
        /// <param name="reader">The <see cref="SqlDataReader"/> instance.</param>
        /// <param name="i">The zero-based column ordinal.</param>
        public static DateTime SafeGetDateTime(this IDataReader reader, int i)
        {
            if (!reader.IsDBNull(i))
                return reader.GetDateTime(i);
            else
                return new DateTime();
        }

        /// <summary>
        /// Gets the value of the specified column as a DateTime in Null-Safe mode.
        /// </summary>
        /// <param name="reader">The <see cref="SqlDataReader"/> instance.</param>
        /// <param name="i">The zero-based column ordinal.</param>
        public static bool SafeGetBool(this IDataReader reader, int i)
        {
            if (!reader.IsDBNull(i))
                return reader.GetBoolean(i);
            else
                return default(bool);
        }

        /// <summary>
        /// Gets the value of the specified column as a DateTime in Null-Safe mode.
        /// </summary>
        /// <param name="reader">The <see cref="SqlDataReader"/> instance.</param>
        /// <param name="i">The name of the column.</param>
        public static bool SafeGetBool(this IDataReader reader, string name)
        {
            int number;
            int.TryParse(reader[name].ToString(), out number);
            if (number > 0)
                return true;
            else
                return false;
        }
    }
}

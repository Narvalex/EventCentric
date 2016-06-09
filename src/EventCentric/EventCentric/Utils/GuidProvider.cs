using System;

namespace EventCentric.Utils
{
    public static class GuidManager
    {
        public static IGuidProvider GetGuidProvider()
        {
            return new SequentialGuid();
        }
    }

    public interface IGuidProvider
    {
        Guid NewGuid();
    }

    /// <summary>
    /// An <see cref="IGuidGenerator" /> that generates <see cref="System.Guid"/> values 
    /// using a strategy suggested Jimmy Nilsson's 
    /// <a href="http://www.informit.com/articles/article.asp?p=25862">article</a>
    /// and <a href="https://blog.codinghorror.com/primary-keys-ids-versus-guids/">.</a>
    /// </summary>
    /// <remarks>
    /// The <c>comb</c> algorithm is designed to make the use of GUIDs as Primary Keys, Foreign Keys, 
    /// and Indexes nearly as efficient as ints.
    /// </remarks>
    public class SequentialGuid : IGuidProvider
    {
        public static Guid New
        {
            get { return new SequentialGuid().NewGuid(); }
        }

        /// <summary>
        /// Generate a new <see cref="Guid"/> using the comb algorithm.
        /// </summary>
        public Guid NewGuid()
        {
            byte[] guidArray = Guid.NewGuid().ToByteArray();

            DateTime baseDate = new DateTime(1900, 1, 1);
            DateTime now = DateTime.Now;

            // Get the days and milliseconds which will be used to build the byte string 
            TimeSpan days = new TimeSpan(now.Ticks - baseDate.Ticks);
            TimeSpan msecs = now.TimeOfDay;

            // Convert to a byte array 
            // Note that SQL Server is accurate to 1/300th of a millisecond so we divide by 3.333333 
            byte[] daysArray = BitConverter.GetBytes(days.Days);
            byte[] msecsArray = BitConverter.GetBytes((long)(msecs.TotalMilliseconds / 3.333333));

            // Reverse the bytes to match SQL Servers ordering 
            Array.Reverse(daysArray);
            Array.Reverse(msecsArray);

            // Copy the bytes into the guid 
            Array.Copy(daysArray, daysArray.Length - 2, guidArray, guidArray.Length - 6, 2);
            Array.Copy(msecsArray, msecsArray.Length - 4, guidArray, guidArray.Length - 4, 4);

            return new Guid(guidArray);
        }
    }
}

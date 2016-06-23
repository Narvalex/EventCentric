using System.Collections.Generic;
using System.Linq;

namespace EventCentric
{
    /// <summary>
    /// A value object.
    /// </summary>
    /// <typeparam name="T">Any class.</typeparam>
    /// <remarks>
    /// // An example: 
    /// public class Hours : ValueObject<Hours>
    /// {
    ///     public readonly int Amount;
    ///     public Hours(int amount)
    ///     {
    ///         this.Amount = amount;
    ///     }
    ///     public static Hours operator -(Hours left, Hours right)
    ///     {
    ///         return new Hours(left.Amount - right.Amount);
    ///     }
    ///     protected override IEnumerable<object> GetAttributesToIncludeInEqualityCheck()
    ///     {
    ///         return new object[] { Amount };
    ///     }
    /// }
    /// </remarks>
    public abstract class ValueObject<T> where T : ValueObject<T>
    {
        protected abstract IEnumerable<object> GetAttributesToIncludeInEqualityCheck();

        public override bool Equals(object other)
        {
            return this.Equals(other as T);
        }

        public bool Equals(T other)
        {
            if (other == null)
                return false;

            return GetAttributesToIncludeInEqualityCheck()
                    .SequenceEqual(other.GetAttributesToIncludeInEqualityCheck());
        }

        public static bool operator ==(ValueObject<T> left, ValueObject<T> right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(ValueObject<T> left, ValueObject<T> right)
        {
            return !(left == right);
        }

        public override int GetHashCode()
        {
            int hash = 17;
            foreach (var obj in this.GetAttributesToIncludeInEqualityCheck())
                hash = hash * 31 + (obj == null ? 0 : obj.GetHashCode());

            return hash;
        }
    }
}

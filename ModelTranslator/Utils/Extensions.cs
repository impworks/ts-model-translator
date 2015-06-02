using System;
using System.Collections.Generic;
using System.Linq;

namespace ModelTranslator.Utils
{
    /// <summary>
    /// List of useful extension methods.
    /// </summary>
    public static class Extensions
    {
        /// <summary>
        /// Rejects items from a set for which any of the specified functions return true.
        /// </summary>
        public static IEnumerable<T> Restrict<T>(this IEnumerable<T> source, IList<Func<T, bool>> restrictions)
        {
            return source.Where(item => !restrictions.Any(fx => fx(item)));
        }
    }
}

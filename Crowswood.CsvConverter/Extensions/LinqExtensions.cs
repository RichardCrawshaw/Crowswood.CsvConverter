namespace Crowswood.CsvConverter.Extensions
{
    /// <summary>
    /// An extensions class to assist with LiNQ expressions.
    /// </summary>
    public static class LinqExtensions
    {
        /// <summary>
        /// Return all the non-null elements from <paramref name="enumerable"/> as an <see cref="IEnumerable{T}"/>
        /// of non-null <typeparamref name="T"/>.
        /// </summary>
        /// <typeparam name="T">The <see cref="Type"/> of the element.</typeparam>
        /// <param name="enumerable">An <see cref="IEnumerable{T}"/> of nullable <typeparamref name="T"/>.</param>
        /// <returns>An <see cref="IEnumerable{T}"/> of non-null <typeparamref name="T"/>.</returns>
        public static IEnumerable<T> NotNull<T>(this IEnumerable<T?> enumerable) where T : class =>
            enumerable
                .Where(e => e != null)
                .Select(e => e!);

        /// <summary>
        ///  Returns all the elements from <paramref name="source"/> once they have been 
        ///  cast to <typeparamref name="TTarget"/> as an <see cref="IEnumerable{T}"/> of 
        ///  <typeparamref name="TTarget"/> from <typeparamref name="TSource"/>.
        /// </summary>
        /// <typeparam name="TSource">The type of the original data.</typeparam>
        /// <typeparam name="TTarget">The type that the data is to be cast to.</typeparam>
        /// <param name="source">An <see cref="IEnumerable{T}"/> of <typeparamref name="TSource"/>.</param>
        /// <returns>An <see cref="IEnumerable{T}"/> of <typeparamref name="TTarget"/>.</returns>
        public static IEnumerable<TTarget> NotNull<TSource, TTarget>(this IEnumerable<TSource> source)
            where TSource : class
            where TTarget : class =>
            source
                .Where(item => item is TTarget target && target != null)
                .Cast<TTarget>();

        /// <summary>
        /// Returns all non-null values from <paramref name="dictionary"/> as an <see cref="IDictionary{TKey, TValue}"/>.
        /// </summary>
        /// <typeparam name="TKey">The <see cref="Type"/> of the key of the dictionary.</typeparam>
        /// <typeparam name="TValue">The <see cref="Type"/> of the value of the dictionary.</typeparam>
        /// <param name="dictionary">An <see cref="IDictionary{TKey, TValue}"/></param>
        /// <returns>An <see cref="IDictionary{TKey, TValue}"/> with non-null values.</returns>
        public static IDictionary<TKey, TValue> NotNull<TKey, TValue>(this IDictionary<TKey, TValue?> dictionary)
            where TKey : notnull
            where TValue : class =>
            dictionary
                .Where(kvp => kvp.Value != null)
                .ToDictionary(kvp => kvp.Key, kvp => kvp.Value!);
    }
}

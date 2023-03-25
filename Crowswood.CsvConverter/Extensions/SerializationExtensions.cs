using Crowswood.CsvConverter.Serializations;

namespace Crowswood.CsvConverter.Extensions
{
    public static class SerializationExtensions
    {
        /// <summary>
        /// Gets the elements from <paramref name="serializations"/> that are assignable to 
        /// <typeparamref name="T"/>.
        /// </summary>
        /// <typeparam name="T">The <see cref="Type"/> of serialization that is wanted.</typeparam>
        /// <param name="serializations">An <see cref="IEnumerable{T}"/> of <see cref="BaseSerializationData"/>.</param>
        /// <returns>An <see cref="IEnumerable{T}"/> of <typeparamref name="T"/>.</returns>
        public static IEnumerable<T> Get<T>(this IEnumerable<BaseSerializationData> serializations)
            where T : BaseSerializationData =>
            serializations
                .Where(s => s.GetType().IsAssignableTo(typeof(T)))
                .Select(s => s as T)
                .NotNull();
    }
}

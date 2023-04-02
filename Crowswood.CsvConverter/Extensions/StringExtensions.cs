using System.Windows.Markup;

namespace Crowswood.CsvConverter.Extensions
{
    public static class StringExtensions
    {
        /// <summary>
        /// Trims the elements in <paramref name="values"/>.
        /// </summary>
        /// <param name="values">A <see cref="string[]"/> containing the values to trim.</param>
        /// <returns>A <see cref="string[]"/>.</returns>
        public static string[] Trim(this string[] values) => 
            values
                .Select(value => value.Trim())
                .ToArray();

        /// <summary>
        /// Trims the specified <paramref name="character"/> from the elements in <paramref name="values"/>.
        /// </summary>
        /// <param name="values">A <see cref="string[]"/> containing the values to trim.</param>
        /// <param name="character">A <see cref="char"/> containing the character to trim.</param>
        /// <returns>A <see cref="string[]"/>.</returns>
        public static string[] Trim(this string[] values, char character) =>
            values
                .Select(value => value.Trim(character))
                .ToArray();

        /// <summary>
        /// Trims the elements in each of <paramref name="values"/>.
        /// </summary>
        /// <param name="values">An <see cref="IEnumerable{T}"/> of <see cref="string[]"/> containing the values to trim.</param>
        /// <returns>A <see cref="IEnumerable{T}"/> of <see cref="string[]"/>.</returns>
        public static IEnumerable<string[]> Trim(this IEnumerable<string[]>? values) =>
            (values ?? Enumerable.Empty<string[]>())
                .Select(values => values.Trim());

        /// <summary>
        /// Trims the specified <paramref name="character"/> from the elements in each of 
        /// <paramref name="values"/>.
        /// </summary>
        /// <param name="values">An <see cref="IEnumerable{T}"/> of <see cref="string[]"/> containing the values to trim.</param>
        /// <param name="character">A <see cref="char"/> containing the character to trim.</param>
        /// <returns>A <see cref="IEnumerable{T}"/> of <see cref="string[]"/>.</returns>
        public static IEnumerable<string[]> Trim(this IEnumerable<string[]> values, char character) =>
            (values ?? Enumerable.Empty<string[]>())
                .Select(values => values.Trim(character));
    }
}

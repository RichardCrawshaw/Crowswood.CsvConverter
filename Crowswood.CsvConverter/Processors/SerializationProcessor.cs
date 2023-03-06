using System.Reflection;
using Crowswood.CsvConverter.Extensions;
using Crowswood.CsvConverter.Handlers;
using Crowswood.CsvConverter.Helpers;

namespace Crowswood.CsvConverter.Processors
{
    internal class SerializationProcessor
    {
        #region Fields

        private readonly Options options;

        private readonly MetadataHandler metadataHandler;

        #endregion

        #region Constructors

        public SerializationProcessor(Options options, MetadataHandler metadataHandler)
        {
            this.options = options;

            this.metadataHandler = metadataHandler;
        }

        #endregion

        #region Methods

        /// <summary>
        /// Serializes the specified <paramref name="values"/> to a <see cref="string"/>.
        /// </summary>
        /// <typeparam name="TBase">The base type of objects to serialize.</typeparam>
        /// <param name="values">An <see cref="IEnumerable{T}"/> of <typeparamref name="TBase"/> to serialize.</param>
        /// <returns>A <see cref="string"/>.</returns>
        internal string Process<T>(params T[] values) where T : class
        {
            var lines = new List<string>();

            var types = this.options.OptionTypes.Any()
                ? this.options.OptionTypes.Select(optionType => optionType.Type)
                : values.Select(value => value.GetType()).Distinct();

            foreach (var type in types)
            {
                lines.AddRange(ConvertFrom(type, values));
                lines.Add("\r\n");
            }

            var text = string.Join("\r\n", lines);
            return text;
        }

        /// <summary>
        /// Serializes the specified <paramref name="data"/> to a <see cref="string"/>.
        /// </summary>
        /// <param name="data">A <see cref="Dictionary{TKey, TValue}"/> of <see cref="Tuple{T1, T2}"/> of <see cref="string[]"/> and <see cref="IEnumerable{T}"/> of <see cref="string[]"/> keyed by <see cref="string"/>.</param>
        /// <returns>A <see cref="string"/>.</returns>
        internal string Process(Dictionary<string, (string[], IEnumerable<string[]>)> data)
        {
            var lines = new List<string>();

            foreach (var kvp in data)
            {
                if (this.metadataHandler.Metadata.ContainsKey(kvp.Key))
                {
                    lines.AddRange(
                        this.metadataHandler.Metadata[kvp.Key]
                            .Select(item => new
                            {
                                Prefix =
                                    this.options.OptionMetadata
                                        .Where(optionsMetadata => optionsMetadata.Type == item.GetType())
                                        .Select(optionsMetadata => optionsMetadata.Prefix)
                                        .FirstOrDefault(),
                                Item = item,
                            })
                            .Where(n => n.Prefix is not null)
                            .Select(n => new
                            {
                                n.Prefix,
                                Values =
                                    n.Item.GetType().GetProperties()
                                        .Select(property => property.GetValue(n.Item)?.ToString())
                                        .Select(value => value is null ? string.Empty : $"\"{value}\"")
                                        .ToArray(),
                            })
                            .Select(n => ConverterHelper.FormatCsvData(n.Prefix!, kvp.Key, n.Values)));
                }
                lines.Add(ConverterHelper.FormatCsvData(this.options.PropertyPrefix, kvp.Key, kvp.Value.Item1)); //  Serialize
                lines.AddRange(
                    kvp.Value.Item2
                        .Select(items => ConverterHelper.FormatCsvData(this.options.ValuesPrefix, kvp.Key, items.Select(n => $"\"{n}\"").ToArray())));
                lines.Add("\r\n");
            }

            var text = string.Join("\r\n", lines);
            return text;
        }

        #endregion

        #region Conversion routines

        /// <summary>
        /// Converts the specified <paramref name="values"/> according to the <paramref name="optionType"/>
        /// into an <see cref="IEnumerable{T}"/> of <see cref="string"/>.
        /// </summary>
        /// <typeparam name="TBase">The type of object to process.</typeparam>
        /// <param name="optionType">An <see cref="OptionType"/> that controls the conversion.</param>
        /// <param name="values">An <see cref="IEnumerable{T}"/> of <typeparamref name="TBase"/> that contains the objects to process.</param>
        /// <returns>An <see cref="IEnumerable{T}"/> of <see cref="string"/>.</returns>
        /// <exception cref="ArgumentException">If <seealso cref="OptionType.Type"/> cannot be assigned to <typeparamref name="TBase"/>.</exception>
        /// <exception cref="InvalidOperationException">If the conversion failed.</exception>
        private IEnumerable<string> ConvertFrom<TBase>(OptionType optionType, IEnumerable<TBase> values)
            where TBase : class =>  //  can't use a new() contraint as TBase may well be abstract
            ConvertFrom<TBase>(optionType.Type, values);

        /// <summary>
        /// Converts the specified <paramref name="values"/> of type <paramref name="type"/> into
        /// an <see cref="IEnumerable{T}"/> of <see cref="string"/>.
        /// </summary>
        /// <typeparam name="TBase">The type of object to process.</typeparam>
        /// <param name="type">The <see cref="Type"/> of object; it must be assignable to <typeparamref name="TBase"/>.</param>
        /// <param name="values">An <see cref="IEnumerable{T}"/> of <typeparamref name="TBase"/> that contains the objects to process.</param>
        /// <returns>An <see cref="IEnumerable{T}"/> of <see cref="string"/>.</returns>
        /// <exception cref="ArgumentException">If <seealso cref="OptionType.Type"/> cannot be assigned to <typeparamref name="TBase"/>.</exception>
        /// <exception cref="InvalidOperationException">If the conversion failed.</exception>
        /// <remarks>This routine calls its namesake by reflection.</remarks>
        private IEnumerable<string> ConvertFrom<TBase>(Type type, IEnumerable<TBase> values)
            where TBase : class //  can't use a new() contraint as TBase may well be abstract
        {
            if (!type.IsAssignableTo(typeof(TBase)))
                throw new ArgumentException(
                    $"Unable to assign an object of type {type.Name} to {typeof(TBase).Name}.");

            var name = nameof(ConvertFrom);

            var returnType = typeof(IEnumerable<>).MakeGenericType(typeof(string));

            // Filter the parameters to those just of the type we're currently working with.
            var parameters = new object[]
                {
                    values
                        .Where(n => n?.GetType().Equals(type) ?? false)
                        .ToList(),
                    type,
                };

            // Explicitly set the types, rather than relying on the type of each parameter.
            var types =
                new[]
                {
                    typeof(IEnumerable<>).MakeGenericType(typeof(TBase)),
                    typeof(Type),
                };

            // name is the name of the method to retrieve.
            // type is the generic type parameter.
            // returnType is the type of the return parameter.
            // types are the types of the parameters the method expects.
            var method = GetType().GetGenericMethod(name, typeof(TBase), returnType, types);
            var result =
                method.Invoke(this, parameters) ??
                throw new InvalidOperationException(
                    $"Failed to convert from type {type.Name}.");

            return (IEnumerable<string>)result;
        }

        /// <summary>
        /// Converts the specified <paramref name="values"/> into an <see cref="IEnumerable{T}"/> 
        /// of <see cref="string"/>.
        /// </summary>
        /// <typeparam name="T">The type of object to process.</typeparam>
        /// <param name="values">An <see cref="IEnumerable{T}"/> of <typeparamref name="T"/> that contains the objects to process.</param>
        /// <returns>An <see cref="IEnumerable{T}"/> of <see cref="string"/>.</returns>
        /// <remarks>This routine is called by reflection.</remarks>
        private IEnumerable<string> ConvertFrom<T>(IEnumerable<T> values, Type type)
            where T : class //  can't use a new() contraint as TBase may well be abstract
        {
            var results = new List<string>();

            var properties =
                type.GetProperties()
                    .Where(property => property.CanRead)
                    .Where(property => property.CanWrite)
                    .Where(property => property.PropertyType == typeof(string) ||
                                       property.PropertyType.IsValueType &&
                                       property.PropertyType != typeof(DateTime))
                    .Select(property => new
                    {
                        Property = property,
                        Depth = property.DeclaringType?.GetDepth() ?? 0,
                    })
                    .OrderBy(n => n.Depth)
                    .Select(n => n.Property)
                    .ToArray();
            var parameters =
                properties
                    .Select(property =>
                        this.options.OptionMembers
                            .Where(assignment => assignment.Property.Name == property.Name)
                            .Select(assignment => assignment.Name)
                            .FirstOrDefault() ??
                        property.GetCustomAttribute<CsvConverterPropertyAttribute>()?.Name ??
                        property.Name)
                    .ToArray();

            var typeName =
                type.GetCustomAttribute<CsvConverterClassAttribute>()?.Name ??
                type.Name;

            results.Add(ConverterHelper.FormatCsvData(this.options.PropertyPrefix, typeName, parameters)); //  ConvertFrom
            foreach (var value in values)
            {
                var asStrings = ConverterHelper.AsStrings(value, properties);
                results.Add(ConverterHelper.FormatCsvData(this.options.ValuesPrefix, typeName, asStrings.ToArray()));
            }

            return results;
        }

        #endregion
    }
}

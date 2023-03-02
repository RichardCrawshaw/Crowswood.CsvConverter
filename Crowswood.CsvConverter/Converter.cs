using System.Reflection;
using System.Runtime.CompilerServices;
using Crowswood.CsvConverter.Extensions;
using Crowswood.CsvConverter.Handlers;
using Crowswood.CsvConverter.Helpers;
using Crowswood.CsvConverter.Model;

[assembly: InternalsVisibleTo("Crowswood.CsvConverter.Tests")]

namespace Crowswood.CsvConverter
{
    /// <summary>
    /// Converter class for CSV data that supports comments and multiple data-types in a single
    /// data-stream. Comments and blank lines are ignored. Each data-type requires a properties
    /// line and multiple values lines; each value line represents a single object instance. 
    /// Property and values lines are distinguished by different prfixes: these can be configured, 
    /// but have usable defaults. Each data-type is distinguished by the name of the data-type; 
    /// this follows the property / values prefix. Example:
    /// Properties,Foo,Field1,Field2,...
    /// Values,Foo,Value1,Value2,...
    /// </summary>
    /// <remarks>
    /// NOTE: Comments are NOT written when the data is serialized, nor are blank lines retained; 
    /// this is because the comments and blank lines are not tracked. Also, the data may have 
    /// changed and it is not possible to know whether they are still relevent to the new data.
    /// </remarks>
    public sealed class Converter
    {
        #region Fields

        private readonly Options options;

        private readonly IndexHandler indexHandler = new();
        private readonly MetadataHandler metadataHandler;

        private ConfigHandler configHandler = new();

        #endregion

        #region Properties

        /// <summary>
        /// Gets the metadata as a dictionary keyed by <see cref="Type"/>.
        /// </summary>
        public Dictionary<string, List<object>> Metadata => this.metadataHandler.Metadata;

        #endregion

        #region Constructors

        /// <summary>
        /// Creates a new instance of <see cref="Converter"/> according to the specified
        /// <paramref name="options"/>.
        /// </summary>
        /// <param name="options">An <see cref="options"/> object.</param>
        public Converter(Options? options)
        {
            this.options = OptionsHelper.ValidateOptions(options ?? Options.None);

            this.metadataHandler = new(this.options, this.indexHandler);
        }

        #endregion

        #region Methods

        /// <summary>
        /// Deserialize the specified <paramref name="text"/> into an <see cref="IEnumerable{T}"/> 
        /// of <typeparamref name="TBase"/> objects.
        /// </summary>
        /// <typeparam name="TBase">The base type of objects to return.</typeparam>
        /// <param name="text">A <see cref="string"/> that contains the data to deserialize.</param>
        /// <returns>An <see cref="IEnumerable{T}"/> of <typeparamref name="TBase"/>.</returns>
        public IEnumerable<TBase> Deserialize<TBase>(string text)
            where TBase : class
        {
            if (string.IsNullOrEmpty(text))
                throw new InvalidOperationException("Text is empty.");

            var lines = SplitLines(text);

            this.configHandler = new(this.options, lines);

            var types =
                this.options.OptionTypes.Any()
                ? this.options.OptionTypes.Select(optionType => optionType.Type)
                : GetTypes<TBase>(lines);

            if (types.Any(type => type == typeof(Type)))
                throw new InvalidOperationException("Dynamic types included.");

            this.metadataHandler.Clear();

            InitialiseIndexes(true, types.Select(type => type.Name).ToArray());

            // Generate a dictionary of type-less data.
            var typelessData =
                types
                    .Where(type => type.IsAssignableTo(typeof(TBase)))
                    .ToDictionary(
                        type => type.Name,
                        type => ConvertTo(type, lines));

            foreach(var data in typelessData.Values)
            {
                ConvertTo(data!, typelessData!);
            }

            // Convert the type-less data to typed entities.
            var results =
                typelessData
                    .Select(kvp => new { kvp.Key, Values = kvp.Value?.Get()})
                    .Where(n => n.Values is not null)
                    .Select(n => ConvertTo<TBase>(n.Key, ((string[], IEnumerable<string[]>))n.Values!))
                    .SelectMany(n => n)
                    .ToList();

            return results;
        }

        /// <summary>
        /// Deserialize the specified <paramref name="text"/> into a <see cref="Dictionary{TKey, TValue}"/> 
        /// of <see cref="Tuple{T1, T2}"/> of <see cref="string[]"/> and <see cref="IEnumerable{T}"/> 
        /// of <see cref="string[]"/>, keyed by <see cref="string"/>.
        /// </summary>
        /// <param name="text">A <see cref="string"/> that contains the data to deserialize.</param>
        /// <returns>A  <see cref="Dictionary{TKey, TValue}"/> of <see cref="Tuple{T1, T2}"/> of <see cref="string[]"/> and <see cref="IEnumerable{T}"/> of <see cref="string[]"/>, keyed by <see cref="string"/>.</returns>
        /// <exception cref="InvalidOperationException">If <paramref name="text"/> is empty.
        /// or
        /// If there are no types defined in the options, or if non-dynamic types are included.</exception>
        public Dictionary<string, (string[], IEnumerable<string[]>)> Deserialize(string text)
        {
            if (string.IsNullOrEmpty(text))
                throw new InvalidOperationException("Text is empty.");

            var lines = SplitLines(text);

            this.configHandler = new(this.options, lines);

            if (!this.options.OptionTypes.Any())
                throw new InvalidOperationException("No types defined.");
            if (this.options.OptionTypes.Any(ot => ot.Type != typeof(Type)))
                throw new InvalidOperationException("Non-dynamic types included.");

            this.metadataHandler.Clear();

            InitialiseIndexes(true, this.options.OptionTypes.Select(optionType => optionType.Name).ToArray());

            var dynamicTypes =
                this.options.OptionTypes
                    .Select(optionType => optionType as OptionDynamicType)
                    .NotNull();

            var dictionary =
                dynamicTypes
                    .Select(dynamicType => new { dynamicType.Name, Values = ConvertTo(dynamicType, lines), })
                    .Where(n => n.Values is not null)
                    .NotNull()
                    .ToDictionary(
                        n => n.Name,
                        n => n.Values!);

            foreach (var data in dictionary.Values)
                if (data is not null)
                    ConvertTo(data, dictionary);

            var results =
                dictionary.ToDictionary(
                    kvp => kvp.Key,
                    kvp => kvp.Value.Get());

            return results;
        }

        /// <summary>
        /// Serializes the specified <paramref name="values"/> to a <see cref="string"/>.
        /// </summary>
        /// <typeparam name="TBase">The base type of objects to serialize.</typeparam>
        /// <param name="values">An <see cref="IEnumerable{T}"/> of <typeparamref name="TBase"/> to serialize.</param>
        /// <returns>A <see cref="string"/>.</returns>
        public string Serialize<TBase>(IEnumerable<TBase> values) where TBase : class
        {
            var lines = new List<string>();

            var types = this.options.OptionTypes.Any()
                ? this.options.OptionTypes.Select(optionType => optionType.Type)
                : values.Select(value => value.GetType()).Distinct();

            foreach(var type in types)
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
        public string Serialize(Dictionary<string, (string[], IEnumerable<string[]>)> data)
        {
            var lines = new List<string>();

            foreach(var kvp in data)
            {
                if (this.Metadata.ContainsKey(kvp.Key))
                {
                    lines.AddRange(
                        this.Metadata[kvp.Key]
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
                            .Select(n => ConversionHelper.FormatCsvData(n.Prefix!, kvp.Key, n.Values)));
                }
                lines.Add(ConversionHelper.FormatCsvData(this.options.PropertyPrefix, kvp.Key, kvp.Value.Item1)); //  Serialize
                lines.AddRange(
                    kvp.Value.Item2
                        .Select(items => ConversionHelper.FormatCsvData(this.options.ValuesPrefix, kvp.Key, items.Select(n => $"\"{n}\"").ToArray())));
                lines.Add("\r\n");
            }

            var text = string.Join("\r\n", lines);
            return text;
        }

        #endregion

        #region ConvertFrom serialization routines

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

            results.Add(ConversionHelper.FormatCsvData(this.options.PropertyPrefix, typeName, parameters)); //  ConvertFrom
            foreach (var value in values)
            {
                var asStrings = ConversionHelper.AsStrings(value, properties);
                results.Add(ConversionHelper.FormatCsvData(this.options.ValuesPrefix, typeName, asStrings.ToArray()));
            }

            return results;
        }

        #endregion

        #region ConvertTo deserialization routines

        /// <summary>
        /// Converts the specified typeless <paramref name="data"/> with a named type of 
        /// <paramref name="typeName"/> into an <see cref="IEnumerable{T}"/> of typed objects that 
        /// derive from <typeparamref name="TBase"/>.
        /// </summary>
        /// <typeparam name="TBase">The base <see cref="Type"/>.</typeparam>
        /// <param name="typeName">A <see cref="string"/> that contains the name of the type</param>
        /// <param name="data">An <see cref="IEnumerable{T}"/> of <see cref="string[]"/> that contains the data to convert.</param>
        /// <returns>An <see cref="IEnumerable{T}"/> of <typeparamref name="TBase"/>.</returns>
        /// <exception cref="ArgumentException">
        /// If <paramref name="typeName"/> does not match a type from the <see cref="Assembly"/> 
        /// that contains <typeparamref name="TBase"/>.
        /// </exception>
        /// <exception cref="InvalidOperationException">If the conversion failed.</exception>
        /// <remarks>
        /// This routine calls <seealso cref="ConvertTo{T}(string[], IEnumerable{string[]})"/> by 
        /// reflection.
        /// </remarks>
        private IEnumerable<TBase> ConvertTo<TBase>(string typeName, (string[], IEnumerable<string[]>) data)
            where TBase : class
        {
            var types =
                Assembly.GetAssembly(typeof(TBase))?.GetTypes()
                    .Where(type => type.Name == typeName)
                    .ToList() ?? new List<Type>();
            if (types.Count > 1)
                throw new InvalidOperationException(
                    $"Unable to identify exactly one type called '{typeName}' from the assembly containing '{typeof(TBase).Name}'.");
            var type = 
                types.FirstOrDefault() ??
                throw new ArgumentException(
                    $"Unable to locate {typeName} from the Assembly containing {typeof(TBase).Name}.",
                    nameof(typeName));

            var returnType = typeof(IEnumerable<>).MakeGenericType(type);
            var methodName = nameof(ConvertTo);
            var arguments = new object[]
            {
                data.Item1, // names
                data.Item2, // values
            };

            var method = GetType().GetGenericMethod(methodName, type, returnType, arguments);
            var result =
                method.Invoke(this, arguments) ??
                throw new InvalidOperationException(
                    $"Failed to convert to type {type.Name}.");

            return (IEnumerable<TBase>)result;
        }

        /// <summary>
        /// Converts the specified typeless data that is made up from <paramref name="names"/> and 
        /// <paramref name="values"/> into an <see cref="IEnumerable{T}"/> of <typeparamref name="T"/>.
        /// </summary>
        /// <typeparam name="T">The <typeparamref name="T"/> of object to convert the data into.</typeparam>
        /// <param name="names">A <see cref="string[]"/> that contains names of the properties that the <paramref name="values"/> are to populate.</param>
        /// <param name="values">An <see cref="IEnumerable{T}"/> of <see cref="string[]"/> that contains the data.</param>
        /// <returns>An <see cref="IEnumerable{T}"/> of <typeparamref name="T"/>.</returns>
        /// <remarks>
        /// This routine is called via reflection by <seealso cref="ConvertTo{TBase}(string, (string[], IEnumerable{string[]}))"/>.
        /// </remarks>
        private IEnumerable<T> ConvertTo<T>(string[] names, IEnumerable<string[]> values)
            where T : class, new()
        {
            var type = typeof(T);
            var propertyAndNamePairs =
                type.GetPropertyAndAttributePairs()
                    .GetPropertyAndNamePairs();

            this.metadataHandler.Update(type);

            foreach (var value in values)
                yield return Create<T>(propertyAndNamePairs, names, value);
        }

        /// <summary>
        /// Converts using the specified <paramref name="optionDynamicType"/> the specified 
        /// <paramref name="lines"/> into a typeless data object.
        /// </summary>
        /// <param name="optionDynamicType">A <see cref="OptionDynamicType"/> object.</param>
        /// <param name="lines">An <see cref="IEnumerable{T}"/> of <see cref="string"/> containing the data to convert.</param>
        /// <returns>A <see cref="TypelessData"/> object.</returns>
        /// <remarks>Used when deserializing into typeless data, when no subsequent conversion will be done.</remarks>
        private TypelessData? ConvertTo(OptionDynamicType optionDynamicType, IEnumerable<string> lines)
        {
            this.metadataHandler.Construct(optionDynamicType.Name, lines);

            var result = ConvertTo(optionDynamicType.Name, optionDynamicType.PropertyNames, null, lines);

            return result;
        }

        /// <summary>
        /// Converts for the specified <paramref name="type"/> the specified <paramref name="lines"/> 
        /// into a typeless data object.
        /// </summary>
        /// <param name="type">A <see cref="Type"/> indicating the type associated with the typeless object.</param>
        /// <param name="lines">An <see cref="IEnumerable{T}"/> of <see cref="string"/> containing the data to convert.</param>
        /// <returns>A <see cref="TypelessData"/> object.</returns>
        /// <remarks>Used when deserializing into typeless data before converting to typed data.</remarks>
        private TypelessData? ConvertTo(Type type, IEnumerable<string> lines)
        {
            this.metadataHandler.Construct(type.Name, lines);

            var typeName =
                type.GetCustomAttribute<CsvConverterClassAttribute>()?.Name ??
                type.Name;

            var propertyAndNamePairs =
                type.GetPropertyAndAttributePairs()
                    .GetPropertyAndNamePairs();

            var propertyNames =
                propertyAndNamePairs
                    .Select(pair => pair.Property.Name)
                    .ToArray();

            var result = ConvertTo(typeName, propertyNames, propertyAndNamePairs?.ToArray(), lines);

            return result;
        }

        /// <summary>
        /// Converts for the specified <paramref name="typeName"/> using the specified <paramref name="propertyNames"/> 
        /// and <paramref name="propertyAndNamePairs"/> the specified <paramref name="lines"/> into 
        /// a typeless data object.
        /// </summary>
        /// <param name="typeName">A <see cref="string"/> containing the type.</param>
        /// <param name="propertyNames">A <see cref="string[]"/> containing the names for the typeless data object.</param>
        /// <param name="propertyAndNamePairs">A <see cref="PropertyAndNamePair[]"/> that can be null.</param>
        /// <param name="lines">An <see cref="IEnumerable{T}"/> of <see cref="string"/> containing the data to convert.</param>
        /// <returns>A <see cref="TypelessData"/> object.</returns>
        /// <exception cref="Exception">If <paramref name="lines"/> did not contain any names for <paramref name="typeName"/>.</exception>
        private TypelessData? ConvertTo(string typeName, string[] propertyNames, PropertyAndNamePair[]? propertyAndNamePairs, IEnumerable<string> lines)
        {
            var names =
                ConversionHelper.GetNames(typeName, lines, this.configHandler.GetPropertyPrefix(typeName));
            var values = 
                ConversionHelper.GetValues(typeName, lines, this.configHandler.GetValuePrefix(typeName));

            if (names is null && values.Any())
                throw new Exception($"Failed to identify property names for {typeName}.");

            if (names is null)
                return null;

            // Create a list of indexes to allow quick look up between the property position and
            // the name position. The names and values share the same position, the properties
            // positions can be different.
            var indexes = GetIndexLookup(names.ToList(), propertyNames, propertyAndNamePairs);

            // This is the object that the values will be set into.
            TypelessData result = new(propertyNames);

            var valueConverter = new ValueConverter(this.indexHandler, typeName);

            // Now iterate through the values read from the CSV data, extracting each individual 
            // value in the order of the names from the CSV and placing it in the position
            // indicated by the properties from the options.
            // There may be names that don't exist in the properties: they will be ignored; and 
            // properties that don't exist in the names: they will remain as empty string.
            foreach (var value in values)
            {
                // Create an array of the length of the properties; initialise it to empty string.
                var array = new string[propertyNames.Length];
                for (var index = 0; index < array.Length; index++)
                    array[index] = string.Empty;

                // index is the position of the name within the properties.
                for (var index = 0; index < array.Length; index++)
                {
                    // Check the position of the value within the row of CSV data, it may not exist!
                    if (indexes[index] == -1 ||
                        indexes[index] >= value.Length)
                    {
                        continue;
                    }

                    var item = value[indexes[index]];
                    var propertyType = item.Trim() == "#" ? typeof(int) : typeof(string);
                    array[index] =
                        valueConverter.ConvertValue(item, propertyType)?.ToString() ??
                        string.Empty;
                }

                result.Add(array);
            }

            return result;
        }

        /// <summary>
        /// Update the specified <paramref name="typelessData"/> that references any of the items 
        /// from <paramref name="data"/> with the referenced value.
        /// </summary>
        /// <param name="typelessData">A <see cref="TypelessData"/> object that is to be updated.</param>
        /// <param name="data">A <see cref="Dictionary{TKey, TValue}"/> of <see cref="TypelessData"/> keyed by <see cref="string"/> that contains the reference data.</param>
        private void ConvertTo(TypelessData typelessData, Dictionary<string, TypelessData> data)
        {
            var datatypes =
                data.Keys
                    .Select(dataType => $"#{dataType}(")
                    .ToArray();

            foreach(var name in typelessData.Names.Get())
                for (var index = 0; index < typelessData.Values.Count(); index++)
                    if (TryConvertValue(typelessData[name, index], datatypes, data, out var value))
                        typelessData[name, index] = value!;
        }

        #endregion

        #region Support routines

        /// <summary>
        /// Creates and returns a new <typeparamref name="T"/> with its properties set according 
        /// to the specified <paramref name="propertyAndNamePairs"/>, <paramref name="names"/>
        /// and <paramref name="values"/>.
        /// </summary>
        /// <typeparam name="T">The type of object to create.</typeparam>
        /// <param name="propertyAndNamePairs">A <see cref="List{T}"/> of <see cref="PropertyAndNamePair"/> objects.</param>
        /// <param name="names">A <see cref="string"/> array containing the names of the properties.</param>
        /// <param name="values">A <see cref="string"/> array containing the values to assign to the properties.</param>
        /// <returns>A <typeparamref name="T"/> instance.</returns>
        /// <exception cref="ArgumentException">If the length of the <paramref name="names"/> and <paramref name="values"/> are different.</exception>
        /// <remarks>
        /// The length of the <paramref name="names"/> and <paramref name="values"/> arrays must
        /// be the same, and their order must match.
        /// </remarks>
        private T Create<T>(IEnumerable<PropertyAndNamePair> propertyAndNamePairs, string[] names, string[] values)
            where T : class, new()
        {
            var valueConverter = new ValueConverter(this.indexHandler, typeof(T).Name);

            var result = 
                new T()
                    .SetProperties(valueConverter,
                                   this.options.OptionMembers,
                                   propertyAndNamePairs,
                                   names,
                                   values);
            return result;
        }

        /// <summary>
        /// Creates and returns an index look-up for the specified <paramref name="names"/> from 
        /// the <seealso cref="Options.OptionMembers"/>, and if not null the specified <paramref name="propertyAndNamePairs"/> 
        /// for the specified <paramref name="propertyNames"/>.
        /// </summary>
        /// <param name="names">A <see cref="string[]"/> containing the names of the fields in the data.</param>
        /// <param name="propertyNames">A <see cref="string[]"/> containing the names of the properties in the type.</param>
        /// <param name="propertyAndNamePairs">A <see cref="PropertyAndNamePair[]"/> that can be null.</param>
        /// <returns>A <see cref="List{T}"/> of <see cref="int"/>.</returns>
        private List<int> GetIndexLookup(List<string> names, string[] propertyNames, PropertyAndNamePair[]? propertyAndNamePairs)=>
            propertyNames
                .Select(propertyName =>
                    this.options.OptionMembers
                        .Where(optionMember => optionMember.Property.Name == propertyName)
                        .Select(optionMember => optionMember.Name)
                        .FirstOrDefault() ??
                    (propertyAndNamePairs ?? Array.Empty<PropertyAndNamePair>())
                        .Where(pair => pair.Name == propertyName)
                        .Select(pair => pair.Name)
                        .FirstOrDefault() ??
                    (propertyAndNamePairs ?? Array.Empty<PropertyAndNamePair>())
                        .Where(pair => pair.Property.Name == propertyName)
                        .Select(pair => pair.Name)
                        .FirstOrDefault() ??
                    propertyName)
                .Select(name => names.IndexOf(name))
                .ToList();

        /// <summary>
        /// Gets the types that exist in the <see cref="Assembly"/> that contains <typeparamref name="TBase"/>
        /// that exist in the specified <paramref name="lines"/>.
        /// </summary>
        /// <typeparam name="TBase">The base type.</typeparam>
        /// <param name="lines">An <see cref="IEnumerable{T}"/> of <see cref="string"/>.</param>
        /// <returns>An <see cref="IEnumerable{T}"/> of <see cref="Type"/>.</returns>
        private IEnumerable<Type> GetTypes<TBase>(IEnumerable<string> lines) where TBase : class
        {
            var typeNames =
                ConversionHelper.GetTypeNames(lines,
                                              this.configHandler.GetPropertyPrefixes(),
                                              tn => this.configHandler.GetPropertyPrefix(tn));
            var types =
                Assembly.GetAssembly(typeof(TBase))?.GetTypes()
                    .Select(type => new
                    {
                        Type = type,
                        Attribute = type.GetCustomAttribute<CsvConverterClassAttribute>(),
                    })
                    .Where(n => typeNames.Contains(n.Type.Name) ||
                                typeNames.Contains(n.Attribute?.Name))
                    .Select(n => n.Type) ?? new List<Type>();
            return types;
        }

        /// <summary>
        /// Initialise the <seealso cref="indexHandler"/>, clearing it if <paramref name="clear"/> 
        /// is true.
        /// </summary>
        /// <param name="clear">A <see cref="bool"/> true to clear existing data; false otherwise.</param>
        /// <param name="types">A <see cref="string"/> array containing the names of the types that are to be tracked.</param>
        private void InitialiseIndexes(bool clear, params string[] typeNames)
        {
            if (clear)
                this.indexHandler.Clear();
            this.indexHandler.Initialise(typeNames);
        }

        /// <summary>
        /// Splits the specified <paramref name="text"/> into lines on end of line characters, 
        /// removing any empty entries, trimming them and ignoring any that are comments.
        /// </summary>
        /// <param name="text">A <see cref="string"/> containing the text to split.</param>
        /// <returns>A <see cref="List{T}"/> of <see cref="string"/>.</returns>
        private List<string> SplitLines(string text) =>
            text.Split("\r\n".ToCharArray(), StringSplitOptions.RemoveEmptyEntries |
                                                StringSplitOptions.TrimEntries)
                .Where(line => !this.options.CommentPrefixes.Any(prefix => line.StartsWith(prefix)))
                .ToList();

        /// <summary>
        /// Attempt to convert the specified <paramref name="value"/> from a reference to another 
        /// data type to the Id of the referenced record using the specified <paramref name="datatypes"/> 
        /// and <paramref name="data"/>; the converted value is returned in <paramref name="result"/>.
        /// </summary>
        /// <param name="value">A <see cref="string"/> that contains the original value.</param>
        /// <param name="datatypes">A <see cref="string[]"/> that contains the names of the data types with a leading '#' and trailing '('.</param>
        /// <param name="data">A <see cref="Dictionary{TKey, TValue}"/> of <see cref="TypelessData"/> keyed by <see cref="string"/> that contains all the data that can be referenced.</param>
        /// <param name="result">A <see cref="string?"/> that contains the result of the conversion; or null is none was done.</param>
        /// <returns>True if a conversion was performed; false otherwise.</returns>
        private bool TryConvertValue(string value, string[] datatypes, Dictionary<string, TypelessData> data, out string? result)
        {
            result = null;

            if (value.StartsWith("#") &&
                datatypes.Any(typeName => value.StartsWith(typeName)))
            {
                var position = value.IndexOf("(");
                var dataType = value[1..position];
                var dataValue = value[(position + 1)..^1].Trim().Trim('"');
                var values = data[dataType];
                var nameIndex = values.Names[this.configHandler.GetReferenceNameColumnName(dataType)];
                var idIndex = values.Names[this.configHandler.GetReferenceIdColumnName(dataType)];
                var referenceRow =
                    values.Values
                        .SingleOrDefault(row => row[nameIndex] == dataValue);

                if (referenceRow is not null)
                    result = referenceRow[idIndex];
            }

            return result is not null;
        }

        #endregion
    }
}

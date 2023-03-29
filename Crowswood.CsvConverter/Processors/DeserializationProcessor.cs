using Crowswood.CsvConverter.Extensions;
using Crowswood.CsvConverter.Handlers;
using Crowswood.CsvConverter.Helpers;
using Crowswood.CsvConverter.Model;

namespace Crowswood.CsvConverter.Processors
{
    internal class DeserializationProcessor
    {
        #region Fields

        private readonly Options options;

        private readonly ConfigHandler configHandler;
        private readonly ConversionHandler conversionHandler;
        private readonly IndexHandler indexHandler;
        private readonly MetadataHandler metadataHandler;

        private readonly IEnumerable<string> lines;

        #endregion

        #region Constructors

        public DeserializationProcessor(Options options,
                                        ConfigHandler configHandler,
                                        ConversionHandler conversionHandler,
                                        IndexHandler indexHandler,
                                        MetadataHandler metadataHandler,
                                        IEnumerable<string> lines)
        {
            this.options = options;

            this.configHandler = configHandler;
            this.conversionHandler = conversionHandler;
            this.indexHandler = indexHandler;
            this.metadataHandler = metadataHandler;

            this.lines = lines;
        }

        #endregion

        #region Methods

        /// <summary>
        /// Deserialize text into an <see cref="IEnumerable{T}"/> of <typeparamref name="TBase"/> 
        /// objects.
        /// </summary>
        /// <typeparam name="TBase">The base type of objects to return.</typeparam>
        /// <param name="types">An <see cref="IEnumerable{T}"/> of <see cref="Type"/> that contains the types to process.</param>
        /// <returns>An <see cref="IEnumerable{T}"/> of <typeparamref name="TBase"/>.</returns>
        internal IEnumerable<TBase> Process<TBase>(IEnumerable<Type> types) 
            where TBase : class
        {
            InitialiseIndexes(true, types.Select(type => type.Name).ToArray());

            // Generate a dictionary of type-less data.
            var typelessData =
                types
                    .Where(type => type.IsAssignableTo(typeof(TBase)))
                    .Select(type => new
                    {
                        TypeName = this.conversionHandler.ConvertType(type.Name),
                        PropertyNames = ConvertTo(type, lines),
                    })
                    .Where(n => n.PropertyNames is not null)
                    .ToDictionary(n => n.TypeName, n => n.PropertyNames);

            foreach (var data in typelessData.Values)
                ConvertTo(data!, typelessData!);

            // Convert the type-less data to typed entities.
            var results =
                typelessData
                    .Select(kvp => new { kvp.Key, Values = kvp.Value?.Get() })
                    .Where(n => n.Values is not null)
                    .Select(n => ConvertTo<TBase>(n.Key, ((string[], IEnumerable<string[]>))n.Values!))
                    .SelectMany(n => n)
                    .ToList();

            return results;
        }

        /// <summary>
        /// Deserialize text into a <see cref="Dictionary{TKey, TValue}"/> of <see cref="Tuple{T1, T2}"/> 
        /// of <see cref="string[]"/> and <see cref="IEnumerable{T}"/> of <see cref="string[]"/>, 
        /// keyed by <see cref="string"/>.
        /// </summary>
        /// <returns>A  <see cref="Dictionary{TKey, TValue}"/> of <see cref="Tuple{T1, T2}"/> of <see cref="string[]"/> and <see cref="IEnumerable{T}"/> of <see cref="string[]"/>, keyed by <see cref="string"/>.</returns>
        /// <exception cref="InvalidOperationException">
        /// If there are no types defined in the options, or if non-dynamic types are included.
        /// </exception>
        internal Dictionary<string, (string[], IEnumerable<string[]>)> Process()
        {
            InitialiseIndexes(true, this.options.OptionTypes.Select(optionType => optionType.Name).ToArray());

            var dynamicTypes =
                this.options.OptionTypes
                    .Select(optionType => optionType as OptionDynamicType)
                    .NotNull();

            var dictionary =
                dynamicTypes
                    .Select(dynamicType => new
                    {
                        Name = this.conversionHandler.ConvertType(dynamicType.Name),
                        Values = ConvertTo(dynamicType, lines),
                    })
                    .Where(n => n.Values is not null)
                    .ToDictionary(n => n.Name, n => n.Values!);

            foreach (var data in dictionary.Values)
                if (data is not null)
                    ConvertTo(data, dictionary);

            var results =
                dictionary.ToDictionary(
                    kvp => kvp.Key,
                    kvp => kvp.Value.Get());

            return results;
        }

        #endregion

        #region Conversion routines

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
            var types = typeof(TBase).GetType(typeName);
            if (types.Length > 1)
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
            var typeName = type.GetTypeName();

            var propertyAndNamePairs =
                type.GetPropertyAndAttributePairs()
                    .GetPropertyAndNamePairs();

            var propertyNames =
                propertyAndNamePairs
                    .Select(pair => pair.Property.Name)
                    .ToArray();

            this.metadataHandler.Construct(typeName, lines);

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
                ConverterHelper.GetNames(typeName, lines, this.configHandler.GetPropertyPrefix(typeName));
            var values =
                ConverterHelper.GetValues(typeName, lines, this.configHandler.GetValuePrefix(typeName));

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

            var valueConverter = new ValueConverter(conversionHandler, this.indexHandler, typeName);

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

            foreach (var name in typelessData.Names.Get())
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
            var valueConverter = new ValueConverter(conversionHandler, this.indexHandler, typeof(T).Name);

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
        private List<int> GetIndexLookup(List<string> names, string[] propertyNames, PropertyAndNamePair[]? propertyAndNamePairs) =>
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

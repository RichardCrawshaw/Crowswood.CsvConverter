using System.Reflection;
using Crowswood.CsvConverter.Extensions;

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

        #endregion

        #region Constructors

        /// <summary>
        /// Creates a new instance of <see cref="Converter"/> according to the specified
        /// <paramref name="options"/>.
        /// </summary>
        /// <param name="options">An <see cref="Options"/> object.</param>
        public Converter(Options? options) => this.options = options ?? Options.None;

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
                throw new InvalidOperationException(
                    "Text is empty.");

            var lines =
                text.Split("\r\n".ToCharArray(),
                           StringSplitOptions.RemoveEmptyEntries |
                           StringSplitOptions.TrimEntries)
                    .Where(line => !options.CommentPrefixes.Any(prefix => line.StartsWith(prefix)))
                    .ToList();

            var results = new List<TBase>();

            if (options.OptionTypes.Any())
            {
                foreach (var optionType in options.OptionTypes)
                {
                    if (optionType.Type.IsAssignableTo(typeof(TBase)))
                        results.AddRange(ConvertTo<TBase>(optionType, lines));
                }
            }
            else
            {
                var typeNames =
                    lines
                        .Where(line => line.StartsWith(this.options.PropertyPrefix))
                        .Select(line => line.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
                        .Select(items => items[1])
                        .Distinct();
                var types =
                    Assembly.GetAssembly(typeof(TBase))?.GetTypes()
                        .Select(type => new { Type = type, Attribute = type.GetCustomAttribute<CsvConverterClassAttribute>(), })
                        .Where(n => typeNames.Contains(n.Type.Name) || typeNames.Contains(n.Attribute?.Name))
                        .Select(n => n.Type) ?? new List<Type>();
                foreach(var type in types)
                {

                    if (type.IsAssignableTo(typeof(TBase)))
                        results.AddRange(ConvertTo<TBase>(type, lines));
                }
            }

            foreach (var result in results)
                yield return result;
        }

        /// <summary>
        /// Deserialize from the <paramref name="stream"/> returning an <see cref="IEnumerable{T}"/> 
        /// of <typeparamref name="TBase"/> objects.
        /// </summary>
        /// <typeparam name="TBase">The base type of objects to return.</typeparam>
        /// <param name="stream">A <see cref="Stream"/> that contains the data to deserialize.</param>
        /// <returns>An <see cref="IEnumerable{T}"/> of <typeparamref name="TBase"/>.</returns>
        public IEnumerable<TBase> Deserialize<TBase>(Stream stream)
            where TBase : class
        {
            using var reader = new StreamReader(stream);
            var text = reader.ReadToEnd();
            var result = Deserialize<TBase>(text);
            return result;
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

            if (options.OptionTypes.Any())
            {
                foreach (var optionType in options.OptionTypes)
                {
                    lines.AddRange(ConvertFrom(optionType, values));
                }
            }
            else
            {
                var types =
                    values
                        .Select(value => value.GetType())
                        .Distinct()
                        .ToList();
                foreach(var type in types)
                {
                    lines.AddRange(ConvertFrom(type, values));
                }
            }

            var text = string.Join("\r\n", lines);
            return text;
        }

        #endregion

        #region Support routines

        /// <summary>
        /// Converts and returns the values of the properties of the specified <paramref name="item"/> 
        /// that are in the <paramref name="properties"/> into an <see cref="IEnumerable{T}"/> of 
        /// <see cref="string"/>.
        /// </summary>
        /// <typeparam name="TBase">The type of item to process.</typeparam>
        /// <param name="item">A <typeparamref name="TBase"/> object.</param>
        /// <param name="properties">A <see cref="PropertyInfo"/> array.</param>
        /// <returns>An <see cref="IEnumerable{T}"/> of <see cref="string"/>.</returns>
        private static IEnumerable<string> AsStrings<TBase>(TBase item, PropertyInfo[] properties)
            where TBase : class
        {
            var results = new List<string>();

            foreach (var property in properties)
            {
                var value = property.GetValue(item)?.ToString();
                var text = GetText(property.PropertyType, value);
                results.Add(text);
            }

            return results;
        }

        /// <summary>
        /// Checks and returns whether the arguments of the specified <paramref name="method"/> can
        /// be assigned to from the specified <paramref name="types"/>, and whether its return type
        /// can be assigned to the specified <paramref name="returnType"/>.
        /// </summary>
        /// <param name="method">A <see cref="MethodInfo"/> containing the method to check.</param>
        /// <param name="types">A <see cref="Type"/> array.</param>
        /// <param name="returnType">A <see cref="Type"/>.</param>
        /// <returns>True if all checks pass; false otherwise.</returns>
        private static bool CheckArguments(MethodInfo method, Type[] types, Type returnType) =>
            CheckArgumentTypes(method.GetParameters(), types) &&
            method.ReturnType.IsAssignableFrom(returnType);

        /// <summary>
        /// Checks and returns whether the specified <paramref name="arguments"/> can be assigned 
        /// to from the specified <paramref name="types"/> and if their respective lengths are the
        /// same.
        /// </summary>
        /// <param name="arguments">A <see cref="ParameterInfo"/> array.</param>
        /// <param name="types">A <see cref="Type"/> array.</param>
        /// <returns>True if all checks pass; false otherwise.</returns>
        private static bool CheckArgumentTypes(ParameterInfo[] arguments, Type[] types) =>
            arguments.Length == types.Length &&
            Enumerable
                .Range(0, arguments.Length)
                .All(index => arguments[index].ParameterType.IsAssignableFrom(types[index]));

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
        /// <remarks>This routine calls its namesake by reflection.</remarks>
        private IEnumerable<string> ConvertFrom<TBase>(OptionType optionType, IEnumerable<TBase> values)
            where TBase : class => ConvertFrom<TBase>(optionType.Type, values);

        private IEnumerable<string> ConvertFrom<TBase>(Type type, IEnumerable<TBase> values)
            where TBase : class
        {
            if (!type.IsAssignableTo(typeof(TBase)))
                throw new ArgumentException(
                    $"Unable to assign an object of type {type.Name} to {typeof(TBase).Name}.");

            var name = nameof(ConvertFrom);

            var returnType = typeof(IEnumerable<>).MakeGenericType(typeof(string));

            // Filter the parameters to those just of the type we're currently working with.
            var parameters =
                GetParameters(values.Where(n => n?.GetType().Equals(type) ?? false))
                    .ToList();
            parameters.Add(type);

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
            var method = GetMethod<TBase>(name, typeof(TBase), returnType, types);
            var result =
                method.Invoke(this, parameters.ToArray()) ??
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
            where T : class, new()
        {
            var results = new List<string>();

            var properties =
                type.GetProperties()
                    .Where(property => property.CanRead)
                    .Where(property => property.CanWrite)
                    .Where(property => property.PropertyType == typeof(string) ||
                                       property.PropertyType.IsValueType &&
                                       property.PropertyType != typeof(DateTime))
                    .Select(property => new { Property = property, Depth = property.DeclaringType?.GetDepth() ?? 0, })
                    .OrderBy(n => n.Depth)
                    .Select(n => n.Property)
                    .ToArray();
            var parameters =
                properties
                    .Select(property =>
                        options.Assignments
                            .Where(assignment => assignment.Property.Name == property.Name)
                            .Select(assignment => assignment.Name)
                            .FirstOrDefault() ?? 
                        property.GetCustomAttribute<CsvConverterPropertyAttribute>()?.Name ??
                        property.Name)
                    .ToArray();

            var typeName = 
                type.GetCustomAttribute<CsvConverterClassAttribute>()?.Name ?? 
                type.Name;

            results.Add($"{options.PropertyPrefix},{typeName},{string.Join(",", parameters)}");
            foreach (var value in values)
            {
                var asStrings = AsStrings(value, properties);
                results.Add($"{options.ValuesPrefix},{typeName},{string.Join(",", asStrings)}");
            }

            return results;
        }

        /// <summary>
        /// Converts the specified <paramref name="lines"/> according to the <paramref name="optionType"/>
        /// into an <see cref="IEnumerable{T}"/> of <typeparamref name="TBase"/>.
        /// </summary>
        /// <typeparam name="TBase">The type of objects to return.</typeparam>
        /// <param name="optionType">An <see cref="OptionType"/> that controls the conversion.</param>
        /// <param name="lines">An <see cref="IEnumerable{T}"/> of <see cref="string"/> that contains the text to convert.</param>
        /// <returns>An <see cref="IEnumerable{T}"/> of <typeparamref name="TBase"/>.</returns>
        /// <exception cref="ArgumentException">If <seealso cref="OptionType.Type"/> cannot be assigned to <typeparamref name="TBase"/>.</exception>
        /// <exception cref="InvalidOperationException">If the conversion failed.</exception>
        /// <remarks>This routine calls its namesake by reflection.</remarks>
        private IEnumerable<TBase> ConvertTo<TBase>(OptionType optionType, IEnumerable<string> lines)
            where TBase : class => ConvertTo<TBase>(optionType.Type, lines);

        private IEnumerable<TBase> ConvertTo<TBase>(Type type, IEnumerable<string> lines)
            where TBase : class
        {
            if (!type.IsAssignableTo(typeof(TBase)))
                throw new ArgumentException(
                    $"Unable to assign an object of type {type.Name} to {typeof(TBase).Name}.");

            var name = nameof(ConvertTo);

            var returnType = typeof(IEnumerable<>).MakeGenericType(type);
            var parameters = GetParameters(lines);

            // name is the name of the method to retrieve.
            // type is the generic type parameter.
            // returnType is the type of the return parameter.
            // parameters are the values that will be passed as the method arguments.
            var method = GetMethod<TBase>(name, type, returnType, parameters);
            var result =
                method.Invoke(this, parameters) ??
                throw new InvalidOperationException(
                    $"Failed to convert to type {type.Name}.");

            return (IEnumerable<TBase>)result;
        }

        /// <summary>
        /// Converts the specified <paramref name="lines"/> into an <see cref="IEnumerable{T}"/> of
        /// <typeparamref name="T"/>.
        /// </summary>
        /// <typeparam name="T">The type of object to process.</typeparam>
        /// <param name="lines">An <see cref="IEnumerable{T}"/> of <see cref="string[]"/>.</param>
        /// <returns>An <see cref="IEnumerable{T}"/> of <typeparamref name="T"/>.</returns>
        /// <remarks>This routine is called by reflection.</remarks>
        private IEnumerable<T> ConvertTo<T>(IEnumerable<string> lines)
            where T : class, new()
        {
            var propertyAttributes = GetPropertyAttributes<T>();

            var typeName = 
                typeof(T).GetCustomAttribute<CsvConverterClassAttribute>()?.Name ?? 
                typeof(T).Name;

            // When spliting the lines into elements on comma the values lines must checked to see
            // if they need to have elements rejoined where if an item of double-quote delimited
            // text is split because it contains a comma. This is not needed on the parameter lines
            // as the parameters do not support double-quote delimited text.

            var names =
                lines
                    .Where(line => line.StartsWith(options.PropertyPrefix))
                    .Select(line => line.Split(','))
                    .Where(items => items[0].Trim() == options.PropertyPrefix)
                    .Where(items => items[1].Trim() == typeName)
                    .Select(items => items[2..^0])
                    .FirstOrDefault();
            if (names is null)
                throw new Exception($"Failed to identify property names for {typeName}.");

            var values =
                lines
                    .Where(line => line.StartsWith(options.ValuesPrefix))
                    .Select(line => line.Split(','))
                    .Select(items => RejoinSplitQuotes(items))
                    .Where(items => items[0].Trim() == options.ValuesPrefix)
                    .Where(items => items[1].Trim() == typeName)
                    .Select(items => items[2..^0]);

            foreach (var value in values)
                yield return Create<T>(propertyAttributes, names, value);
        }

        /// <summary>
        /// Convert the specified <paramref name="value"/> into an <see cref="object"/> according 
        /// to the specified <paramref name="type"/>.
        /// </summary>
        /// <param name="value">A <see cref="string"/> containing the value to convert.</param>
        /// <param name="type">The <see cref="Type"/> to convert the value into.</param>
        /// <returns>A boxed value of <paramref name="type"/>.</returns>
        /// <exception cref="ArgumentException">
        /// If the <paramref name="type"/> is not either a <see cref="string"/>, an enum or a value 
        /// type, excluding <see cref="DateTime"/>.
        /// </exception>
        /// <remarks>
        /// <seealso cref="GetText(Type, string?)"/> does the reverse.
        /// </remarks>
        private static object ConvertValue(string value, Type type)
        {
            if (type == typeof(string))
                return value.Trim().Trim('"');
            if (!type.IsValueType || type == typeof(DateTime))
                throw new ArgumentException("Must be either a string, enum or numeric type.", nameof(type));

            if (type == typeof(bool))
                return bool.Parse(value);
            if (type.IsEnum)
                return Enum.Parse(type, value, true);

            return Convert.ChangeType(double.Parse(value), type);
        }

        /// <summary>
        /// Creates and returns a new <typeparamref name="TBase"/> setting the value of the 
        /// properties according to the specified <paramref name="properties"/>, <paramref name="names"/>
        /// and <paramref name="values"/>.
        /// </summary>
        /// <typeparam name="TBase">The type of object to create.</typeparam>
        /// <param name="propertyAttributes">A <see cref="List{T}"/> of <see cref="PropertyInfo"/> and <see cref="CsvConverterPropertyAttribute"/> pairs.</param>
        /// <param name="names">A <see cref="string"/> array containing the names of the properties.</param>
        /// <param name="values">A <see cref="string"/> array containing the values to assign to the properties.</param>
        /// <returns>A <typeparamref name="TBase"/> instance.</returns>
        /// <remarks>
        /// The length of the <paramref name="names"/> and <paramref name="values"/> arrays must
        /// be the same, and their order must match.
        /// </remarks>
        private TBase Create<TBase>(List<(PropertyInfo Property, CsvConverterPropertyAttribute? Attribute)> propertyAttributes,
                                    string[] names,
                                    string[] values)
            where TBase : class, new()
        {
            if (values.Length != names.Length)
                throw new ArgumentException(
                    $"The length of {nameof(names)} ({string.Join("|", names)}) and {nameof(values)} ({string.Join("|", values)}) do not match.");

            var result = new TBase();
            for (var index = 0; index < values.Length; index++)
            {
                var property = GetProperty(this.options.Assignments, propertyAttributes, names[index]);

                // Don't assign ConvertValue to a variable, instead pass its result directly to
                // the SetValue method, we don't want to resolve it if the property is null.

                property?.SetValue(result, ConvertValue(values[index], property.PropertyType));
            }
            return result;
        }

        /// <summary>
        /// Retrieves an instance non-public generic method with a generic type parameter of
        /// <typeparamref name="TParam"/>, has a name of <paramref name="name"/> and accepts the 
        /// specified <paramref name="arguments"/> to process objects of type <paramref name="type"/>.
        /// </summary>
        /// <typeparam name="TParam">The generic type parameter to apply to the method.</typeparam>
        /// <typeparam name="TReturn">The type of the return value of the method.</typeparam>
        /// <param name="name">A <see cref="string"/> that contains the name of the method.</param>
        /// <param name="type">A <see cref="Type"/> that indicates the type of object that the method will handle.</param>
        /// <param name="arguments">An <see cref="object"/> array that contains the arguments to supply to the method.</param>
        /// <returns>A <see cref="MethodInfo"/>.</returns>
        private MethodInfo GetMethod<TParam>(string name, Type type, Type returnType, object[] arguments) =>
            GetMethod<TParam>(name, type, returnType, arguments.Select(argument => argument.GetType()).ToArray());

        /// <summary>
        /// Retrieves an instance non-public generic method with a generic type parameter of
        /// <typeparamref name="TParam"/>, has a name of <paramref name="name"/> and accepts the 
        /// arguments of the specified <paramref name="types"/> to process objects of type 
        /// <paramref name="type"/>.
        /// </summary>
        /// <typeparam name="TParam">The generic type parameter to apply to the method.</typeparam>
        /// <typeparam name="TReturn">The type of the return value of the method.</typeparam>
        /// <param name="name">A <see cref="string"/> that contains the name of the method.</param>
        /// <param name="type">A <see cref="Type"/> that indicates the type of object that the method will handle.</param>
        /// <param name="types">A <see cref="Type"/> array that contains the types of arguments to supply to the method.</param>
        /// <returns>A <see cref="MethodInfo"/>.</returns>
        /// <exception cref="InvalidOperationException">If the a matcing method is not found.</exception>
        private MethodInfo GetMethod<TParam>(string name, Type type, Type returnType, Type[] types)
        {
            // We need to check the method parameters AFTER constructing the generic method to be 
            // able to check parameter types, otherwise any parameters based on the type-parameter
            // won't have been constructed themselves. Because constructing a generic method is
            // fairly heavy, we check the number of arguments match the number of types before
            // doing the construction so that we can filter out any methods with the wrong number
            // of parameters.
            var method =
                GetType().GetMethods(BindingFlags.Instance |
                                     BindingFlags.NonPublic)
                    .Where(method => method.Name == name)
                    .Where(method => method.IsGenericMethod)
                    .Where(method => method.GetParameters().Length == types.Length)
                    .Select(method => method.MakeGenericMethod(new Type[] { type, }))
                    .Where(method => CheckArguments(method, types, returnType))
                    .SingleOrDefault();

            return 
                method ??
                throw new InvalidOperationException(
                    $"Failed to bind {name}<{type.Name}>({string.Join(", ", types.Select(t => t.GetName()))}) : {returnType.GetName()}.");
        }

        /// <summary>
        /// Constructs and returns an <see cref="object"/> array containing the specified 
        /// <paramref name="parameters"/>.
        /// </summary>
        /// <param name="parameters">A params <see cref="object"/> array.</param>
        /// <returns>An <see cref="object"/> array.</returns>
        private static object[] GetParameters(params object[] parameters) => parameters;

        /// <summary>
        /// Attempts to identify and return a <see cref="PropertyInfo"/> associated with the 
        /// specified <paramref name="name"/> using the specified <paramref name="propertyAttributes"/>.
        /// </summary>
        /// <param name="assignments"></param>
        /// <param name="propertyAttributes">A <see cref="List{T}"/> of <see cref="PropertyInfo"/> and <see cref="CsvConverterPropertyAttribute"/> pairs.</param>
        /// <returns>A <see cref="PropertyInfo"/> or null if none match.</returns>
        /// <remarks>
        /// Attempt to find the property in this order:
        /// <list type="number">
        /// <item>by option assignment; this overrides all others.</item>
        /// <item>by attribute name.</item>
        /// <item>by property name.</item>
        /// </list>
        /// otherwise ignore it.
        /// In all instances the name must match exactly, including case.
        /// </remarks>
        /// <param name="name">A <see cref="string"/> containing the name.</param>
        private static PropertyInfo? GetProperty(Assignment[] assignments,
                                                 List<(PropertyInfo Property, CsvConverterPropertyAttribute? Attribute)> propertyAttributes,
                                                 string name) =>
            assignments
                .Where(assignment => assignment.Name == name)
                .Select(assignment => assignment.Property)
                .FirstOrDefault() ??
            propertyAttributes
                .Where(n => n.Attribute?.Name == name)
                .Select(n => n.Property)
                .FirstOrDefault() ??
            propertyAttributes
                .Where(n => n.Property.Name == name)
                .Select(n => n.Property)
                .FirstOrDefault();

        /// <summary>
        /// Constructs and returns a <see cref="List{T}"/> of pairs of <see cref="PropertyInfo"/> 
        /// and the corresponding <see cref="CsvConverterPropertyAttribute"/> for the <typeparamref name="TBase"/>.
        /// </summary>
        /// <typeparam name="TBase">The <see cref="Type"/> to retrieve for.</typeparam>
        /// <returns>A <see cref="List{T}"/> of <see cref="PropertyInfo"/> and <see cref="CsvConverterPropertyAttribute"/> pairs.</returns>
        private static List<(PropertyInfo Property, CsvConverterPropertyAttribute? Attribute)> GetPropertyAttributes<TBase>()
            where TBase : class =>
                typeof(TBase).GetProperties()
                    .Select(property => (
                        Property: property,
                        Attribute: property.GetCustomAttribute<CsvConverterPropertyAttribute>(true)))
                    .ToList();

        /// <summary>
        /// Retrieves the <seealso cref="MemberInfo.Name"/> of the properties of the <typeparamref name="T"/>.
        /// </summary>
        /// <typeparam name="T">The type of object to retreive the property names for.</typeparam>
        /// <returns>A <see cref="string"/> array.</returns>
        private static string[] GetPropertyNames<T>() where T : class =>
            typeof(T)
                .GetProperties(BindingFlags.Instance | BindingFlags.Public)
                .Select(property => property.Name)
                .OrderBy(name => name)
                .ToArray();

        /// <summary>
        /// Returns a <see cref="string"/> containing the textual representation of the specified
        /// <paramref name="value"/> according to the specified <paramref name="type"/>.
        /// </summary>
        /// <param name="type">The <see cref="Type"/> of the value.</param>
        /// <param name="value">A <see cref="string"/> containing the raw value.</param>
        /// <returns>A <see cref="string"/>.</returns>
        /// <remarks>
        /// <seealso cref="ConvertValue(string, Type)"/> does the reverse.
        /// </remarks>
        private static string GetText(Type type, string? value)
        {
            if (type == typeof(string))
                return $"\"{value ?? string.Empty}\"";

            if (type == typeof(bool))
                return value ?? false.ToString();

            if (type.IsEnum)
                // value is null or empty return empty
                // value is not defined for enum return empty
                // return name, if null return empty.
                return
                    string.IsNullOrEmpty(value) ? string.Empty :
                    !Enum.IsDefined(type, value) ? string.Empty :
                    Enum.GetName(type, value) ?? string.Empty;

            if (type.IsValueType)
                return value ?? 0.ToString();

            return string.Empty;
        }

        /// <summary>
        /// Recombines adjacent elements where a quote delimited string has been split on a comma.
        /// </summary>
        /// <param name="elements">A <see cref="string"/> array containing the elements to check.</param>
        /// <returns>A <see cref="string"/> array.</returns>
        private static string[] RejoinSplitQuotes(string[] elements)
        {
            // Use a list as it's easier to remove elements.
            var list = new List<string>(elements);

            // Start the index at one, check the previous element and if needed combine the
            // previous and current elements. This makes the end of bounds check easier.
            var index = 1;
            while (index < list.Count)
            {
                // We need to trim the elements before testing for leading / trailing double-quotes
                // as they still retain any leading or trailing spaces at this point. This is
                // important as there may be spaces that must be retained within the quoted text.
                // For example, "There is a comma, contained in here" would be split into two (the
                // square brackets [] are used to delimit the text for easy reading):
                //  ["There is a comma]
                //  [ contained in here"]
                // If the leading space were trimmed from the second part then they would be
                // recombined thus: "There is a comma,contained in here"; note the missing space.
                if (list[index - 1].Trim().StartsWith('"') &&
                    !list[index - 1].Trim().EndsWith('"'))
                {
                    list[index] = $"{list[index - 1]},{list[index]}";
                    list.RemoveAt(index);
                }
                else
                    // Only increment the index if the original element didn't fail the check.
                    // Otherwise re-check it after it's been recombined with the following index.
                    index++;
            }

            // If the number of elements hasn't changed then return the original array.
            return elements.Length == list.Count ? elements : list.ToArray();
        }

        #endregion
    }
}

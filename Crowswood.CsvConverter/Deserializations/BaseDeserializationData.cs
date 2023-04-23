using System.Reflection;
using Crowswood.CsvConverter.Extensions;
using Crowswood.CsvConverter.Model;
using static Crowswood.CsvConverter.Deserialization;

namespace Crowswood.CsvConverter.Deserializations
{
    /// <summary>
    /// Base abstract class for all deserialization data objects.
    /// </summary>
    internal abstract class BaseDeserializationData
    {
        protected internal readonly DeserializationFactory factory;

        protected BaseDeserializationData(DeserializationFactory factory) => this.factory = factory;

        /// <summary>
        /// Deserializes the data held in the current instance.
        /// </summary>
        public abstract void Deserialize();

        /// <summary>
        /// Gets the items from the <seealso cref="DeserializationFactory.Lines"/> for the 
        /// specified <paramref name="typeName"/> and any of the specified <paramref name="prefixes"/>.
        /// </summary>
        /// <param name="typeName">A <see cref="string"/> containing the name of the data type.</param>
        /// <param name="prefixes">A <see cref="string[]"/> containing the prefixes.</param>
        /// <returns>An <see cref="IEnumerable{T}"/> of <see cref="string[]"/>.</returns>
        protected IEnumerable<string[]> GetItems(string? typeName, params string[] prefixes) =>
            this.factory.Lines.GetItems(typeName, prefixes);

        /// <summary>
        /// Converts the specified <paramref name="textValue"/> according to the rules that apply 
        /// to the specified <paramref name="targetType"/> and return an instance of that type.
        /// </summary>
        /// <param name="textValue">A <see cref="string"/> containing the value to be converted.</param>
        /// <param name="targetType">The <see cref="Type"/> of the expected result.</param>
        /// <returns>A nullable <see cref="object"/> that contains the converted value, or null.</returns>
        /// <exception cref="ArgumentException">If <paramref name="targetType"/> is not a supported type.</exception>
        protected static object? ConvertValue(string textValue, Type targetType)
        {
            if (targetType == typeof(string))
                // Remove any white space surrounding the value, then remove the double-quotes if
                // any, finally remove any white space that was within the double-quotes.
                return textValue.Trim().Trim('"').Trim();

            if (!targetType.IsValueType || targetType == typeof(DateTime))
                throw new ArgumentException(
                    $"Must be either a string, bool, enum, or numeric type: {targetType.Name}.",
                    nameof(targetType));

            if (targetType.IsGenericType)
            {
                var genericArguments = targetType.GetGenericArguments();
                if (genericArguments.Length == 1)
                {
                    var nullableType = typeof(Nullable<>).MakeGenericType(genericArguments[0]);
                    if (targetType == nullableType)
                        targetType = genericArguments[0];
                }
            }

            if (targetType == typeof(bool))
            {
                if (string.IsNullOrWhiteSpace(textValue))
                    return null;
                return bool.Parse(textValue);
            }

            if (targetType.IsEnum)
            {
                textValue = textValue.Trim().Trim('"').Trim();
                if (textValue.StartsWith(targetType.Name + '.'))
                    textValue = textValue[(targetType.Name.Length + 1)..].Trim();
                if (!Enum.IsDefined(targetType, textValue))
                {
                    var enumValues = Enum.GetValues(targetType);
                    return (enumValues?.Length ?? 0) == 0 ? 0 : enumValues?.GetValue(0) ?? 0;
                }
                return Enum.Parse(targetType, textValue, true);
            }

            if (double.TryParse(textValue, out var numericValue))
                return Convert.ChangeType(numericValue, targetType);

            return null;
        }

        /// <summary>
        /// Gets the properties that have the specified <paramref name="propertyNames"/> from the 
        /// specified <paramref name="optionMembers"/> and <paramref name="propertyAndNamePairs"/>.
        /// </summary>
        /// <param name="propertyNames">A <see cref="string[]"/> containing the names of the required properties.</param>
        /// <param name="optionMembers">An <see cref="OptionMember[]"/> containing the members of the object as defined in <see cref="Options"/>.</param>
        /// <param name="propertyAndNamePairs">An <see cref="IEnumerable{T}"/> of <see cref="PropertyAndNamePair"/> containing the Property and the name alias of the object.</param>
        /// <returns>A <see cref="PropertyInfo?[]"/> in the order defined by <paramref name="propertyNames"/>.</returns>
        protected static PropertyInfo?[] GetProperties(string[] propertyNames, OptionMember[] optionMembers, IEnumerable<PropertyAndNamePair> propertyAndNamePairs) =>
            propertyNames
                .Select(propertyName =>
                    optionMembers.GetProperty(propertyName) ??
                    propertyAndNamePairs.GetProperty(propertyName))
                .ToArray();

        /// <summary>
        /// Sets the properties on the specified <paramref name="obj"/> using the specified 
        /// <paramref name="properties"/> to determine which are to be set and the specified 
        /// <paramref name="values"/> for their value.
        /// </summary>
        /// <typeparam name="T">The <see cref="Type"/> of object.</typeparam>
        /// <param name="obj">The <typeparamref name="T"/> object.</param>
        /// <param name="properties">A <see cref="PropertyInfo?[]"/> containing the properties to set.</param>
        /// <param name="values">A <see cref="string[]"/> containing the values to use.</param>
        /// <returns>The <paramref name="obj"/>.</returns>
        /// <remarks>
        /// The order of <paramref name="properties"/> and <paramref name="values"/> must 
        /// correspond; that is properties[0] will use values[0], etc.. The length of the two 
        /// arrays can differ, but any elements beyond the shortest length are ignored.
        /// Any of the <paramref name="properties"/> can be null; this will prevent the 
        /// corresponding value from being assigned.
        /// </remarks>
        private static T SetProperties<T>(T obj, PropertyInfo?[] properties, string[] values)
            where T : class, new()
        {

            for (var index = 0; index < values.Length && index < properties.Length; index++)
                SetProperty(properties[index], values[index], obj);

            return obj;
        }

        /// <summary>
        /// Sets the specified <paramref name="property"/> using the specified <paramref name="value"/> 
        /// on the specified <paramref name="obj"/>.
        /// </summary>
        /// <param name="property">A <see cref="PropertyInfo"/> that identifies the property to set.</param>
        /// <param name="value">A <see cref="string"/> containing the value to set.</param>
        /// <param name="obj">An <see cref="object"/> that will have its property set.</param>
        /// <remarks>
        /// We only want to determine the new value if <paramref name="property"/> is not null, 
        /// so ONLY call <seealso cref="ConvertValue(string, Type)"/> within the call to 
        /// <seealso cref="PropertyInfo.SetValue(object?, object?)"/>. Also we know that 
        /// <paramref name="property"/> will be not-null when passed to <seealso cref="ConvertValue(string, Type)"/>.
        /// </remarks>
        private static void SetProperty(PropertyInfo? property, string value, object? obj) => 
            property?.SetValue(obj, ConvertValue(value, property!.PropertyType));

        /// <summary>
        /// Sets the values on an instance of <paramref name="type"/> using the specified 
        /// <paramref name="propertyNames"/>, <paramref name="values"/>, <paramref name="optionMembers"/>, 
        /// and <paramref name="propertyAndNamePairs"/>.
        /// </summary>
        /// <param name="type">The <see cref="Type"/> of object to create and set the value on.</param>
        /// <param name="propertyNames">A <see cref="string[]"/> containing the names of the properties to set.</param>
        /// <param name="values">A <see cref="string[]"/> containing the values to set.</param>
        /// <param name="optionMembers">An <see cref="OptionMember[]"/> containing option information about the membes of the object.</param>
        /// <param name="propertyAndNamePairs">An <see cref="IEnumerable{T}"/> of <see cref="PropertyAndNamePair"/> containing properties and name aliases for the object..</param>
        /// <returns>An <see cref="object"/> containing the created instance of <paramref name="type"/>.</returns>
        /// <exception cref="InvalidOperationException">If the creation of the instance of the <paramref name="type"/> failed.</exception>
        protected static object SetValues(Type type, string[] propertyNames, string[] values, OptionMember[] optionMembers, IEnumerable<PropertyAndNamePair> propertyAndNamePairs)
        {
            var properties = GetProperties(propertyNames, optionMembers, propertyAndNamePairs);

            var obj = 
                Activator.CreateInstance(type) ?? 
                throw new InvalidOperationException($"Failed to create instance of '{type.Name}'.");

            var result = SetProperties(obj, properties, values);

            return result;
        }
    }
}

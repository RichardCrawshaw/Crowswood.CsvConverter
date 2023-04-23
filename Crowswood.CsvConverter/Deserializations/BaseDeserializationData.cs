using System.Reflection;
using Crowswood.CsvConverter.Extensions;
using Crowswood.CsvConverter.Model;
using static Crowswood.CsvConverter.Deserialization;

namespace Crowswood.CsvConverter.Deserializations
{
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

        protected static PropertyInfo?[] GetProperties(string[] propertyNames, OptionMember[] optionMembers, IEnumerable<PropertyAndNamePair> propertyAndNamePairs) =>
            Enumerable
                .Range(0, propertyNames.Length)
                .Select(index =>
                    optionMembers.GetProperty(propertyNames[index]) ??
                    propertyAndNamePairs.GetProperty(propertyNames[index]))
                .ToArray();

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

        protected static T SetValues<T>(string[] propertyNames, string[] values, OptionMember[] optionMembers, IEnumerable<PropertyAndNamePair> propertyAndNamePairs)
            where T : class, new()
        {
            var properties = GetProperties(propertyNames, optionMembers, propertyAndNamePairs);

            var result = SetProperties<T>(new T(), properties, values);

            return result;
        }

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

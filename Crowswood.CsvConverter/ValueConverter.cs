using Crowswood.CsvConverter.Handlers;

namespace Crowswood.CsvConverter
{
    /// <summary>
    /// A converter class to handle the conversion of a string value to another data-type.
    /// </summary>
    public class ValueConverter
    {
        #region Fields

        private readonly IndexHandler indexHandler;
        private readonly string typeName;

        #endregion

        #region Constructors

        internal ValueConverter(IndexHandler indexHandler, string typeName)
        {
            this.indexHandler = indexHandler;
            this.typeName = typeName;
        }

        #endregion

        #region Methods

        /// <summary>
        /// Attempts to convert and return the specified <paramref name="textValue"/> into the 
        /// specified <paramref name="targetType"/>.
        /// </summary>
        /// <param name="textValue">A <see cref="string"/> containing the value to convert.</param>
        /// <param name="targetType">The <see cref="Type"/> to convert the value into.</param>
        /// <returns>An object that boxes the converted value; or null if the value could not be converted.</returns>
        /// <exception cref="ArgumentException">If the <paramref name="targetType"/> is not a value type or is a <see cref="DateTime"/>.</exception>
        public object? ConvertValue(string textValue, Type targetType)
        {
            if (targetType == typeof(string))
                // Remove any white space surrounding the value, then remove the double-quotes.
                return textValue.Trim().Trim('"').Trim();
            if (!targetType.IsValueType || targetType == typeof(DateTime))
                throw new ArgumentException("Must be either a string, enum or numeric type.",
                    nameof(targetType));

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

            var result = ConvertValueToNumeric(textValue, targetType);
            return result;
        }

        #endregion

        #region Support routines

        /// <summary>
        /// Converts the specified <paramref name="textValue"/> to the specified <paramref name="targetType"/>
        /// while handling automatic increments for the specified <paramref name="typeName"/>.
        /// </summary>
        /// <param name="textValue">A <see cref="string"/> containing the value to convert.</param>
        /// <param name="targetType">A <see cref="Type"/> that the <paramref name="textValue"/> is to be converted to.</param>
        /// <returns>An <see cref="object?"/> containing the boxed value, or null if the conversion couldn't be completed.</returns>
        private object? ConvertValueToNumeric(string textValue, Type targetType)
        {
            if (textValue == "#")
            {
                if (this.indexHandler.Contains(this.typeName))
                    return indexHandler.Get(this.typeName);
            }
            else if (double.TryParse(textValue, out var numericValue))
                return Convert.ChangeType(numericValue, targetType);

            return null;
        }

        #endregion
    }
}

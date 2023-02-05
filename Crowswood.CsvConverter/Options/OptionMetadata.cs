using System.Reflection;
using Crowswood.CsvConverter.Extensions;

namespace Crowswood.CsvConverter
{
    /// <summary>
    /// An abstract class that contains information about the metadata that has been configured
    /// to be expected.
    /// </summary>
    public abstract class OptionMetadata
    {
        #region Properties

        /// <summary>
        /// Gets the prefix used within the CSV data to indicate the metadata record.
        /// </summary>
        public string Prefix { get; }
        
        /// <summary>
        /// Gets the <see cref="string[]"/> containing the property names.
        /// </summary>
        public string[] PropertyNames { get; }

        /// <summary>
        /// Gets the <see cref="Type"/> of object to create.
        /// </summary>
        public abstract Type Type { get; }

        #endregion

        #region Constructors

        protected OptionMetadata(string prefix, params string[] propertyNames)
        {
            this.Prefix = prefix;
            this.PropertyNames = propertyNames;
        }

        #endregion

        #region Static factory creation methods

        /// <summary>
        /// Static factory method to create and return an instance of <see cref="OptionMetadata{T}"/>
        /// for the specified <paramref name="prefix"/> and <paramref name="propertyNames"/>.
        /// </summary>
        /// <typeparam name="T">The <see cref="Type"/> that this instance handles.</typeparam>
        /// <param name="prefix">A <see cref="string"/> that contains the prefix.</param>
        /// <param name="propertyNames">A <see cref="string[]"/> that contains the property names.</param>
        /// <returns>An <see cref="OptionMetadata{T}"/> instance.</returns>
        /// <remarks>
        /// The <paramref name="propertyNames"/> must match the name of the properties of 
        /// <typeparamref name="T"/>.
        /// </remarks>
        public static OptionMetadata<T> Create<T>(string prefix, params string[] propertyNames)
            where T : class, new() =>
            propertyNames.All(name => typeof(T).GetProperty(name) != null)
                ? new(prefix, propertyNames)
                : throw new ArgumentException(
                    $"The {nameof(propertyNames)} do not match the properties of '{typeof(T).Name}.");

        /// <summary>
        /// Static factory method to create and return an instance of <see cref="OptionMetadata"/> 
        /// for <paramref name="genericType"/> with the specified <paramref name="prefix"/> and 
        /// <paramref name="propertyNames"/>.
        /// </summary>
        /// <param name="genericType">The <see cref="Type"/> that this instance handles.</param>
        /// <param name="prefix">A <see cref="string"/> that contains the prefix.</param>
        /// <param name="propertyNames">A <see cref="string[]"/> that contains the property names.</param>
        /// <returns>An <see cref="OptionMetadata"/> instance.</returns>
        /// <exception cref="ArgumentException">
        /// If <paramref name="genericType"/> is not a class with a parameterless constructor.
        /// or If <paramref name="prefix"/>is null, empty or only white space.
        /// or If <paramref name="propertyNames"/> is an empty array.
        /// </exception>
        /// <exception cref="InvalidOperationException">If the create method for <see cref="OptionMetadata{T}"/> cannot be bound to.</exception>
        /// <remarks>
        /// Uses reflection to find and invoke the generic <seealso cref="Create{T}(string, string[])"/>
        /// method.
        /// </remarks>
        public static OptionMetadata Create(Type genericType, string prefix, params string[] propertyNames)
        {
            if (!genericType.IsClass || genericType.GetConstructor(Array.Empty<Type>()) is null)
                throw new ArgumentException(
                    $"'{genericType.Name}' must be a reference type with a parameterless constructor.",
                    nameof(genericType));
            if (string.IsNullOrWhiteSpace(prefix))
                throw new ArgumentException(
                    $"'{nameof(prefix)}' must be non-null, non-empty and non-whitespace.",
                    nameof(prefix));
            if (propertyNames.Length == 0)
                throw new ArgumentException(
                    $"'{nameof(propertyNames)}' must not be an empty array.",
                    nameof(propertyNames));

            var myType = typeof(OptionMetadata);
            const string methodName = "Create";

            var parameters =
                new object[]
                {
                    prefix,
                    propertyNames,
                };
            var argumentTypes =
                parameters
                    .Select(parameter => parameter.GetType())
                    .ToArray();
            var returnType = myType.MakeGenericType(genericType);

            var method =
                myType.GetGenericMethod(methodName,
                                        BindingFlags.Public |
                                        BindingFlags.Static,
                                        genericType, argumentTypes, returnType);
            var result = (OptionMetadata?)
                method?.Invoke(null, parameters) ??
                throw new InvalidOperationException($"Failed to bind to '{methodName}' for '{genericType.Name}'.");
            return result;
        }

        #endregion

        #region Methods

        /// <summary>
        /// Create an instance of the generic type.
        /// </summary>
        /// <returns></returns>
        public abstract object CreateInstance();

        #endregion
    }

    /// <summary>
    /// A sealed generic class that contains information about a specific type of metadata that has
    /// been configured.
    /// </summary>
    /// <typeparam name="T">The type of object to be created.</typeparam>
    public sealed class OptionMetadata<T> : OptionMetadata
        where T : class, new()
    {

        #region Properties

        /// <inheritdoc/>
        public override Type Type => typeof(T);

        #endregion

        #region Constructors

        public OptionMetadata(string prefix, params string[] propertyNames)
            : base(prefix, propertyNames) { }

        #endregion

        #region Methods

        /// <inheritdoc/>
        public override object CreateInstance() => new T();

        #endregion
    }
}

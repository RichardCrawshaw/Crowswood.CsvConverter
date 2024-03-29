﻿namespace Crowswood.CsvConverter
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
    /// <typeparam name="TMetadata">The type of the metadata.</typeparam>
    public sealed class OptionTypedMetadata<TMetadata> : OptionMetadata
        where TMetadata : class, new()
    {
        #region Properties

        /// <inheritdoc/>
        public override Type Type => typeof(TMetadata);

        #endregion

        #region Constructors

        public OptionTypedMetadata(string prefix, params string[] propertyNames)
            : base(prefix, propertyNames) { }

        #endregion

        #region Methods

        /// <inheritdoc/>
        public override object CreateInstance() => new TMetadata();

        #endregion
    }

    /// <summary>
    /// A sealed class that indicates that the metadata is typeless and contains information about 
    /// a specific type of metadata that has been configured; the value may be nullable depending
    /// on <seealso cref="AllowNulls"/>.
    /// </summary>
    public sealed class OptionTypelessMetadata : OptionMetadata
    {
        #region Properties

        /// <summary>
        /// Indicates whether the dictionary should support a nullable value or not.
        /// </summary>
        public bool AllowNulls { get; init; } = false;

        /// <inheritdoc/>
        public override Type Type =>
            this.AllowNulls
            ? typeof(Dictionary<string, string?>)
            : typeof(Dictionary<string, string>);

        #endregion

        #region Constructors

        public OptionTypelessMetadata(string prefix, params string[] propertyNames)
            : base(prefix, propertyNames) { }

        #endregion

        #region Methods

        /// <inheritdoc/>
        public override object CreateInstance() =>
            this.AllowNulls
            ? (object)new Dictionary<string, string?>()
            : (object)new Dictionary<string, string>();

        #endregion
    }
}

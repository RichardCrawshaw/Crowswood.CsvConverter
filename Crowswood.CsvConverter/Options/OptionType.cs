using System.Reflection;

namespace Crowswood.CsvConverter
{
    /// <summary>
    /// An abstract base class that allows a type to be added to an <see cref="options"/>
    /// instance.
    /// </summary>
    public abstract class OptionType
    {
        #region Properties

        /// <summary>
        /// Gets the <see cref="Type"/> that indicates the type of object the current instance
        /// is for.
        /// </summary>
        public abstract Type Type { get; }

        /// <summary>
        /// Gets a <see cref="string"/> that contains the name of the type that is used in the 
        /// CSV data.
        /// </summary>
        public string Name { get; }

        #endregion

        #region Constructors

        protected OptionType() => this.Name = this.Type.Name;

        protected OptionType(string name) : this() => this.Name = name;

        #endregion
    }

    /// <summary>
    /// A generic class that derives from <see cref="OptionType"/>. It allows the specification
    /// of the actual type.
    /// </summary>
    /// <typeparam name="T">The Type of data that this instance handles.</typeparam>
    public sealed class OptionType<T> : OptionType
        where T : class, new()
    {
        #region Properties

        /// <inheritdoc/>
        public override Type Type => typeof(T);

        #endregion

        #region Constructors

        /// <summary>
        /// Creates an instance of the <see cref="OptionType{T}"/> that uses the <seealso cref="MemberInfo.Name"/>
        /// of the <seealso cref="Type"/> in the CSV data.
        /// </summary>
        public OptionType() : base() { }

        /// <summary>
        /// Creates an instance of the <see cref="OptionType{T}"/> that uses <paramref name="name"/>
        /// for the <seealso cref="Type"/> in the CSV data.
        /// </summary>
        /// <param name="name">A <see cref="string"/> that contains the name of the data-type used within the CSV data.</param>
        public OptionType(string name) : base(name) { }

        #endregion
    }

    /// <summary>
    /// A non-generic class that derives from <see cref="OptionType"/>. It indicates that the 
    /// <see cref="Type"/> is 'dynamic', that is there is no associated entity type.
    /// </summary>
    public sealed class OptionDynamicType : OptionType
    {
        #region Properties

        /// <inheritdoc/>
        /// <remarks>
        /// <c>null</c> indicates dynamci type.
        /// </remarks>
        public override Type Type => typeof(Type);

        /// <summary>
        /// Gets a <see cref="string[]"/> containing the names of the properties.
        /// </summary>
        public string[] PropertyNames { get; }

        #endregion

        #region Constructors

        public OptionDynamicType(string name, string propertyName, string[] propertyNames)
            : base(name)
        {
            var list = new List<string>
            {
                propertyName,
            };
            list.AddRange(propertyNames);
            this.PropertyNames = list.ToArray();
        }

        #endregion
    }
}

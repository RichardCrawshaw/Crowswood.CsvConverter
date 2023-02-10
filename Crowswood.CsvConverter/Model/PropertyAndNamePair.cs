using System.Reflection;

namespace Crowswood.CsvConverter.Model
{
    /// <summary>
    /// A helper model class to hold a pair of <see cref="PropertyInfo"/> and a <see cref="string?"/> .
    /// </summary>
    public class PropertyAndNamePair : Tuple<PropertyInfo, string>
    {
        #region Properties

        /// <summary>
        /// Gets the <see cref="PropertyInfo"/>.
        /// </summary>
        public PropertyInfo Property => this.Item1;

        /// <summary>
        /// Gets the <see cref="string"/> containing the name.
        /// </summary>
        public string Name => this.Item2;

        #endregion

        #region Constructors

        /// <summary>
        /// Creates a new instance from a <see cref="PropertyInfo"/> and a <see cref="string"/>.
        /// </summary>
        /// <param name="property">A <see cref="PropertyInfo"/> object.</param>
        /// <param name="name">A <see cref="string"/> containing the name.</param>
        public PropertyAndNamePair(PropertyInfo property, string name)
            : base(property, name) { }

        /// <summary>
        /// Creates a new instance from a <see cref="PropertyAndAttributePair"/>.
        /// </summary>
        /// <param name="propertyAndAttributePair">A <see cref="PropertyAndAttributePair"/> object.</param>
        public PropertyAndNamePair(PropertyAndAttributePair propertyAndAttributePair)
            : this(propertyAndAttributePair.Property, propertyAndAttributePair.Attribute?.Name ?? propertyAndAttributePair.Property.Name) { }

        #endregion
    }
}

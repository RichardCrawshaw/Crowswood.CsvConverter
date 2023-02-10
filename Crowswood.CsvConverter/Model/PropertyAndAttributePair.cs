using System.Reflection;

namespace Crowswood.CsvConverter.Model
{
    /// <summary>
    /// A helper model class to hold a pair of <see cref="PropertyInfo"/> and 
    /// <see cref="CsvConverterPropertyAttribute?"/>.
    /// </summary>
    public class PropertyAndAttributePair : Tuple<PropertyInfo, CsvConverterPropertyAttribute?>
    {
        #region Properties

        /// <summary>
        /// Gets the <see cref="PropertyInfo"/>.
        /// </summary>
        public PropertyInfo Property => this.Item1;

        /// <summary>
        /// Gets the <see cref="CsvConverterPropertyAttribute"/>.
        /// </summary>
        public CsvConverterPropertyAttribute? Attribute => this.Item2;

        #endregion

        #region Constructors

        /// <summary>
        /// Creates a new instance from a <see cref="PropertyInfo"/> object.
        /// </summary>
        /// <param name="property">A <see cref="PropertyInfo"/> object.</param>
        public PropertyAndAttributePair(PropertyInfo property)
            : base(property, property.GetCustomAttribute<CsvConverterPropertyAttribute>(true)) { }

        #endregion
    }
}

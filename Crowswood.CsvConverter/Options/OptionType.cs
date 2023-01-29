using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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

        #endregion
    }

    /// <summary>
    /// A generic class that derives from <see cref="OptionType"/>. It allows the specification
    /// of the actual type.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public sealed class OptionType<T> : OptionType
        where T : class, new()
    {
        #region Properties

        /// <inheritdoc/>
        public override Type Type => typeof(T);

        #endregion
    }
}

using System.Reflection;

namespace Crowswood.CsvConverter
{
    /// <summary>
    /// An abstract base class that allows the details of an assignment to be added to an
    /// <see cref="Options"/> instance. This allows rules to be set that define how a property
    /// of a serialized or deserialized object are handled.
    /// </summary>
    public abstract class Assignment
    {
        #region Properties

        /// <summary>
        /// Gets the <see cref="string"/> that contains the name associated with the property.
        /// </summary>
        /// <remarks>This will be different to the name of the property.</remarks>
        public string Name { get; }

        /// <summary>
        /// Gets the <see cref="PropertyInfo"/> that contains the property the current
        /// instance refers to.
        /// </summary>
        public abstract PropertyInfo Property { get; }

        /// <summary>
        /// Gets the <see cref="Type"/> that indicates the type of object the current instance
        /// is for.
        /// </summary>
        public abstract Type Type { get; }

        #endregion

        #region Constructors

        /// <summary>
        /// Creates a new <see cref="Assignment"/> instance with the specified <paramref name="name"/>.
        /// </summary>
        /// <param name="name">A <see cref="string"/> that contains the name.</param>
        protected Assignment(string name) => Name = name;

        #endregion
    }

    public sealed class Assignment<T> : Assignment
        where T : class, new()
    {
        #region Properties

        /// <inheritdoc/>
        public override PropertyInfo Property { get; }

        /// <inheritdoc/>
        public override Type Type => typeof(T);

        #endregion

        #region Constructors

        /// <summary>
        /// Creates a new <see cref="Assignment"/> instance with the specified <paramref name="property"/>
        /// and <paramref name="name"/>.
        /// </summary>
        /// <param name="property">A <see cref="PropertyInfo"/> that contains the Property of <typeparamref name="T"/>.</param>
        /// <param name="name">A <see cref="string"/> that contains the name.</param>
        public Assignment(PropertyInfo property, string name)
            : base(name) => Property = property;

        #endregion
    }
}

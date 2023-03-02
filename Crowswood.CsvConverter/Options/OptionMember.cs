using System.Linq.Expressions;
using System.Reflection;

namespace Crowswood.CsvConverter
{
    /// <summary>
    /// An abstract base class that allows the details of an option for a member to be added to an
    /// <see cref="Options"/> instance. This allows rules to be set that define how a property
    /// of a serialized or deserialized object are handled.
    /// </summary>
    public abstract class OptionMember
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
        /// Creates a new <see cref="OptionMember"/> instance with the specified <paramref name="name"/>.
        /// </summary>
        /// <param name="name">A <see cref="string"/> that contains the name.</param>
        protected OptionMember(string name) => this.Name = name;

        #endregion
    }

    /// <summary>
    /// A sealed generic class that specifies both the type of the object and the type of the 
    /// property to be spedified.
    /// </summary>
    /// <typeparam name="TObject">The <see cref="Type"/> of the object.</typeparam>
    /// <typeparam name="TMember">The <see cref="Type"/> of the member.</typeparam>
    public sealed class OptionMember<TObject, TMember> : OptionMember
        where TObject : class, new()
    {
        #region Properties

        /// <inheritdoc/>
        public override PropertyInfo Property { get; }

        /// <inheritdoc/>
        public override Type Type => typeof(TObject);

        #endregion

        #region Constructors

        /// <summary>
        /// Creates a new <see cref="OptionMember{TObject, TMember}"/> instance with the specified
        /// <paramref name="property"/> and <paramref name="name"/>.
        /// </summary>
        /// <param name="property">A <see cref="PropertyInfo"/>,</param>
        /// <param name="name">A <see cref="string"/> that contains the name.</param>
        public OptionMember(PropertyInfo property, string name)
            : base(name) => this.Property = property;

        /// <summary>
        /// Creates a new <see cref="OptionMember{TObject, TMember}"/> instance with the specified
        /// <paramref name="expression"/> and <paramref name="name"/>.
        /// </summary>
        /// <param name="expression">An <see cref="Expression{TDelegate}"/> of <see cref="Func{T, TResult}"/> that takes a <typeparamref name="TObject"/> and returns a <typeparamref name="TMember"/>.</param>
        /// <param name="name">A <see cref="string"/> that contains the name.</param>
        public OptionMember(Expression<Func<TObject, TMember>> expression, string name  )
            : base(name) => this.Property = GetProperty(expression);

        #endregion

        #region Support routines

        /// <summary>
        /// Identifies and returns a <see cref="PropertyInfo"/> from the specified 
        /// <paramref name="lambdaExpression"/>.
        /// </summary>
        /// <param name="lambdaExpression">A <see cref="LambdaExpression"/>.</param>
        /// <returns>A <see cref="PropertyInfo"/>.</returns>
        /// <exception cref="ArgumentException">If the <paramref name="lambdaExpression"/> does not resolve to a <see cref="PropertyInfo"/>.</exception>
        private static PropertyInfo GetProperty(LambdaExpression lambdaExpression) =>
            lambdaExpression.Body switch
            {
                MemberExpression
                {
                    Member: var member,
                    Expression.NodeType: ExpressionType.Parameter,
                } => (PropertyInfo)member,
                _ => throw new ArgumentException(
                        $"Expression '{lambdaExpression}' must resolve to a property.",
                        nameof(lambdaExpression))
            };

        #endregion
    }
}

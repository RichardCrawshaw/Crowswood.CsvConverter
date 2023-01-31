using System.Linq.Expressions;

namespace Crowswood.CsvConverter
{
    /// <summary>
    /// An options class to provide a range of options to a <see cref="Converter"/> 
    /// instance.
    /// </summary>
    public sealed class Options
    {
        #region Fields

        private readonly List<OptionMember> optionMembers = new();
        private readonly List<OptionType> optionTypes = new();
        private readonly bool none;

        #endregion

        #region Properties

        /// <summary>
        /// A static <see cref="Options"/> instance with no additional details specified.
        /// </summary>
        public static Options None => new(true);

        /// <summary>
        /// Gets the <see cref="OptionMember"/> instances assigned to the current <see cref="Options"/>
        /// instance.
        /// </summary>
        public OptionMember[] OptionMembers => optionMembers.ToArray();

        /// <summary>
        /// Gets and sets the <see cref="string"/> array of comment prefixes.
        /// </summary>
        /// <remarks>
        /// Any line that starts with one of these sequences, ignoring any white space, will
        /// be ignored.
        /// </remarks>
        public string[] CommentPrefixes { get; set; } = new[] { "!", "#", ";", "//", "--", };

        /// <summary>
        /// Gets the <see cref="OptionType"/> instances assigned to the curent <see cref="Options"/>
        /// instance.
        /// </summary>
        public OptionType[] OptionTypes => optionTypes.ToArray();

        /// <summary>
        /// Gets the <see cref="string"/> that contains the property prefix.
        /// </summary>
        public string PropertyPrefix { get; private set; } = "Properties";

        /// <summary>
        /// Gets the <see cref="string"/> that contains the values prefix.
        /// </summary>
        public string ValuesPrefix { get; private set; } = "Values";

        #endregion

        #region Constructors

        /// <summary>
        /// Private constructor to support the static instance with no additional options set.
        /// </summary>
        /// <param name="none">True to prevent any additional options from being set.</param>
        private Options(bool none) : this() => this.none = none;

        /// <summary>
        /// Creates a new <see cref="Options"/> instance.
        /// </summary>
        public Options() { }

        #endregion

        #region Methods

        /// <summary>
        /// Adds the specified <paramref name="optionMember"/>.
        /// </summary>
        /// <typeparam name="TObject">The type of the object.</typeparam>
        /// <typeparam name="TMember">The type of the member.</typeparam>
        /// <param name="member">An <see cref="Expression{TDelegate}"/> of <see cref="Func{T, TResult}"/> that takes a <typeparamref name="TObject"/> and returns a <typeparamref name="TMember"/>.</param>
        /// <param name="name">A <see cref="string"/> that contains the name to use for the member in the CSV data.</param>
        /// <returns>The <see cref="Options"/> object to allow calls to be chained.</returns>
        public Options ForMember<TObject, TMember>(Expression<Func<TObject, TMember>> member, string name)
            where TObject : class, new() =>
            AddOptionMember(new OptionMember<TObject, TMember>(member, name));

        /// <summary>
        /// Adss the specified <paramref name="optionType"/>.
        /// </summary>
        /// <typeparam name="TObject">The type that the <see cref="OptionType"/> is for.</typeparam>
        /// <param name="optionType">An <see cref="OptionType{T}"/> to add.</param>
        /// <returns>The <see cref="Options"/> object to allow calls to be chained.</returns>
        public Options ForType<TObject>(OptionType<TObject> optionType)
            where TObject : class, new() =>
            AddType(optionType);

        /// <summary>
        /// Adds a new <see cref="OptionType{T}"/> for <typeparamref name="TObject"/> to the 
        /// current <see cref="Options"/> instance.
        /// </summary>
        /// <typeparam name="TObject">The <see cref="Type"/> of object that the instance to handle.</typeparam>
        /// <returns>The <see cref="Options"/> object to allow calls to be chained.</returns>
        public Options ForType<TObject>()
            where TObject : class, new() => 
            ForType(new OptionType<TObject>());

        /// <summary>
        /// Adds a new <see cref="OptionType{T}"/> for <typeparamref name="TObject"/> with the 
        /// specified <paramref name="name"/> to the current <see cref="Options"/> instance 
        /// </summary>
        /// <typeparam name="TObject">The <see cref="Type"/> of object that the instance to handle.</typeparam>
        /// <param name="name">A <see cref="string"/> containing the name to use for the object in the CSV data.</param>
        /// <returns>The <see cref="Options"/> object to allow calls to be chained.</returns>
        public Options ForType<TObject>(string name)
            where TObject : class, new() =>
            ForType(new OptionType<TObject>(name));

        #endregion

        #region Support routines

        private Options AddOptionMember<TObject, TMember>(OptionMember<TObject, TMember> optionMember)
            where TObject : class, new()
        {
            if (!none)
                this.optionMembers.Add(optionMember);
            return this;
        }

        private Options AddType<TObject>(OptionType<TObject> optionType) 
            where TObject : class, new()
        {
            if (!none)
                optionTypes.Add(optionType);
            return this;
        }

        #endregion
    }
}

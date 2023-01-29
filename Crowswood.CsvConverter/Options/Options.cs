using static Crowswood.CsvConverter.Converter;

namespace Crowswood.CsvConverter
{
    /// <summary>
    /// An options class to provide a range of options to a <see cref="Converter"/> 
    /// instance.
    /// </summary>
    public sealed class Options
    {
        #region Fields

        private readonly List<Assignment> assignments = new();
        private readonly List<OptionType> optionTypes = new();
        private readonly bool none;

        #endregion

        #region Properties

        /// <summary>
        /// A static <see cref="Options"/> instance with no additional details specified.
        /// </summary>
        public static Options None => new(true);

        /// <summary>
        /// Gets the <see cref="Assignment"/> instances assigned to the current <see cref="Options"/>
        /// instance.
        /// </summary>
        public Assignment[] Assignments => assignments.ToArray();

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
        /// Adds the specified <paramref name="assignment"/>.
        /// </summary>
        /// <typeparam name="T">The type that the assignement is for.</typeparam>
        /// <param name="assignment">An <see cref="Assignment{T}"/> to add.</param>
        /// <returns>The <see cref="Options"/> object to allow calls to be chained.</returns>
        public Options ForMember<T>(Assignment<T> assignment) where T : class, new()
        {
            if (!none)
                assignments.Add(assignment);
            return this;
        }

        /// <summary>
        /// Adss the specified <paramref name="optionType"/>.
        /// </summary>
        /// <typeparam name="T">The type that the <see cref="OptionType"/> is for.</typeparam>
        /// <param name="optionType">An <see cref="OptionType{T}"/> to add.</param>
        /// <returns>The <see cref="Options"/> object to allow calls to be chained.</returns>
        public Options ForType<T>(OptionType<T> optionType)
            where T : class, new()
        {
            if (!none)
                optionTypes.Add(optionType);
            return this;
        }

        #endregion
    }
}

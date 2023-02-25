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
        private readonly List<OptionMetadata> optionMetadata = new();
        private readonly List<OptionReference> optionsReferences = new();
        private readonly List<OptionType> optionTypes = new();

        private readonly bool none;

        #endregion

        #region Properties

        /// <summary>
        /// A static <see cref="Options"/> instance with no additional details specified.
        /// </summary>
        public static Options None => new(true);

        /// <summary>
        /// Gets and sets the <see cref="string"/> array of comment prefixes.
        /// </summary>
        /// <remarks>
        /// Any line that starts with one of these sequences, ignoring any white space, will
        /// be ignored.
        /// </remarks>
        public string[] CommentPrefixes { get; set; } = new[] { "!", "#", ";", "//", "--", };

        /// <summary>
        /// Gets the <see cref="OptionMember"/> instances assigned to the current <see cref="Options"/>
        /// instance.
        /// </summary>
        internal OptionMember[] OptionMembers => optionMembers.ToArray();

        /// <summary>
        /// Gets the <see cref="OptionMetadata"/> instances assigned to the <see cref="Options"/>
        /// instance.
        /// </summary>
        internal OptionMetadata[] OptionMetadata => optionMetadata.ToArray();

        /// <summary>
        /// Gets the <see cref="OptionReferenceType"/> instances assigned to the current <see cref="Options"/> 
        /// instance.
        /// </summary>
        internal OptionReference[] OptionsReferences => this.optionsReferences.ToArray();

        /// <summary>
        /// Gets the <see cref="OptionType"/> instances assigned to the curent <see cref="Options"/>
        /// instance.
        /// </summary>
        internal OptionType[] OptionTypes => optionTypes.ToArray();

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
            AddMember(new OptionMember<TObject, TMember>(member, name));


        /// <summary>
        /// Adds a new <see cref="OptionMetadata{T}"/> for the specified <paramref name="prefix"/>
        /// and <paramref name="propertyNames"/>
        /// </summary>
        /// <typeparam name="TMetadata">The generic parameter of the <see cref="OptionMetadata{T}"/>.</typeparam>
        /// <param name="prefix">A <see cref="string"/> that contains the prefix.</param>
        /// <param name="propertyNames">A <see cref="string[]"/> that contains the property names.</param>
        /// <returns>The <see cref="Options"/> object to allow calls to be chained.</returns>
        public Options ForMetadata<TMetadata>(string prefix, params string[] propertyNames)
            where TMetadata : class, new() =>
            AddMetadata(new OptionMetadata<TMetadata>(prefix, propertyNames));

        /// <summary>
        /// Adds a new <see cref="OptionMetadataDictionary"/> for the specific <paramref name="prefix"/>
        /// and <paramref name="propertyNames"/> with nulls being allowed or not through 
        /// <paramref name="allowNulls"/>.
        /// </summary>
        /// <param name="prefix">A <see cref="string"/> that contains the prefix.</param>
        /// <param name="allowNulls">True if null values are allowed; false otherwise.</param>
        /// <param name="propertyNames">A <see cref="string[]"/> that contains the property names.</param>
        /// <returns>The <see cref="Options"/> object to allow calls to be chained.</returns>
        public Options ForMetadata(string prefix, bool allowNulls,params string[] propertyNames)=>
            AddMetadata(new OptionMetadataDictionary(prefix, propertyNames) 
            {
                AllowNulls = allowNulls
            });


        /// <summary>
        /// Adds a new <see cref="OptionReference"/> to define the reference defaults with the 
        /// specified <paramref name="idPropertyName"/> and <paramref name="namePropertyName"/>.
        /// </summary>
        /// <param name="idPropertyName">A <see cref="string"/> containing the name of the Id property.</param>
        /// <param name="namePropertyName">A <see cref="string"/> containing the name of the Name property.</param>
        /// <returns>The <see cref="Options"/> object to allow calls to be chained.</returns>
        public Options ForReferences(string idPropertyName, string namePropertyName) =>
            AddReference(new OptionReference(idPropertyName, namePropertyName));

        /// <summary>
        /// Adds a new <see cref="OptionReferenceType"/> for the specified <paramref name="typeName"/> 
        /// with the specified <paramref name="idPropertyName"/> and <paramref name="namePropertyName"/>.
        /// </summary>
        /// <param name="typeName">A <see cref="string"/> containing the name of the type.</param>
        /// <param name="idPropertyName">A <see cref="string"/> containing the name of the Id property.</param>
        /// <param name="namePropertyName">A <see cref="string"/> containing the name of the Name property.</param>
        /// <returns>The <see cref="Options"/> object to allow calls to be chained.</returns>
        public Options ForReferences(string typeName, string idPropertyName, string namePropertyName) =>
            AddReference(new OptionReferenceType(typeName, idPropertyName, namePropertyName));

        /// <summary>
        /// Adds a new <see cref="OptionReferenceType{T}"/> for the specified <typeparamref name="T"/> 
        /// with the specified <paramref name="idPropertyName"/> and <paramref name="namePropertyName"/>.
        /// </summary>
        /// <typeparam name="T">The <see cref="Type"/> of object that is being referenced.</typeparam>
        /// <param name="idPropertyName">A <see cref="string"/> containing the name of the Id property.</param>
        /// <param name="namePropertyName">A <see cref="string"/> containing the name of the Name property.</param>
        /// <returns>The <see cref="Options"/> object to allow calls to be chained.</returns>
        public Options ForReferences<T>(string idPropertyName, string namePropertyName)=>
            AddReference(new OptionReferenceType<T>(idPropertyName, namePropertyName));


        /// <summary>
        /// Adds a new <see cref="OptionType{T}"/> for <typeparamref name="TObject"/> to the 
        /// current <see cref="Options"/> instance.
        /// </summary>
        /// <typeparam name="TObject">The <see cref="Type"/> of object that the instance to handle.</typeparam>
        /// <returns>The <see cref="Options"/> object to allow calls to be chained.</returns>
        public Options ForType<TObject>()
            where TObject : class, new() => 
            AddType(new OptionType<TObject>());

        /// <summary>
        /// Adds a new <see cref="OptionType{T}"/> for <typeparamref name="TObject"/> with the 
        /// specified <paramref name="name"/> to the current <see cref="Options"/> instance 
        /// </summary>
        /// <typeparam name="TObject">The <see cref="Type"/> of object that the instance to handle.</typeparam>
        /// <param name="name">A <see cref="string"/> containing the name to use for the object in the CSV data.</param>
        /// <returns>The <see cref="Options"/> object to allow calls to be chained.</returns>
        public Options ForType<TObject>(string name)
            where TObject : class, new() =>
            AddType(new OptionType<TObject>(name));

        /// <summary>
        /// Adds a new <see cref="OptionDynamicType"/> with the specified <paramref name="name"/> 
        /// and <paramref name="propertyNames"/> to the current <see cref="Options"/> instance.
        /// </summary>
        /// <param name="name">A <see cref="string"/> containing the name to use for the object in the CSV data.</param>
        /// <param name="propertyNames">A <see cref="string[]"/> containing the names of the parameters.</param>
        /// <returns>The <see cref="Options"/> object to allow calls to be chained.</returns>
        public Options ForType(string name, string propertyName, params string[] propertyNames) =>
            AddType(new OptionDynamicType(name, propertyName, propertyNames));


        /// <summary>
        /// Sets the <seealso cref="PropertyPrefix"/> and <seealso cref="ValuesPrefix"/> according
        /// to the specified <paramref name="propertiesPrefix"/> and <paramref name="valuesPrefix"/>.
        /// </summary>
        /// <param name="propertiesPrefix">A <see cref="string"/> containing the new properties prefix value or null for no change.</param>
        /// <param name="valuesPrefix">A <see cref="string"/> containing the new values prefix value or null for no change.</param>
        /// <returns>The <see cref="Options"/> object to allow calls to be chained.</returns>
        public Options SetPrefixes(string? propertiesPrefix, string? valuesPrefix)
        {
            if (!string.IsNullOrWhiteSpace(propertiesPrefix))
                this.PropertyPrefix = propertiesPrefix;
            if (!string.IsNullOrWhiteSpace(valuesPrefix))
                this.ValuesPrefix = valuesPrefix;
            return this;
        }

        #endregion

        #region Support routines

        private Options AddMember<TObject, TMember>(OptionMember<TObject, TMember> optionMember)
            where TObject : class, new() => Add(optionMember, this.optionMembers);

        private Options AddMetadata(OptionMetadata optionMetadata) =>
            Add(optionMetadata, this.optionMetadata);

        private Options AddReference(OptionReference optionReference) =>
            Add(optionReference, this.optionsReferences);

        private Options AddType(OptionType optionType) =>
            Add(optionType, this.optionTypes);

        private Options Add<T>(T item, List<T> items) where T : class
        {
            if (!this.none)
                items.Add(item);
            return this;
        }

        #endregion
    }
}

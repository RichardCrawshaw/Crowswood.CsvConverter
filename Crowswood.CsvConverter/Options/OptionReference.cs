namespace Crowswood.CsvConverter
{
    /// <summary>
    /// A base class that allows the default names of the properties that should be used when 
    /// referencing an entity.
    /// </summary>
    public class OptionReference
    {
        #region Properties

        /// <summary>
        /// Gets a <see cref="string"/> containing the name of the Id property.
        /// </summary>
        public string IdPropertyName { get; }

        /// <summary>
        /// Gets a <see cref="string"/> containing the name of the Name property.
        /// </summary>
        public string NamePropertyName { get; }

        #endregion

        #region Constructors

        public OptionReference(string idPropertyName, string namePropertyName)
        {
            this.IdPropertyName = idPropertyName;
            this.NamePropertyName = namePropertyName;
        }

        #endregion
    }

    /// <summary>
    /// A derived class that allows the type of an entity object to be held together with the 
    /// names of the properties that should be used when referencing that entity.
    /// </summary>
    public class OptionReferenceType : OptionReference
    {
        #region Properties

        /// <summary>
        /// Gets a <see cref="string"/> containing the name of the referenced type.
        /// </summary>
        public string TypeName { get; }

        #endregion

        #region Constructors

        public OptionReferenceType(string typeName, string idPropertyName, string namePropertyName)
            : base(idPropertyName, namePropertyName) => this.TypeName = typeName;

        public OptionReferenceType(Type type, string idPropertyName, string namePropertyName)
            : this(type.Name, idPropertyName, namePropertyName) { }

        #endregion
    }

    /// <summary>
    /// A derived generic class that allows an entity type to be specified using generics.
    /// </summary>
    /// <typeparam name="T">The <see cref="Type"/> of entity object.</typeparam>
    internal class OptionReferenceType<T> : OptionReferenceType
    {
        public OptionReferenceType(string idPropertyName, string namePropertyName)
            : base(typeof(T), idPropertyName, namePropertyName) { }
    }
}

namespace Crowswood.CsvConverter.Handlers
{
    /// <summary>
    /// An internal class that is to manage the tracking of the automatic increment values.
    /// </summary>
    internal class IndexHandler
    {
        #region Fields

        /// <summary>
        /// A dictionary of the indexed type names and their current values.
        /// </summary>
        private readonly Dictionary<string, int> indexes = new();

        #endregion

        #region Constructors

        /// <summary>
        /// Create a new instance of the <see cref="IndexHandler"/>.
        /// </summary>
        internal IndexHandler() { }

        #endregion

        #region Methods

        /// <summary>
        /// Clear the indexes, both the tracked type names and their values.
        /// </summary>
        internal void Clear() => this.indexes.Clear();

        /// <summary>
        /// Determines and returns whether the specified <paramref name="typeName"/> is tracked or 
        /// not.
        /// </summary>
        /// <param name="typeName">A <see cref="string"/> containing the type name to check.</param>
        /// <returns>True if it is tracked; false otherwise.</returns>
        internal bool Contains(string typeName) => this.indexes.ContainsKey(typeName);

        /// <summary>
        /// Gets the next value for the specified <paramref name="typeName"/>.
        /// </summary>
        /// <param name="typeName">A <see cref="string"/> containing the name of the type.</param>
        /// <returns>An <see cref="int"/> that contains the value.</returns>
        internal int Get(string typeName) => this.indexes[typeName]++;

        /// <summary>
        /// Initialises the tracking of the specified <paramref name="typeNames"/>.
        /// </summary>
        /// <param name="typeNames">A <see cref="string[]"/> containing the names of the types to track.</param>
        internal void Initialise(params string[] typeNames)
        {
            foreach (var typeName in typeNames)
                if (!indexes.ContainsKey(typeName))
                    this.indexes[typeName] = 1;
        }

        #endregion
    }
}

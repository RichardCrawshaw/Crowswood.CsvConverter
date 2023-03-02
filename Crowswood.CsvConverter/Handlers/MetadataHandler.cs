﻿using System.ComponentModel;
using System.Reflection;
using Crowswood.CsvConverter.Extensions;
using Crowswood.CsvConverter.Helpers;

namespace Crowswood.CsvConverter.Handlers
{
    internal class MetadataHandler
    {
        #region Fields

        private readonly Dictionary<string, TypeDescriptionProvider> providers = new();

        #endregion

        #region Properties

        private IndexHandler IndexHandler { get; }

        private Options Options { get; }

        internal Dictionary<string, List<object>> Metadata { get; } = new();

        private string[] MetadataPrefixes =>
            this.Options.OptionMetadata
                .Select(metadata => metadata.Prefix)
                .ToArray();

        #endregion

        #region Constructors

        public MetadataHandler(Options options, IndexHandler indexHandler)
        {
            this.Options = options;
            this.IndexHandler = indexHandler;
        }

        #endregion

        #region Methods

        /// <summary>
        /// Clears the metadata.
        /// </summary>
        internal void Clear()
        {
            foreach (var kvp in providers)
                TypeDescriptor.RemoveProvider(kvp.Value, kvp.Key);
            this.providers.Clear();
            this.Metadata.Clear();
        }

        /// <summary>
        /// Constructs and applies the metadata for the specified <paramref name="type"/> from the 
        /// specified <paramref name="lines"/>.
        /// </summary>
        /// <param name="type">The <see cref="Type"/> for the metadata.</param>
        /// <param name="lines">An <see cref="IEnumerable{T}"/> of <see cref="string"/>.</param>
        /// <remarks>
        /// If the any of the metadata is an <see cref="Attribute"/> then they will be attached to 
        /// the <see cref="Type"/>
        /// </remarks>
        internal void Construct(Type type, IEnumerable<string> lines)
        {
            var metadata = GetMetadata(type.Name, lines);

            Apply(type, metadata);
            Apply(type.Name, metadata);
        }

        /// <summary>
        /// Constructs and applies the metadata for the specified <paramref name="typeName"/> from 
        /// the specified <paramref name="lines"/>.
        /// </summary>
        /// <param name="typeName">A <see cref="string"/> that contains the name for the metadata.</param>
        /// <param name="lines">An <see cref="IEnumerable{T}"/> of <see cref="string"/>.</param>
        /// <exception cref="ArgumentException">If the <paramref name="typeName"/> is null, empty string or white space.</exception>
        internal void Construct(string typeName, IEnumerable<string> lines)
        {
            if (string.IsNullOrWhiteSpace(typeName)) 
                throw new ArgumentException($"Failed to determine {nameof(typeName)}.");

            var metadata = GetMetadata(typeName, lines);

            Apply(typeName, metadata, always: true);
        }

        /// <summary>
        /// Updates the metadata for the specified <paramref name="type"/> so that any items that 
        /// derive from <see cref="Attribute"/> are applied to the <see cref="Type"/> and removed 
        /// from the <seealso cref="Metadata"/> property.
        /// </summary>
        /// <param name="type">The <see cref="Type"/> for which to update the metadata.</param>
        internal void Update(Type type)
        {
            if (!this.Metadata.ContainsKey(type.Name))
                return;

            // Apply the metadata to the Type.
            Apply(type, this.Metadata[type.Name]);

            // Remove the attribute based metadata from the property.
            this.Metadata[type.Name]
                .RemoveAll(item => item is Attribute);
        }

        #endregion

        #region Support routines

        /// <summary>
        /// Applies to the <paramref name="type"/> the <paramref name="metadata"/> as an 
        /// <see cref="Attribute"/>.
        /// </summary>
        /// <param name="type">A <see cref="Type"/> to attach the <paramref name="metadata"/> as <see cref="Attribute"/>.</param>
        /// <param name="metadata">A <see cref="List{T}"/> of <see cref="object"/> containing the metadata.</param>
        private void Apply(Type type, List<object> metadata)
        {
            var attributes =
                metadata
                    .Select(item => item as Attribute)
                    .NotNull()
                    .Where(attribute =>
                        attribute.GetType().GetCustomAttributes<AttributeUsageAttribute>(true)
                            .Any(a => (a.ValidOn & AttributeTargets.Class) == AttributeTargets.Class))
                    .ToArray();
            if (attributes.Any())
            {
                // If the type of any of the metadata derives from Attribute then add them to the
                // custom attributes for that type and retain an instance of TypeDescriptorProvider
                // so it can be removed if the converter is used to load different data.
                this.providers[type.Name] = TypeDescriptor.AddAttributes(type, attributes);
            }
        }

        /// <summary>
        /// Applies to the specified <paramref name="typeName"/> the specified <paramref name="metadata"/>
        /// if the <see cref="Type"/> of the metadata is not an <see cref="Attribute"/>, or if the 
        /// <paramref name="always"/> is true.
        /// </summary>
        /// <param name="typeName">A <see cref="string"/> containing the key for the <paramref name="metadata"/>.</param>
        /// <param name="metadata">A <see cref="List{T}"/> of <see cref="object"/> containing the metadata.</param>
        /// <param name="always">True to always apply the metadata even if it is an <see cref="Attribute"/>; false otherwise.</param>
        private void Apply(string typeName, List<object> metadata, bool always = false) =>
            this.Metadata[typeName] =
                metadata
                    .Where(item => always || (item is not Attribute))
                    .ToList();

        /// <summary>
        /// Generate and return a set of parsed metadata lines from the specified <paramref name="lines"/> 
        /// for the specified <paramref name="typeName"/>.
        /// </summary>
        /// <param name="typeName"A <see cref="string"/> containing the name of the type to filter by.</param>
        /// <param name="lines">An <see cref="IEnumerable{T}"/> of <see cref="string"/>.</param>
        /// <returns>A <see cref="IEnumerable{T}"/> of <see cref="string[]"/>.</returns>
        private List<object> GetMetadata(string typeName, IEnumerable<string> lines) =>
            ConversionHelper.GetItems(lines,
                                      rejoinSplitQuotes: true,
                                      trimItems: false,
                                      typeName, 
                                      this.MetadataPrefixes)
                .Select(items => GetMetadata(GetOptionMetadata(items[0]), items[2..^0]))
                .NotNull()
                .ToList();

        /// <summary>
        /// Generate and return a single metadata item from the specified <paramref name="optionMetadata"/>
        /// and <paramref name="propertyValues"/>.
        /// </summary>
        /// <param name="optionMetadata">An <see cref="OptionMetadata"/> instance.</param>
        /// <param name="propertyValues">A <see cref="string"/>array containing the values to be assigned to the metadata.</param>
        /// <returns>An <see cref="object"/> that has been initialised with the <paramref name="propertyValues"/>.</returns>
        /// <remarks>
        /// The order of the properties is controlled by the order of the property names supplied
        /// to the <paramref name="optionMetadata"/> object.
        /// </remarks>
        private object? GetMetadata(OptionMetadata? optionMetadata, string[] propertyValues)
        {
            if (optionMetadata is null) return null;

            var typeName = optionMetadata.Prefix;

            this.IndexHandler.Initialise(typeName);

            var valueConverter = new ValueConverter(this.IndexHandler, typeName);
            if (optionMetadata is OptionMetadataDictionary omd)
            {
                var dictionaryMetadata =
                    GetMetadataDictionary(valueConverter,
                                          optionMetadata.PropertyNames,
                                          propertyValues,
                                          omd.AllowNulls);
                return omd.AllowNulls ? (object)dictionaryMetadata : dictionaryMetadata.NotNull();
            }

            var result =
                optionMetadata
                    .CreateInstance()
                    .SetProperties(valueConverter,
                                   optionMetadata.GetProperties(),
                                   optionMetadata.PropertyNames,
                                   propertyValues);
            return result;
        }

        /// <summary>
        /// Creates and returns a dictionary of values using the specified <paramref name="valueConverter"/>
        /// for the specified <paramref name="names"/> and <paramref name="values"/> and 
        /// <paramref name="allowNulls"/> flag.
        /// </summary>
        /// <param name="valueConverter">The <see cref="ValueConverter"/> to use to convert the values.</param>
        /// <param name="names">A <see cref="string"/> array containing the names that will form the keys.</param>
        /// <param name="values">A <see cref="string"/> array containing the values.</param>
        /// <param name="allowNulls">A <see cref="bool"/> that if true indicates that an empty string will generate null; false to generate an empty string. An string containing empty double-quotes will always generate an empty string.</param>
        /// <returns></returns>
        private static Dictionary<string, string?> GetMetadataDictionary(ValueConverter valueConverter,
                                                                         string[] names,
                                                                         string[] values,
                                                                         bool allowNulls) =>
            Enumerable
                .Range(0, Math.Min(names.Length, values.Length))
                .ToDictionary(
                    index => names[index],
                    index => values[index] switch
                    {
                        "" => allowNulls ? null : string.Empty,
                        "\"\"" => string.Empty,
                        _ => valueConverter.ConvertValue(values[index], typeof(string))?.ToString() ??
                            string.Empty,
                    });

        /// <summary>
        /// Gets the <see cref="OptionMetadata"/> that has the <seealso cref="OptionMetadata.Prefix"/> 
        /// that matches the specified <paramref name="typeName"/>.
        /// </summary>
        /// <param name="typeName">A <see cref="string"/> that contains the name of the type of the metadata.</param>
        /// <returns>An <see cref="OptionMetadata"/> object; or null if none match.</returns>
        private OptionMetadata? GetOptionMetadata(string typeName) =>
            this.Options.OptionMetadata
                .FirstOrDefault(metadata => metadata.Prefix == typeName);

        #endregion
    }
}

namespace Crowswood.CsvConverter.Helpers
{
    /// <summary>
    /// Static helper class to help with <see cref="Options"/>.
    /// </summary>
    internal static class OptionsHelper
    {
        /// <summary>
        /// Validate the <seealso cref="Options"/>.
        /// </summary>
        /// <exception cref="ArgumentException">If the property and values prefixes are not different.
        /// or
        /// If any of the metadata prefixes are not different to both the property and values prefixes.</exception>
        public static Options ValidateOptions(Options options)
        {
            if (options.PropertyPrefix == options.ValuesPrefix) // ValidateOptions
                throw new ArgumentException(
                    "The property prefix and the values prefix must be different.",
                    nameof(options));

            if (options.OptionMetadata
                    .Any(om => om.Prefix == options.PropertyPrefix || // ValidateOptions
                               om.Prefix == options.ValuesPrefix)) // ValidateOptions
                throw new ArgumentException(
                    "The metadata prefix must be different to that of the property prefix and values prefix.",
                    nameof(options));

            if (options.OptionMetadata
                    .Where(om => om is not OptionMetadataDictionary)
                    .Select(om => new { OptionsMetadata = om, Properties = om.Type.GetProperties(), })
                    .Select(n => new { n.OptionsMetadata, PropertyNames = n.Properties.Select(p => p.Name).ToList(), })
                    .Any(n => n.OptionsMetadata.PropertyNames.Any(pn => !n.PropertyNames.Contains(pn))))
                throw new ArgumentException(
                    "The metadata may only contain property names defined by the targeted type.",
                    nameof(options));

            return options;
        }
    }
}

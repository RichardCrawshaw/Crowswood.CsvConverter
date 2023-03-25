namespace Crowswood.CsvConverter.Serializations
{
    /// <summary>
    /// A sealed class for serializing comments.
    /// </summary>
    internal sealed class CommentData : BaseSerializationData
    {
        private readonly string prefix;
        private readonly string[] comments;

        public CommentData(string prefix, params string[] comments)
        {
            this.prefix = prefix;
            this.comments =
                !comments.Any()
                ? new[] { string.Empty, }
                : comments
                    // Split using `\r\n` CharArray rather than `Environment.NewLine` to cater
                    // for files that use a differnet standard from the current OS.
                    // Don't trim the resultant comments to allow them to have leading spaces.
                    .Select(comment => comment.Split("\r\n".ToCharArray(),
                                                     StringSplitOptions.RemoveEmptyEntries))
                    .SelectMany(comment => comment)
                    .ToArray();
        }

        /// <inheritdoc/>
        public override string[] Serialize() =>
            this.comments
                .Select(comment => $"{prefix} {comment}")
                .ToArray();
    }

}

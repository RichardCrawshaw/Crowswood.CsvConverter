namespace Crowswood.CsvConverter.Model
{
    internal abstract class Typeless
    {
    }

    internal abstract class Typeless<T> : Typeless
    {
        /// <summary>
        /// Gets the underlying value of the current instance.
        /// </summary>
        /// <returns>A <typeparamref name="T"/> instance.</returns>
        public abstract T Get();
    }
}

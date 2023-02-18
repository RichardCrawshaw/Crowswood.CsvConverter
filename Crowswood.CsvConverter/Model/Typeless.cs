namespace Crowswood.CsvConverter.Model
{
    internal abstract class Typeless
    {
    }

    internal abstract class Typeless<T> : Typeless
    {
        public abstract T Get();
    }
}

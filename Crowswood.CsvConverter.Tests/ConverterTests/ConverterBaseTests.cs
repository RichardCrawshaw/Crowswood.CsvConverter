namespace Crowswood.CsvConverter.Tests.ConverterTests
{
    /// <summary>
    /// Abstract base class to hold the model classes for the various converter tests.
    /// </summary>
    public abstract class ConverterBaseTests
    {
        public enum TestEnum
        {
            None = 0,

            Value,
            Data,
            Entity,
        }

        // WARNING: Don't change the names of the Foo, Bar and Baz classes, their names are tied
        // to the textual data in some of the tests and those tests will then fail.

        protected class Foo
        {
            public int Id { get; set; }
            public string? Name { get; set; }

            public TestEnum TestEnum { get; set; }
        }

        protected class Bar : Foo
        {
            public bool Flag { get; set; }
            public double Value { get; set; }
            public decimal Number { get; set; }
        }

        protected class Baz : Foo
        {
            public string? Code { get; set; }
            public string? Description { get; set; }
        }

        protected class OtherFoo : Foo
        {
            public int FooId { get; set; }
        }
    }
}
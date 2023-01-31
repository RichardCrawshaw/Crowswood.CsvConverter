# Crowswood.CsvConverter

A compact and simple conversion library that supports CSV files with multiple 
data types and comments within a single file.

## Why a new converter?

And why just CSV? Surely anyone can simply read and write CSV files without any 
real difficulty‽

On a number of occassions now I have had need to handle largish amounts of human
readable and modifiable data. Often this is where the humans aren't sufficiently
familiar with XML, and are even less familiar with JSON. Many people find 
tabulated data easier to work with; probably one of the reasons why Excel has 
been so popular in business environments, even where its users haven't been 
doing any form of number crunching.

Often it is useful to be able to support multiple different data-types in the 
one file. Other CSV converters that I've looked at haven't supported this.

It is also extremely useful to be able to have comments in the data.

## Data Format

Essentially it is tabulated data separated by commas. Each line represents a 
separate instance, each comma separated item within the line is the value of one
property of that instance.

The property names are specified on a separate line; this allows them to be 
defined once and all instances use them, rather than specifying them as part of 
each set of values. Because multiple data-types are supported within a single 
file there must be some way of defining which data-type each set of values or 
property names are.

String values are contained in "double-quotes". Double-quoted string can contain 
commas.

### Example Data

```
Properties,Foo,Id,Name
Values,Foo,1,"Fred"
Values,Foo,2,"Bert"
```

For a Foo object defined with an integer Id and a string Name properties:
```
public class Foo
{
    public int Id { get; set; }
    public string? Name { get; set; }
}
```

The `Foo` in all lines of the data is the name of the class that will be 
created. The property names in the Properties line are the names of the 
properties of Foo. These can be redefined: see the **Options** and 
**Attributes** sections below.

### Comments

All the common single-line comment characters are supported:
- **!** Exclamation mark, point, bang
- **\#** Hash
- **;** Semi-colon
- **//** Double forward-slashes
- **\-\-** Double dashes

If different comment prefixes are required they can be specified by writing a
`string[]` containing the desired prefixes to the `CommentPrefixes` property of 
the `options` object.

There is no support for block comments such as `/* This is a comment */` that 
can be used with C, C#, T-SQL, etc.

Comments are ignored when reading the data; they are not included if the data is
written; this is a possible future enhancement.

Blank lines are also ignored; as are any lines that cannot be interpreted as 
valid data.

## Usage

Simply create an instance of `Crowswood.CsvConverter.Converter` supplying the 
required `Options` then call either `Deserialize(text)` or 
`Deserialize(stream)`, or `Serialize(values)`.
- `text` is a `string` that contains the serialized data.
- `stream` is a `Stream` that contains the serialized data.
- `values` is an `IEnumerable<T>` of the data that is to be serialized.

### Options

Control the behaviour of the converter. If in doubt simply use `Options.None`, 
this provides a useful default.

Options allow control over Properties and Values prefixes and the set of Comment
prefixes. They also allow the definition of the data types that are to be 
converted, including changing the name of the data-type, and or of any property.

#### Example

```
Properties,Bas,Identity,FullName
Values,Bas,1,"Fred"
Values,Bas,2,"Bert"
```

Can be used to create instances of the same `Foo` object as above when using 
these `Options`:
```
var options =
    new Options()
        .ForType<Foo>("Bas")
        .ForMember<Foo>(foo => foo.Id, "Identity")
        .ForMember<Foo>(foo => foo.Name, "FullName");
```

## Attributes

There are a pair of `Attribute`s that can be used to decorate classes and their 
members rather than using an `Options` object with `OptionsType<T>` and 
`OptionsMember<T>`.
- `CsvConverterClassAttribute` can be used to decorate a class.
- `CsvConverterPropertyAttribute` can be used to decorate the properties of a 
class.

They should be used in conjunction with `Options.None`.

Mixing `OptionsType<T>` and `OptionsMember<T>` with `CsvConverterClassAttribute`
and `CsvConverterPropertyAttribute` is not supported, is not recommended, and is
not guaranteed to work: use one or the other.

### Example
```
[CsvConverterClass()]
public class Foo
{
    [CsvConverterProperty()]
    public int Id { get; set; }
    
    [CsvConverterProperty()]
    public string? Name { get; set; }
}
```
Or to change the class / property names in the CSV data:
```
[CsvConverterClass("Bas")]
public class Foo
{
    [CsvConverterProperty("Identity")]
    public int Id { get; set; }
    
    [CsvConverterProperty("FullName")]
    public string? Name { get; set; }
}
```

## Usage Examples

I have used previous versions of this approach to CSVs in a number of different
situations.
- Managing static entities with Entity Framework.
- Managing static entities for SQL-Server via T-SQL scripts.
- Constructing test data for injection into an in-memory database for
test projects.
- Code generation of multiple (100+) classes from a large dataset.

## Future Enhancements

The following future enhancements are under active development:
- Adding support for dynamic types. 
    
    This will be useful for data transformation such as when processing through 
    a T4 template where the data is simply written out as text without any need 
    entity types.

- Adding Meta-Data to data-types.

    Will allow control over how, and if, particular data-types are to be 
    processed. One data-set will be able to be used with multiple T4 templates 
    for Code generation. It will also enable the T4 templates to be simplified 
    by using a common code module that is controlled by setting properties for 
    each usage.
    
- Adding conversions of data-type names.

    This will be useful for T4 templates where a single data source is used to 
    generate multiple different outputs and each output requires subtly 
    different values from the same inputs, such as generating Entity Framework 
    seed data vs T-SQL script where the table names are pluralised but the 
    entity classes are singular.
    
- Adding conversions of values.

    This will enable Enums to be specified in the CSV data, but strings, without
    the Enum type, to be output in a T-SQL script.

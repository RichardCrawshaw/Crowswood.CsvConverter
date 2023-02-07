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

It also supports meta-data. This is applied on a per-data-type basis. See 
**[Meta-data](#metadata)** below.

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

The "Foo" in all lines of the data is the name of the class that will be 
created. The property names in the Properties line are the names of the 
properties of `Foo`. These can be redefined: see the **[Options](#options)**
and **[Attributes](#attributes)** sections below.

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

### Automatic Increment

This is a feature to support the creation of Primary Key values for database 
entities. These generally start from 1, so this is the default. It is only 
supported on numeric types. A future enhancement will be to allow this to be 
configured on a per data-type basis.

To use the Automatic Increment feature simply supply a single `#` character 
as the data, no quotes.
```
Properties,Foo,Id,Name
Values,Foo,#,"Fred"
Values,Foo,#,"Bert"
Values,Foo,#,"Harry"
```
When the above CSV data is deserialized the Fred instance will have an Id of 1,
the Bert instance an Id of 2, and Harry 3.

This makes the management of the data generally easier; just copy and paste,
move lines around, etc., and the Id fields will be updated accordingly.

If multiple uses are made per line of CSV data then the value will be incremented 
that number of times per entity.
```
Properties,Foo,Id,Value,Number
Values,Foo,#,#,#
```
Would result in an entity with 
```
    Id = 1,
    Value = 2,
    Number = 3,
```

Also skipping a line, will not increment the value for that entity:
```
Properties,Foo,Id,Name
Values,Foo,#,"Fred"
Values,Foo,99,"Bert"
Values,Foo,#,"Harry"
```
Results in
```
    Fred: Id = 1,
    Bert: Id = 99,
    Harry: Id = 2,
```

#### Serializing data

When serializing data to text there is no support for automatic increment of 
values. The actual value of the entity will be included. This prevents the 
situation where there is a data-set with values from 1-n, which serializes 
using the automatic increment feature, then when one of those entities is 
removed that almost exactly identical data cannot serialize with automatic 
increment. This type of change in bahaviour is only going to cause confusion; 
it is better that it does not exist in the first place.

## Usage

Simply create an instance of `Crowswood.CsvConverter.Converter` supplying the 
required `Options` then call either `Deserialize(text)` or 
`Deserialize(stream)`, or `Serialize(values)`.
- `text` is a `string` that contains the serialized data.
- `stream` is a `Stream` that contains the serialized data.
- `values` is an `IEnumerable<T>` of the data that is to be serialized.

### Options [#options]

Control the behaviour of the converter. If in doubt simply use `Options.None`, 
this provides a useful default.

Options allow control over Properties and Values prefixes and the set of Comment
prefixes. They also allow the definition of the data types that are to be 
converted, this includes changing the name of the data-type, and or the name of 
any property.

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

## Attributes [#attributes]

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

## Meta-data {#metadata}

Meta-data can be defined in the CSV data on a per-data-type basis. This is then
applied to the appropriate data-type. The format of the meta-data in the CSV is
very similar to that of the other lines: a prefix, the data-type, followed by a
list of values.

Example:
```
SomeMetadata,Bas,Field1,Field2
Properties,Bas,Identity,FullName
Values,Bas,1,"Fred"
Values,Bas,2,"Bert"
```

Mutliple sets of meta-data can be applied to a data-type; including multiple sets
of the same type of meta-data.

There are three types of meta-data:
1. A dictionary of string values keyed by string

    There are two flavours, one where the value *cannot* be null, and one where
    it can be null. The practical difference between the two is how empty strings
    are handled. If the value cannot be null then both no value `...,,...` two 
    commas without anything between them, and an empty string `...,"",...` both 
    generate a value of empty string. If the value can be null then no value will
    produce null.

2. An object with properties

    These can be accessed via the `Metadata` property on the `converter`. The 
    `Metadata` property is a dictionary that is keyed by `Type`. The value of the
    property is a `List` of `object`, and each object in the list is the instance
    of the meta-data for that data-type.
    
3. An attribute on the type

    These can be accessed by the static `TypeDescriptor` class.
    
    Example
    ```
    var attributes = TypeDescriptor.GetAttributes(typeof(Foo));
    ```
    If the specified type for the meta-data is derived from `Attribute` then this
    type of meta-data is automatically generated.
    In all other respects they are the same as the meta-data accessed through the 
    `Metadata` property on the `converter`.
    
    **Note**: these attributes *cannot* be accessed through reflection and the 
    `GetCustomAttribute` method.

### Useage

Meta-data is enabled through `Options`.

Each separate meta-data type must be registered. This includes the prefix used to 
identify that the line is a particular meta-data type and the names of the properties
that are to be populated in the same order that the values are specified in the CSV
data.

Example:
```
var options =
    new Options()
        .ForMetadata<T>(prefix, Field1, Field2, ... Fieldn)
```
Where `T` is the type of the meta-data, `prefix` is a string containing the prefix,
`Field1`, `Field2` through `Fieldn` are strings containing the field names. The 
field names are case sensitive.

When using dictionary metadata use this
```
var options =
    new Options()
        .ForMetadata(prefix, allowNulls, Field1, Field2, ... Fieldn)
```
The absence of the generic parameter and the presence of the `allowNulls` parameter 
automatically indicates that the meta-data is to be held in a dictionary and its 
value controls whether the dictionary allows null values or not.

### Restrictions and Limitations

When registering the meta-data through `Options` the prefixes must be unique for
each meta-data, and they cannot use the same prefixes as the Properties or Values.

If using an Attribute meta-data type then it must be valid for use on `class`.

### Uses for Meta-data

Meta-data can be used to control how the data once loaded is handled. For example
when managing static entity data it can be used to target either the ORM for 
code-first managed data, or to create T-SQL scripts that are run against the 
database.

The converter does nothing directly with the meta-data; it just loads the meta-data
and makes it available to the application.

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
- ~~Automatic increments.~~

    ~~This is intended to assist with generating seed data for database entity
    objects. Have a character combination that is the same on each line, so the
    human maintainer doesn't have to worry about ensuring unique IDs for each 
    data row.~~

- Referencing records of other data.

    For those entity objects that form a data hierarchy as generally seen in
    relational databases by linking to the Primary Key column of the reference
    data. A textual value would be supplied, which would then be looked up in 
    the values of the reference data and the Primary Key of that record would
    be used instead. Configuration of which property to look up and which to 
    return would be needed.

- Adding support for dynamic types. 
    
    This will be useful for data transformation such as when processing through 
    a T4 template where the data is simply written out as text without any need 
    entity types. It would remove the need to have classes defined for each 
    data-type.

- ~~Adding Meta-Data to data-types.~~

    ~~Will allow control over how, and if, particular data-types are to be 
    processed. One data-set will be able to be used with multiple T4 templates 
    for Code generation. It will also enable the T4 templates to be simplified 
    by using a common code module that is controlled by setting properties for 
    each usage.~~
    
- Adding conversions of data-type names.

    This will be useful for T4 templates where a single data source is used to 
    generate multiple different outputs and each output requires subtly 
    different values from the same inputs, such as generating Entity Framework 
    seed data vs T-SQL script where the table names are pluralised but the 
    entity classes are singular.
    
    For example a `User` entity class, but a `Users` database table.
    
    Just appending 's' on the end is **not** sufficient: a `Matrix` entity and 
    a `Matrices` table, or `Child` entity and `Children` table, etc.
    
- Adding conversions of values.

    This will enable Enums to be specified in the CSV data, but strings, without
    the Enum type, to be output in a T-SQL script.

- Configuration information in the CSV data.

    Add support for configuration data in the CSV data itself. This gives the data 
    maintainer direct control over how the data is handled. Such as being able to 
    specify the initial value of Automatic Increment values, or which fields are 
    used when referencing other data.

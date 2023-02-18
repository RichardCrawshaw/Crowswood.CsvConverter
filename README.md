# Crowswood.CsvConverter

A compact and simple conversion library that supports CSV files with multiple data types and comments within a single file.

For details of why and how, refer to the [Wiki](https://github.com/RichardCrawshaw/Crowswood.CsvConverter/wiki).

## Dynamic Types or Typeless Data

The idea is to provide a dataset that does not rely on pre-defined entity classes. 
Instead the data is presented as a Dictionary of data, one per data type, keyed by the name from the CSV. 
The value of each item in is a `Tuple` containing the field names of the data as `string[]` and  the values as `IEnumerable` of `string[]`. 
`Dictionary<string, (string[], IEnumerable<string[]>)>`

This will be useful in situations where strings are used, such as with T4 templates for code generation.

The automatic increment feature is still supported.
Internally its data-type is `int`, but it is presented as a `string` in keeping with the rest of the data.

Metadata is also supported on this data structure. 
However, it isn't possible to provide attribute based metadata, as this relies on a `Type` to carry the metadata.

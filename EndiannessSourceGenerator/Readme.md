# Endianness Source Generator

When annotating a struct with the `StructEndianness` attribute, this code generator will generate properties for the declared fields.

Each time a property is read or written, the endianness is converted from runtime endianness to struct endianness or vice-versa.

using System;

namespace SQLite
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    internal sealed class TableAttribute : Attribute
    {
        public TableAttribute(string name) => Name = name;

        public string Name { get; }
    }

    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
    internal sealed class PrimaryKeyAttribute : Attribute
    {
    }

    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
    internal sealed class AutoIncrementAttribute : Attribute
    {
    }

    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
    internal sealed class IndexedAttribute : Attribute
    {
    }

    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
    internal sealed class MaxLengthAttribute : Attribute
    {
        public MaxLengthAttribute(int length) => Length = length;

        public int Length { get; }
    }
}

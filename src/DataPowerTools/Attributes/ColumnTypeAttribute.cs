using System;

namespace DataPowerTools.Attributes;

[AttributeUsage(AttributeTargets.Field | AttributeTargets.Property, AllowMultiple = false)]
public class ColumnTypeAttribute : Attribute
{
    /// <summary>Initializes a new instance of the <see cref="T:System.ComponentModel.DataAnnotations.Schema.ColumnAttribute" /> class.</summary>
    public ColumnTypeAttribute()
    {

    }

    /// <summary>
    /// Maps the column to a backing CLR type.
    /// </summary>
    /// <param name="type"></param>
    public ColumnTypeAttribute(Type type)
    {
        Type = type;
    }

    public Type Type { get; }
    
}
    

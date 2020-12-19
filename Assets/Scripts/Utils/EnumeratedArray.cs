using System;
using UnityEngine;


public class EnumeratedArrayAttribute : PropertyAttribute
{
    public readonly string[] names;
    public EnumeratedArrayAttribute(Type enumtype)
    {
        names = Enum.GetNames(enumtype);
    }
}

﻿// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System;
using System.Linq;


namespace DigitalRise
{
  /// <summary>
  /// Auxiliary methods for enumerations.
  /// </summary>
  public static class EnumHelper
  {
    /// <summary>
    /// Retrieves an array of the values of the constants in a specified enumeration.
    /// </summary>
    /// <param name="enumType">An enumeration type.</param>
    /// <returns>
    /// An array of the enumeration values in <paramref name="enumType"/>. 
    /// </returns>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="enumType"/> is <see langword="null"/>.
    /// </exception>
    /// <exception cref="ArgumentException">
    /// <paramref name="enumType"/> is not an <see cref="Enum"/>.
    /// </exception>
    public static object[] GetValues(Type enumType)
    {
      return Enum.GetValues(enumType).Cast<object>().ToArray();
    }


    /// <summary>
    /// Converts the string representation of the name or numeric value of one or more enumerated 
    /// constants to an equivalent enumerated object.
    /// </summary>
    /// <typeparam name="T">The type of enumeration.</typeparam>
    /// <param name="text">The string representation of the name or numeric value.</param>
    /// <param name="ignoreCase">
    /// If set to <see langword="true"/> ignore case; otherwise, regard case.
    /// </param>
    /// <param name="value">The converted enumeration value.</param>
    /// <returns>
    /// <see langword="true"/> if the string was converted successfully; otherwise, 
    /// <see langword="false"/>.
    /// </returns>
    public static bool TryParse<T>(string text, bool ignoreCase, out T value)
    {
      try
      {
        value = (T)Enum.Parse(typeof(T), text, ignoreCase);
        return true;
      }
      catch (Exception)
      {
        value = default(T);
        return false;
      }
    }
  }
}

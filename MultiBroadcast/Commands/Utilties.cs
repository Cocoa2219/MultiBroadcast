using System.Collections.Generic;
using System.Linq;

namespace MultiBroadcast.Commands;

/// <summary>
///     Command utilities.
/// </summary>
public static class Utilties
{
    /// <summary>
    ///    Get the integer arguments from a string.
    /// </summary>
    /// <param name="text">The text to parse.</param>
    /// <param name="args">The integer arguments.</param>
    /// <returns>The integer arguments.</returns>
    public static bool GetIntArguments(string text, out int[] args)
    {
        var arg = text.Split('.');

        for (var i = 0; i < arg.Length; i++)
        {
            if (!int.TryParse(arg[i], out var result))
            {
                args = [];
                return false;
            }

            arg[i] = result.ToString();
        }

        args = arg.Select(int.Parse).ToArray();
        return true;
    }

    /// <summary>
    ///    Get the string from an array.
    /// </summary>
    /// <param name="array">The array to parse.</param>
    /// <typeparam name="T">The type of the array.</typeparam>
    /// <returns>The string from the array.</returns>
    public static string GetStringFromArray<T>(IEnumerable<T> array)
    {
        return string.Join(", ", array);
    }

}
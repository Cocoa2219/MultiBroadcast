using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace MultiBroadcast.API;

/// <summary>
///     Broadcast utilities.
/// </summary>
public static class BroadcastUtilities
{
    /// <summary>
    ///    Automatically close tags in a string.
    /// </summary>
    /// <param name="input">The input string.</param>
    /// <returns>The string with closed tags.</returns>
    public static string AutoCloseTags(string input)
    {
        const string pattern = @"<(\w+)(?:=[^>]*)?>";
        var regex = new Regex(pattern);

        var tagStack = new Stack<string>();

        foreach (Match match in regex.Matches(input))
        {
            tagStack.Push(match.Groups[1].Value);
        }

        while (tagStack.Count > 0)
        {
            var tag = tagStack.Pop();
            input += $"</{tag}>";
        }

        return input;
    }
}
using System.Text;

namespace Buttercup.Web.HtmlHelpers;

/// <summary>
/// Provides utilities methods relating to BEM classes.
/// </summary>
public static class Bem
{
    /// <summary>
    /// Builds a space-delimited list of BEM class names based on a block name and optional list of
    /// modifiers.
    /// </summary>
    /// <param name="block">
    /// The block name.
    /// </param>
    /// <param name="modifiers">
    /// The list of modifier names. Any null or empty values are ignored.
    /// </param>
    /// <returns>
    /// The BEM class names.
    /// </returns>
    public static string Block(string block, params string?[] modifiers)
    {
        var builder = new StringBuilder(block);

        foreach (var modifier in modifiers)
        {
            if (!string.IsNullOrEmpty(modifier))
            {
                builder.Append(' ').Append(block).Append("--").Append(modifier);
            }
        }

        return builder.ToString();
    }
}

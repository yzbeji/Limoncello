using System.Text.RegularExpressions;

public static class UrlHelper
{
    public static List<string> ExtractUrls(string content)
    {
        var urls = new List<string>();
        if (content == null)
        {
            return urls;
        }

        var regex = new Regex(@"(http|https)://[^\s/$.?#].[^\s]*", RegexOptions.Compiled | RegexOptions.IgnoreCase);
        var matches = regex.Matches(content);

        foreach (Match match in matches)
        {
            urls.Add(match.Value);
        }

        return urls;
    }

    public static string RemoveUrls(string content)
    {
        if (content == null)
        {
            return string.Empty;
        }

        var regex = new Regex(@"(http|https)://[^\s/$.?#].[^\s]*", RegexOptions.Compiled | RegexOptions.IgnoreCase);

        return regex.Replace(content, string.Empty);
    }
}

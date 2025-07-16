using System;
using System.Collections.Specialized;
using System.Text.RegularExpressions;

namespace Assets.SimpleSignIn.VK.Scripts
{
    public static class Utils
    {
        public static NameValueCollection ParseQueryString(string url)
        {
            var result = new NameValueCollection();

            foreach (Match match in Regex.Matches(url, @"(?<key>\w+)=(?<value>[^&#]+)"))
            {
                result.Add(match.Groups["key"].Value, Uri.UnescapeDataString(match.Groups["value"].Value));
            }

            return result;
        }
    }
}
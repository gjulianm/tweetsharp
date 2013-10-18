using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Tweetsharp
{
    public static class Utils
    {
        public static string TrimUrl(string url)
        {
            if (url == null)
                return "";

            url = url.Replace("http://", "");
            url = url.Replace("https://", "");
            if (url.Length > 25)
            {
                int SlashIndex = url.IndexOf('/');
                url = url.Substring(0, SlashIndex + 1);
                url += "...";
            }

            return url;
        }
    }
}

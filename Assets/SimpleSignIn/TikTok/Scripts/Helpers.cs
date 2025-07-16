using System;
using System.Collections.Specialized;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;

namespace Assets.SimpleSignIn.TikTok.Scripts
{
    public static class Helpers
    {
        /// <summary>
        /// http://stackoverflow.com/a/3978040
        /// </summary>
        public static int GetRandomUnusedPort()
        {
            var listener = new TcpListener(IPAddress.Loopback, 0);

            listener.Start();

            var port = ((IPEndPoint) listener.LocalEndpoint).Port;

            listener.Stop();

            return port;
        }

        public static NameValueCollection ParseQueryString(string url)
        {
            var result = new NameValueCollection();

            foreach (Match match in Regex.Matches(url, @"(?<key>\w+)=(?<value>[^&#]+)"))
            {
                result.Add(match.Groups["key"].Value, Uri.UnescapeDataString(match.Groups["value"].Value));
            }

            return result;
        }
		
		public static string ComputeHashSha256(string codeVerifier)
        {
            using var sha256 = SHA256.Create();
            var hash = sha256.ComputeHash(Encoding.UTF8.GetBytes(codeVerifier));

            return string.Concat(hash.Select(b => b.ToString("x2")).ToArray());
        }

        public static string ComputeHashSha512(string input)
        {
            using var sha512 = SHA512.Create();
            var hash = sha512.ComputeHash(Encoding.UTF8.GetBytes(input));

            return string.Concat(hash.Select(b => b.ToString("x2")).ToArray());
        }
    }
}
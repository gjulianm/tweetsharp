﻿using AsyncOAuth;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;


namespace TweetSharp
{
    /// <summary>
    /// Defines a contract for a <see cref="TwitterService" /> implementation.
    /// </summary>
    /// <seealso href="http://dev.twitter.com/doc" />
    public partial class TwitterService
    {
        private HttpClient _noAuthClient = new HttpClient();
        private HttpClient _authClient;

        private HttpClient _client
        {
            get
            {
                return _authClient ?? _noAuthClient;
            }
        }

        public bool TraceEnabled { get; set; }
        public bool IncludeEntities { get; set; }
        public bool IncludeRetweets { get; set; }

        public string Authority { get; set; }
        public string Version { get; set; }

        private string _consumerKey;
        private string _consumerSecret;
        private string _token;
        private string _tokenSecret;
        private JsonSerializer _serializer;

        public TwitterService(TwitterClientInfo info)
            : this()
        {
            _consumerKey = info.ConsumerKey;
            _consumerSecret = info.ConsumerSecret;
            IncludeEntities = info.IncludeEntities;

            _info = info;
        }

        public TwitterService(string consumerKey, string consumerSecret)
            : this()
        {
            _consumerKey = consumerKey;
            _consumerSecret = consumerSecret;
        }

        public TwitterService(string consumerKey, string consumerSecret, string proxy)
            : this()
        {
            _consumerKey = consumerKey;
            _consumerSecret = consumerSecret;
        }

        public TwitterService(string consumerKey, string consumerSecret, string token, string tokenSecret)
            : this()
        {
            _consumerKey = consumerKey;
            _consumerSecret = consumerSecret;
            _token = token;
            _tokenSecret = tokenSecret;

            CreateAuthenticatedClient();
        }

        public void AuthenticateWith(string token, string secret)
        {
            _token = token;
            _tokenSecret = secret;

            CreateAuthenticatedClient();
        }

        public void AuthenticateWith(string consumerKey, string consumerSecret, string token, string tokenSecret)
        {
            _consumerKey = consumerKey;
            _consumerSecret = consumerSecret;
            _token = token;
            _tokenSecret = tokenSecret;

            CreateAuthenticatedClient();
        }

        private void CreateAuthenticatedClient()
        {
            _authClient = OAuthUtility.CreateOAuthClient(_consumerKey, _consumerSecret, new AccessToken(_token, _tokenSecret));
            _authClient.BaseAddress = new Uri(Authority + Version, UriKind.Absolute);
        }

        public TwitterService()
        {
            FormatAsString = ".json";

            Authority = "https://api.twitter.com/";
            Version = "1.1/";

            _noAuthClient.BaseAddress = new Uri(Authority + Version, UriKind.Absolute);

            _serializer = new JsonSerializer();

            InitializeService();
        }

        private void InitializeService()
        {
            IncludeEntities = true;
            IncludeRetweets = true;
        }

        private readonly Func<HttpRequestMessage> _noAuthQuery
            = () =>
                  {
                      var request = new HttpRequestMessage();
                      return request;
                  };

        private readonly TwitterClientInfo _info;

        private HttpRequestMessage PrepareQuery(string path)
        {
            HttpRequestMessage request = new HttpRequestMessage();
            request.RequestUri = new Uri(path, UriKind.Relative);

            SetTwitterClientInfo(request);

            // TODO: Add file uploads.

            return request;
        }


        private void SetTwitterClientInfo(HttpRequestMessage request)
        {
            if (_info == null) return;
            if (!_info.ClientName.IsNullOrBlank())
            {
                request.Headers.Add("X-Twitter-Name", _info.ClientName);
                request.Headers.Add("User-Agent", _info.ClientName);
            }
            if (!_info.ClientVersion.IsNullOrBlank())
            {
                request.Headers.Add("X-Twitter-Version", _info.ClientVersion);
            }
            if (!_info.ClientUrl.IsNullOrBlank())
            {
                request.Headers.Add("X-Twitter-URL", _info.ClientUrl);
            }
        }

        public T Deserialize<T>(ITwitterModel model) where T : ITwitterModel
        {
            return Deserialize<T>(model.RawSource);
        }

        public T Deserialize<T>(string content)
        {
            return _serializer.Deserialize<T>(content);
        }

        internal string FormatAsString { get; private set; }

        private string ResolveUrlSegments(string path, List<object> segments)
        {
            if (segments == null) throw new ArgumentNullException("segments");

            var cleansed = new List<object>();
            for (var i = 0; i < segments.Count; i++)
            {
                if (i == 0)
                {
                    cleansed.Add(segments[i]);
                }
                if (i > 0 && i % 2 == 0)
                {
                    var key = segments[i - 1];
                    var value = segments[i];
                    if (value != null)
                    {
                        if (cleansed.Count == 1 && key is string)
                        {
                            var keyString = key.ToString();
                            if (keyString.StartsWith("&"))
                            {
                                key = "?" + keyString.Substring(1);
                            }
                        }
                        cleansed.Add(key);
                        cleansed.Add(value);
                    }
                }
            }
            segments = cleansed;

            for (var i = 0; i < segments.Count; i++)
            {
                if (segments[i] is DateTime)
                {
                    segments[i] = ((DateTime)segments[i]).ToString("yyyy-MM-dd");
                }

                if (segments[i] is bool)
                {
                    var flag = (bool)segments[i];
                    segments[i] = flag ? "1" : "0";
                }

                if (segments[i] is double)
                {
                    segments[i] = ((double)segments[i]).ToString(CultureInfo.InvariantCulture);
                }

                if (segments[i] is decimal)
                {
                    segments[i] = ((decimal)segments[i]).ToString(CultureInfo.InvariantCulture);
                }

                if (segments[i] is float)
                {
                    segments[i] = ((float)segments[i]).ToString(CultureInfo.InvariantCulture);
                }

                if (segments[i] is IEnumerable && !(segments[i] is string))
                {
                    ResolveEnumerableUrlSegments(segments, i);
                }
            }

            path = PathHelpers.ReplaceUriTemplateTokens(segments, path);

            PathHelpers.EscapeDataContainingUrlSegments(segments);

            const string includeEntities = "include_entities";
            const string includeRetweets = "include_rts";

            if (IncludeEntities && !IsKeyAlreadySet(segments, includeEntities))
            {
                segments.Add(segments.Count() > 1 ? "&" + includeEntities + "=" : "?" + includeEntities + "=");
                segments.Add("1");
            }
            if (IncludeRetweets && !IsKeyAlreadySet(segments, includeRetweets))
            {
                segments.Add(segments.Count() > 1 ? "&" + includeRetweets + "=" : "?" + includeRetweets + "=");
                segments.Add("1");
            }

            segments.Insert(0, path);

            return string.Concat(segments.ToArray()).ToString();
        }

        private static bool IsKeyAlreadySet(IList<object> segments, string key)
        {
            for (var i = 1; i < segments.Count; i++)
            {
                if (i % 2 != 1 || !(segments[i] is string)) continue;
                var segment = ((string)segments[i]).Trim(new[] { '&', '=', '?' });

                if (!segment.Contains(key)) continue;
                return true;
            }
            return false;
        }

        private static void ResolveEnumerableUrlSegments(IList<object> segments, int i)
        {
            // [DC] Enumerable segments will be typed, but we only care about string values
            var collection = (from object item in (IEnumerable)segments[i] select item.ToString()).ToList();
            var total = collection.Count();
            var sb = new StringBuilder();
            var count = 0;
            foreach (var item in collection)
            {
                sb.Append(item);
                if (count < total - 1)
                {
                    sb.Append(",");
                }
                count++;
            }
            segments[i] = sb.ToString();
        }

        private async Task<TwitterResponse<T>> ExecuteRequest<T>(string path)
        {
            var request = PrepareQuery(path);

            return await ExecuteRequestImpl<T>(request);
        }

        private async Task<TwitterResponse<T>> ExecuteRequest<T>(string path, params object[] segments)
        {
            var url = ResolveUrlSegments(path, segments.ToList());
            return await ExecuteRequest<T>(url);
        }

        private async Task<TwitterResponse<T>> ExecuteRequest<T>(HttpMethod method, string path)
        {
            var request = PrepareQuery(path);
            request.Method = method;

            return await ExecuteRequestImpl<T>(request);
        }



        private byte[] ReadStreamContents(Stream stream)
        {
            int length = (int)stream.Length;
            byte[] contents = new byte[length];

            if (stream.CanSeek)
                stream.Seek(0, SeekOrigin.Begin);

            stream.Read(contents, 0, length);

            return contents;
        }

        /// <summary>
        /// Creates a HttpContent object for the given file.
        /// </summary>
        /// <param name="file">File.</param>
        /// <returns>HttpContent wrapping the file contents.</returns>
        protected virtual HttpContent CreateFileContentFor(string name, Stream file)
        {
            var fileContent = new ByteArrayContent(ReadStreamContents(file));
            fileContent.Headers.ContentType = GetContentTypeFor(name);

            return fileContent;
        }

        /// <summary>
        /// Returns a content type based on the file name
        /// </summary>
        /// <param name="filename">File name.</param>
        /// <returns>Media type.</returns>
        protected virtual MediaTypeHeaderValue GetContentTypeFor(string filename)
        {
            var extension = filename.Split('.').Last();

            //TODO: Complete.
            switch (extension)
            {
                case "png":
                    return MediaTypeHeaderValue.Parse("image/png");
                case "jpg":
                    return MediaTypeHeaderValue.Parse("image/jpeg");
                default:
                    return MediaTypeHeaderValue.Parse("text/plain");

            }
        }

        private async Task<TwitterResponse<T>> ExecuteRequest<T>(HttpMethod method, string path, IDictionary<string, Stream> files, params object[] segments)
        {
            var url = ResolveUrlSegments(path, segments.ToList());
            var request = PrepareQuery(url);
            request.Method = method;

            var content = new MultipartFormDataContent();

            foreach (var file in files)
            {
                var fileContent = CreateFileContentFor(file.Key, file.Value);
                content.Add(fileContent, "media[]", file.Key);
            }

            request.Content = content;

            return await ExecuteRequestImpl<T>(request);
        }



        private async Task<TwitterResponse<T>> ExecuteRequest<T>(HttpMethod method, string path, params object[] segments)
        {
            var url = ResolveUrlSegments(path, segments.ToList());

            return await ExecuteRequest<T>(method, url);
        }

        private async Task<TwitterResponse<T>> ExecuteRequestImpl<T>(HttpRequestMessage request)
        {
            HttpResponseMessage response = null;
            try
            {
                var responseTask = _client.SendAsync(request);
                await responseTask;
                response = responseTask.Result;
            }
            catch (Exception e)
            {
                Debug.WriteLine("Error executing request: {0}", e);
                throw;
            }

            var twitterResponse = new TwitterResponse<T>(response);

            if (response.IsSuccessStatusCode)
                twitterResponse.Content = Deserialize<T>(twitterResponse.Response);
            else
                twitterResponse.Error = Deserialize<TwitterError>(twitterResponse.Response);

            return twitterResponse;
        }

        private static T TryAsyncResponse<T>(Func<T> action, out Exception exception)
        {
            exception = null;
            var entity = default(T);
            try
            {
                entity = action();
            }
            catch (Exception ex)
            {
                exception = ex;
            }
            return entity;
        }
    }
}

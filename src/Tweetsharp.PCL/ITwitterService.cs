using System;
using System.Globalization;
using System.Net.Http;
using Tweetsharp.PCL;

namespace TweetSharp
{
    public partial interface ITwitterService
    {
        void AuthenticateWith(string token, string tokenSecret);
        void AuthenticateWith(string consumerKey, string consumerSecret, string token, string tokenSecret);
        Uri GetAuthorizationUri(OAuthRequestToken oauth);
        Uri GetAuthorizationUri(OAuthRequestToken oauth, string callback);
        Uri GetAuthorizationUri(OAuthRequestToken oauth, CultureInfo culture);
        Uri GetAuthorizationUri(OAuthRequestToken oauth, string callback, CultureInfo culture);
        Uri GetAuthenticationUrl(OAuthRequestToken oauth);
        Uri GetAuthenticationUrl(OAuthRequestToken oauth, string callback);
        Uri GetAuthenticationUrl(OAuthRequestToken oauth, CultureInfo culture);
        Uri GetAuthenticationUrl(OAuthRequestToken oauth, string callback, CultureInfo culture);
        OAuthRequestToken GetRequestToken(string callback);
        OAuthRequestToken GetRequestToken();
        OAuthAccessToken GetAccessTokenWithXAuth(string username, string password);
        OAuthAccessToken GetAccessToken(OAuthRequestToken requestToken);
        OAuthAccessToken GetAccessToken(OAuthRequestToken requestToken, string verifier);
        string GetEchoRequest(string url);
        HttpRequestMessage PrepareEchoRequest(string realm = "http://api.twitter.com");
        IDeserializer Deserializer { get; set; }
        ISerializer Serializer { get; set; }
        T Deserialize<T>(ITwitterModel model) where T : ITwitterModel;
        T Deserialize<T>(string content);
        void CancelStreaming();

    }
}
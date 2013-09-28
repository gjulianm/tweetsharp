
namespace TweetSharp
{
    public partial interface ITwitterService
    {
        T Deserialize<T>(ITwitterModel model) where T : ITwitterModel;
        T Deserialize<T>(string content);
        void AuthenticateWith(string token, string tokenSecret);
        void AuthenticateWith(string consumerKey, string consumerSecret, string token, string tokenSecret);
    }
}

namespace TweetSharp
{
    public partial interface ITwitterService
    {
        T Deserialize<T>(ITwitterModel model) where T : ITwitterModel;
        T Deserialize<T>(string content);
    }
}
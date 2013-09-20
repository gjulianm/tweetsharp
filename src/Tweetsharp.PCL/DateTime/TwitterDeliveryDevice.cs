using System.Runtime.Serialization;

namespace TweetSharp
{
    /// <summary>
    /// Represents a delivery device for receiving updates from Twitter.
    /// </summary>
    [DataContract]
    public enum TwitterDeliveryDevice
    {
        /// <summary>
        /// No delivery device is used.
        /// </summary>
        [EnumMember]
        None,
        /// <summary>
        /// An SMS-capable delivery device is used.
        /// </summary>
        [EnumMember]
        Sms
    }
}
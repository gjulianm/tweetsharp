using System;
using System.Runtime.Serialization;
using Tweetsharp;
using Newtonsoft.Json;

namespace TweetSharp.Model
{

#if !Smartphone && !NET20
    [DataContract]
#endif
    [JsonObject(MemberSerialization.OptIn)]
    public class WhereOnEarthPlaceType : PropertyChangedBase
    {
        private int _code;
        private string _name;

#if !Smartphone && !NET20
        [DataMember]
#endif
        [JsonProperty("code")]
        public virtual int Code
        {
            get { return _code; }
            set
            {
                if (_code == value)
                {
                    return;
                }

                _code = value;
                OnPropertyChanged("Code");
            }
        }

#if !Smartphone && !NET20
        [DataMember]
#endif
        public virtual string Name
        {
            get { return _name; }
            set
            {
                if (_name == value)
                {
                    return;
                }

                _name = value;
                OnPropertyChanged("Name");
            }
        }
    }
}
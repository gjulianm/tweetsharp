using System;
using System.Collections.Generic;
using System.Globalization;
using System.Reflection;
using System.Runtime.Serialization;
using System.Threading;
using Tweetsharp.PCL;


namespace TweetSharp
{
    /// <summary>
    /// Represents a normalized date from the Twitter API. 
    /// </summary>
    [DataContract]
    public class TwitterDateTime : ITwitterModel
    {
        private static readonly IDictionary<string, string> _map =
            new Dictionary<string, string>();

        /// <summary>
        /// Gets or sets the Twitter-based date format.
        /// </summary>
        /// <value>The format.</value>
        public virtual TwitterDateFormat Format { get; private set; }

        /// <summary>
        /// Gets or sets the actual date time.
        /// </summary>
        /// <value>The date time.</value>
        public virtual DateTime DateTime { get; private set; }

        private static readonly object _lock = new object();

        /// <summary>
        /// Initializes a new instance of the <see cref="TwitterDateTime"/> class.
        /// </summary>
        /// <param name="dateTime">The date time.</param>
        /// <param name="format">The format.</param>
        public TwitterDateTime(DateTime dateTime, TwitterDateFormat format)
        {
            Format = format;
            DateTime = dateTime;
        }

        private static readonly IList<string> _names = new List<string>();

        /// <summary>
        /// Converts from date time.
        /// </summary>
        /// <param name="input">The input.</param>
        /// <param name="format">The format.</param>
        /// <returns></returns>
        public static string ConvertFromDateTime(DateTime input, TwitterDateFormat format)
        {
            EnsureDateFormatsAreMapped();

            var name = Enum.GetName(typeof(TwitterDateFormat), format);

            GetReadLockOnMap();
            var value = _map[name];
            ReleaseReadLockOnMap();

            value = value.Replace(" zzzzz", " +0000");

            var converted = input.ToString(value, CultureInfo.InvariantCulture);
            return converted;
        }

        /// <summary>
        /// Converts to date time.
        /// </summary>
        /// <param name="input">The input.</param>
        /// <returns></returns>
        public static DateTime ConvertToDateTime(string input)
        {
            EnsureDateFormatsAreMapped();
            GetReadLockOnMap();
            var formats = _map.Values;
            ReleaseReadLockOnMap();
            foreach (var format in formats)
            {
                DateTime date;
                if (DateTime.TryParseExact(input, format,
                                           CultureInfo.InvariantCulture,
                                           DateTimeStyles.AdjustToUniversal, out date))
                {
                    return date;
                }
            }

            return default(DateTime);
        }

        /// <summary>
        /// Converts to twitter date time.
        /// </summary>
        /// <param name="input">The input.</param>
        /// <returns></returns>
        public static TwitterDateTime ConvertToTwitterDateTime(string input)
        {
            EnsureDateFormatsAreMapped();
            GetReadLockOnMap();
            try
            {
                foreach (var format in _map)
                {
                    DateTime date;
                    if (DateTime.TryParseExact(input, format.Value,
                                               CultureInfo.InvariantCulture,
                                               DateTimeStyles.AdjustToUniversal, out date))
                    {
                        var kind = Enum.Parse(typeof(TwitterDateFormat), format.Key, true);
                        return new TwitterDateTime(date, (TwitterDateFormat)kind);
                    }
                }

                return default(TwitterDateTime);
            }
            finally
            {
                ReleaseReadLockOnMap();
            }
        }

        private static void EnsureDateFormatsAreMapped()
        {
            var type = typeof(TwitterDateFormat);

            EnsureEnumNamesAreMapped(type);
            var names = _names;

            GetReadLockOnMap();
            try
            {
                foreach (var name in names)
                {
                    if (_map.ContainsKey(name))
                    {
                        continue;
                    }
                    GetWriteLockOnMap();
                    try
                    {
                        var fi = typeof(TwitterDateFormat).GetField(name);
                        var attributes = fi.GetCustomAttributes(typeof(DescriptionAttribute), false);
                        var format = (DescriptionAttribute)attributes[0];

                        _map.Add(name, format.Description);
                    }
                    finally
                    {
                        ReleaseWriteLockOnMap();
                    }
                }
            }
            finally
            {
                ReleaseReadLockOnMap();
            }
        }


        private static void GetReadLockOnMap()
        {
            Monitor.Enter(_lock);
        }

        private static void ReleaseReadLockOnMap()
        {
            Monitor.Exit(_lock);
        }

        private static void GetWriteLockOnMap()
        {
            //already have exclusive access
        }

        private static void ReleaseWriteLockOnMap()
        {
            //will exit when we give up read lock
        }

        private static void EnsureEnumNamesAreMapped(Type type)
        {
            GetReadLockOnMap();
            GetWriteLockOnMap();
            try
            {
                var fields = type.GetFields(BindingFlags.Public | BindingFlags.Static);
                foreach (var field in fields)
                {
                    if (_names.Contains(field.Name))
                    {
                        continue;
                    }
                    _names.Add(field.Name);
                }
            }
            finally
            {
                ReleaseWriteLockOnMap();
                ReleaseReadLockOnMap();
            }
        }

        /// <summary>
        /// Returns a <see cref="System.String"/> that represents this instance.
        /// </summary>
        /// <returns>
        /// A <see cref="System.String"/> that represents this instance.
        /// </returns>
        public override string ToString()
        {
            return ConvertFromDateTime(DateTime, Format);
        }

        [DataMember]
        public virtual string RawSource { get; set; }
    }
}
using System;
using System.Globalization;
using Newtonsoft.Json;
using System.Collections.Generic;

namespace Gelf4Net
{
    public class GelfMessage : Dictionary<string, object>
    {

        private const string FacilityKey = "facility";
        private const string FileKey = "file";
        private const string FullMessageKey = "full_message";
        private const string HostKey = "host";
        private const string LevelKey = "level";
        private const string LineKey = "line";
        private const string ShortMessageKey = "short_message";
        private const string VersionKey = "version";
        private const string TimeStampKey = "timestamp";


        public string Facility
        {
            get { return PullStringValue(FacilityKey); }
            set { StoreValue(FacilityKey, value); }
        }

        public string File
        {
            get { return PullStringValue(FileKey); }
            set { StoreValue(FileKey, value); }
        }

        public string FullMessage
        {
            get { return PullStringValue(FullMessageKey); }
            set { StoreValue(FullMessageKey, value); }
        }
        
        public string Host
        {
            get { return PullStringValue(HostKey); }
            set { StoreValue(HostKey, value); }
        }
        
        public long Level
        {
            get
            {
                if (!this.ContainsKey(LevelKey))
                    return int.MinValue;

                return (long)this[LevelKey];
            }
            set { StoreValue(LevelKey, value); }
        }

        public string Line
        {
            get { return PullStringValue(LineKey); }
            set { StoreValue(LineKey, value); }
        }

        public string ShortMessage
        {
            get { return PullStringValue(ShortMessageKey); }
            set { StoreValue(ShortMessageKey, value); }
        }

        public DateTime TimeStamp
        {
            get
            {
                if (!this.ContainsKey(TimeStampKey))
                    return DateTime.MinValue;

                var val = this[TimeStampKey];
                if (_sentTimeStampAsString)
                {
                    double value;
                    var parsed = double.TryParse(val as string, NumberStyles.Any, CultureInfo.InvariantCulture, out value);
                    return parsed ? value.FromUnixTimestamp() : DateTime.MinValue;
                }

                return Convert.ToDouble(val).FromUnixTimestamp();
            }
            set
            {
                var timestamp = value.ToUnixTimestamp();
                StoreValue(TimeStampKey, timestamp);
                if (_sentTimeStampAsString)
                {
                    StoreValue(TimeStampKey, timestamp.ToString(CultureInfo.InvariantCulture));
                }
                
            }
        }

        public string Version
        {
            get { return PullStringValue(VersionKey); }
            set { StoreValue(VersionKey, value); }
        }

        private string PullStringValue(String key)
        {
            return ContainsKey(key) ? this[key].ToString() : string.Empty;
        }

        private void StoreValue(string key, object value)
        {
            if (!ContainsKey(key))
                Add(key, value);
            else
                this[key] = value;
        }

        private bool _sentTimeStampAsString;

        public GelfMessage(bool sendTimeStampAsString = false)
        {
            _sentTimeStampAsString = sendTimeStampAsString;
        }

        public GelfMessage()
        {

        }
    }
}
using System;
using System.Collections.Generic;
using InstaSharper.Converters.Json;
using InstaSharper.Helpers;
using Newtonsoft.Json;

namespace InstaSharper.Classes.Models
{
    public class RecentActivities : List<RecentActivity>{}

    public class RecentActivity
    {
        [JsonProperty("args")]
        private Args args;
        private DateTime? _timeStamp;


        public string Text { get => args.Text; }

        public string MediaId { get => args.MediaId; }

        public string MediaImage { get => args.MediaImage; }

        public long ProfileId { get => args.ProfileId; }

        public string ProfileImage { get => args.ProfileImage; }

        public DateTime TimeStamp
        {
            get
            {
                if (_timeStamp == null)
                {
                    _timeStamp = DateTimeHelper.FromUnixSeconds(args.TimeStamp);
                }

                return _timeStamp.Value;
            }
        }

        public object CommentId { get => args.CommentId; }


        [JsonProperty("type")]
        public int Type { get; set; }

        [JsonConverter(typeof(JsonPathConverter))]
        private class Args
        {
            [JsonProperty("text")]
            public string Text { get; set; }

            [JsonProperty("media[0].id")]
            public string MediaId { get; set; }

            [JsonProperty("media[0].image")]
            public string MediaImage { get; set; }

            [JsonProperty("profile_id")]
            public long ProfileId { get; set; }

            [JsonProperty("profile_image")]
            public string ProfileImage { get; set; }

            [JsonProperty("timestamp")]
            public string TimeStamp { get; set; }

            [JsonProperty("comment_ids")]
            public object CommentId { get; set; }
        }
    }
}
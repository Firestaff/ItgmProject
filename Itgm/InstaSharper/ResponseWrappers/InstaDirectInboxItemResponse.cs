﻿using System;
using InstaSharper.ResponseWrappers.BaseResponse;
using Newtonsoft.Json;

namespace InstaSharper.ResponseWrappers
{
    internal class InstaDirectInboxItemResponse : BaseStatusResponse
    {
        [JsonProperty("text")]
        public string Text { get; set; }

        [JsonProperty("user_id")]
        public string UserId { get; set; }

        [JsonProperty("timestamp")]
        public string TimeStamp { get; set; }

        [JsonProperty("item_id")]
        public string ItemId { get; set; }

        [JsonProperty("item_type")]
        public string ItemType { get; set; }

        [JsonProperty("media_share")]
        public InstaMediaItemResponse MediaShare { get; set; }

        [JsonProperty("media")]
        public InstaMediaItemResponse Media { get; set; }

        [JsonProperty("client_context")]
        public Guid ClientContext { get; set; }
    }
}
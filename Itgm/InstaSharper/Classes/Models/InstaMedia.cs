﻿using System;
using System.Collections.Generic;

namespace InstaSharper.Classes.Models
{
    public class InstaMedia
    {
        public DateTime TakenAt { get; set; }
        public string Pk { get; set; }

        public string InstaIdentifier { get; set; }

        public DateTime DeviceTimeStap { get; set; }
        public InstaMediaType MediaType { get; set; }

        public string Code { get; set; }

        public string ClientCacheKey { get; set; }
        public string FilterType { get; set; }


        public List<Image> Images { get; set; } = new List<Image>();


        public int Width { get; set; }


        public string Height { get; set; }


        public UserInfo User { get; set; }

        public string TrakingToken { get; set; }


        public int LikesCount { get; set; }


        public string NextMaxId { get; set; }


        public InstaCaption Caption { get; set; }

        public int CommentsCount { get; set; }

        public bool PhotoOfYou { get; set; }

        public bool HasLiked { get; set; }

        public List<InstaUserTag> Tags { get; set; } = new List<InstaUserTag>();

        public InstaUserList Likers { get; set; } = new InstaUserList();
    }
}
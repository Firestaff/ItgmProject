﻿namespace InstaSharper.Classes.Models
{
    public class InstaInlineFollow
    {
        public bool IsOutgoingRequest { get; set; }
        public bool IsFollowing { get; set; }
        public UserInfo User { get; set; }
    }
}
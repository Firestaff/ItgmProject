using Newtonsoft.Json;

namespace InstaSharper.Classes.Models
{
    public class UserInfo
    {
        [JsonProperty("username")]
        public string UserName { get; set; }

        [JsonProperty("has_anonymous_profile_picture")]
        public bool HasAnonymousProfilePicture { get; set; }

        public InstaFriendshipStatus FriendshipStatus { get; set; }

        [JsonProperty("profile_pic_url")]
        public string ProfilePicture
        {
            get => ProfileImage?.Url;
            set => ProfileImage= new Image(value);
        }

        [JsonProperty("hd_profile_pic_url_info")]
        public Image ProfileImage { get; set; }

        [JsonProperty("full_name")]
        public string FullName { get; set; }

        [JsonProperty("is_verified")]
        public bool IsVerified { get; set; }

        [JsonProperty("is_private")]
        public bool IsPrivate { get; set; }

        [JsonProperty("following_count")]
        public int FollowingCount { get; set; }

        [JsonProperty("follower_count")]
        public int FollowerCount { get; set; }

        [JsonProperty("pk")]
        public string Id { get; set; }

        [JsonProperty("usertags_count")]
        public int UserTagsCount { get; set; }

        [JsonProperty("media_count")]
        public int MediaCount { get; set; }
    }
}
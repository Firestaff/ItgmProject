using System;
using InstaSharper.Classes.Models;
using InstaSharper.ResponseWrappers;

namespace InstaSharper.Converters
{
    internal class InstaUsersConverter : IObjectConverter<UserInfo, InstaUserResponse>
    {
        public InstaUserResponse SourceObject { get; set; }

        public UserInfo Convert()
        {
            if (SourceObject == null) throw new ArgumentNullException($"Source object");
            var user = new UserInfo();
            if (!string.IsNullOrEmpty(SourceObject.FullName)) user.FullName = SourceObject.FullName;
            if (!string.IsNullOrEmpty(SourceObject.ProfilePicture)) user.ProfilePicture = SourceObject.ProfilePicture;
            if (!string.IsNullOrEmpty(SourceObject.UserName)) user.UserName = SourceObject.UserName;
            if (!string.IsNullOrEmpty(SourceObject.Pk)) user.Id = SourceObject.Pk;
            if (SourceObject.Friendship != null)
                user.FriendshipStatus = ConvertersFabric.GetFriendShipStatusConverter(SourceObject.Friendship).Convert();
            user.HasAnonymousProfilePicture = SourceObject.HasAnonymousProfilePicture;
            user.IsVerified = SourceObject.IsVerified;
            user.IsPrivate = SourceObject.IsPrivate;
            return user;
        }
    }
}
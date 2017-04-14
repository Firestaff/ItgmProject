using InstaSharper.Classes.Models;
using InstaSharper.Helpers;
using InstaSharper.ResponseWrappers;

namespace InstaSharper.Converters
{
    internal class InstaRecentActivityConverter :
        IObjectConverter<RecentActivity, InstaRecentActivityFeedResponse>
    {
        public InstaRecentActivityFeedResponse SourceObject { get; set; }

        public RecentActivity Convert()
        {
            var activityStory = new RecentActivity
            {
                //Pk = SourceObject.Pk,
                Type = SourceObject.Type,
                //ProfileId = SourceObject.Args.ProfileId,
                //ProfileImage = SourceObject.Args.ProfileImage,
                //Text = SourceObject.Args.Text,
                //TimeStamp = DateTimeHelper.UnixTimestampToDateTime(SourceObject.Args.TimeStamp)
            };
            return activityStory;
        }
    }
}
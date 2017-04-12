using InstaSharper.Classes.Models;
using InstaSharper.Helpers;
using InstaSharper.ResponseWrappers;

namespace InstaSharper.Converters
{
    internal class InstaCaptionConverter : IObjectConverter<InstaCaption, InstaCaptionResponse>
    {
        public InstaCaptionResponse SourceObject { get; set; }

        public InstaCaption Convert()
        {
            if (SourceObject == null)
            {
                return null;
            }

            var caption = new InstaCaption
            {
                Pk = SourceObject.Pk,
                CreatedAt = DateTimeHelper.FromUnixSeconds(SourceObject.CreatedAtUnixLike),
                CreatedAtUtc = DateTimeHelper.FromUnixSeconds(SourceObject.CreatedAtUtcUnixLike),
                MediaId = SourceObject.MediaId,
                Text = SourceObject.Text,
                User = ConvertersFabric.GetUserConverter(SourceObject.User).Convert(),
                UserId = SourceObject.UserId
            };
            return caption;
        }
    }
}
using System;
using InstaSharper.Classes.Models;
using InstaSharper.Helpers;
using InstaSharper.ResponseWrappers;

namespace InstaSharper.Converters
{
    internal class InstaCommentConverter
        : IObjectConverter<InstaComment, InstaCommentResponse>
    {
        public InstaCommentResponse SourceObject { get; set; }

        public InstaComment Convert()
        {
            var comment = new InstaComment
            {
                BitFlags = SourceObject.BitFlags,
                ContentType = (InstaContentType)Enum.Parse(typeof(InstaContentType), SourceObject.ContentType, true),
                CreatedAt = DateTimeHelper.FromUnixSeconds(SourceObject.CreatedAt),
                CreatedAtUtc = DateTimeHelper.FromUnixSeconds(SourceObject.CreatedAtUtc),
                LikesCount = SourceObject.LikesCount,
                Pk = SourceObject.Pk,
                Status = SourceObject.Status,
                Text = SourceObject.Text,
                Type = SourceObject.Type,
                UserId = SourceObject.UserId,
                User = ConvertersFabric.GetUserConverter(SourceObject.User).Convert()
            };
            return comment;
        }
    }
}
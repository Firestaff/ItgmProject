﻿using InstaSharper.Classes.Models;
using InstaSharper.Helpers;
using InstaSharper.ResponseWrappers;

namespace InstaSharper.Converters
{
    internal class InstaDirectThreadItemConverter : IObjectConverter<InstaDirectInboxItem, InstaDirectInboxItemResponse>
    {
        public InstaDirectInboxItemResponse SourceObject { get; set; }

        public InstaDirectInboxItem Convert()
        {
            var threadItem = new InstaDirectInboxItem
            {
                ClientContext = SourceObject.ClientContext,
                ItemId = SourceObject.ItemId
            };
            switch (SourceObject.ItemType)
            {
                case "text":
                    threadItem.ItemType = InstaDirectThreadItemType.Text;
                    break;
                case "media_share":
                    threadItem.ItemType = InstaDirectThreadItemType.MediaShare;
                    break;
            }
            threadItem.Text = SourceObject.Text;
            threadItem.TimeStamp = DateTimeHelper.FromUnixSeconds(SourceObject.TimeStamp);
            threadItem.UserId = SourceObject.UserId;
            if (SourceObject.MediaShare == null) return threadItem;
            var converter = ConvertersFabric.GetSingleMediaConverter(SourceObject.MediaShare);
            threadItem.MediaShare = converter.Convert();
            return threadItem;
        }
    }
}
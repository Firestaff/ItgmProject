﻿using System.Collections.Generic;
using System.Linq;
using InstaSharper.Classes.Models;
using InstaSharper.Helpers;
using InstaSharper.ResponseWrappers;

namespace InstaSharper.Converters
{
    internal class InstaDirectThreadConverter : IObjectConverter<InstaDirectInboxThread, InstaDirectInboxThreadResponse>
    {
        public InstaDirectInboxThreadResponse SourceObject { get; set; }

        public InstaDirectInboxThread Convert()
        {
            var thread = new InstaDirectInboxThread();
            thread.Canonical = SourceObject.Canonical;
            thread.HasNewer = SourceObject.HasNewer;
            thread.HasOlder = SourceObject.HasOlder;
            thread.IsSpam = SourceObject.IsSpam;
            thread.Muted = SourceObject.Muted;
            thread.Named = SourceObject.Named;
            thread.Pending = SourceObject.Pending;
            thread.VieweId = SourceObject.VieweId;
            thread.LastActivity = DateTimeHelper.UnixTimestampMilisecondsToDateTime(SourceObject.LastActivity);
            thread.ThreadId = SourceObject.ThreadId;
            thread.OldestCursor = thread.OldestCursor;
            thread.ThreadType = SourceObject.ThreadType;
            thread.Title = SourceObject.Title;
            if (SourceObject.Inviter != null)
            {
                var userConverter = ConvertersFabric.GetUserConverter(SourceObject.Inviter);
                thread.Inviter = userConverter.Convert();
            }

            thread.Users = new InstaUserList();
            if (SourceObject.Users != null && SourceObject.Users.Count > 0)
            {
                foreach (var user in SourceObject.Users)
                {
                    var converter = ConvertersFabric.GetUserConverter(user);
                    thread.Users.Add(converter.Convert());
                }
            }
            if (SourceObject.Items != null && SourceObject.Items.Count > 0)
            {
                thread.Items = new List<InstaDirectInboxItem>();
                foreach (var item in SourceObject.Items)
                {
                    var converter = ConvertersFabric.GetDirectThreadItemConverter(item);
                    var threadItem = converter.Convert();
                    threadItem.UserName = thread.Users.SingleOrDefault(u => u.Id == threadItem.UserId)?.UserName;
                    thread.Items.Add(threadItem);
                }
            }
            return thread;
        }
    }
}
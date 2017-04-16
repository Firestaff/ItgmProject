using System;

namespace InstaSharper.Classes.Models
{
    public class InstaDirectInboxItem
    {
        public string Text { get; set; }

        public string UserId { get; set; }

        public string UserName { get; set; }


        public DateTime TimeStamp { get; set; }


        public string ItemId { get; set; }


        public InstaDirectThreadItemType ItemType { get; set; }


        public InstaMedia MediaShare { get; set; }

        public Guid ClientContext { get; set; }
    }
}
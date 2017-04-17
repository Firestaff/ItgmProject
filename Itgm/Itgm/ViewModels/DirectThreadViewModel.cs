using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using InstaSharper.Classes.Models;
using Itgm.Interfaces;

namespace Itgm.ViewModels
{
    public class DirectThreadViewModel : BaseViewModel
    {
        private IService _service;
        private string _title;
        private string _lastMessage;
        private bool _hasUnseen;
        private bool _isLongProcessStarted;

        public DirectThreadViewModel(InstaDirectInboxThread thread, IService service)
        {
            _service = service;
            ThreadId = thread.ThreadId;
            thread.Users.ForEach(u => Users.Add(u));

            UpdateThreadPreview(thread, false);
        }

        #region Properties
        public ObservableCollection<InstaDirectInboxItem> Messages { get; private set; } 
            = new ObservableCollection<InstaDirectInboxItem>();

        public List<UserInfo> Users { get; set; } = new List<UserInfo>();

        public bool HasUnseen
        {
            get => _hasUnseen;
            set
            {
                if (_hasUnseen == value)
                {
                    return;
                }

                _hasUnseen = value;
                OnPropertyChanged("HasUnseen");
            }
        }

        public string LastMessage
        {
            get
            {
                return _lastMessage;
            }
            private set
            {
                if (_lastMessage == value)
                {
                    return;
                }

                _lastMessage = value;
                OnPropertyChanged("LastMessage");
            }
        }

        public bool IsLongProcessStarted
        {
            get
            {
                return _isLongProcessStarted;
            }
            private set
            {
                if (_isLongProcessStarted == value)
                {
                    return;
                }

                _isLongProcessStarted = value;
                OnPropertyChanged("IsLongProcessStarted");
            }
        }

        public string ThreadId { get; private set; }

        public string Title
        {
            get => _title;
            private set
            {
                if (_title == value)
                {
                    return;
                }

                _title = value;
                OnPropertyChanged("Title");
            }
        }
        #endregion

        #region Commands
        #endregion

        public override void InitializeViewModel()
        {
        }

        public override void ClearViewModel()
        {
            Messages.Clear();
            Users.Clear();
        }

        public async override void UpdateViewModel()
        {
            await LoadMessagesAsync(true);
        }

        public async Task LoadMessagesAsync(bool onlyNew)
        {
            if (IsLongProcessStarted)
            {
                return;
            }
            IsLongProcessStarted = true;

            var fromId = Messages.FirstOrDefault()?.ItemId;
            if (fromId != null)
            {
                bool isIntersect = false;
                while (!isIntersect)
                {
                    var newMessages = await _service.GetDirectThreadAsync(ThreadId);

                    if (newMessages.Items == null || newMessages.Items.Count == 0)
                    {
                        isIntersect = true;
                    }

                    foreach (var newItem in newMessages.Items)
                    {
                        if (!Messages.Any(m => m.ItemId == newItem.ItemId))
                        {
                            InsertMessage(newItem, true);
                        }
                        else
                        {
                            isIntersect = true;
                            break;
                        }
                    }
                }
            }

            if (!onlyNew || Messages.Count == 0)
            {
                var result = await _service.GetDirectThreadAsync(ThreadId, fromId);
                result.Items?.ForEach(i => InsertMessage(i, false));
            }

            HasUnseen = false;
            IsLongProcessStarted = false;
        }

        public void UpdateThreadPreview(InstaDirectInboxThread thread, bool hasUnseen)
        {
            Title = thread.Title;
            HasUnseen = hasUnseen;

            var item = thread.Items[0];
            LastMessage = item.Text ?? (item.MediaShare != null ? "Media" : "");
        }

        private void InsertMessage(InstaDirectInboxItem item, bool inTheEnd)
        {
            var index = inTheEnd ? Messages.Count : 0;
            if (item.Text == null && item.MediaShare == null)
            {
                return;
            }

            if (item.UserName == null)
            {
                item.UserName = _service.LoggedUser.UserName;
            }

            Messages.Insert(index, item);
        }
    }
}

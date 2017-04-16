using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using System.Windows;
using System.Windows.Data;
using System.Windows.Input;
using InstaSharper.Classes.Models;
using Itgm.Interfaces;
using Itgm.ViewModels;
using Microsoft.Expression.Interactivity.Core;

namespace Itgm.ViewModels
{
    public class DirectThreadViewModel : BaseViewModel
    {
        private InstaDirectInboxThread _thread;
        private IService _service;
        private string _title;
        private bool _hasUnseen;
        private bool _isLongProcessStarted;

        public DirectThreadViewModel(InstaDirectInboxThread thread, IService service)
        {
            _thread = thread;
            _service = service;

            UpdateThreadCommand = new ActionCommand(UpdateViewModel);

            InitializeViewModel();
        }

        #region Properties
        public ObservableCollection<InstaDirectInboxItem> Messages { get; set; } 
            = new ObservableCollection<InstaDirectInboxItem>();

        public ObservableCollection<UserInfo> Users { get; set; }
            = new ObservableCollection<UserInfo>();

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

        public string LastMessage { get => Messages.LastOrDefault(m => m.Text != null)?.Text; }

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
        public ICommand UpdateThreadCommand { get; private set; }
        #endregion

        public override void InitializeViewModel()
        {
            Title = _thread.Title;
            ThreadId = _thread.ThreadId;

            _thread.Items.ForEach(i => InsertMessage(i));
            _thread.Users.ForEach(u => Users.Add(u));
        }

        public override void ClearViewModel()
        {
            Messages.Clear();
            Users.Clear();
        }

        public async override void UpdateViewModel()
        {
            await UpdateViewModelAsync(false);
        }

        public async Task UpdateViewModelAsync(bool hasUnseen)
        {
            if (IsLongProcessStarted)
            {
                return;
            }
            IsLongProcessStarted = true;


            _thread = await _service.GetDirectThreadAsync(ThreadId);
            ClearViewModel();
            InitializeViewModel();

            HasUnseen = hasUnseen;
            OnPropertyChanged("LastMessage");

            IsLongProcessStarted = false;
        }

        private void InsertMessage(InstaDirectInboxItem item)
        {
            if (item.Text == null && item.MediaShare == null)
            {
                return;
            }

            if (item.UserName == null)
            {
                item.UserName = _service.LoggedUser.UserName;
            }

            Messages.Insert(0, item);
        }
    }
}

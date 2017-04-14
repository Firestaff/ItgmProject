using Microsoft.Expression.Interactivity.Core;
using System;
using System.Windows.Input;
using Itgm.Classes;
using Itgm.Interfaces;
using InstaSharper.Classes.Models;
using System.Collections.Generic;

namespace Itgm.ViewModels
{
    /// <summary>
    /// Вью модель для основного контента.
    /// </summary>
    public class ContentViewModel : ResolvingViewModel
    {
        #region Fields
        private BaseViewModel _currentContent;
        private ContentType _contentIndex = ContentType.None;
        private Dictionary<ContentType, BaseViewModel> _contents = new Dictionary<ContentType, BaseViewModel>();
        public UserInfo _user;
        #endregion

        public ContentViewModel(IService service, Action<ViewTypes> resolveView) : base(service, resolveView)
        {
            ReloginCommand = new ActionCommand(OnRelogin);
            SwitchContentCommand = new ActionCommand(OnSwitchContent);
            UpdateCommand = new ActionCommand(OnUpdateContent);
        }

        #region Properties

        /// <summary>
        /// Активное представление.
        /// </summary>
        public BaseViewModel CurrentContent
        {
            get
            {
                return _currentContent;
            }
            set
            {
                if (_currentContent != value)
                {
                    _currentContent = value;
                    OnPropertyChanged("CurrentContent");
                }
            }
        }

        /// <summary>
        /// Активное представление.
        /// </summary>
        public ContentType ContentIndex
        {
            get
            {
                return _contentIndex;
            }
            set
            {
                if (_contentIndex != value)
                {
                    _contentIndex = value;
                    OnPropertyChanged("ContentIndex");

                    if (_contents.TryGetValue(_contentIndex, out var content))
                    {
                        CurrentContent = content;
                        return;
                    }

                    switch (_contentIndex)
                    {
                        case ContentType.MediaComments:
                            _contents[_contentIndex] = new CommentsViewModel(_service);
                            CurrentContent = _contents[_contentIndex];
                            break;

                        case ContentType.RecentActivity:
                            _contents[_contentIndex] = new RecentViewModel(_service);
                            CurrentContent = _contents[_contentIndex];
                            break;

                        case ContentType.None:
                            _contents[_contentIndex] = null;
                            CurrentContent = _contents[_contentIndex];
                            break;
                    }
                }
            }
        }

        /// <summary>
        /// Текущий пользователь.
        /// </summary>
        public UserInfo User
        {
            get
            {
                return _user;
            }
            set
            {
                if (_user != value)
                {
                    _user = value;
                    OnPropertyChanged("User");
                }
            }
        }

        #endregion

        #region Commands

        /// <summary>
        /// Команда выхода для текущего пользователя.
        /// Переводит на представление авторизации.
        /// </summary>
        public ICommand ReloginCommand
        {
            get; private set;
        }

        /// <summary>
        /// Команда переключения контента.
        /// </summary>
        public ICommand SwitchContentCommand
        {
            get; private set;
        }

        /// <summary>
        /// Команда обновления контента.
        /// </summary>
        public ICommand UpdateCommand
        {
            get; private set;
        }

        #endregion

        #region Methods

        /// <summary>
        /// Инициализирует вью модель при активации.
        /// </summary>
        public override void InitializeViewModel()
        {
            User = _service.LoggedUser;
            ContentIndex = ContentType.RecentActivity;
        }

        /// <summary>
        /// Вызывает отрисовку представления получения пин-кода.
        /// </summary>
        private void OnRelogin()
        {
            ResolveNewView(ViewTypes.Auth);
        }

        /// <summary>
        /// Переключает контент.
        /// </summary>
        private void OnSwitchContent()
        {
            //if (_currentContent == null ||
            //    _currentContent == _graphViewModel)
            //{
            //    CurrentContent = _tweetsViewModel;
            //}
            //else
            //{
            //    CurrentContent = _graphViewModel;
            //}
        }

        /// <summary>
        /// Переключает контент.
        /// </summary>
        private void OnUpdateContent()
        {
            CurrentContent.UpdateViewModel();
        }

        #endregion

        public enum ContentType
        {
            RecentActivity = 0,
            MediaComments = 1,
            Direct = 2,
            None = 3,
        }
    }
}

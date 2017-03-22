using Microsoft.Expression.Interactivity.Core;
using System;
using System.Windows.Input;
using Itgm.Classes;
using Itgm.Interfaces;
using InstaSharper.Classes.Models;

namespace Itgm.ViewModels
{
    /// <summary>
    /// Вью модель для основного контента.
    /// </summary>
    public class ContentViewModel : ResolvingViewModel
    {
        #region Fields

        /// <summary>
        /// Активное представление.
        /// </summary>
        private BaseViewModel _currentContent;

        /// <summary>
        /// Вью модель для твитов.
        /// </summary>
        private TweetsViewModel _tweetsViewModel;

        /// <summary>
        /// Текущий пользователь.
        /// </summary>
        public InstaUser _user;

        #endregion

        /// <summary>
        /// Создание вью модели.
        /// </summary>
        /// <param name="service">Сервис.</param>
        /// <param name="resolveView">Метод отрисовки нового представления.</param>
        public ContentViewModel(IService service, Action<ViewTypes> resolveView) : base(service, resolveView)
        {
            _tweetsViewModel = new TweetsViewModel(service);

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
        /// Текущий пользователь.
        /// </summary>
        public InstaUser User
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
            User = _service.GetUserInfo();

            _tweetsViewModel.InitializeViewModel();

            CurrentContent = _tweetsViewModel;
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
    }
}

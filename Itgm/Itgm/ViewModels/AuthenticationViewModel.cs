using Microsoft.Expression.Interactivity.Core;
using System;
using System.Net;
using System.Windows.Input;
using Itgm.Classes;
using Itgm.Interfaces;


namespace Itgm.ViewModels
{
    /// <summary>
    /// Вью модель для страницы авторизации.
    /// </summary>
    public class AuthenticationViewModel : ResolvingViewModel
    {
        #region Fields

        /// <summary>
        /// Состояние авторизации для формы.
        /// </summary>
        private WebExceptionStatus _authenticationState;

        /// <summary>
        /// Показывает запущена ли длительная операция.
        /// </summary>
        private bool _isLongProcessStarted;

        /// <summary>
        /// Логин.
        /// </summary>
        private string _login;

        /// <summary>
        /// Пароль.
        /// </summary>
        private string _password;

        #endregion

        /// <summary>
        /// Создание вью модели.
        /// </summary>
        /// <param name="service">Сервис.</param>
        /// <param name="resolveView">Метод отрисовки нового представления.</param>
        public AuthenticationViewModel(IService service, Action<ViewTypes> resolveView) : base(service, resolveView)
        {
            EnterCredentialsCommand = new ActionCommand(OnEnterCredentialsAsync);
        }

        #region Properties

        /// <summary>
        /// Логин.
        /// </summary>
        public string Login
        {
            get
            {
                return _login;
            }
            set
            {
                if (_login != value)
                {
                    _login = value;
                    OnPropertyChanged("Login");
                }
            }
        }

        /// <summary>
        /// Пароль.
        /// </summary>
        public string Password
        {
            get
            {
                return _password;
            }
            set
            {
                if (_password != value)
                {
                    _password = value;
                    OnPropertyChanged("Password");
                }
            }
        }

        /// <summary>
        /// Состояние авторизации для формы.
        /// </summary>
        public WebExceptionStatus AuthenticationState
        {
            get
            {
                return _authenticationState;
            }
            set
            {
                if (_authenticationState != value)
                {
                    _authenticationState = value;
                    OnPropertyChanged("AuthenticationState");
                }
            }
        }

        /// <summary>
        /// Показывает запущена ли длительная операция.
        /// </summary>
        public bool IsLongProcessStarted
        {
            get
            {
                return _isLongProcessStarted;
            }
            set
            {
                if (_isLongProcessStarted != value)
                {
                    _isLongProcessStarted = value;
                    OnPropertyChanged("IsLongProcessStarted");
                }
            }
        }

        #endregion

        #region Commands

        /// <summary>
        /// Команда ввода параметров авторизации.
        /// </summary>
        public ICommand EnterCredentialsCommand
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
            Login = null;
            Password = null;
            IsLongProcessStarted = false;
            AuthenticationState = WebExceptionStatus.Success;
        }

        /// <summary>
        /// Передает введенные данные в сервис и получает результат запроса авторизации.
        /// </summary>
        private async void OnEnterCredentialsAsync()
        {

            if (string.IsNullOrWhiteSpace(_login) || 
                string.IsNullOrWhiteSpace(_password))
            {
                return;
            }

            IsLongProcessStarted = true;
            await _service.LoginAsync(_login, _password);
            IsLongProcessStarted = false;

            AuthenticationState = _service.AuthenticationState;
            if (AuthenticationState == WebExceptionStatus.Success)
            {
                ResolveNewView(ViewTypes.Content);
            }
        }
        #endregion
    }
}

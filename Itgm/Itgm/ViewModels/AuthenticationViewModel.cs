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
        /// Пин-код.
        /// </summary>
        private string _pinCode;

        #endregion

        /// <summary>
        /// Создание вью модели.
        /// </summary>
        /// <param name="service">Сервис.</param>
        /// <param name="resolveView">Метод отрисовки нового представления.</param>
        public AuthenticationViewModel(IService service, Action<ViewTypes> resolveView) : base(service, resolveView)
        {
            EnterPinCodeCommand = new ActionCommand(OnEnterPinCodeAsync);
            GetNewPincodeCommand = new ActionCommand(OnGetPinCodeAsync);
        }

        #region Properties

        /// <summary>
        /// Пин-код.
        /// </summary>
        public string PinCode
        {
            get
            {
                return _pinCode;
            }
            set
            {
                if (_pinCode != value)
                {
                    if (AuthenticationState == WebExceptionStatus.TrustFailure)
                    {
                        AuthenticationState = 0;
                    }

                    _pinCode = value;
                    OnPropertyChanged("PinCode");
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
        /// Команда ввода пин-кода.
        /// </summary>
        public ICommand EnterPinCodeCommand
        {
            get; private set;
        }

        /// <summary>
        /// Команда получения пин-кода от твиттера.
        /// </summary>
        public ICommand GetNewPincodeCommand
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
            PinCode = null;
            IsLongProcessStarted = false;
            AuthenticationState = WebExceptionStatus.Success;
        }

        /// <summary>
        /// Передает пин-код в сервис и получает результат запроса авторизации.
        /// </summary>
        private async void OnEnterPinCodeAsync()
        {
            if (string.IsNullOrWhiteSpace(PinCode))
            {
                return;
            }

            IsLongProcessStarted = true;
            await _service.AuthenticateAsync(PinCode);
            IsLongProcessStarted = false;

            AuthenticationState = _service.AuthenticationState;
            if (AuthenticationState == WebExceptionStatus.Success)
            {
                ResolveNewView(ViewTypes.Content);
            }
        }

        /// <summary>
        /// Вызывает запрос на получение пин-кода из твиттера.
        /// </summary>
        private async void OnGetPinCodeAsync()
        {
            IsLongProcessStarted = true;
            await _service.GetPincodeAsync();
            IsLongProcessStarted = false;

            AuthenticationState = _service.AuthenticationState;
        }

        #endregion
    }
}

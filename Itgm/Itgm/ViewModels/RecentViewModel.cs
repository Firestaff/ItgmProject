using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;
using Microsoft.Expression.Interactivity.Core;
using Itgm.Interfaces;
using InstaSharper.Classes.Models;
using System.Collections.Generic;
using System.Timers;
using System.Windows;

namespace Itgm.ViewModels
{
    /// <summary>
    /// Вью модель для отображения твитов.
    /// </summary>
    public class RecentViewModel : ResolvingViewModel
    {
        /// <summary>
        /// Показывает запущена ли длительная операция.
        /// </summary>
        private MediaViewModel _currentMedia;

        /// <summary>
        /// Показывает запущена ли длительная операция.
        /// </summary>
        private bool _isLongProcessStarted;

        private Timer _timer;

        /// <summary>
        /// Создание вью модели.
        /// </summary>
        /// <param name="service">Сервис.</param>
        public RecentViewModel(IService service) : base(service, null)
        {
            LoadMediasCommand = new ActionCommand(() => LoadActivityAsync(false));
            UpdateMediasCommand = new ActionCommand(UpdateActivity);

            InitializeViewModel();
        }

        private void Timer_Elapsed(object sender, ElapsedEventArgs e)
        {
            Application.Current.Dispatcher.Invoke(UpdateViewModel);
        }

        #region Properties
        /// <summary>
        /// Устанавливает текущий пост.
        /// </summary>
        public MediaViewModel CurrentMedia
        {
            get
            {
                return _currentMedia;
            }
            set
            {
                if (_currentMedia == value)
                {
                    return;
                }

                _currentMedia?.StopTimer();
                _currentMedia = value;
                _currentMedia?.UpdateComments();
                OnPropertyChanged("CurrentMedia");
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
            private set
            {
                if (_isLongProcessStarted == value)
                {
                    return;
                }

                if (value == true)
                {
                    _timer.Stop();
                }
                else
                {
                    _timer.Start();
                }

                _isLongProcessStarted = value;
                OnPropertyChanged("IsLongProcessStarted");
            }
        }

        /// <summary>
        /// Коллекция твитов.
        /// </summary>
        public ObservableCollection<MediaViewModel> Activity { get; private set; } 
            = new ObservableCollection<MediaViewModel>();

        #endregion

        #region Commands
        /// <summary>
        /// Команда подгрузки постов.
        /// </summary>
        public ICommand LoadMediasCommand { get; private set; }

        /// <summary>
        /// Команда перезагрузки комментов.
        /// </summary>
        public ICommand UpdateMediasCommand { get; private set; }
        #endregion

        #region Methods

        /// <summary>
        /// Инициализирует вью модель при активации.
        /// </summary>
        public override void InitializeViewModel()
        {
            _timer = new Timer(60000);
            _timer.Elapsed += Timer_Elapsed;

            LoadActivityAsync(false);
        }

        /// <summary>
        /// Очищает вью модель.
        /// </summary>
        public override void ClearViewModel()
        {
            CurrentMedia = null;

            if (_timer != null)
            {
                _timer.Stop();
                _timer.Elapsed -= Timer_Elapsed;
                _timer.Dispose();
            }

            Activity.Clear();
        }

        /// <summary>
        /// Обновляет вью модель.
        /// </summary>
        public override void UpdateViewModel()
        {
            if (IsLongProcessStarted)
            {
                return;
            }

            LoadActivityAsync(true);
        }

        private async void LoadActivityAsync(bool onlyNew)
        {
            // Останавливаем новые запросы, пока ожидаем хотя бы один запущенный
            if (IsLongProcessStarted)
            {
                return;
            }

            IsLongProcessStarted = true;

            var result = await _service.GetRecentActivityAsync(onlyNew);
            result.Reverse();
            result.ForEach(a => Activity.Insert(0, new MediaViewModel(null, _service)));

            //if (CurrentMedia == null)
            //{
            //    CurrentMedia = Activity.FirstOrDefault();
            //}

            IsLongProcessStarted = false;
        }

        private void UpdateActivity()
        {
            ClearViewModel();
            InitializeViewModel();
        }
        #endregion
    }
}

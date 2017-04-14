using System.Collections.ObjectModel;
using System.Windows.Input;
using System.Timers;
using System.Windows;
using Microsoft.Expression.Interactivity.Core;
using Itgm.Interfaces;

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
        private bool _isLongProcessStarted;

        private Timer _timer;

        /// <summary>
        /// Создание вью модели.
        /// </summary>
        /// <param name="service">Сервис.</param>
        public RecentViewModel(IService service) : base(service, null)
        {
            LoadActivitiesCommand = new ActionCommand(() => LoadActivityAsync(true));
            UpdateActivitiesCommand = new ActionCommand(UpdateViewModel);

            InitializeViewModel();
        }

        private void Timer_Elapsed(object sender, ElapsedEventArgs e)
        {
            Application.Current.Dispatcher.Invoke(() => LoadActivityAsync(true));
        }

        #region Properties
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
        public ObservableCollection<ActivityViewModel> Activity { get; private set; } 
            = new ObservableCollection<ActivityViewModel>();

        #endregion

        #region Commands
        /// <summary>
        /// Команда подгрузки постов.
        /// </summary>
        public ICommand LoadActivitiesCommand { get; private set; }

        /// <summary>
        /// Команда перезагрузки комментов.
        /// </summary>
        public ICommand UpdateActivitiesCommand { get; private set; }
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

            ClearViewModel();
            InitializeViewModel();
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
            result.ForEach(a => 
            {
                if (a.CommentId != null && a.Type == 1)
                {
                    Activity.Insert(0, new ActivityViewModel(a));
                }
            });

            IsLongProcessStarted = false;
        }
        #endregion
    }
}

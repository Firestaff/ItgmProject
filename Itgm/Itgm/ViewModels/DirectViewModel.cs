using System.Collections.ObjectModel;
using System.Linq;
using System.Timers;
using System.Windows;
using System.Windows.Input;

using Microsoft.Expression.Interactivity.Core;
using Itgm.Interfaces;


namespace Itgm.ViewModels
{
    /// <summary>
    /// Вью модель для отображения твитов.
    /// </summary>
    public class DirectViewModel : ResolvingViewModel
    {
        /// <summary>
        /// Показывает запущена ли длительная операция.
        /// </summary>
        private bool _isLongProcessStarted;
        private int _pendingRequestsCount; 
        private long _unseenCount;
        private DirectThreadViewModel _currentThread;

        private Timer _timer;

        /// <summary>
        /// Создание вью модели.
        /// </summary>
        /// <param name="service">Сервис.</param>
        public DirectViewModel(IService service) : base(service, null)
        {
            UpdateDirectCommand = new ActionCommand(LoadDirectAsync);

            LoadMessagesCommand = new ActionCommand(LoadMessagesAsync);
            UpdateMessagesCommand = new ActionCommand(UpdateMessagesAsync);

            InitializeViewModel();
        }

        private void Timer_Elapsed(object sender, ElapsedEventArgs e)
        {
            Application.Current.Dispatcher.Invoke(LoadDirectAsync);
        }

        #region Properties
        public DirectThreadViewModel CurrentThread
        {
            get
            {
                return _currentThread;
            }
            set
            {
                if (_currentThread == value)
                {
                    return;
                }

                _currentThread = value;
                _currentThread?.UpdateViewModel();
                OnPropertyChanged("CurrentThread");
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

        public int PendingRequestsCount
        {
            get
            {
                return _pendingRequestsCount;
            }
            private set
            {
                if (_pendingRequestsCount == value)
                {
                    return;
                }

                _pendingRequestsCount = value;
                OnPropertyChanged("PendingRequestsCount");
            }
        }
        
        public long UnseenCount
        {
            get
            {
                return _unseenCount;
            }
            private set
            {
                if (_unseenCount == value)
                {
                    return;
                }

                _unseenCount = value;
                OnPropertyChanged("UnseenCount");
            }
        }

        public ObservableCollection<DirectThreadViewModel> DirectThreads { get; private set; } 
            = new ObservableCollection<DirectThreadViewModel>();

        #endregion

        #region Commands
        public ICommand UpdateDirectCommand { get; private set; }
        public ICommand LoadMessagesCommand { get; private set; }
        public ICommand UpdateMessagesCommand { get; private set; }
        #endregion

        #region Methods

        /// <summary>
        /// Инициализирует вью модель при активации.
        /// </summary>
        public override void InitializeViewModel()
        {
            _timer = new Timer(60000);
            _timer.Elapsed += Timer_Elapsed;

            LoadDirectAsync();
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

            DirectThreads.Clear();
        }

        /// <summary>
        /// Обновляет вью модель.
        /// </summary>
        public override void UpdateViewModel()
        {
        }

        private async void LoadDirectAsync()
        {
            // Останавливаем новые запросы, пока ожидаем хотя бы один запущенный
            if (IsLongProcessStarted)
            {
                return;
            }

            IsLongProcessStarted = true;

            var direct = await _service.GetDirectAsync();
            var inbox = direct.Inbox;
            var threads = inbox.Threads;

            PendingRequestsCount = direct.PendingRequestsCount;
            UnseenCount = inbox.UnseenCount;

            var itemsCount = DirectThreads.Count;
            threads.ForEach(t => 
            {
                if (!DirectThreads.Any(x => x.ThreadId == t.ThreadId))
                {
                    DirectThreads.Insert(itemsCount, new DirectThreadViewModel(t, _service));
                    itemsCount++;
                }
            });

            for (int i = 0; i < threads.Count; i++)
            {
                var dt = DirectThreads.Single(t => t.ThreadId == threads[i].ThreadId);
                if (i < UnseenCount)// && dt.Messages.LastOrDefault()?.ItemId != threads[i].Items.Single().ItemId)
                {
                    dt.UpdateThreadPreview(threads[i], true);
                }
            }

            IsLongProcessStarted = false;
        }

        private async void LoadMessagesAsync()
        {
            if (CurrentThread != null)
            {
                await CurrentThread.LoadMessagesAsync(false);
            }
        }

        private async void UpdateMessagesAsync()
        {
            if (CurrentThread != null)
            {
                CurrentThread.Messages.Clear();
                await CurrentThread.LoadMessagesAsync(true);
            }
        }
        #endregion
    }
}

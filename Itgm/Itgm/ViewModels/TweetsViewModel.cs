using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using System.Windows;
using System.Windows.Input;
using Microsoft.Expression.Interactivity.Core;
using Itgm.Interfaces;
using Itgm.Classes;
using System.Threading.Tasks;
using InstaSharper.Classes.Models;

namespace Itgm.ViewModels
{
    /// <summary>
    /// Вью модель для отображения твитов.
    /// </summary>
    public class TweetsViewModel : ResolvingViewModel
    {
        /// <summary>
        /// Показывает запущена ли длительная операция.
        /// </summary>
        private bool _isLongProcessStarted;

        /// <summary>
        /// Маркер отмены задания.
        /// </summary>
        private CancellationTokenSource _cts;

        /// <summary>
        /// Создание вью модели.
        /// </summary>
        /// <param name="service">Сервис.</param>
        public TweetsViewModel(IService service) : base(service, null)
        {
            AddOldTweetsCommand = new ActionCommand((t => AddOldTweets(t)));
            //_service.RateLimitOver += CheckRateLimitIsOver;
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

                _isLongProcessStarted = value;
                OnPropertyChanged("IsLongProcessStarted");
            }
        }

        /// <summary>
        /// Коллекция твитов.
        /// </summary>
        public ObservableCollection<InstaComment> Tweets { get; private set; } 
            = new ObservableCollection<InstaComment>();

        #endregion

        #region Commands

        /// <summary>
        /// Команда подгрузки твитов.
        /// </summary>
        public ICommand AddOldTweetsCommand { get; private set; }

        #endregion

        #region Methods

        /// <summary>
        /// Инициализирует вью модель при активации.
        /// </summary>
        public override void InitializeViewModel()
        {
            ClearViewModel();
            LoadTweetsAsync();
        }

        /// <summary>
        /// Очищает вью модель.
        /// </summary>
        public override void ClearViewModel()
        {
            Tweets.Clear();
        }

        /// <summary>
        /// Обновляет вью модель.
        /// </summary>
        public override void UpdateViewModel()
        {
            InitializeViewModel();
        }

        /// <summary>
        /// Реагирует на команду запроса твитов.
        /// </summary>
        /// <param name="t">Последний загруженный твит.</param>
        private void AddOldTweets(object t)
        {
            string tweet = (string)t;
            if (tweet == null)
            {
                return;
            }

            LoadTweetsAsync(0);
        }

        /// <summary>
        /// Запрашивает более старые твиты из сервиса.
        /// </summary>
        /// <param name="maxId">Идентификатор последнего загруженного твита.</param>
        private async void LoadTweetsAsync(long maxId = 0)
        {
            // Останавливаем новые запросы, пока ожидаем хотя бы один запущенный
            if (_isLongProcessStarted)
            {
                return;
            }

            IsLongProcessStarted = true;

            _cts = new CancellationTokenSource();

            var result = await Task.Run(() => _service.GetTweetsAsync(), _cts.Token);
            if (result != null && IsLongProcessStarted)
            {
                var tweets = result.ToList();
                tweets.ForEach(t => Tweets.Add(t));
            }

            IsLongProcessStarted = false;
        }

        /// <summary>
        /// Обработчик события достижения предела запросов.
        /// </summary>
        //private void CheckRateLimitIsOver(object sender, RateLimitEventArgs e)
        //{
        //    if (e.Limit == RateLimitType.TweetsRateLimit)
        //    {
        //        if (_cts != null && !_cts.IsCancellationRequested)
        //        {
        //            _cts.Cancel();
        //            MessageBox.Show($"Вы достигли предела запросов, повторите попытку через {e.RemainingMinutes} минут.");
        //        }
        //    }
        //}
        #endregion
    }
}

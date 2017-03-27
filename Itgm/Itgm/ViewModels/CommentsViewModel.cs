using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using System.Windows.Input;
using Microsoft.Expression.Interactivity.Core;
using Itgm.Interfaces;
using System.Threading.Tasks;
using Itgm.Models;

namespace Itgm.ViewModels
{
    /// <summary>
    /// Вью модель для отображения твитов.
    /// </summary>
    public class CommentsViewModel : ResolvingViewModel
    {
        /// <summary>
        /// Показывает запущена ли длительная операция.
        /// </summary>
        private MediaViewModel _currentMedia;

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
        public CommentsViewModel(IService service) : base(service, null)
        {
            //SetCurrentMediaCommand = new ActionCommand((m => SetCurrentMedia(m)));
            //_service.RateLimitOver += CheckRateLimitIsOver;
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

                _currentMedia = value;
                UpdateMediaCommentsAsync(_currentMedia);
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

                _isLongProcessStarted = value;
                OnPropertyChanged("IsLongProcessStarted");
            }
        }

        /// <summary>
        /// Коллекция твитов.
        /// </summary>
        public ObservableCollection<MediaViewModel> Medias { get; private set; } 
            = new ObservableCollection<MediaViewModel>();

        #endregion

        #region Commands

        /// <summary>
        /// Команда подгрузки твитов.
        /// </summary>
        public ICommand SetCurrentMediaCommand { get; private set; }

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
            Medias.Clear();
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
        private async void UpdateMediaCommentsAsync(MediaViewModel m)
        {
            m.Comments.Clear();
            var comments = (await _service.GetMediaCommentsAsync(m.Pk)).ToList();
            comments.ForEach(c => m.Comments.Add(c));
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

            var result = await Task.Run(() => _service.GetMediasAsync(), _cts.Token);
            if (result != null && IsLongProcessStarted)
            {
                var medias = result.ToList();
                medias.ForEach(m => Medias.Add(new MediaViewModel(m)));
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

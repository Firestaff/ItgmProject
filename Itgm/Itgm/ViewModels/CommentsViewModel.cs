using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using System.Windows.Input;
using Microsoft.Expression.Interactivity.Core;
using Itgm.Interfaces;
using System.Threading.Tasks;
using InstaSharper.Classes.Models;

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

        private UserInfo _user; 

        /// <summary>
        /// Создание вью модели.
        /// </summary>
        /// <param name="service">Сервис.</param>
        public CommentsViewModel(IService service) : base(service, null)
        {
            _user = service.LoggedUser;
            LoadMediasCommand = new ActionCommand(LoadMediasAsync);
            LoadCommentsCommand = new ActionCommand(LoadCommentsAsync);
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
                _currentMedia.UpdateViewModel();
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
        /// Команда подгрузки постов.
        /// </summary>
        public ICommand LoadMediasCommand { get; private set; }

        /// <summary>
        /// Команда подгрузки комментов.
        /// </summary>
        public ICommand LoadCommentsCommand { get; private set; }
        #endregion

        #region Methods

        /// <summary>
        /// Инициализирует вью модель при активации.
        /// </summary>
        public override void InitializeViewModel()
        {
            ClearViewModel();
            LoadMediasAsync();
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

        private async void LoadMediasAsync()
        {
            // Останавливаем новые запросы, пока ожидаем хотя бы один запущенный
            if (_isLongProcessStarted)
            {
                return;
            }

            IsLongProcessStarted = true;

            _user = await _service.UpdateCurrentUser();

            if (Medias.Count == _user.MediaCount)
            {
                IsLongProcessStarted = false;
                return;
            }

            var nextId = Medias.LastOrDefault()?.Pk;

            var result = await _service.GetCurrentUserMediasAsync(nextId);
            if (result != null && IsLongProcessStarted)
            {
                var medias = result.ToList();
                //for (int i = 0; i < 29; i++)
                {
                    medias.ForEach(m => Medias.Add(new MediaViewModel(m, _service)));
                }
                CurrentMedia = Medias.FirstOrDefault();
            }

            IsLongProcessStarted = false;
        }

        private async void LoadCommentsAsync()
        {
            if (CurrentMedia != null)
            {
                await CurrentMedia.LoadComments();
            }
        }
        #endregion
    }
}

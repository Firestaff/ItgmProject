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
        private Timer _timer;

        /// <summary>
        /// Создание вью модели.
        /// </summary>
        /// <param name="service">Сервис.</param>
        public CommentsViewModel(IService service) : base(service, null)
        {
            LoadMediasCommand = new ActionCommand(() => LoadMediasAsync(false));
            LoadCommentsCommand = new ActionCommand(LoadCommentsAsync);
            UpdateMediasCommand = new ActionCommand(UpdateMedias);
            UpdateCommentsCommand = new ActionCommand(UpdateComments);

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

        /// <summary>
        /// Команда перезагрузки комментов.
        /// </summary>
        public ICommand UpdateMediasCommand { get; private set; }

        /// <summary>
        /// Команда перезагрузки комментов.
        /// </summary>
        public ICommand UpdateCommentsCommand { get; private set; }
        #endregion

        #region Methods

        /// <summary>
        /// Инициализирует вью модель при активации.
        /// </summary>
        public override void InitializeViewModel()
        {
            _user = _service.LoggedUser;
            _timer = new Timer(60000);
            _timer.Elapsed += Timer_Elapsed;

            LoadMediasAsync(false);
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
            _user = null;

            Medias.Clear();
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

            LoadMediasAsync(true);
        }

        private async void LoadMediasAsync(bool onlyNew)
        {
            // Останавливаем новые запросы, пока ожидаем хотя бы один запущенный
            if (IsLongProcessStarted)
            {
                return;
            }

            IsLongProcessStarted = true;

            _user = await _service.UpdateCurrentUser();

            var fromId = Medias.LastOrDefault()?.Pk;
            if (fromId != null)
            {
                int firstEntry = 0;
                var topMedias = new List<InstaMedia>();

                while (true)
                {
                    var firstId = topMedias.LastOrDefault()?.Pk; //"1375636587895080536";
                    var newMedias = await _service.GetCurrentUserOldMediasAsync(firstId);
                    topMedias.AddRange(newMedias);

                    firstEntry = topMedias.FindIndex(e => e.Pk == Medias.First().Pk);
                    if (firstEntry != -1 || newMedias.Count() == 0)
                    {
                        break;
                    }
                }

                topMedias.RemoveRange(firstEntry, topMedias.Count - firstEntry);
                topMedias.Reverse();
                topMedias.ForEach(m => Medias.Insert(0, new MediaViewModel(m, _service)));
            }

            if (!onlyNew || Medias.Count == 0)
            {
                var result = await _service.GetCurrentUserOldMediasAsync(fromId);
                var medias = result.ToList();

                medias.ForEach(m => Medias.Add(new MediaViewModel(m, _service)));
            }

            if (CurrentMedia == null)
            {
                CurrentMedia = Medias.FirstOrDefault();
            }

            IsLongProcessStarted = false;
        }

        private void UpdateMedias()
        {
            ClearViewModel();
            InitializeViewModel();
        }

        private async void LoadCommentsAsync()
        {
            if (CurrentMedia != null)
            {
                await CurrentMedia.LoadCommentsAsync(false);
            }
        }

        private void UpdateComments()
        {
            if (CurrentMedia != null)
            {
                CurrentMedia.Comments.Clear();
                CurrentMedia.UpdateComments();
            }
        }
        #endregion
    }
}

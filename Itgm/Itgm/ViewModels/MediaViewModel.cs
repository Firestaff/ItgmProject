using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using InstaSharper.Classes.Models;
using Itgm.Interfaces;
using Itgm.ViewModels;

namespace Itgm.ViewModels
{
    public class MediaViewModel : BaseViewModel
    {
        private InstaMedia _model;
        private IService _service;
        private int _commentsCount;
        private int _likesCount;
        private bool _isLongProcessStarted;
        private DateTime _takenAt;
        private Image _image;

        public MediaViewModel(InstaMedia media, IService service)
        {
            _model = media;
            _service = service;

            InitializeViewModel();
        }

        #region Properties
        public string LikesCount => FormatByThousand(nameof(LikesCount), _likesCount);

        public string CommentsCount => FormatByThousand(nameof(CommentsCount), _commentsCount);

        public ObservableCollection<InstaComment> Comments { get; set; } 
            = new ObservableCollection<InstaComment>();

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
                OnPropertyChanged(nameof(IsLongProcessStarted));
            }
        }

        public DateTime TakenAt
        {
            get
            {
                return _takenAt;
            }
            private set
            {
                if (_takenAt == value)
                {
                    return;
                }

                _takenAt = value;
                OnPropertyChanged(nameof(TakenAt));
            }
        }

        public string Pk { get; private set; }


        public Image Image
        {
            get
            {
                return _image;
            }
            private set
            {
                if (_image == value)
                {
                    return;
                }

                _image = value;
                OnPropertyChanged(nameof(Image));
            }
        }
        #endregion

        public override void InitializeViewModel()
        {
            _commentsCount = _model.CommentsCount;
            _likesCount = _model.LikesCount;

            Image = _model.Images[0];
            Pk = _model.Pk;
            TakenAt = _model.TakenAt;
        }

        public override void ClearViewModel()
        {
            _commentsCount = 0;
            _likesCount = 0;

            Image = null;
            Pk = _model.Pk;
            TakenAt = _model.TakenAt;
            Comments.Clear();
        }

        public override async void UpdateViewModel()
        {
            if (Comments.Count == 0)
            {
                await LoadComments();
            }
        }

        public async Task LoadComments()
        {
            // Останавливаем новые запросы, пока ожидаем хотя бы один запущенный
            if (_isLongProcessStarted)
            {
                return;
            }

            IsLongProcessStarted = true;

            var nextId = Comments.LastOrDefault()?.Pk;

            var result = await _service.GetMediaCommentsAsync(Pk, nextId);
            if (result != null && IsLongProcessStarted)
            {
                var comments = result.Where(c => c.User.Id != _service.LoggedUser.Id)
                                     .ToList();

                var commentsCount = Comments.Count;
                comments.ForEach(c => Comments.Insert(commentsCount, c));
            }

            IsLongProcessStarted = false;
        }

        private string FormatByThousand(string prop, int value)
        {
            if (value < 1000)
            {
                return value.ToString();
            }

            string thousands = (value / 1000).ToString();
            return $"{thousands}k";
        }
    }
}

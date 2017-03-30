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

        public MediaViewModel(InstaMedia media, IService service)
        {
            _model = media;
            _service = service;

            InitializeViewModel();
        }

        public DateTime TakenAt { get; set; }
        public string Pk { get; set; }

        public string InstaIdentifier { get; set; }

        public Image Image { get; set; }

        public InstaUser User { get; set; }

        public string LikesCount => GetThousandCount(_likesCount);

        public string CommentsCount => GetThousandCount(_commentsCount);

        public string NextMaxId { get; set; }

        public InstaUserList Likers { get; set; } = new InstaUserList();

        public ObservableCollection<InstaComment> Comments { get; set; } 
            = new ObservableCollection<InstaComment>();

        public override void InitializeViewModel()
        {
            _commentsCount = _model.CommentsCount;
            _likesCount = _model.LikesCount;

            Image = _model.Images[0];
            InstaIdentifier = _model.InstaIdentifier;
            Likers = _model.Likers;
            NextMaxId = _model.NextMaxId;
            Pk = _model.Pk;
            TakenAt = _model.TakenAt;
            User = _model.User;
        }

        public override void ClearViewModel()
        {
            Comments.Clear();
        }

        public override async void UpdateViewModel()
        {
            Comments.Clear();
            var comments = (await _service.GetMediaCommentsAsync(Pk))
                            .Where(c => c.User.Pk != _service.GetUserInfo().Pk)
                            .ToList();
            comments.Reverse();

            for (int i = 0; i < 20; i++)
            {

                comments.ForEach(c => Comments.Add(c));
            }
        }

        private string GetThousandCount(int value)
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

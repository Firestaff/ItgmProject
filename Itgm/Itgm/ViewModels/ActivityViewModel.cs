using System;
using InstaSharper.Classes.Models;

namespace Itgm.ViewModels
{
    public class ActivityViewModel : BaseViewModel
    {
        private RecentActivity _model;

        public ActivityViewModel(RecentActivity model)
        {
            _model = model;
        }

        #region Properties
        public string Text { get => _model.Text; }
        public string MediaImage { get => _model.MediaImage; }
        public string ProfileImage { get => _model.ProfileImage; }
        public DateTime TimeStamp { get => _model.TimeStamp; }
        #endregion

        public override void InitializeViewModel()
        {
        }

        public override void ClearViewModel()
        {
        }

        public override void UpdateViewModel()
        {
        }
    }
}

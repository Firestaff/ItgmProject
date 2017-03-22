using System.ComponentModel;


namespace Itgm.ViewModels
{
    /// <summary>
    /// Базовая вью модель для всех вью моделей, реализует INotifyPropertyChanged.
    /// </summary>
    public abstract class BaseViewModel : INotifyPropertyChanged
    {
        /// <summary>
        /// Инициализирует вью модель при активации.
        /// </summary>
        public abstract void InitializeViewModel();

        /// <summary>
        /// Очищает вью модель.
        /// </summary>
        public abstract void ClearViewModel();

        /// <summary>
        /// Обновляет вью модель.
        /// </summary>
        public abstract void UpdateViewModel();

        #region Implementation of INotifyPropertyChanged

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string name)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null)
            {
                handler(this, new PropertyChangedEventArgs(name));
            }
        }
        
        #endregion
    }
}

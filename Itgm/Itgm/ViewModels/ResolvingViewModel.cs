using System;
using Itgm.Classes;
using Itgm.Interfaces;


namespace Itgm.ViewModels
{
    /// <summary>
    /// Базовый класс вью моделей, способных вызвать отрисовку нового представления.
    /// </summary>
    public abstract class ResolvingViewModel : BaseViewModel
    {
        /// <summary>
        /// Метод смены представления.
        /// </summary>
        private readonly Action<ViewTypes> _resolveViewAction;

        /// <summary>
        /// Сервис.
        /// </summary>
        protected readonly IService _service;

        /// <summary>
        /// Создает вью модель с заданными параметрами.
        /// </summary>
        /// <param name="service"></param>
        /// <param name="resolveView"></param>
        public ResolvingViewModel(IService service, Action<ViewTypes> resolveView)
        {
            _service = service ?? throw new ArgumentNullException();
            _resolveViewAction = resolveView;
        }

        /// <summary>
        /// Очищает вью модель.
        /// </summary>
        public override void ClearViewModel() { }

        /// <summary>
        /// Обновляет вью модель.
        /// </summary>
        public override void UpdateViewModel() { }

        /// <summary>
        /// Вызывает метод смены представления.
        /// </summary>
        /// <param name="viewType">Тип нового представления.</param>
        protected void ResolveNewView(ViewTypes viewType)
        {
            _resolveViewAction?.Invoke(viewType);
        }
    }
}

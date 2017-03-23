using InstaSharper.API.Builder;
using InstaSharper.Classes;
using Itgm.Classes;
using Itgm.Interfaces;
using Itgm.ViewModels;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace Itgm
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        /// <summary>
        /// Главное окно приложения.
        /// </summary>
        private Window _mainWindow;

        /// <summary>
        /// Словарь созданных вью моделей.
        /// </summary>
        private Dictionary<ViewTypes, ResolvingViewModel> ViewModels =
            new Dictionary<ViewTypes, ResolvingViewModel>();

        /// <summary>
        /// Сервис для твиттера.
        /// </summary>
        private IService _service;

        /// <summary>
        /// Обработчик загрузки окна.
        /// </summary>
        private void Application_Startup(object sender, StartupEventArgs e)
        {
            _mainWindow = new MainWindow();
            _service = new Service();

            // Отрисовываем страницу получения логина/пароля
            ResolveViewModel(ViewTypes.Auth);

            _mainWindow.Show();
        }

        /// <summary>
        /// Присваивает новый дата контекст в зависимости от переданного параметра.
        /// </summary>
        /// <param name="viewType">Тип вью для отображения.</param>
        private void ResolveViewModel(ViewTypes viewType)
        {
            ResolvingViewModel newContent;
            if (!ViewModels.TryGetValue(viewType, out newContent))
            {
                switch (viewType)
                {
                    //case ViewTypes.GetPinCode:
                    //    newContent = new GettingPinCodeViewModel(_service, ResolveViewModel);
                    //    ViewModels.Add(viewType, newContent);
                    //    break;

                    case ViewTypes.Auth:
                        newContent = new AuthenticationViewModel(_service, ResolveViewModel);
                        ViewModels.Add(viewType, newContent);
                        break;

                    case ViewTypes.Content:
                        newContent = new ContentViewModel(_service, ResolveViewModel);
                        ViewModels.Add(viewType, newContent);
                        break;

                    default:
                        break;
                }
            }

            newContent.InitializeViewModel();
            _mainWindow.DataContext = newContent;
        }
    }
}

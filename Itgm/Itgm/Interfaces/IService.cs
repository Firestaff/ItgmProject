using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using Itgm.Classes;
using InstaSharper.API;
using InstaSharper.API.Builder;
using InstaSharper.Classes;
using InstaSharper.Classes.Models;

namespace Itgm.Interfaces
{
    /// <summary>
    /// Интерфейс сервиса для получения данных.
    /// </summary>
    public interface IService
    {
        /// <summary>
        /// Состояние авторизации.
        /// </summary>
        WebExceptionStatus AuthenticationState { get; }

        /// <summary>
        /// Текущий пользователь.
        /// </summary>
        UserInfo LoggedUser { get; }

        /// <summary>
        /// Попытка авторизации пользователя.
        /// </summary>
        /// <param name="login">Логин.</param>
        /// <param name="password">Пароль.</param>
        Task LoginAsync(string login, string password);

        /// <summary>
        /// Выход из системы.
        /// </summary>
        Task LogoutAsync();

        Task<IEnumerable<InstaComment>> GetNewMediaCommentsAsync(string mediaId, string fromId);
        Task<IEnumerable<InstaComment>> GetOldMediaCommentsAsync(string mediaId, string fromId);

        /// <summary>
        /// Запрос твитов для залогиненного пользователя.
        /// </summary>
        /// <param name="maxId">Максимальный идентификатор твита с которого необходимо провести запрос.</param>
        /// <param name="count">Количество запрашиваемых твитов.</param>
        /// <returns>Коллекция твитов.</returns>
        Task<IEnumerable<InstaMedia>> GetCurrentUserMediasAsync(string fromId);

        Task<UserInfo> UpdateCurrentUser();
    }
}

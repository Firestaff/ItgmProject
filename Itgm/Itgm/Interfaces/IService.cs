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


        Task<IEnumerable<InstaMedia>> GetCurrentUserMediasAsync(string fromId);
        Task<RecentActivities> GetRecentActivityAsync(bool onlyNew);

        Task<InstaDirectInboxContainer> GetDirectAsync();
        Task<InstaDirectInboxThread> GetDirectThreadAsync(string threadId);
        
        Task<UserInfo> UpdateCurrentUser();
    }
}

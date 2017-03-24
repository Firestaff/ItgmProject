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

        ///// <summary>
        ///// Событие оповещающее о том, что кончились запросы.
        ///// </summary>
        //event EventHandler<RateLimitEventArgs> RateLimitOver;

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

        /// <summary>
        /// Получение подписчиков и подписок для пользователя с заданным идентификатором.
        /// </summary>
        /// <param name="userId">Идентификатор пользователя.</param>
        /// <returns>Список подписчиков и подписок(не содержит дубликатов).</returns>
        IEnumerable<long> GetFollowersAndSubsIds(long userId);

        /// <summary>
        /// Попытка получить пин-код от твиттера.
        /// </summary>
        Task GetPincodeAsync();

        /// <summary>
        /// Запрос твитов для залогиненного пользователя.
        /// </summary>
        /// <param name="maxId">Максимальный идентификатор твита с которого необходимо провести запрос.</param>
        /// <param name="count">Количество запрашиваемых твитов.</param>
        /// <returns>Коллекция твитов.</returns>
        Task<IEnumerable<InstaComment>> GetTweetsAsync();

        /// <summary>
        /// Получение залогиненного пользователя.
        /// </summary>
        /// <returns>Текущий пользователь.</returns>
        InstaUser GetUserInfo();

        /// <summary>
        /// Получение имени пользователя с указанным идентификатором.
        /// </summary>
        /// <param name="userId"></param>
        /// <returns>Имя пользователя.</returns>
        string GetUserName(long userId);
    }
}

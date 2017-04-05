using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using Itgm.Classes;
using InstaSharper.API;
using InstaSharper.API.Builder;
using InstaSharper.Classes;
using InstaSharper.Classes.Models;
using Itgm.Interfaces;


namespace Itgm.Classes
{
    /// <summary>
    /// Реализация сервиса для запросов к API твиттера.
    /// </summary>
    public class Service : IService
    {
        private IInstaApi _app;

        /// <summary>
        /// Состояние авторизации приложения.
        /// </summary>
        public WebExceptionStatus AuthenticationState { get; private set; }

        /// <summary>
        /// Текущий пользователь.
        /// </summary>
        public UserInfo LoggedUser { get; private set; }

        #region Public methods
        #region Media
        public async Task<IEnumerable<InstaMedia>> GetCurrentUserMediasAsync(string fromId = null)
        {
            return (await _app.GetUserMediaAsync(LoggedUser.UserName, fromId)).Value;
        }

        public async Task<IEnumerable<InstaComment>> GetNewMediaCommentsAsync(string mediaId, string fromId)
        {
            var result = await _app.GetMediaCommentsAsync(mediaId, fromId, "min");
            return result.Value.Comments;
        }

        public async Task<IEnumerable<InstaComment>> GetOldMediaCommentsAsync(string mediaId, string fromId)
        {
            var result = await _app.GetMediaCommentsAsync(mediaId, fromId, "max");
            return result.Value.Comments;
        }
        #endregion

        #region User
        public async Task<UserInfo> UpdateCurrentUser()
        {
            var result = await _app.GetUserInfoByIdAsync(LoggedUser.Id);
            LoggedUser = result.Value;
            return LoggedUser;
        }

        public async Task LoginAsync(string login, string password)
        {
            // create user session data and provide login details
            var userSession = new UserSessionData
            {
                UserName = login,
                Password = password
            };

            // create new InstaApi instance using Builder
            _app = new InstaApiBuilder()
                   .SetUser(userSession)
                   .Build();

            // login
            var logInResult = await _app.LoginAsync();

            SetState(logInResult.Info);

            if (AuthenticationState == WebExceptionStatus.Success)
            {
                LoggedUser = userSession.LoggedInUser;
            }
        }

        public async Task LogoutAsync()
        {
            await _app.LogoutAsync();
        }
        #endregion
        #endregion

        #region Private methods
        /// <summary>
        /// Установка состояния авторизации в зависимости от сообщения.
        /// </summary>
        /// <param name="message">Сообщение об ошибке или успешном выполнении запроса.</param>
        private void SetState(ResultInfo result)
        {
            var message = result.Message;

            if (message.StartsWith("Unable to connect") ||
                message.StartsWith("The remote name could not be resolved"))
            {
                AuthenticationState = WebExceptionStatus.ConnectFailure;
            }
            else if (message.StartsWith("The password you entered is incorrect. Please try again."))
            {
                AuthenticationState = WebExceptionStatus.TrustFailure;
            }
            else if (result.Exception == null
                     && String.IsNullOrEmpty(message))
            {
                AuthenticationState = WebExceptionStatus.Success;
            }
            else
            {
                AuthenticationState = WebExceptionStatus.UnknownError;
            }
        }

        /// <summary>
        /// Установка успешного состояния авторизации.
        /// </summary>
        private void SetSuccessState()
        {
            AuthenticationState = WebExceptionStatus.Success;
        }
        #endregion

        //public Service()
        //{
        //    return;

        //    //var b = _instaApi.GetRecentActivity(1);
        //    //var c = _instaApi.(1);

        //    //var userMedia = _app.GetUserMedia(currentUser.UserName, 1).Value;
        //    //var d = userMedia[0].InstaIdentifier;
        //    //var z = Task.Run(async () => await _app.GetMediaCommentsAsync(d, 1)).Result;
        //    //var z = Task.Run(async ()=> await _instaApi.GetMediaByCode("48B3kA2skbFbEbtKS/NLDq4HAUk=")).Result;


        //    //var mails = _app.GetDirectInbox();
        //    //var a = _instaApi.GetDirectInboxThread("340282366841710300949128133717524366136");
        //    //var e = _instaApi.GetUserRequestersAsync();


        //    //// get followers 
        //    //var followers = _instaApi.GetUserFollowersAsync(currentUser.UserName, 5).Result.Value;
        //    //Console.WriteLine($"Count of followers [{currentUser.UserName}]:{followers.Count}");
        //    //// get user's media 
        //    //var currentUserMedia = _instaApi.GetUserMedia(currentUser.UserName, 5);
        //    //if (currentUserMedia.Succeeded)
        //    //{
        //    //    Console.WriteLine($"Media count [{currentUser.UserName}]: {currentUserMedia.Value.Count}");
        //    //    foreach (var media in currentUserMedia.Value) Console.WriteLine($"Media [{currentUser.UserName}]: {media.Caption.Text}, {media.Code}, likes: {media.LikesCount}, image link: {media.Images.LastOrDefault()?.Url}");
        //    //}

        //    ////get user feed, first 5 pages
        //    //var userFeed = _instaApi.GetUserTimelineFeed(5);
        //    //if (userFeed.Succeeded)
        //    //{
        //    //    Console.WriteLine($"Feed items (in {userFeed.Value.Pages} pages) [{currentUser.UserName}]: {userFeed.Value.MediaItemsCount}");
        //    //    foreach (var media in userFeed.Value.Medias) Console.WriteLine($"Feed item - code:{media.Code}, likes: {media.LikesCount}");
        //    //}
        //    //// get tag feed, first 5 pages
        //    //var tagFeed = _instaApi.GetTagFeed("gm", 5);
        //    //if (userFeed.Succeeded)
        //    //{
        //    //    Console.WriteLine($"Tag feed items (in {tagFeed.Value.Pages} pages) [{currentUser.UserName}]: {tagFeed.Value.MediaItemsCount}");
        //    //    foreach (var media in tagFeed.Value.Medias) Console.WriteLine($"Tag feed item - code: {media.Code}, likes: {media.LikesCount}");
        //    //}
        //}
    }
}

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
        #region Fields

        /// <summary>
        /// Токен приложения.
        /// </summary>
        //private ITwitterCredentials _appCredentials;

        /// <summary>
        /// Открытый ключ приложения.
        /// </summary>
        private string _consumerKey = "HCsG1rc6E2VVqGNZcX2ZN7BZ3";

        /// <summary>
        /// Закрытый ключ приложения.
        /// </summary>
        private string _consumerSecret = "ACMXNzNm4VMPdIAm36b0js364EW73XFONnTkIzg597OdumONLU";

        /// <summary>
        /// Текущий пользователь.
        /// </summary>
        private InstaUser _loggedUser;

        #endregion

        /// <summary>
        /// Создает новый сервис.
        /// </summary>
        public Service()
        {
            //ExceptionHandler.SwallowWebExceptions = false;
            //_appCredentials = new TwitterCredentials(_consumerKey, _consumerSecret);
            return;
            // create user session data and provide login details
            var userSession = new UserSessionData
            {
                UserName = "iesopoval",
                Password = "1234567u"
            };
            // create new InstaApi instance using Builder
            var instaApi = new InstaApiBuilder()
                .SetUser(userSession)
                .Build();
            // login
            var logInResult = instaApi.Login();
            if (!logInResult.Succeeded) { Console.WriteLine($"Unable to login: {logInResult.Info}"); }
            else
            {
                // get currently logged in user
                var currentUser = instaApi.GetCurrentUser().Value;
                Console.WriteLine($"Logged in: username - {currentUser.UserName}, full name - {currentUser.FullName}");
                //var b = _instaApi.GetRecentActivity(1);
                //var c = _instaApi.(1);

                var userMedia = instaApi.GetUserMedia(currentUser.UserName, 1).Value;
                var d = userMedia[0].InstaIdentifier;
                var z = Task.Run(async () => await instaApi.GetMediaCommentsAsync(d, 1)).Result;
                //var z = Task.Run(async ()=> await _instaApi.GetMediaByCode("48B3kA2skbFbEbtKS/NLDq4HAUk=")).Result;


                var mails = instaApi.GetDirectInbox();
                //var a = _instaApi.GetDirectInboxThread("340282366841710300949128133717524366136");
                //var e = _instaApi.GetUserRequestersAsync();


                //// get followers 
                //var followers = _instaApi.GetUserFollowersAsync(currentUser.UserName, 5).Result.Value;
                //Console.WriteLine($"Count of followers [{currentUser.UserName}]:{followers.Count}");
                //// get user's media 
                //var currentUserMedia = _instaApi.GetUserMedia(currentUser.UserName, 5);
                //if (currentUserMedia.Succeeded)
                //{
                //    Console.WriteLine($"Media count [{currentUser.UserName}]: {currentUserMedia.Value.Count}");
                //    foreach (var media in currentUserMedia.Value) Console.WriteLine($"Media [{currentUser.UserName}]: {media.Caption.Text}, {media.Code}, likes: {media.LikesCount}, image link: {media.Images.LastOrDefault()?.Url}");
                //}

                ////get user feed, first 5 pages
                //var userFeed = _instaApi.GetUserTimelineFeed(5);
                //if (userFeed.Succeeded)
                //{
                //    Console.WriteLine($"Feed items (in {userFeed.Value.Pages} pages) [{currentUser.UserName}]: {userFeed.Value.MediaItemsCount}");
                //    foreach (var media in userFeed.Value.Medias) Console.WriteLine($"Feed item - code:{media.Code}, likes: {media.LikesCount}");
                //}
                //// get tag feed, first 5 pages
                //var tagFeed = _instaApi.GetTagFeed("gm", 5);
                //if (userFeed.Succeeded)
                //{
                //    Console.WriteLine($"Tag feed items (in {tagFeed.Value.Pages} pages) [{currentUser.UserName}]: {tagFeed.Value.MediaItemsCount}");
                //    foreach (var media in tagFeed.Value.Medias) Console.WriteLine($"Tag feed item - code: {media.Code}, likes: {media.LikesCount}");
                //}
                var logoutResult = instaApi.Logout();
                if (logoutResult.Value) Console.WriteLine("Logout succeed");
            }
        }

        /// <summary>
        /// Состояние авторизации приложения.
        /// </summary>
        public WebExceptionStatus AuthenticationState { get; private set; }

        /// <summary>
        /// Событие оповещающее о том, что кончились запросы.
        /// </summary>
        //public event EventHandler<RateLimitEventArgs> RateLimitOver;

        #region Public methods

        /// <summary>
        /// Попытка авторизации пользователя.
        /// </summary>
        /// <param name="pinCode">Пин-код, выданный твиттером.</param>
        public async Task AuthenticateAsync(string pinCode)
        {
            // Запрос к серверу для получения токенов
            //ITwitterCredentials userCredentials = await Task.Run(() => GetAccessToken(pinCode));

            //if (userCredentials == null)
            //{
            //    return;
            //}

            //// Запоминаем полученные значения для следующих запросов
            //Auth.Credentials = Auth.ApplicationCredentials;
            //Auth.Credentials.AccessToken = userCredentials.AccessToken;
            //Auth.Credentials.AccessTokenSecret = userCredentials.AccessTokenSecret;

            //TweetinviConfig.APPLICATION_RATELIMIT_TRACKER_OPTION = RateLimitTrackerOptions.TrackOnly;
            //TweetinviConfig.CURRENT_RATELIMIT_TRACKER_OPTION = RateLimitTrackerOptions.TrackOnly;

            //_loggedUser = Tweetinvi.User.GetLoggedUser();
        }

        /// <summary>
        /// Получение подписчиков и подписок для пользователя с заданным идентификатором.
        /// </summary>
        /// <param name="userId">Идентификатор пользователя.</param>
        /// <returns>Список подписчиков и подписок(не содержит дубликатов).</returns>
        public IEnumerable<long> GetFollowersAndSubsIds(long userId)
        {
            //var rateLimits = RateLimit.GetCredentialsRateLimits(Auth.ApplicationCredentials, useRateLimitCache: true);
            //if (rateLimits.FriendsIdsLimit.Remaining == 0 ||
            //    rateLimits.FollowersIdsLimit.Remaining == 0)
            //{
            //    OnRateLimitOver(
            //        new RateLimitEventArgs(
            //            RateLimitType.SubscribersRateLimit,
            //            rateLimits.FriendsIdsLimit.ResetDateTime.Minute));

            //    return null;
            //}

            //var friendIds = Tweetinvi.User.GetFriendIds(userId);
            //var followerIds = Tweetinvi.User.GetFollowerIds(userId);

            //var unionIds = Enumerable.Union(friendIds, followerIds);

            //return unionIds.Count() == 0 ? null : unionIds;
            return null;
        }

        /// <summary>
        /// Попытка получить пин-код от твиттера.
        /// </summary>
        public async Task GetPincodeAsync()
        {
            //RateLimit.RateLimitTrackerOption = RateLimitTrackerOptions.None;

            //Запрос к серверу для авторизации приложения
            if (!await Task.Run(() => InitializeCredentials()))
            {
                return;
            }

            // Открываем страничку в браузере
            RequestBrowser();
        }

        /// <summary>
        /// Запрос твитов для залогиненного пользователя.
        /// </summary>
        /// <param name="maxId">Максимальный идентификатор твита, с которого необходимо провести запрос.</param>
        /// <param name="count">Количество запрашиваемых твитов.</param>
        /// <returns>Коллекция твитов.</returns>
        public async Task<IEnumerable<InstaComment>> GetTweetsAsync()
        {
            //var rateLimits = RateLimit.GetCredentialsRateLimits(Auth.Credentials, useRateLimitCache: true);
            //if (rateLimits.StatusesHomeTimelineLimit.Remaining == 0)
            //{
            //    OnRateLimitOver(
            //        new RateLimitEventArgs(
            //            RateLimitType.TweetsRateLimit, 
            //            rateLimits.StatusesHomeTimelineLimit.ResetDateTime.Minute));

            //    return null;
            //}

            //IHomeTimelineParameters parameters = new HomeTimelineParameters()
            //{
            //    MaxId = maxId - 1,
            //    MaximumNumberOfTweetsToRetrieve = count
            //};

            //var tweets = await Task.Run(() => GetTweets(parameters));
            //return tweets;

            return null;
        }

        /// <summary>
        /// Получение залогиненного пользователя.
        /// </summary>
        /// <returns>Текущий пользователь.</returns>
        public InstaUser GetUserInfo()
        {
            return _loggedUser;
        }

        /// <summary>
        /// Получение имени пользователя с указанным идентификатором.
        /// </summary>
        /// <param name="userId"></param>
        /// <returns>Имя пользователя.</returns>
        public string GetUserName(long userId)
        {
            //var userInfo = Tweetinvi.User.GetUserFromId(userId);
            //if (userInfo == null)
            //{
            //    userInfo = Tweetinvi.User.GetUserFromId(userId);
            //}

            //return userInfo?.ScreenName;

            return null;
        }

        #endregion

        #region Private methods

        #region Twitter service

        /// <summary>
        /// Попытка авторизации пользователя через пин-код.
        /// </summary>
        /// <param name="pinCode">Пин-код выданный твиттером.</param>
        /// <returns>Токен пользователя.</returns>
        //private ITwitterCredentials GetAccessToken(string pinCode)
        //{
        //    SetSucessState();

        //    try
        //    {
        //        return CredentialsCreator.GetCredentialsFromVerifierCode(pinCode, Auth.ApplicationCredentials);
        //    }
        //    catch (TwitterException ex)
        //    {
        //        SetState(ex.TwitterDescription ?? ex.Message);
        //        return null;
        //    }
        //}

        /// <summary>
        /// Попытка получения твитов для залогиненного пользователя.
        /// </summary>
        /// <param name="parameters">Параметры запроса.</param>
        /// <returns>Коллекция твитов.</returns>
        //private IEnumerable<InstaComment> GetTweets(IHomeTimelineParameters parameters)
        //{
        //    SetSucessState();

        //    try
        //    {
        //        return Timeline.GetHomeTimeline(parameters);
        //    }
        //    catch (TwitterException ex)
        //    {
        //        SetState(ex.Message);
        //        return Enumerable.Empty<ITweet>();
        //    }
        //}

        /// <summary>
        /// Попытка авторизации приложения.
        /// </summary>
        /// <returns>Статус выполнения запроса.</returns>
        private bool InitializeCredentials()
        {
            SetSucessState();
            //Auth.ApplicationCredentials = null;

            //try
            //{
            //    Auth.SetApplicationOnlyCredentials(_consumerKey, _consumerSecret);
            //}
            //catch (TwitterException ex)
            //{
            //    SetState(ex.Message);
            //    return false;
            //}

            return true;
        }

        /// <summary>
        /// Открытие страницы авторизации твиттера в браузере.
        /// </summary>
        private void RequestBrowser()
        {
            // Создаем ссылку и открываем ее в брузере по умолчанию.
            //string url = CredentialsCreator.GetAuthorizationURL(Auth.ApplicationCredentials);
            //Application.Current.Dispatcher.BeginInvoke(new Action(() => Process.Start(url)));
        }
        #endregion

        /// <summary>
        /// Инициатор события <see cref="RateLimitOver"/>.
        /// </summary>
        //protected virtual void OnRateLimitOver(RateLimitEventArgs e)
        //{
        //    EventHandler<RateLimitEventArgs> temp = Volatile.Read(ref RateLimitOver);
        //    if (temp != null)
        //    {
        //        temp(this, e);
        //    }
        //}

        /// <summary>
        /// Установка состояния авторизации в зависимости от сообщения.
        /// </summary>
        /// <param name="message">Сообщение об ошибке или успешном выполнении запроса.</param>
        private void SetState(string message)
        {
            if (message.StartsWith("Unable to connect") ||
                message.StartsWith("The remote name could not be resolved"))
            {
                AuthenticationState = WebExceptionStatus.ConnectFailure;
            }
            else if (message.StartsWith("Unauthorized"))
            {
                AuthenticationState = WebExceptionStatus.TrustFailure;
            }
            else if (message.StartsWith("Success"))
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
        private void SetSucessState()
        {
            SetState("Success");
        }

        #endregion
    }
}

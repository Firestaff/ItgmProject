﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using InstaSharper.Classes;
using InstaSharper.Classes.Android.DeviceInfo;
using InstaSharper.Classes.Models;
using InstaSharper.Converters;
using InstaSharper.Converters.Json;
using InstaSharper.Helpers;
using InstaSharper.Logger;
using InstaSharper.ResponseWrappers;
using InstaSharper.ResponseWrappers.BaseResponse;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using InstaRecentActivityConverter = InstaSharper.Converters.Json.InstaRecentActivityConverter;

namespace InstaSharper.API
{
    public class InstaApi : IInstaApi
    {
        private readonly AndroidDevice _deviceInfo;
        private readonly HttpClient _httpClient;
        private readonly HttpClientHandler _httpHandler;
        private readonly ILogger _logger;
        private readonly ApiRequestMessage _requestMessage;
        private readonly UserSessionData _user;

        public InstaApi(UserSessionData user,
            ILogger logger,
            HttpClient httpClient,
            HttpClientHandler httpHandler,
            ApiRequestMessage requestMessage,
            AndroidDevice deviceInfo)
        {
            _user = user;
            _logger = logger;
            _httpClient = httpClient;
            _httpHandler = httpHandler;
            _requestMessage = requestMessage;
            _deviceInfo = deviceInfo;
        }

        public bool IsUserAuthenticated { get; private set; }

        #region sync part

        public IResult<bool> Login()
        {
            return LoginAsync().Result;
        }

        public IResult<bool> Logout()
        {
            return LogoutAsync().Result;
        }

        public IResult<InstaMedia> GetMediaByCode(string mediaCode)
        {
            return GetMediaByIdAsync(mediaCode).Result;
        }


        public IResult<InstaFeed> GetUserTimelineFeed(int maxPages = 0)
        {
            return GetUserTimelineFeedAsync(maxPages).Result;
        }

        public IResult<InstaMediaList> GetUserMedia(string username, int maxPages)
        {
            return GetUserMediaAsync(username, maxPages).Result;
        }

        public IResult<InstaUser> GetUser(string username)
        {
            return GetUserAsync(username).Result;
        }

        public IResult<InstaUser> GetCurrentUser()
        {
            return GetCurrentUserAsync().Result;
        }

        public IResult<InstaUserList> GetUserFollowers(string username, int maxPages = 0)
        {
            return GetUserFollowersAsync(username, maxPages).Result;
        }

        public IResult<InstaFeed> GetTagFeed(string tag, int maxPages = 0)
        {
            return GetTagFeedAsync(tag, maxPages).Result;
        }

        public IResult<InstaFeed> GetExploreFeed(int maxPages = 0)
        {
            return GetExploreFeedAsync(maxPages).Result;
        }

        public IResult<InstaMediaList> GetUserTags(string username, int maxPages = 0)
        {
            return GetUserTagsAsync(username, maxPages).Result;
        }

        public IResult<InstaUserList> GetCurentUserFollowers(int maxPages = 0)
        {
            return GetCurrentUserFollowersAsync(maxPages).Result;
        }

        public IResult<InstaDirectInboxContainer> GetDirectInbox()
        {
            return GetDirectInboxAsync().Result;
        }

        public IResult<InstaDirectInboxThread> GetDirectInboxThread(string threadId)
        {
            return GetDirectInboxThreadAsync(threadId).Result;
        }

        public IResult<InstaRecipients> GetRecentRecipients()
        {
            return GetRecentRecipientsAsync().Result;
        }

        public IResult<InstaRecipients> GetRankedRecipients()
        {
            return GetRankedRecipientsAsync().Result;
        }

        public IResult<InstaActivityFeed> GetRecentActivity(int maxPages = 0)
        {
            return GetRecentActivityAsync(maxPages).Result;
        }

        public IResult<InstaActivityFeed> GetFollowingRecentActivity(int maxPages = 0)
        {
            return GetFollowingRecentActivityAsync(maxPages).Result;
        }

        #endregion

        #region async part

        public async Task<IResult<bool>> LoginAsync()
        {
            ValidateCurrentUser();
            ValidateRequestMessage();
            try
            {
                var csrftoken = string.Empty;
                var firstResponse = await _httpClient.GetAsync(_httpClient.BaseAddress);
                var cookies = _httpHandler.CookieContainer.GetCookies(_httpClient.BaseAddress);
                foreach (Cookie cookie in cookies)
                    if (cookie.Name == InstaApiConstants.CSRFTOKEN) csrftoken = cookie.Value;
                _user.CsrfToken = csrftoken;
                var instaUri = UriCreator.GetLoginUri();
                var signature = $"{_requestMessage.GenerateSignature()}.{_requestMessage.GetMessageString()}";
                var fields = new Dictionary<string, string>
                {
                    {InstaApiConstants.HEADER_IG_SIGNATURE, signature},
                    {InstaApiConstants.HEADER_IG_SIGNATURE_KEY_VERSION, InstaApiConstants.IG_SIGNATURE_KEY_VERSION}
                };
                var request = HttpHelper.GetDefaultRequest(HttpMethod.Post, instaUri, _deviceInfo);
                request.Content = new FormUrlEncodedContent(fields);
                request.Properties.Add(InstaApiConstants.HEADER_IG_SIGNATURE, signature);
                request.Properties.Add(InstaApiConstants.HEADER_IG_SIGNATURE_KEY_VERSION,
                    InstaApiConstants.IG_SIGNATURE_KEY_VERSION);
                var response = await _httpClient.SendAsync(request);
                var json = await response.Content.ReadAsStringAsync();

                if (response.StatusCode == HttpStatusCode.OK)
                {
                    var loginInfo =
                        JsonConvert.DeserializeObject<InstaLoginResponse>(json);
                    IsUserAuthenticated = loginInfo.User != null && loginInfo.User.UserName == _user.UserName;
                    var converter = ConvertersFabric.GetUserConverter(loginInfo.User);
                    _user.LoggedInUser = converter.Convert();
                    _user.RankToken = $"{_user.LoggedInUser.Pk}_{_requestMessage.phone_id}";

                    instaUri = UriCreator.GetUserInfo(_user.LoggedInUser.Pk);
                    request = HttpHelper.GetDefaultRequest(HttpMethod.Get, instaUri, _deviceInfo);
                    response = await _httpClient.SendAsync(request);
                    json = await response.Content.ReadAsStringAsync();

                    return Result.Success(true);
                }
                else
                {
                    var loginInfo = GetBadStatusFromJsonString(json);
                    if (loginInfo.ErrorType == "checkpoint_logged_out")
                        return Result.Fail("Please go to instagram.com and confirm checkpoint",
                            ResponseType.CheckPointRequired, false);
                    if (loginInfo.ErrorType == "login_required")
                        return Result.Fail("Please go to instagram.com and check if you account marked as unsafe",
                            ResponseType.LoginRequired, false);
                    if (loginInfo.ErrorType == "Sorry, too many requests.Please try again later")
                        return Result.Fail("Please try again later, maximum amount of requests reached",
                            ResponseType.LoginRequired, false);
                    return Result.Fail(loginInfo.Message, false);
                }
            }
            catch (Exception exception)
            {
                return Result.Fail(exception.Message, false);
            }
        }

        public async Task<IResult<bool>> LogoutAsync()
        {
            ValidateCurrentUser();
            ValidateLoggedIn();
            try
            {
                var instaUri = UriCreator.GetLogoutUri();
                var request = HttpHelper.GetDefaultRequest(HttpMethod.Get, instaUri, _deviceInfo);
                var response = await _httpClient.SendAsync(request);
                var json = await response.Content.ReadAsStringAsync();
                if (response.StatusCode == HttpStatusCode.OK)
                {
                    var logoutInfo = JsonConvert.DeserializeObject<BaseStatusResponse>(json);
                    IsUserAuthenticated = logoutInfo.Status == "ok";
                    return Result.Success(true);
                }
                else
                {
                    var logoutInfo = GetBadStatusFromJsonString(json);
                    return Result.Fail(logoutInfo.Message, false);
                }
            }
            catch (Exception exception)
            {
                return Result.Fail(exception.Message, false);
            }
        }

        public async Task<IResult<InstaFeed>> GetUserTimelineFeedAsync(int maxPages = 0)
        {
            ValidateCurrentUser();
            ValidateLoggedIn();
            var userFeedUri = UriCreator.GetUserFeedUri();
            var request = HttpHelper.GetDefaultRequest(HttpMethod.Get, userFeedUri, _deviceInfo);
            var response = await _httpClient.SendAsync(request);
            var json = await response.Content.ReadAsStringAsync();
            var feed = new InstaFeed();
            if (response.StatusCode != HttpStatusCode.OK)
                return Result.Fail(GetBadStatusFromJsonString(json).Message, (InstaFeed) null);
            var feedResponse = JsonConvert.DeserializeObject<InstaFeedResponse>(json,
                new InstaFeedResponseDataConverter());
            var converter = ConvertersFabric.GetFeedConverter(feedResponse);
            var feedConverted = converter.Convert();
            feed.Medias.AddRange(feedConverted.Medias);
            var nextId = feedResponse.NextMaxId;
            while (feedResponse.MoreAvailable && feed.Pages < maxPages)
            {
                if (string.IsNullOrEmpty(nextId)) break;
                var nextFeed = await GetUserFeedWithMaxIdAsync(nextId);
                if (!nextFeed.Succeeded) Result.Success($"Not all pages was downloaded: {nextFeed.Info.Message}", feed);
                nextId = nextFeed.Value.NextMaxId;
                feed.Medias.AddRange(
                    nextFeed.Value.Items.Select(ConvertersFabric.GetSingleMediaConverter).Select(conv => conv.Convert()));
                feed.Pages++;
            }
            return Result.Success(feed);
        }

        public async Task<IResult<InstaFeed>> GetExploreFeedAsync(int maxPages = 0)
        {
            ValidateCurrentUser();
            ValidateLoggedIn();
            try
            {
                if (maxPages == 0) maxPages = int.MaxValue;
                var exploreUri = UriCreator.GetExploreUri();
                var request = HttpHelper.GetDefaultRequest(HttpMethod.Get, exploreUri, _deviceInfo);
                var response = await _httpClient.SendAsync(request);
                var json = await response.Content.ReadAsStringAsync();
                var exploreFeed = new InstaFeed();
                if (response.StatusCode != HttpStatusCode.OK) return Result.Fail("", (InstaFeed) null);
                var mediaResponse = JsonConvert.DeserializeObject<InstaMediaListResponse>(json,
                    new InstaMediaListDataConverter());
                exploreFeed.Medias.AddRange(
                    mediaResponse.Medias.Select(ConvertersFabric.GetSingleMediaConverter)
                        .Select(converter => converter.Convert()));
                exploreFeed.Stories.AddRange(
                    mediaResponse.Stories.Select(ConvertersFabric.GetSingleStoryConverter)
                        .Select(converter => converter.Convert()));
                var pages = 1;
                var nextId = mediaResponse.NextMaxId;
                while (!string.IsNullOrEmpty(nextId) && pages < maxPages)
                    if (string.IsNullOrEmpty(nextId) || nextId == "0") break;
                return Result.Success(exploreFeed);
            }
            catch (Exception exception)
            {
                return Result.Fail(exception.Message, (InstaFeed) null);
            }
        }

        public async Task<IResult<InstaMediaList>> GetUserMediaAsync(string username, int maxPages)
        {
            if (maxPages == 0)
            {
                maxPages = int.MaxValue;
            }

            var user = GetUser(username).Value;
            var instaUri = UriCreator.GetUserMediaListUri(user.Pk);
            var request = HttpHelper.GetDefaultRequest(HttpMethod.Get, instaUri, _deviceInfo);
            var response = await _httpClient.SendAsync(request);
            var json = await response.Content.ReadAsStringAsync();

            if (response.StatusCode == HttpStatusCode.OK)
            {
                var mediaResponse = JsonConvert.DeserializeObject<InstaMediaListResponse>(json,
                    new InstaMediaListDataConverter());
                var converter = ConvertersFabric.GetMediaListConverter(mediaResponse);
                var mediaList = converter.Convert();
                mediaList.Pages++;
                var nextId = mediaResponse.NextMaxId;
                while (mediaResponse.MoreAvailable && mediaList.Pages < maxPages)
                {
                    instaUri = UriCreator.GetMediaListWithMaxIdUri(user.Pk, nextId);
                    var result = await GetUserMediaListWithMaxIdAsync(instaUri);
                    mediaResponse = result.Value;
                    mediaList.Pages++;
                    if (!result.Succeeded)
                        Result.Success($"Not all pages were downloaded: {result.Info.Message}", mediaList);
                    nextId = mediaResponse.NextMaxId;
                    converter = ConvertersFabric.GetMediaListConverter(mediaResponse);
                    mediaList.AddRange(converter.Convert());
                }
                return Result.Success(mediaList);
            }
            return Result.Fail(GetBadStatusFromJsonString(json).Message, (InstaMediaList) null);
        }

        public async Task<IResult<InstaMedia>> GetMediaByIdAsync(string mediaId)
        {
            ValidateCurrentUser();
            var mediaUri = UriCreator.GetMediaUri(mediaId);
            var request = HttpHelper.GetDefaultRequest(HttpMethod.Get, mediaUri, _deviceInfo);
            var response = await _httpClient.SendAsync(request);
            var json = await response.Content.ReadAsStringAsync();
            if (response.StatusCode == HttpStatusCode.OK)
            {
                var mediaResponse = JsonConvert.DeserializeObject<InstaMediaListResponse>(json,
                    new InstaMediaListDataConverter());
                if (mediaResponse.Medias?.Count != 1)
                {
                    string errorMessage = $"Got wrong media count for request with media id={mediaId}";
                    _logger.Write(errorMessage);
                    return Result.Fail<InstaMedia>(errorMessage);
                }
                var converter = ConvertersFabric.GetSingleMediaConverter(mediaResponse.Medias.FirstOrDefault());
                return Result.Success(converter.Convert());
            }
            return Result.Fail(GetBadStatusFromJsonString(json).Message, (InstaMedia) null);
        }

        public async Task<IResult<InstaUser>> GetUserAsync(string username)
        {
            if (_user.UserName == username)
            {
                return Result.Success(_user.LoggedInUser);
            }

            ValidateCurrentUser();
            var userUri = UriCreator.GetUserUri(username);
            var request = HttpHelper.GetDefaultRequest(HttpMethod.Get, userUri, _deviceInfo);
            request.Properties.Add(new KeyValuePair<string, object>(InstaApiConstants.HEADER_TIMEZONE,
                InstaApiConstants.TIMEZONE_OFFSET.ToString()));
            request.Properties.Add(new KeyValuePair<string, object>(InstaApiConstants.HEADER_COUNT, "1"));
            request.Properties.Add(new KeyValuePair<string, object>(InstaApiConstants.HEADER_RANK_TOKEN, _user.RankToken));
            var response = await _httpClient.SendAsync(request);
            var json = await response.Content.ReadAsStringAsync();
            if (response.StatusCode == HttpStatusCode.OK)
            {
                var userInfo = JsonConvert.DeserializeObject<InstaSearchUserResponse>(json);
                var user = userInfo.Users?.FirstOrDefault(u => u.UserName == username);
                if (user == null)
                {
                    string errorMessage = $"Can't find this user: {username}";
                    _logger.Write(errorMessage);
                    return Result.Fail<InstaUser>(errorMessage);
                }
                var converter = ConvertersFabric.GetUserConverter(user);
                return Result.Success(converter.Convert());
            }
            return Result.Fail(GetBadStatusFromJsonString(json).Message, (InstaUser) null);
        }

        public async Task<IResult<InstaUser>> GetCurrentUserAsync()
        {
            ValidateCurrentUser();
            ValidateLoggedIn();
            var instaUri = UriCreator.GetCurrentUserUri();
            dynamic jsonObject = new JObject();
            jsonObject._uuid = _deviceInfo.DeviceGuid;
            jsonObject._uid = _user.LoggedInUser.Pk;
            jsonObject._csrftoken = _user.CsrfToken;
            var fields = new Dictionary<string, string>
            {
                {"_uuid", _deviceInfo.DeviceGuid.ToString()},
                {"_uid", _user.LoggedInUser.Pk},
                {"_csrftoken", _user.CsrfToken}
            };
            var request = HttpHelper.GetDefaultRequest(HttpMethod.Post, instaUri, _deviceInfo);
            request.Content = new FormUrlEncodedContent(fields);
            var response = await _httpClient.SendAsync(request);
            var json = await response.Content.ReadAsStringAsync();
            if (response.StatusCode == HttpStatusCode.OK)
            {
                var user = JsonConvert.DeserializeObject<InstaCurrentUserResponse>(json);
                var converter = ConvertersFabric.GetUserConverter(user.User);
                var userConverted = converter.Convert();

                return Result.Success(userConverted);
            }
            return Result.Fail(GetBadStatusFromJsonString(json).Message, (InstaUser) null);
        }

        public async Task<IResult<InstaFeed>> GetTagFeedAsync(string tag, int maxPages = 0)
        {
            ValidateCurrentUser();
            ValidateLoggedIn();
            if (maxPages == 0) maxPages = int.MaxValue;
            var userFeedUri = UriCreator.GetTagFeedUri(tag);
            var request = HttpHelper.GetDefaultRequest(HttpMethod.Get, userFeedUri, _deviceInfo);
            var response = await _httpClient.SendAsync(request);
            var json = await response.Content.ReadAsStringAsync();
            if (response.StatusCode == HttpStatusCode.OK)
            {
                var feedResponse = JsonConvert.DeserializeObject<InstaMediaListResponse>(json,
                    new InstaMediaListDataConverter());
                var converter = ConvertersFabric.GetMediaListConverter(feedResponse);
                var tagFeed = new InstaFeed();
                tagFeed.Medias.AddRange(converter.Convert());
                tagFeed.Pages++;
                var nextId = feedResponse.NextMaxId;
                while (feedResponse.MoreAvailable && tagFeed.Pages < maxPages)
                {
                    var nextMedia = await GetTagFeedWithMaxIdAsync(tag, nextId);
                    tagFeed.Pages++;
                    if (!nextMedia.Succeeded)
                        return Result.Success($"Not all pages was downloaded: {nextMedia.Info.Message}", tagFeed);
                    nextId = nextMedia.Value.NextMaxId;
                    converter = ConvertersFabric.GetMediaListConverter(nextMedia.Value);
                    tagFeed.Medias.AddRange(converter.Convert());
                }
                return Result.Success(tagFeed);
            }
            return Result.Fail(GetBadStatusFromJsonString(json).Message, (InstaFeed) null);
        }

        public async Task<IResult<InstaUserList>> GetUserFollowersAsync(string username, int maxPages = 0)
        {
            ValidateCurrentUser();
            ValidateLoggedIn();
            try
            {
                if (maxPages == 0) maxPages = int.MaxValue;
                var user = await GetUserAsync(username);
                var userFeedUri = UriCreator.GetUserFollowersUri(user.Value.Pk, _user.RankToken);
                var request = HttpHelper.GetDefaultRequest(HttpMethod.Get, userFeedUri, _deviceInfo);
                var response = await _httpClient.SendAsync(request);
                var json = await response.Content.ReadAsStringAsync();
                var followers = new InstaUserList();
                if (response.StatusCode != HttpStatusCode.OK) return Result.Fail("", (InstaUserList) null);
                var followersResponse = JsonConvert.DeserializeObject<InstaFollowersResponse>(json);
                if (!followersResponse.IsOK())
                    Result.Fail(GetBadStatusFromJsonString(json).Message, (InstaUserList) null);
                followers.AddRange(
                    followersResponse.Items.Select(ConvertersFabric.GetUserConverter)
                        .Select(converter => converter.Convert()));
                if (!followersResponse.IsBigList) return Result.Success(followers);
                var pages = 1;
                while (!string.IsNullOrEmpty(followersResponse.NextMaxId) && pages < maxPages)
                {
                    var nextFollowers = Result.Success(followersResponse);
                    nextFollowers = await GetUserFollowersWithMaxIdAsync(username, nextFollowers.Value.NextMaxId);
                    if (!nextFollowers.Succeeded)
                        return Result.Success($"Not all pages was downloaded: {nextFollowers.Info.Message}", followers);
                    followersResponse = nextFollowers.Value;
                    followers.AddRange(
                        nextFollowers.Value.Items.Select(ConvertersFabric.GetUserConverter)
                            .Select(converter => converter.Convert()));
                    pages++;
                }
                return Result.Success(followers);
            }
            catch (Exception exception)
            {
                return Result.Fail(exception.Message, (InstaUserList) null);
            }
        }

        public async Task<IResult<InstaUserList>> GetUserRequestersAsync()
        {
            ValidateCurrentUser();
            ValidateLoggedIn();
            try
            {
                //if (maxPages == 0) maxPages = int.MaxValue;
                //var user = await GetUserAsync(username);
                var userRequestersUri = UriCreator.GetRequestersUri(_user.RankToken);
                var request = HttpHelper.GetDefaultRequest(HttpMethod.Get, userRequestersUri, _deviceInfo);
                var response = await _httpClient.SendAsync(request);
                var json = await response.Content.ReadAsStringAsync();
                var followers = new InstaUserList();
                return Result.Success(followers);
                if (response.StatusCode != HttpStatusCode.OK) return Result.Fail("", (InstaUserList)null);
                var followersResponse = JsonConvert.DeserializeObject<InstaFollowersResponse>(json);
                if (!followersResponse.IsOK())
                    Result.Fail(GetBadStatusFromJsonString(json).Message, (InstaUserList)null);
                followers.AddRange(
                    followersResponse.Items.Select(ConvertersFabric.GetUserConverter)
                        .Select(converter => converter.Convert()));
                if (!followersResponse.IsBigList) return Result.Success(followers);
                var pages = 1;
                //while (!string.IsNullOrEmpty(followersResponse.NextMaxId) && pages < maxPages)
                //{
                //    var nextFollowers = Result.Success(followersResponse);
                //    nextFollowers = await GetUserFollowersWithMaxIdAsync(username, nextFollowers.Value.NextMaxId);
                //    if (!nextFollowers.Succeeded)
                //        return Result.Success($"Not all pages was downloaded: {nextFollowers.Info.Message}", followers);
                //    followersResponse = nextFollowers.Value;
                //    followers.AddRange(
                //        nextFollowers.Value.Items.Select(ConvertersFabric.GetUserConverter)
                //            .Select(converter => converter.Convert()));
                //    pages++;
                //}
                return Result.Success(followers);
            }
            catch (Exception exception)
            {
                return Result.Fail(exception.Message, (InstaUserList)null);
            }
        }

        public async Task<IResult<InstaUserList>> GetCurrentUserFollowersAsync(int maxPages = 0)
        {
            ValidateCurrentUser();
            return await GetUserFollowersAsync(_user.UserName, maxPages);
        }

        public async Task<IResult<InstaMediaList>> GetUserTagsAsync(string username, int maxPages = 0)
        {
            ValidateCurrentUser();
            ValidateLoggedIn();
            try
            {
                if (maxPages == 0) maxPages = int.MaxValue;
                var user = await GetUserAsync(username);
                if (!user.Succeeded || string.IsNullOrEmpty(user.Value.Pk))
                    return Result.Fail($"Unable to get user {username}", (InstaMediaList) null);
                var uri = UriCreator.GetUserTagsUri(user.Value?.Pk, _user.RankToken);
                var request = HttpHelper.GetDefaultRequest(HttpMethod.Get, uri, _deviceInfo);
                var response = await _httpClient.SendAsync(request);
                var json = await response.Content.ReadAsStringAsync();
                var userTags = new InstaMediaList();
                if (response.StatusCode != HttpStatusCode.OK) return Result.Fail("", (InstaMediaList) null);
                var mediaResponse = JsonConvert.DeserializeObject<InstaMediaListResponse>(json,
                    new InstaMediaListDataConverter());
                var nextId = mediaResponse.NextMaxId;
                userTags.AddRange(
                    mediaResponse.Medias.Select(ConvertersFabric.GetSingleMediaConverter)
                        .Select(converter => converter.Convert()));
                var pages = 1;
                while (!string.IsNullOrEmpty(nextId) && pages < maxPages)
                {
                    uri = UriCreator.GetUserTagsUri(user.Value?.Pk, _user.RankToken, nextId);
                    var nextMedia = await GetUserMediaListWithMaxIdAsync(uri);
                    if (!nextMedia.Succeeded)
                        Result.Success($"Not all pages was downloaded: {nextMedia.Info.Message}", userTags);
                    nextId = nextMedia.Value.NextMaxId;
                    userTags.AddRange(
                        mediaResponse.Medias.Select(ConvertersFabric.GetSingleMediaConverter)
                            .Select(converter => converter.Convert()));
                    pages++;
                }
                return Result.Success(userTags);
            }
            catch (Exception exception)
            {
                return Result.Fail(exception.Message, (InstaMediaList) null);
            }
        }

        public async Task<IResult<InstaDirectInboxContainer>> GetDirectInboxAsync()
        {
            ValidateCurrentUser();
            ValidateLoggedIn();
            try
            {
                var directInboxUri = UriCreator.GetDirectInboxUri();
                var request = HttpHelper.GetDefaultRequest(HttpMethod.Get, directInboxUri, _deviceInfo);
                var response = await _httpClient.SendAsync(request);
                var json = await response.Content.ReadAsStringAsync();
                if (response.StatusCode != HttpStatusCode.OK) return Result.Fail("", (InstaDirectInboxContainer) null);
                var inboxResponse = JsonConvert.DeserializeObject<InstaDirectInboxContainerResponse>(json);
                var converter = ConvertersFabric.GetDirectInboxConverter(inboxResponse);
                return Result.Success(converter.Convert());
            }
            catch (Exception exception)
            {
                return Result.Fail<InstaDirectInboxContainer>(exception);
            }
        }

        public async Task<IResult<InstaDirectInboxThread>> GetDirectInboxThreadAsync(string threadId)
        {
            ValidateCurrentUser();
            ValidateLoggedIn();
            try
            {
                var directInboxUri = UriCreator.GetDirectInboxThreadUri(threadId);
                var request = HttpHelper.GetDefaultRequest(HttpMethod.Get, directInboxUri, _deviceInfo);
                var response = await _httpClient.SendAsync(request);
                var json = await response.Content.ReadAsStringAsync();
                if (response.StatusCode != HttpStatusCode.OK) return Result.Fail("", (InstaDirectInboxThread) null);
                var threadResponse = JsonConvert.DeserializeObject<InstaDirectInboxThreadResponse>(json,
                    new InstaThreadDataConverter());
                var converter = ConvertersFabric.GetDirectThreadConverter(threadResponse);
                return Result.Success(converter.Convert());
            }
            catch (Exception exception)
            {
                return Result.Fail<InstaDirectInboxThread>(exception);
            }
        }

        public async Task<IResult<InstaRecipients>> GetRecentRecipientsAsync()
        {
            ValidateCurrentUser();
            ValidateLoggedIn();
            var userUri = UriCreator.GetRecentRecipientsUri();
            var request = HttpHelper.GetDefaultRequest(HttpMethod.Get, userUri, _deviceInfo);
            var response = await _httpClient.SendAsync(request);
            var json = await response.Content.ReadAsStringAsync();

            if (response.StatusCode == HttpStatusCode.OK)
            {
                var responseRecipients = JsonConvert.DeserializeObject<InstaRecipientsResponse>(json,
                    new InstaRecipientsDataConverter("recent_recipients"));
                var converter = ConvertersFabric.GetRecipientsConverter(responseRecipients);
                return Result.Success(converter.Convert());
            }
            return Result.Fail(GetBadStatusFromJsonString(json).Message, (InstaRecipients) null);
        }

        public async Task<IResult<InstaRecipients>> GetRankedRecipientsAsync()
        {
            ValidateCurrentUser();
            ValidateLoggedIn();
            var userUri = UriCreator.GetRankedRecipientsUri();
            var request = HttpHelper.GetDefaultRequest(HttpMethod.Get, userUri, _deviceInfo);
            var response = await _httpClient.SendAsync(request);
            var json = await response.Content.ReadAsStringAsync();
            if (response.StatusCode == HttpStatusCode.OK)
            {
                var responseRecipients = JsonConvert.DeserializeObject<InstaRecipientsResponse>(json,
                    new InstaRecipientsDataConverter("ranked_recipients"));
                var converter = ConvertersFabric.GetRecipientsConverter(responseRecipients);
                return Result.Success(converter.Convert());
            }
            return Result.Fail(GetBadStatusFromJsonString(json).Message, (InstaRecipients) null);
        }

        public async Task<IResult<InstaActivityFeed>> GetRecentActivityAsync(int maxPages = 0)
        {
            var uri = UriCreator.GetRecentActivityUri();
            return await GetRecentActivityInternalAsync(uri, maxPages);
        }

        public async Task<IResult<InstaActivityFeed>> GetFollowingRecentActivityAsync(int maxPages = 0)
        {
            var uri = UriCreator.GetFollowingRecentActivityUri();
            return await GetRecentActivityInternalAsync(uri, maxPages);
        }

        public async Task<IResult<bool>> CheckpointAsync(string checkPointUrl)
        {
            if (string.IsNullOrEmpty(checkPointUrl)) return Result.Fail("Empty checkpoint URL", false);
            var instaUri = new Uri(checkPointUrl);
            var request = HttpHelper.GetDefaultRequest(HttpMethod.Get, instaUri, _deviceInfo);
            var response = await _httpClient.SendAsync(request);
            var json = await response.Content.ReadAsStringAsync();
            if (response.StatusCode == HttpStatusCode.OK) return Result.Success(true);
            return Result.Fail(GetBadStatusFromJsonString(json).Message, false);
        }

        public async Task<IResult<bool>> LikeMediaAsync(string mediaId)
        {
            ValidateCurrentUser();
            ValidateLoggedIn();
            try
            {
                throw new NotImplementedException();
            }
            catch (Exception exception)
            {
                return Result.Fail(exception.Message, false);
            }
        }

        public async Task<IResult<InstaCommentList>> GetMediaCommentsAsync(string mediaId, int maxPages = 0)
        {
            ValidateCurrentUser();
            ValidateLoggedIn();
            try
            {
                if (maxPages == 0) maxPages = int.MaxValue;
                var commentsUri = UriCreator.GetMediaCommentsUri(mediaId);
                var request = HttpHelper.GetDefaultRequest(HttpMethod.Get, commentsUri, _deviceInfo);
                var response = await _httpClient.SendAsync(request);
                var json = await response.Content.ReadAsStringAsync();
                if (response.StatusCode != HttpStatusCode.OK)
                    return Result.Fail($"Unexpected response status: {response.StatusCode}", (InstaCommentList)null);
                var commentListResponse = JsonConvert.DeserializeObject<InstaCommentListResponse>(json);
                var converter = ConvertersFabric.GetCommentListConverter(commentListResponse);
                var instaComments = converter.Convert();
                instaComments.Pages++;
                var nextId = commentListResponse.NextMaxId;
                while (commentListResponse.MoreComentsAvailable && instaComments.Pages < maxPages)
                {
                    if (string.IsNullOrEmpty(nextId)) break;
                    var nextComments = await GetCommentListWithMaxIdAsync(mediaId, nextId);
                    if (!nextComments.Succeeded)
                        Result.Success($"Not all pages was downloaded: {nextComments.Info.Message}", instaComments);
                    nextId = nextComments.Value.NextMaxId;
                    converter = ConvertersFabric.GetCommentListConverter(nextComments.Value);
                    instaComments.Comments.AddRange(converter.Convert().Comments);
                    instaComments.Pages++;
                }
                return Result.Success(instaComments);
            }
            catch (Exception exception)
            {
                return Result.Fail<InstaCommentList>(exception);
            }
        }

        public async Task<IResult<InstaUserList>> GetMediaLikersAsync(string mediaId)
        {
            ValidateCurrentUser();
            ValidateLoggedIn();
            try
            {
                var likersUri = UriCreator.GetMediaLikersUri(mediaId);
                var request = HttpHelper.GetDefaultRequest(HttpMethod.Get, likersUri, _deviceInfo);
                var response = await _httpClient.SendAsync(request);
                var json = await response.Content.ReadAsStringAsync();
                if (response.StatusCode != HttpStatusCode.OK) return Result.Fail("", (InstaUserList)null);
                var instaUsers = new InstaUserList();
                var mediaLikersResponse = JsonConvert.DeserializeObject<InstaMediaLikersResponse>(json);
                if (mediaLikersResponse.UsersCount < 1) return Result.Success(instaUsers);
                instaUsers.AddRange(
                    mediaLikersResponse.Users.Select(ConvertersFabric.GetUserConverter)
                        .Select(converter => converter.Convert()));
                return Result.Success(instaUsers);
            }
            catch (Exception exception)
            {
                return Result.Fail<InstaUserList>(exception);
            }
        }

        #endregion

        #region private part
        private void ValidateCurrentUser()
        {
            if (string.IsNullOrEmpty(_user.UserName) || string.IsNullOrEmpty(_user.Password))
                throw new ArgumentException("user name and password must be specified");
        }

        private void ValidateLoggedIn()
        {
            if (!IsUserAuthenticated) throw new ArgumentException("user must be authenticated");
        }

        private void ValidateRequestMessage()
        {
            if (_requestMessage == null || _requestMessage.IsEmpty())
                throw new ArgumentException("API request message null or empty");
        }

        private BadStatusResponse GetBadStatusFromJsonString(string json)
        {
            var badStatus = new BadStatusResponse();
            try
            {
                badStatus = JsonConvert.DeserializeObject<BadStatusResponse>(json);
            }
            catch (Exception ex)
            {
                badStatus.Message = ex.Message;
            }
            return badStatus;
        }

        private async Task<IResult<InstaFeedResponse>> GetUserFeedWithMaxIdAsync(string maxId)
        {
            Uri instaUri;
            if (!Uri.TryCreate(new Uri(InstaApiConstants.INSTAGRAM_URL), InstaApiConstants.TIMELINEFEED, out instaUri))
                throw new Exception("Cant create search user URI");
            var userUriBuilder = new UriBuilder(instaUri) {Query = $"max_id={maxId}"};
            var request = HttpHelper.GetDefaultRequest(HttpMethod.Get, userUriBuilder.Uri, _deviceInfo);
            request.Properties.Add(new KeyValuePair<string, object>(InstaApiConstants.HEADER_PHONE_ID,
                _requestMessage.phone_id));
            request.Properties.Add(new KeyValuePair<string, object>(InstaApiConstants.HEADER_TIMEZONE,
                InstaApiConstants.TIMEZONE_OFFSET.ToString()));
            var response = await _httpClient.SendAsync(request);
            var json = await response.Content.ReadAsStringAsync();
            if (response.StatusCode == HttpStatusCode.OK)
            {
                var feedResponse = JsonConvert.DeserializeObject<InstaFeedResponse>(json,
                    new InstaFeedResponseDataConverter());
                return Result.Success(feedResponse);
            }
            return Result.Fail(GetBadStatusFromJsonString(json).Message, (InstaFeedResponse) null);
        }

        private async Task<IResult<InstaRecentActivityResponse>> GetFollowingActivityWithMaxIdAsync(string maxId)
        {
            var uri = UriCreator.GetFollowingRecentActivityUri(maxId);
            var request = HttpHelper.GetDefaultRequest(HttpMethod.Get, uri, _deviceInfo);
            var response = await _httpClient.SendAsync(request);
            var json = await response.Content.ReadAsStringAsync();
            if (response.StatusCode == HttpStatusCode.OK)
            {
                var followingActivity = JsonConvert.DeserializeObject<InstaRecentActivityResponse>(json,
                    new InstaRecentActivityConverter());
                return Result.Success(followingActivity);
            }
            return Result.Fail(GetBadStatusFromJsonString(json).Message, (InstaRecentActivityResponse) null);
        }

        private async Task<IResult<InstaMediaListResponse>> GetUserMediaListWithMaxIdAsync(Uri instaUri)
        {
            var request = HttpHelper.GetDefaultRequest(HttpMethod.Get, instaUri, _deviceInfo);
            var response = await _httpClient.SendAsync(request);
            var json = await response.Content.ReadAsStringAsync();
            if (response.StatusCode == HttpStatusCode.OK)
            {
                var mediaResponse = JsonConvert.DeserializeObject<InstaMediaListResponse>(json,
                    new InstaMediaListDataConverter());
                return Result.Success(mediaResponse);
            }
            return Result.Fail("", (InstaMediaListResponse) null);
        }

        private async Task<IResult<InstaFollowersResponse>> GetUserFollowersWithMaxIdAsync(string username, string maxId)
        {
            ValidateCurrentUser();
            try
            {
                if (!IsUserAuthenticated) throw new ArgumentException("user must be authenticated");
                var user = await GetUserAsync(username);
                var userFeedUri = UriCreator.GetUserFollowersUri(user.Value.Pk, _user.RankToken, maxId);
                var request = HttpHelper.GetDefaultRequest(HttpMethod.Get, userFeedUri, _deviceInfo);
                var response = await _httpClient.SendAsync(request);
                var json = await response.Content.ReadAsStringAsync();
                if (response.StatusCode == HttpStatusCode.OK)
                {
                    var followersResponse = JsonConvert.DeserializeObject<InstaFollowersResponse>(json);
                    if (!followersResponse.IsOK()) Result.Fail("", (InstaFollowersResponse) null);
                    return Result.Success(followersResponse);
                }
                return Result.Fail(GetBadStatusFromJsonString(json).Message, (InstaFollowersResponse) null);
            }
            catch (Exception exception)
            {
                return Result.Fail(exception.Message, (InstaFollowersResponse) null);
            }
        }

        private async Task<IResult<InstaActivityFeed>> GetRecentActivityInternalAsync(Uri uri, int maxPages = 0)
        {
            ValidateLoggedIn();
            if (maxPages == 0) maxPages = int.MaxValue;

            var request = HttpHelper.GetDefaultRequest(HttpMethod.Get, uri, _deviceInfo);
            var response = await _httpClient.SendAsync(request, HttpCompletionOption.ResponseContentRead);
            var activityFeed = new InstaActivityFeed();
            var json = await response.Content.ReadAsStringAsync();
            if (response.StatusCode != HttpStatusCode.OK)
                return Result.Fail(GetBadStatusFromJsonString(json).Message, (InstaActivityFeed) null);
            var feedPage = JsonConvert.DeserializeObject<InstaRecentActivityResponse>(json,
                new InstaRecentActivityConverter());
            activityFeed.IsOwnActivity = feedPage.IsOwnActivity;
            var nextId = feedPage.NextMaxId;
            activityFeed.Items.AddRange(
                feedPage.Stories.Select(ConvertersFabric.GetSingleRecentActivityConverter)
                    .Select(converter => converter.Convert()));
            var pages = 1;
            while (!string.IsNullOrEmpty(nextId) && pages < maxPages)
            {
                var nextFollowingFeed = await GetFollowingActivityWithMaxIdAsync(nextId);
                if (!nextFollowingFeed.Succeeded)
                    return Result.Success($"Not all pages was downloaded: {nextFollowingFeed.Info.Message}",
                        activityFeed);
                nextId = nextFollowingFeed.Value.NextMaxId;
                activityFeed.Items.AddRange(
                    feedPage.Stories.Select(ConvertersFabric.GetSingleRecentActivityConverter)
                        .Select(converter => converter.Convert()));
                pages++;
            }
            return Result.Success(activityFeed);
        }

        private async Task<IResult<InstaMediaListResponse>> GetTagFeedWithMaxIdAsync(string tag, string nextId)
        {
            ValidateCurrentUser();
            ValidateLoggedIn();
            try
            {
                var instaUri = UriCreator.GetTagFeedUri(tag);
                instaUri = new UriBuilder(instaUri) {Query = $"max_id={nextId}"}.Uri;
                var request = HttpHelper.GetDefaultRequest(HttpMethod.Get, instaUri, _deviceInfo);
                var response = await _httpClient.SendAsync(request);
                var json = await response.Content.ReadAsStringAsync();
                if (response.StatusCode == HttpStatusCode.OK)
                {
                    var feedResponse = JsonConvert.DeserializeObject<InstaMediaListResponse>(json,
                        new InstaMediaListDataConverter());
                    return Result.Success(feedResponse);
                }
                return Result.Fail(GetBadStatusFromJsonString(json).Message, (InstaMediaListResponse) null);
            }
            catch (Exception exception)
            {
                return Result.Fail(exception.Message, (InstaMediaListResponse) null);
            }
        }

        private async Task<IResult<InstaCommentListResponse>> GetCommentListWithMaxIdAsync(string mediaId, string nextId)
        {
            var commentsUri = UriCreator.GetMediaCommentsUri(mediaId);
            var commentsUriMaxId = new UriBuilder(commentsUri) {Query = $"max_id={nextId}"}.Uri;
            var request = HttpHelper.GetDefaultRequest(HttpMethod.Get, commentsUriMaxId, _deviceInfo);
            var response = await _httpClient.SendAsync(request);
            var json = await response.Content.ReadAsStringAsync();
            if (response.StatusCode == HttpStatusCode.OK)
            {
                var comments = JsonConvert.DeserializeObject<InstaCommentListResponse>(json);
                return Result.Success(comments);
            }
            return Result.Fail("", (InstaCommentListResponse) null);
        }
        #endregion
    }
}
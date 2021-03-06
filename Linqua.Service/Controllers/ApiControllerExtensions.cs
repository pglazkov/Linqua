﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Caching;
using System.Security.Claims;
using System.Security.Principal;
using System.Threading.Tasks;
using System.Web.Http;
using Linqua.Service.Models;
using Microsoft.Azure.Mobile.Server.Authentication;

namespace Linqua.Service.Controllers
{
    public static class ApiControllerExtensions
    {
        private const string UserCacheKeyPrefix = "User_";

        private static readonly MemoryCache Cache = MemoryCache.Default;

        private static readonly object UserLoadLock = new object();

        public static async Task<LinquaUserInfo> GetUserInfoAsync(this ApiController controller)
        {
            var providerUser = await GetUserEntityAsync(controller);

            var appSpecificUserId = GetAppSpecificUserId(controller);

            return new LinquaUserInfo(appSpecificUserId, providerUser);
        }

        private static string GetAppSpecificUserId(ApiController controller)
        {
            string legacyUserId = null;
            IEnumerable<string> legacyUserIdHeaderValues;
            if (controller.Request.Headers.TryGetValues(LegacyUserId.HeaderKey, out legacyUserIdHeaderValues))
            {
                var legacyUserIdHeaderValue = legacyUserIdHeaderValues.SingleOrDefault();

                legacyUserId = $"MicrosoftAccount:{legacyUserIdHeaderValue}";
            }
            return legacyUserId;
        }

        private static async Task<User> GetUserEntityAsync(ApiController controller)
        {
            ClaimsPrincipal principal = (ClaimsPrincipal)controller.User;
            string provider = principal.FindFirst("http://schemas.microsoft.com/identity/claims/identityprovider").Value;

            ProviderCredentials creds;

            if (string.Equals(provider, "microsoftaccount", StringComparison.OrdinalIgnoreCase))
            {
                creds = await controller.User.GetAppServiceIdentityAsync<MicrosoftAccountCredentials>(controller.Request);
            }
            //else if (string.Equals(provider, "facebook", StringComparison.OrdinalIgnoreCase))
            //{
            //    creds = await controller.User.GetAppServiceIdentityAsync<FacebookCredentials>(controller.Request);
            //}
            //else if (string.Equals(provider, "google", StringComparison.OrdinalIgnoreCase))
            //{
            //    creds = await controller.User.GetAppServiceIdentityAsync<GoogleCredentials>(controller.Request);
            //}
            //else if (string.Equals(provider, "twitter", StringComparison.OrdinalIgnoreCase))
            //{
            //    creds = await controller.User.GetAppServiceIdentityAsync<TwitterCredentials>(controller.Request);
            //}
            else
            {
                throw new NotImplementedException();
            }
            
            return GetUserEntity(creds);
        }

        private static User GetUserEntity(ProviderCredentials creds)
        {
            var userId = creds.UserClaims.Single(x => x.Type == ClaimTypes.NameIdentifier).Value;
            var cacheKey = UserCacheKeyPrefix + userId;

            User user = (User)Cache.Get(cacheKey);

            if (user != null)
            {
                return user;
            }

            lock (UserLoadLock)
            {
                using (var dbContext = new LinquaContext())
                {
                    user = dbContext.Users.SingleOrDefault(x => x.MicrosoftAccountId == userId);

                    const int AttemptsCount = 3;
                    int attempt = 1;

                    while (user == null)
                    {
                        user = new User(Guid.NewGuid(), userId, creds.UserClaims.SingleOrDefault(x => x.Type == ClaimTypes.Email)?.Value);

                        dbContext.Users.Add(user);

                        try
                        {
                            dbContext.SaveChanges();
                        }
                        catch (Exception)
                        {
                            if (attempt > AttemptsCount)
                            {
                                throw;
                            }

                            dbContext.Users.Remove(user);

                            attempt++;
                        }

                        user = dbContext.Users.SingleOrDefault(x => x.MicrosoftAccountId == userId);
                    }
                }
            }

            Cache.Set(cacheKey, user, DateTimeOffset.Now.AddHours(12));

            return user;
        }
    }

    public class LinquaUserInfo
    {
        public LinquaUserInfo(string appSpecificMicrosoftUserId, User userEntity)
        {
            UserEntity = userEntity;
            AppSpecificMicrosoftUserId = appSpecificMicrosoftUserId;
        }

        public User UserEntity { get; private set; }

        public string AppSpecificMicrosoftUserId { get; private set; }
    }
}
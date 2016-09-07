﻿using Microsoft.AspNet.Identity;
using Newtonsoft.Json;
using Promact.Core.Repository.DataRepository;
using Promact.Core.Repository.HttpClientRepository;
using Promact.Erp.DomainModel.ApplicationClass;
using Promact.Erp.DomainModel.ApplicationClass.SlackRequestAndResponse;
using Promact.Erp.DomainModel.Models;
using Promact.Erp.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Promact.Core.Repository.ExternalLoginRepository
{
    public class OAuthLoginRepository : IOAuthLoginRepository
    {
        private readonly ApplicationUserManager _userManager;
        private readonly IHttpClientRepository _httpClientRepository;
        private readonly IRepository<SlackUserDetails> _slackUserDetails;
        private readonly IRepository<SlackChannelDetails> _slackChannelDetails;
        public OAuthLoginRepository(ApplicationUserManager userManager)
        {
            _userManager = userManager;
        }
        public async Task<ApplicationUser> AddNewUserFromExternalLogin(string email, string accessToken, string slackUserName)
        {
            try
            {
                ApplicationUser user = new ApplicationUser() { Email = email, UserName = email, SlackUserName = slackUserName };
                //Creating a user with email only. Password not required
                var result = await _userManager.CreateAsync(user);
                if (result.Succeeded)
                {
                    //Adding external Oauth details
                    UserLoginInfo info = new UserLoginInfo(StringConstant.PromactStringName, accessToken);
                    result = await _userManager.AddLoginAsync(user.Id, info);
                }
                return user;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public OAuthApplication ExternalLoginInformation(string refreshToken)
        {
            try
            {
                var clientId = Environment.GetEnvironmentVariable(StringConstant.PromactOAuthClientId, EnvironmentVariableTarget.User);
                var clientSecret = Environment.GetEnvironmentVariable(StringConstant.PromactOAuthClientSecret, EnvironmentVariableTarget.User);
                OAuthApplication oAuth = new OAuthApplication();
                oAuth.ClientId = clientId;
                oAuth.ClientSecret = clientSecret;
                oAuth.RefreshToken = refreshToken;
                oAuth.ReturnUrl = StringConstant.ClientReturnUrl;
                return oAuth;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public async Task AddSlackUserInformation(string code)
        {
            try
            {
                var slackOAuthRequest = string.Format("?client_id={0}&client_secret={1}&code={2}&pretty=1", Environment.GetEnvironmentVariable(StringConstant.SlackOAuthClientId, EnvironmentVariableTarget.User), Environment.GetEnvironmentVariable(StringConstant.SlackOAuthClientSecret, EnvironmentVariableTarget.User), code);
                var slackOAuthResponse = await _httpClientRepository.GetAsync(StringConstant.OAuthAcessUrl, slackOAuthRequest, null);
                var slackOAuth = JsonConvert.DeserializeObject<SlackOAuthResponse>(slackOAuthResponse);
                var userDetailsRequest = string.Format("?token={0}&pretty=1", slackOAuth.AccessToken);
                var userDetailsResponse = await _httpClientRepository.GetAsync(StringConstant.SlackUserListUrl, userDetailsRequest, null);
                var slackUsers = JsonConvert.DeserializeObject<SlackUserResponse>(userDetailsResponse);
                foreach (var user in slackUsers.Members)
                {
                    if (user.Name != StringConstant.SlackBotStringName)
                    {
                        _slackUserDetails.Insert(user);
                        _slackUserDetails.Save();
                    }
                }
                var channelDetailsResponse = await _httpClientRepository.GetAsync(StringConstant.SlackChannelListUrl, userDetailsRequest, null);
                var channels = JsonConvert.DeserializeObject<SlackChannelResponse>(channelDetailsResponse);
                foreach (var channel in channels.Channels)
                {
                    _slackChannelDetails.Insert(channel);
                    _slackChannelDetails.Save();
                }

                var groupDetailsResponse = await _httpClientRepository.GetAsync(StringConstant.SlackGroupListUrl, userDetailsRequest, null);
                var groups = JsonConvert.DeserializeObject<SlackGroupDetails>(groupDetailsResponse);
                foreach (var channel in groups.Groups)
                {
                    _slackChannelDetails.Insert(channel);
                    _slackChannelDetails.Save();
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public void SlackEventUpdate(SlackEventApiAC slackEvent)
        {
            try
            {
                slackEvent.Event.User.TeamId = slackEvent.TeamId;
                _slackUserDetails.Insert(slackEvent.Event.User);
                _slackUserDetails.Save();
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
    }
}

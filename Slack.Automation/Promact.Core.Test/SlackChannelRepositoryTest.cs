﻿using Autofac;
using Promact.Core.Repository.SlackChannelRepository;
using Promact.Erp.DomainModel.ApplicationClass.SlackRequestAndResponse;
using Promact.Erp.Util.StringConstants;
using System.Threading.Tasks;
using Xunit;

namespace Promact.Core.Test
{
    public class SlackChannelRepositoryTest
    {
        private readonly IComponentContext _componentContext;
        private readonly ISlackChannelRepository _slackChannelRepository;
        private readonly IStringConstantRepository _stringConstant;
        private SlackChannelDetails slackChannelDetails = new SlackChannelDetails();
        public SlackChannelRepositoryTest()
        {
            _componentContext = AutofacConfig.RegisterDependancies();
            _slackChannelRepository = _componentContext.Resolve<ISlackChannelRepository>();
            _stringConstant = _componentContext.Resolve<IStringConstantRepository>();
            Initialize();
        }


        /// <summary>
        /// A method is used to initialize variables which are repetitively used
        /// </summary>
        public void Initialize()
        {
            slackChannelDetails.Deleted = false;
            slackChannelDetails.ChannelId = _stringConstant.ChannelIdForTest;
            slackChannelDetails.Name = _stringConstant.ChannelNameForTest;
        }

        #region Test Cases


        /// <summary>
        /// Method to check the functionality of Slack Channel add method
        /// </summary>
        [Fact, Trait("Category", "Required")]
        public async Task SlackChannelAdd()
        {
            await _slackChannelRepository.AddSlackChannelAsync(slackChannelDetails);
            Assert.Equal(slackChannelDetails.ChannelId, _stringConstant.ChannelIdForTest);
        }

        /// <summary>
        /// Test case to check the functionality of GetbyId method of Slack Channel Repository - true case
        /// </summary>
        [Fact, Trait("Category", "Required")]
        public async Task GetByIdAsync()
        {
            await _slackChannelRepository.AddSlackChannelAsync(slackChannelDetails);
            var slackChannel = _slackChannelRepository.GetByIdAsync(_stringConstant.ChannelIdForTest).Result;
            Assert.Equal(slackChannel.ChannelId, _stringConstant.ChannelIdForTest);
        }


        /// <summary>
        /// Test case to check the functionality of GetbyId method of Slack Channel Repository - false case
        /// </summary>
        [Fact, Trait("Category", "Required")]
        public async Task GetByIdFalseAsync()
        {
            await _slackChannelRepository.AddSlackChannelAsync(slackChannelDetails);
            var slackUser = _slackChannelRepository.GetByIdAsync(_stringConstant.ChannelIdForTest).Result;
            Assert.NotEqual(slackUser.ChannelId, _stringConstant.TeamLeaderIdForTest);
        }


        #endregion


    }
}

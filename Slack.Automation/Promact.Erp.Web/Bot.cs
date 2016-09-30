﻿using Autofac;
using Autofac.Extras.NLog;
using Promact.Core.Repository.ScrumRepository;
using Promact.Core.Repository.SlackChannelRepository;
using Promact.Core.Repository.SlackUserRepository;
using Promact.Core.Repository.TaskMailRepository;
using Promact.Erp.Util;
using SlackAPI;
using SlackAPI.WebSocketMessages;
using System;

namespace Promact.Erp.Web
{
    public class Bot
    {
        private static ITaskMailRepository _taskMailRepository;
        private static ISlackUserRepository _slackUserDetails;
        private static ILogger _logger;
        private static ISlackChannelRepository _slackChannelDetails;
        private static IScrumBotRepository _scrumBotRepository;

        public static void Main(IComponentContext container)
        {
            try
            {
                _taskMailRepository = container.Resolve<ITaskMailRepository>();
                _slackUserDetails = container.Resolve<ISlackUserRepository>();
                _logger = container.Resolve<ILogger>();
                // assigning bot token on Slack Socket Client
                string botToken = Environment.GetEnvironmentVariable(StringConstant.TaskmailAccessToken, EnvironmentVariableTarget.Process);

                SlackSocketClient client = new SlackSocketClient(botToken);
                try
                {
                    // Creating a Action<MessageReceived> for Slack Socket Client to get connect. No use in task mail bot
                    MessageReceived messageReceive = new MessageReceived();
                    messageReceive.ok = true;
                    Action<MessageReceived> showMethod = (MessageReceived messageReceived) => new MessageReceived();
                    // Telling Slack Socket Client to the bot whose access token was given early
                    client.Connect((connected) => { });
                    // Method will hit when someone send some text in task mail bot
                    client.OnMessageReceived += (message) =>
                    {
                        var user = _slackUserDetails.GetById(message.user);
                        string replyText = "";
                        var text = message.text;
                        if (text.ToLower() == StringConstant.TaskMailSubject.ToLower())
                        {
                            replyText = _taskMailRepository.StartTaskMail(user.Name).Result;
                        }
                        else
                        {
                            replyText = _taskMailRepository.QuestionAndAnswer(user.Name, text).Result;
                        }
                        // Method to send back response to task mail bot
                        client.SendMessage(showMethod, message.channel, replyText);
                    };
                }
                catch (Exception ex)
                {
                    _logger.Error(StringConstant.LoggerErrorMessageTaskMailBot, ex);
                    client.CloseSocket();
                }
            }
            catch (Exception ex)
            {
                _logger.Error(StringConstant.LoggerErrorMessageTaskMailBot, ex);
            }
        }




        public static void ScrumMain(IComponentContext container)
        {
            try
            {
                string botToken = Environment.GetEnvironmentVariable(StringConstant.ScrumBotToken, EnvironmentVariableTarget.Process);
                if (!(string.IsNullOrEmpty(botToken)))
                {
                    SlackSocketClient client = new SlackSocketClient(botToken);//scrumBot
                    _scrumBotRepository = container.Resolve<IScrumBotRepository>();
                    _slackUserDetails = container.Resolve<ISlackUserRepository>();
                    _slackChannelDetails = container.Resolve<ISlackChannelRepository>();
                    _logger = container.Resolve<ILogger>();
                    // Creating a Action<MessageReceived> for Slack Socket Client to get connected.
                    MessageReceived messageReceive = new MessageReceived();
                    messageReceive.ok = true;
                    Action<MessageReceived> showMethod = (MessageReceived messageReceived) => new MessageReceived();
                    //Connecting the bot of the given token 
                    client.Connect((connected) => { });

                    // Method will be called when someone sends message
                    client.OnMessageReceived += (message) =>
                    {
                        ScrumMessages(message, client, showMethod);
                    };
                }
                else
                {
                    _logger.Error(StringConstant.LoggerScrumBot, StringConstant.TokenEmpty);
                }
            }
            catch (Exception ex)
            {
                _logger.Error(StringConstant.LoggerScrumBot, ex.ToString());
            }
        }


        /// <summary>
        /// Called when a user sends a message to bot either on a direct conversation or in a group/channel where the bot is a member of.
        /// </summary>
        /// <param name="message"></param>
        /// <param name="client"></param>
        /// <param name="showMethod"></param>
        public static void ScrumMessages(NewMessage message, SlackSocketClient client, Action<MessageReceived> showMethod)
        {
            string replyText = string.Empty;
            try
            {
                var user = _slackUserDetails.GetById(message.user);
                var channel = _slackChannelDetails.GetById(message.channel);
                string text = message.text;

                if (user != null && text.ToLower().Equals(StringConstant.ScrumHelp))
                {
                    replyText = StringConstant.ScrumHelpMessage;
                }
                else if (user != null && channel != null)
                {
                    var simpleText = text.Split(null);

                    //start scrum,halt or re-start scrum
                    if (text.ToLower().Equals(StringConstant.ScrumTime) || text.ToLower().Equals(StringConstant.ScrumHalt) || text.ToLower().Equals(StringConstant.ScrumResume))
                    {
                        replyText = _scrumBotRepository.Scrum(channel.Name, user.Name, simpleText[1].ToLower()).Result;
                    }
                    //a particular employee is on leave, geeting marked as later or asked question again
                    else if (((simpleText[0].ToLower().Equals(StringConstant.Leave) || simpleText[0].ToLower().Equals(StringConstant.Later) || simpleText[0].ToLower().Equals(StringConstant.Scrum)) && simpleText.Length == 2))
                    {
                        int from = text.IndexOf("<@") + "<@".Length;
                        int to = text.LastIndexOf(">");
                        if (to > 0)
                        {
                            try
                            {
                                string applicantId = text.Substring(from, to - from);
                                string applicant = _slackUserDetails.GetById(applicantId).Name;
                                replyText = _scrumBotRepository.Leave(channel.Name, user.Name, applicant, simpleText[0].ToLower()).Result;
                            }
                            catch (Exception)
                            {
                                replyText = StringConstant.ScrumHelpMessage;
                            }
                        }
                        else
                            replyText = _scrumBotRepository.AddScrumAnswer(user.Name, text, channel.Name).Result;
                    }
                    //all other texts
                    else
                    {
                        try
                        {
                            replyText = _scrumBotRepository.AddScrumAnswer(user.Name, text, channel.Name).Result;
                        }
                        catch (Exception ex)
                        {
                            _logger.Error(StringConstant.LoggerScrumBot, ex.ToString());
                            client.CloseSocket();
                        }
                    }
                }

                if (!String.IsNullOrEmpty(replyText))
                {
                    // Method to send back response through bot
                    client.SendMessage(showMethod, message.channel, replyText);
                }
            }
            catch (Exception ex)
            {
                _logger.Error(StringConstant.LoggerScrumBot, ex.ToString());
                client.SendMessage(showMethod, message.channel, StringConstant.ErrorMsg);
                client.CloseSocket();
            }
        }

    }
}
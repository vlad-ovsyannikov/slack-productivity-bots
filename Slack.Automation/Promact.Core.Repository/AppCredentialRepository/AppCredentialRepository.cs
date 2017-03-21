﻿using AutoMapper;
using Promact.Erp.DomainModel.DataRepository;
using Promact.Erp.DomainModel.Models;
using System;
using System.Threading.Tasks;

namespace Promact.Core.Repository.AppCredentialRepository
{
    public class AppCredentialRepository : IAppCredentialRepository
    {

        private readonly IRepository<AppCredential> _appCredentialDataRepository;
        private readonly IMapper _mapper;

        public AppCredentialRepository(IMapper mapper,
            IRepository<AppCredential> appCredentialDataRepository)
        {
            _appCredentialDataRepository = appCredentialDataRepository;
            _mapper = mapper;
        }

        /// <summary>
        /// Get the app credentials of the given module - JJ
        /// </summary>
        /// <param name="module">Name of app</param>
        /// <returns>object of AppCredential</returns>
        public async Task<AppCredential> FetchAppCredentialByModuleAsync(string module)
        {
            var app = await _appCredentialDataRepository.FirstOrDefaultAsync(x => x.Module == module);
            return app;
        }

        /// <summary>
        /// Adds the app's credentials to the database - JJ
        /// </summary>
        /// <param name="appCredential">object of app credential</param>
        /// <returns>status of operation</returns>
        public async Task<int> AddUpdateAppCredentialAsync(AppCredential appCredential)
        {
            AppCredential credential = await FetchAppCredentialByModuleAsync(appCredential?.Module);
            if (credential == null)
            {
                appCredential.CreatedOn = DateTime.UtcNow.Date;
                _appCredentialDataRepository.Insert(appCredential);
            }
            else
            {
                credential = _mapper.Map(appCredential, credential);
                _appCredentialDataRepository.Update(credential);
            }
            return await _appCredentialDataRepository.SaveChangesAsync();
        }


        /// <summary>
        /// Get the app credentials of the app which has been selected for integration - JJ
        /// </summary>
        /// <returns>object of AppCredential</returns>
        public async Task<AppCredential> FetchSelectedAppAsync()
        {
            return await _appCredentialDataRepository.FirstOrDefaultAsync(x => x.IsSelected);
        }

        /// <summary>
        /// Updates the app's credentials to the database - JJ
        /// </summary>
        /// <param name="appCredntial">object of app credential</param>
        /// <returns>status of operation</returns>
        public async Task<int> UpdateBotTokenAsync(AppCredential appCredntial)
        {
            _appCredentialDataRepository.Update(appCredntial);
            return await _appCredentialDataRepository.SaveChangesAsync();
        }

    }
}
﻿using AutoMapper;
using EasyRbac.Domain.Entity;
using EasyRbac.Dto;
using EasyRbac.Dto.Application;
using EasyRbac.Reponsitory;
using EasyRbac.Reponsitory.BaseRepository;
using EasyRbac.Utils;
using MyUtility.Commons.Encrypt;
using MyUtility.Commons.IdGenerate;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.Transactions;

namespace EasyRbac.Application.Application
{
    public class ApplicationService : IApplicationService
    {
        private readonly IApplicationRepository _appRepository;
        private readonly IMapper _mapper;
        private readonly IIdGenerator _idGenerator;
        private readonly IEncryptHelper _encryptHelper;
        private readonly IRepository<UserEntity> _userRepository;
        private readonly IRepository<RoleEntity> _userRoleRelRepository;

        public ApplicationService(IApplicationRepository appRepository, IIdGenerator idGenerator, IMapper mapper, IEncryptHelper encryptHelper, IRepository<UserEntity> userRepository,IRepository<RoleEntity> roleRepository)
        {
            this._appRepository = appRepository;
            this._idGenerator = idGenerator;
            this._mapper = mapper;
            this._encryptHelper = encryptHelper;
            this._userRepository = userRepository;
            this._userRoleRelRepository = roleRepository;
        }

        public async Task DisableApp(long id)
        {
            using (var tran = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
            {
                var app = await this._appRepository.QueryFirstAsync(x => x.Id == id);

                await this._appRepository.UpdateAsync(
                    () => new ApplicationEntity()
                    {
                        Enable = false
                    },
                    x => x.Id == id);
                await this._userRepository.UpdateAsync(() => new UserEntity
                {
                    Enable = false
                }, x => x.Id == app.AppUserId);
            }
        }

        public Task EditAsync(long id, ApplicationInfoDto value)
        {
            var appEntity = this._mapper.Map<ApplicationEntity>(value);
            appEntity.CallbackConfigs.ForEach(x =>
            {
                if (x.Id == -1)
                {
                    x.Id = this._idGenerator.NewId();
                    x.AppId = value.Id;
                }
            });
            return this._appRepository.UpdateApplicationConfigInfo(appEntity);

            //return this._appRepository.UpdateAsync(() => new ApplicationEntity
            //{
            //    AppName = value.AppName,
            //    AppCode = value.AppCode,
            //    Descript = value.Descript,
            //    CallbackUrl = value.CallbackUrl
            //}, x => x.Id == id);
        }

        public async Task<ApplicationInfoDto> AddAppAsync(ApplicationInfoDto app)
        {
            var applicationEntity = this._mapper.Map<ApplicationEntity>(app);
            var role = await this._userRoleRelRepository.QueryFirstAsync(x => x.RoleName == "application");

            var pwd = this._encryptHelper.GenerateSalt(10);

            var userEntity = UserEntity.NewUser(this._idGenerator.NewId(), app.AppCode, pwd, app.AppName, this._encryptHelper);
            userEntity.AccountType = Domain.Enums.AccountType.Application;

            applicationEntity.Id = this._idGenerator.NewId();
            applicationEntity.Account = userEntity;
            applicationEntity.AppUserId = userEntity.Id;

            app.CallbackConfigs.ForEach(x => {
                x.Id = this._idGenerator.NewId();
                x.AppId = applicationEntity.Id;
                });
            applicationEntity.UserRole.Add(role);
            await this._appRepository.InsertAsync(applicationEntity);
            app.AppScret = pwd;
            return app;
        }

        public async Task<ApplicationInfoDto> GetOneAsync(long id)
        {

            return await this._appRepository.GetAppInfoEntityAsync(x => x.Id == id)
                .ContinueWith(x => this._mapper.Map<ApplicationInfoDto>(x.Result));
        }

        public Task<ApplicationInfoDto> GetOneAsync(string code)
        {
            return this._appRepository.GetAppInfoEntityAsync(x => x.AppCode == code)
                .ContinueWith(x => this._mapper.Map<ApplicationInfoDto>(x.Result));
        }

        public async Task<PagingList<ApplicationInfoDto>> SearchAppAsync(string appName, int pageIndex, int pageSize)
        {
            PagingList<ApplicationEntity> rsult = await this._appRepository.QueryByPagingAsync(x => x.Enable == true && (x.AppName.StartsWith(appName) || x.AppCode.StartsWith(appName)), x => x.Id, pageIndex, pageSize);
            return this._mapper.Map<PagingList<ApplicationInfoDto>>(rsult);
        }

        public async Task<string> ChangeAppSecuretAsync(long id)
        {
            var newSecuret = this._encryptHelper.GenerateSalt();
            var appInfo = await this._appRepository.GetAppInfoEntityAsync(x=>x.Id == id);
            appInfo.ChangeSecuret(newSecuret, this._encryptHelper);
            await this._appRepository.ChangeAppSecuretAsync(appInfo);
            return newSecuret;
        }

        public Task<ApplicationEntity> GetAppByUserId(long userId)
        {
            return this._appRepository.QueryFirstAsync(x => x.AppUserId == userId);
        }
    }
}

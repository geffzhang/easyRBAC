﻿// Copyright (c) ULiiAn. All rights reserved.

using EasyRbac.Application.User;
using EasyRbac.Dto;
using EasyRbac.Dto.User;
using EasyRbac.Web.WebExtentions;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace EasyRbac.Web.Controllers
{
    [Route("[controller]")]
    public class UserController : Controller
    {
        private IUserService _userService;

        public UserController(IUserService userService)
        {
            this._userService = userService;
        }

        [HttpPost]
        [ResourceTag]
        public Task<long> CreateUser([FromBody]CreateUserDto dto)
        {
            return this._userService.AddUser(dto);
        }

        [HttpPut("{userId:long}/pwd")]
        [ResourceTag]
        public Task ChangeUserPassword(long userId, [FromBody]ChangePwd dto)
        {
            return this._userService.ChangePwd(userId, dto);
        }

        [HttpDelete("{userId}")]
        [ResourceTag]
        public Task DisableUser(long userId)
        {
            return this._userService.DisableUser(userId);
        }

        [HttpPatch("{userId}")]
        [ResourceTag]
        public Task EnableUser(long userId)
        {
            return this._userService.EnableUser(userId);
        }

        [HttpGet("{userId}")]
        [ResourceTag]
        public Task<UserInfoDto> GetUserInfo(long userId)
        {
            return this._userService.GetUserInfo(userId);
        }

        [HttpPut("resource/{userId}/{appId}")]
        [ResourceTag]
        public Task ChangeResources(long userId, long appId,[FromBody]List<string> resouceIdList)
        {
            return this._userService.ChangeResouces(userId, appId, resouceIdList);
        }

        [HttpGet("resource/{userId}/{appId}")]
        [ResourceTag]
        public Task<Dictionary<string, List<string>>> GetUserResourceIds(long userId, long appId)
        {
            return this._userService.GetUserResourceIds(userId, appId);
        }

        [HttpGet]
        [ResourceTag]
        public Task<PagingList<UserInfoDto>> SearchUsers(string userName,int pageIndex,int pageSize)
        {
            return this._userService.SearchUser(userName, pageIndex, pageSize);
        }
    }
}
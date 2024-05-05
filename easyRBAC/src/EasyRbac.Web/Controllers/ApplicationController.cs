﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using EasyRbac.Application.Application;
using EasyRbac.Dto;
using EasyRbac.Dto.Application;
using EasyRbac.Dto.User;
using EasyRbac.Web.WebExtentions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace EasyRbac.Web.Controllers
{
    [Route("Application")]
    public class ApplicationController : Controller
    {
        private IApplicationService _applicationService;

        public ApplicationController(IApplicationService applicationService)
        {
            this._applicationService = applicationService;
        }
        
        [ResourceTag("GetAppInfo")]
        [AllowAnonymous]
        [HttpGet("{id}")]
        public Task<ApplicationInfoDto> Get(long id)
        {
            return this._applicationService.GetOneAsync(id);
        }

        [ResourceTag("AddApp")]
        [HttpPost]
        public Task<ApplicationInfoDto> Post([FromBody]ApplicationInfoDto app)
        {
            var user = (this.HttpContext.User.Identity as UserIdentity);
            var realName = user?.RealName;
            app.CallbackConfigs.ForEach(x => x.CreateBy = realName);
            return this._applicationService.AddAppAsync(app);
        }
        
        [ResourceTag("UpdateAppInfo")]
        [HttpPut("{id}")]
        public Task Put(long id, [FromBody]ApplicationInfoDto value)
        {
            value.CallbackConfigs?.ForEach(x => x.CreateBy = this.User.Identity.Name);
            return this._applicationService.EditAsync(id, value);
        }
        
        [ResourceTag("DeleteApp")]
        [HttpDelete("{id}")]
        public Task Delete(long id)
        {
            return this._applicationService.DisableApp(id);
        }

        public Task<string> ChangeAppSecuret(long id)
        {
            return this._applicationService.ChangeAppSecuretAsync(id);
        }
    }
}

﻿using Microsoft.AspNetCore.Antiforgery;
using Microsoft.AspNetCore.Http;

namespace VDS.AspNetCore.Security.AntiForgery
{
    public class AspNetCoreAntiForgeryManager : IAntiForgeryManager
    {
        public IAntiForgeryConfiguration Configuration { get; }

        private readonly IAntiforgery _antiforgery;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public AspNetCoreAntiForgeryManager(
            IAntiforgery antiforgery,
            IHttpContextAccessor httpContextAccessor,
            IAntiForgeryConfiguration configuration)
        {
            Configuration = configuration;
            _antiforgery = antiforgery;
            _httpContextAccessor = httpContextAccessor;
        }

        public string GenerateToken()
        {
            return _antiforgery.GetAndStoreTokens(_httpContextAccessor.HttpContext).RequestToken;
        }
    }
}
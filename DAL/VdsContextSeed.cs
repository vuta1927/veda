using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Polly;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VDS.Security;
using VDS.Storage.EntityFrameworkCore;

namespace DAL
{
    public class VdsContextSeed : IDbContextSeed
    {
        private readonly ILogger<VdsContext> _logger;
        private readonly VdsContext _ctx;
        public VdsContextSeed(ILogger<VdsContext> logger, VdsContext context)
        {
            _logger = logger;
            _ctx = context;
        }
        public Type ContextType => typeof(VdsContext);
        public async Task SeedAsync()
        {
            var policy = CreatePolicy(nameof(VdsContext));
        }

        private Policy CreatePolicy(string prefix, int retries = 3)
        {
            return Policy.Handle<SqlException>()
                .WaitAndRetryAsync(
                    retryCount: retries,
                    sleepDurationProvider: retry => TimeSpan.FromSeconds(5),
                    onRetry: (exception, timeSpan, retry, ctx) =>
                    {
                        _logger.LogTrace($"[{prefix}] Exception {exception.GetType().Name} with message ${exception.Message} detected on attempt {retry} of {retries}");
                    }
                );
        }
    }
}

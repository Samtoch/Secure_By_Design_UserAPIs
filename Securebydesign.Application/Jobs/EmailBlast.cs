using Securebydesign.Application.Interfaces.EmailServices;
using Securebydesign.Application.Interfaces.Repositories;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text;

namespace Securebydesign.Application.Jobs
{
    public class EmailBlast
    {
        private readonly ILogger<EmailBlast> _logger;
        private readonly IEmailService _emailService;
        private readonly IConfiguration _configuration;
        private const string JobName = "NotificationJob";

        public EmailBlast(ILogger<EmailBlast> logger, IEmailService emailService, IConfiguration configuration)
        {
            _logger = logger;
            _emailService = emailService;
            _configuration = configuration;
        }

        public async Task ExecuteAsync()
        {
            var jobStartTime = DateTime.UtcNow;
            _logger.LogInformation("NotificationJob started at {Time}", jobStartTime);

            try
            {
                var lastRunTime = string.Empty; //await _repository.GetLastRunTimeAsync(JobName);
                _logger.LogInformation("Processing feed items posted after {LastRunTime}", lastRunTime);

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "NotificationJob failed");
                throw;
            }
        }

       
    }
}
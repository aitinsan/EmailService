using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace EmailService
{
    public class Worker : BackgroundService
    {
        private readonly ILogger<Worker> _logger;
        private readonly IEmailSender _sender;
        private HttpClient client;

        public Worker(ILogger<Worker> logger, IEmailSender sender)
        {
            _logger = logger;
            _sender = sender;
        }

        public override Task StartAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("-----Email Service Started---------");
            client = new HttpClient();
            return base.StartAsync(cancellationToken);
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                
                _logger.LogInformation("Worker running at: {time}", DateTimeOffset.Now);

                    var response = await client.GetAsync("http://localhost:60123/api/queue/retrieve/email");
                    string result = response.Content.ReadAsStringAsync().Result;
                    _logger.LogInformation(result);
                    try
                    {
                        var email = JsonConvert.DeserializeObject<MessageDTO>(result);
                        if(email != null)
                        {
                            var message = new Message(new string[] {"irepect@mail.ru"}, "Email Service", email.JsonContent, "");
                            await _sender.SendEmail(message);
                            await client.GetAsync("http://localhost:60123/api/queue/handled/"+email.Id);
                        }
                    }
                    catch(Exception ex)
                    {
                        _logger.LogError(ex.Message);
                    }

                await Task.Delay(15000, stoppingToken);
            }
        }

        public override Task StopAsync(CancellationToken cancellationToken)
        {
            client.Dispose();
            _logger.LogInformation("-----------Email Service Stopped---------");
            return base.StopAsync(cancellationToken);
        }
    }
}

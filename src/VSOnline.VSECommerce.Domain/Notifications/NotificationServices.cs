using FirebaseAdmin;
using FirebaseAdmin.Messaging;
using Google.Apis.Auth.OAuth2;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VSOnline.VSECommerce.Domain.Notifications
{
    public class NotificationServices
    {
        IConfigurationRoot _configuration = new ConfigurationBuilder().SetBasePath(Directory.GetCurrentDirectory()).AddJsonFile("appsettings.json").Build();
        private readonly IConfigurationSection _appSettings;

        public NotificationServices()
        {
            _appSettings = _configuration.GetSection("AppSettings");
        }
        public async Task SendTotificationBYToKen(string title, string body, string deviceToken)
        {
            try
            {
                if (!string.IsNullOrEmpty(deviceToken))
                {
                    if (FirebaseApp.DefaultInstance == null)
                    {
                        FirebaseApp.Create(new AppOptions()
                        {
                            Credential = GoogleCredential.FromFile(@"app-notification.json").CreateScoped(_appSettings.GetValue<string>("FirebaseUrl")),
                            ProjectId = _appSettings.GetValue<string>("FirebaseProjectId")
                        });
                    }
                    var message = new Message()
                    {
                        Apns = new ApnsConfig()
                        {
                            Aps = new Aps()
                            {
                                Sound = "default"
                            }
                        },
                        Notification = new Notification()
                        {
                            Title = title,
                            Body = body,
                        },
                        Token = deviceToken,
                    };
                    string response = await FirebaseMessaging.DefaultInstance.SendAsync(message);
                }
            }
            catch (Exception ex)
            {
            }
        }
        public async Task<string> SendTotificationBYTopic(string title, string body)
        {
            try
            {
                if (FirebaseApp.DefaultInstance == null)
                {
                    FirebaseApp.Create(new AppOptions()
                    {
                        Credential = GoogleCredential.FromFile(@"app-notification.json").CreateScoped(_appSettings.GetValue<string>("FirebaseUrl")),
                        ProjectId = _appSettings.GetValue<string>("FirebaseProjectId")
                    });
                }
                var message = new Message()
                {
                    Apns = new ApnsConfig()
                    {
                        Aps = new Aps()
                        {
                            Sound = "default"
                        }
                    },
                    Notification = new Notification()
                    {
                        Title = title,
                        Body = body,
                    },
                    Topic = _appSettings.GetValue<string>("FirebaseTopic")
                };
                string response = await FirebaseMessaging.DefaultInstance.SendAsync(message);
                return response;
            }
            catch (Exception ex)
            {
                return ex.ToString();
            }
        }
    }
}

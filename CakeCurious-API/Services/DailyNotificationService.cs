using BusinessObject;
using CakeCurious_API.Utilities;
using CakeCurious_API.Utilities.FirebaseCloudMessaging;
using FirebaseAdmin.Messaging;
using Repository.Interfaces;
using System.Globalization;

namespace CakeCurious_API.Services
{
    public class DailyNotificationService : IHostedService, IDisposable
    {
        public IServiceProvider Services { get; }
        private readonly ILogger<DailyNotificationService>? _logger;
        private Timer _timer = null!;
        private const int TAKE = 50;
        private const string DEFAULT = "07:00";
        private const string DEFAULT_INPUT_GMT = "7";
        private const int DEFAULT_GMT = 7;
        private static readonly string[] FORMATS = { @"hh\:mm\:ss", "hh\\:mm" };

        public DailyNotificationService(ILogger<DailyNotificationService> logger, IServiceProvider services)
        {
            _logger = logger;
            Services = services;
        }

        private static TimeSpan GetScheduledTime()
        {
            var dailyScheduledTime = Environment.GetEnvironmentVariable(EnvironmentHelper.DailyScheduledTime);
            string startTime = dailyScheduledTime ?? DEFAULT;
            TimeSpan.TryParseExact(startTime, FORMATS, CultureInfo.InvariantCulture, out TimeSpan scheduledTimespan);
            return scheduledTimespan;
        }

        private static TimeSpan GetTimeToDelay()
        {
            var inputGMT = Environment.GetEnvironmentVariable(EnvironmentHelper.InputGMT) ?? DEFAULT_INPUT_GMT;
            if (!int.TryParse(inputGMT, out int gmt)) gmt = DEFAULT_GMT;
            var scheduledTime = GetScheduledTime();
            var currentTime = TimeSpan.Parse(DateTime.Now.TimeOfDay.Add(TimeSpan.FromHours(gmt)).ToString("hh\\:mm"));
            Console.WriteLine(currentTime.ToString());
            var timeToDelay = scheduledTime >= currentTime
                ? scheduledTime - currentTime
                : new TimeSpan(24, 0, 0) - currentTime + scheduledTime;
            Console.WriteLine(timeToDelay.ToString());
            return timeToDelay;
        }

        public void Dispose()
        {
            _timer?.Dispose();
            _logger!.LogInformation("Disposed timer for daily notification service.");
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            var now = DateTime.Now;

            _timer = new Timer(NotifyDailyAsync, null, GetTimeToDelay(), TimeSpan.FromHours(24));
            _logger!.LogInformation("Timer for daily notification service started.");
            return Task.CompletedTask;
        }

        private async void NotifyDailyAsync(object? state)
        {
            using (var scope = Services.CreateScope())
            {
                try
                {
                    _logger!.LogInformation("Notifying daily.");
                    var userDeviceRepository = scope.ServiceProvider.GetRequiredService<IUserDeviceRepository>();
                    var batches = new List<IEnumerable<UserDevice>>();
                    var initialBatch = userDeviceRepository.GetDevicesAfter(TAKE, "0");
                    batches.Add(initialBatch);
                    while (batches.Count > 0)
                    {
                        var userDevices = batches.FirstOrDefault();
                        if (userDevices != null)
                        {
                            var tokens = userDevices.Select(x => x.Token).ToList();
                            var message = new MulticastMessage
                            {
                                Tokens = tokens,
                                Notification = new FirebaseAdmin.Messaging.Notification
                                {
                                    Title = "CakeCurious",
                                    Body = ":trollge:"
                                }
                            };

                            try
                            {
                                var response = await FirebaseCloudMessageSender.SendMulticastAsync(message);
                                await InvalidFcmTokenCollector.HandleMulticastBatchResponse(response, tokens!, userDeviceRepository);
                            }
                            catch (FirebaseMessagingException fbme)
                            {
                                _logger!.LogInformation(fbme.Message);
                            }

                            var nextBatch = userDeviceRepository.GetDevicesAfter(TAKE, userDevices.LastOrDefault()!.Token!);
                            batches.RemoveAt(0);
                            if (nextBatch != null && nextBatch.Count() > 0)
                            {
                                batches.Add(nextBatch);
                            }
                        }
                    }
                }
                catch (Exception e)
                {
                    _logger!.LogInformation("Daily notification operation was not successful.");
                    _logger!.LogError(e, string.Empty, Array.Empty<int>());
                }
            }
        }

        Task IHostedService.StopAsync(CancellationToken cancellationToken)
        {
            _timer.Change(Timeout.Infinite, 0);
            _logger!.LogInformation("Timer for daily notification service stopped.");
            return Task.CompletedTask;
        }
    }
}

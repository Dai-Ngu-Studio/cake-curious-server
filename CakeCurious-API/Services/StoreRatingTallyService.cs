using BusinessObject;
using Repository.Interfaces;

namespace CakeCurious_API.Services
{
    public class StoreRatingTallyService : IHostedService, IDisposable
    {
        public IServiceProvider Services { get; }
        private readonly ILogger<StoreRatingTallyService>? _logger;
        private Timer _timer = null!;
        private const int TAKE = 10;

        public StoreRatingTallyService(ILogger<StoreRatingTallyService> logger, IServiceProvider services)
        {
            _logger = logger;
            Services = services;
        }
        public void Dispose()
        {
            _timer?.Dispose();
            _logger!.LogInformation("Disposed timer for store rating tally service.");
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            _timer = new Timer(TallyStoreRatingAsync, null, TimeSpan.Zero, TimeSpan.FromDays(1));
            _logger!.LogInformation("Timer for store rating tally service started.");
            return Task.CompletedTask;
        }

        private async void TallyStoreRatingAsync(object? state)
        {
            using (var scope = Services.CreateScope())
            {
                try
                {
                    _logger!.LogInformation("Tallying store rating.");
                    var storeRepository = scope.ServiceProvider.GetRequiredService<IStoreRepository>();
                    var batches = new List<IEnumerable<Store>>();
                    var initialBatch = storeRepository.GetStoresAfter(TAKE, null);
                    batches.Add(initialBatch);
                    while (batches.Count > 0)
                    {
                        var stores = batches.FirstOrDefault();
                        if (stores != null)
                        {
                            foreach (var store in stores)
                            {
                                store.Rating = await storeRepository.GetRatingForStore((Guid)store.Id!);
                            }
                            await storeRepository.UpdateRange(stores);
                            var nextBatch = storeRepository.GetStoresAfter(TAKE, stores.LastOrDefault()?.Id);
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
                    _logger!.LogInformation("Store rating tally operation was not successful.");
                    _logger!.LogError(e, string.Empty, Array.Empty<int>());
                }
            }
        }

        Task IHostedService.StopAsync(CancellationToken cancellationToken)
        {
            _timer.Change(Timeout.Infinite, 0);
            _logger!.LogInformation("Timer for store rating tally service stopped.");
            return Task.CompletedTask;
        }
    }
}

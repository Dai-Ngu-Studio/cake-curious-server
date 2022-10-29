using BusinessObject;
using Repository.Constants.Coupons;
using Repository.Interfaces;

namespace CakeCurious_API.Services
{
    public class CouponExpireCheckService : IHostedService, IDisposable
    {
        public IServiceProvider Services { get; }
        private readonly ILogger<CouponExpireCheckService>? _logger;
        private Timer _timer = null!;
        public CouponExpireCheckService(ILogger<CouponExpireCheckService> logger, IServiceProvider services)
        {
            _logger = logger;
            Services = services;
        }
        public void Dispose()
        {
            _timer?.Dispose();
            _logger!.LogInformation("Disposed timer for booking status check.");
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            _timer = new Timer(CheckBookingStatusAsync, null, TimeSpan.Zero, TimeSpan.FromMinutes(30));
            _logger!.LogInformation("Timer for coupon status check started.");
            return Task.CompletedTask;
        }

        private async void CheckBookingStatusAsync(object? state)
        {
            using (var scope = Services.CreateScope())
            {
                try
                {
                    _logger!.LogInformation("Checking coupon status.");
                    var couponRepository = scope.ServiceProvider.GetRequiredService<ICouponRepository>();
                    List<Coupon> coupons = (await couponRepository.GetAllActiveCoupon()).ToList();
                    foreach (var coupon in coupons)
                    {
                        try
                        {
                            if (DateTime.Now >= coupon.ExpiryDate)
                            {                             
                                    coupon.Status = (int)CouponStatusEnum.Inactive;
                            }                  
                        }
                        catch
                        {
                            _logger!.LogInformation("Skipping faulty data.");
                            continue;
                        }
                    }
                    await couponRepository.UpdateRange(coupons.ToArray());
                }
                catch (Exception e)
                {
                    _logger!.LogInformation("Coupon status check operation was not successful.");
                    _logger!.LogError(e, string.Empty, Array.Empty<int>());
                }
            }
        }

        Task IHostedService.StopAsync(CancellationToken cancellationToken)
        {
            _timer.Change(Timeout.Infinite, 0);
            _logger!.LogInformation("Timer for coupon status check stopped.");
            return Task.CompletedTask;
        }
    }
}

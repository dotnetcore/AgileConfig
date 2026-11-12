using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AgileConfig.Server.Apisite.Metrics;
using AgileConfig.Server.Apisite.Utilites;
using AgileConfig.Server.IService;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace AgileConfig.Server.Apisite;

public class InitService : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly InitializationState _initialization;
    private readonly TimeProvider _timeProvider;
    private IServiceScope _eventScope;
    private readonly ILogger _logger;
    private readonly IMeterService _meterService;

    public InitService(IServiceScopeFactory serviceScopeFactory,
        IMeterService meterService,
        ILogger<InitService> logger,
        InitializationState initialization,
        TimeProvider timeProvider)
    {
        _logger = logger;
        _meterService = meterService;
        _scopeFactory = serviceScopeFactory;
        _initialization = initialization;
        _timeProvider = timeProvider;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        if (!string.IsNullOrEmpty(Appsettings.OtlpMetricsEndpoint)) _meterService.Start();
        var failures = 0;
        while (!stoppingToken.IsCancellationRequested)
        {
            IServiceScope scope = null;
            try
            {
                scope = _scopeFactory.CreateScope();
                var services = scope.ServiceProvider;
                var initializer = services.GetRequiredService<ISystemInitializationService>();
                if (!initializer.TryInitDefaultEnvironment())
                    throw new InvalidOperationException("Default environment initialization failed.");

                if (Appsettings.IsAdminConsoleMode)
                {
                    if (!initializer.TryInitJwtSecret() ||
                        !await initializer.TryInitSystemRolesAndPermissions() ||
                        !initializer.TryInitSaPassword() ||
                        !initializer.TryInitDefaultApp())
                        throw new InvalidOperationException("Admin console initialization failed.");
                }

                // Keep later JWT option creation from depending on a live database.
                services.GetRequiredService<IJwtService>().GetSecurityKey();
                stoppingToken.ThrowIfCancellationRequested();
                if (Appsettings.Cluster)
                {
                    var ip = GetIp();
                    if (!string.IsNullOrEmpty(ip))
                    {
                        var desc = Appsettings.IsAdminConsoleMode ? "Console node" : "";
                        await services.GetRequiredService<IServerNodeService>().JoinAsync(ip, 5000, desc);
                        _logger.LogInformation("AgileConfig node http://{Ip}:5000 joined.", ip);
                    }
                }

                if (Appsettings.IsAdminConsoleMode)
                    services.GetRequiredService<IEventHandlerRegister>().Register();
                _eventScope = scope;
                scope = null;
                _initialization.MarkReady();
                _logger.LogInformation("Database initialization completed; the node is ready.");
                return;
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                return;
            }
            catch (Exception exception)
            {
                _logger.LogError(exception, "Database initialization failed; the node remains unavailable and will retry.");
            }
            finally
            {
                scope?.Dispose();
            }

            await Task.Delay(DatabaseMaintenanceService.RetryDelay(++failures), _timeProvider, stoppingToken);
        }
    }

    public override async Task StopAsync(CancellationToken cancellationToken)
    {
        await base.StopAsync(cancellationToken);
        if (_initialization.IsReady && Appsettings.Cluster && !cancellationToken.IsCancellationRequested)
        {
            var ip = GetIp();
            if (!string.IsNullOrEmpty(ip))
            {
                try
                {
                    using var scope = _scopeFactory.CreateScope();
                    await scope.ServiceProvider.GetRequiredService<IServerNodeService>().DeleteAsync($"http://{ip}:5000");
                    _logger.LogInformation("AgileConfig node http://{Ip}:5000 removed.", ip);
                }
                catch (Exception exception)
                {
                    _logger.LogWarning(exception, "Could not unregister the node during shutdown.");
                }
            }
        }
    }

    public override void Dispose()
    {
        base.Dispose();
        _eventScope?.Dispose();
    }

    private string GetIp()
    {
        try
        {
            var myips = IpExt.GetEndpointIp();
            _logger.LogInformation("AgileConfig node's IP " + string.Join(',', myips));

            return myips.FirstOrDefault();
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Try get node's IP error .");
        }

        return "";
    }
}
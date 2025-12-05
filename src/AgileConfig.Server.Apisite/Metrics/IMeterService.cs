using System.Diagnostics.Metrics;

namespace AgileConfig.Server.Apisite.Metrics;

public interface IMeterService
{
    ObservableGauge<int> AppGauge { get; }
    ObservableGauge<int> ClientGauge { get; }
    ObservableGauge<int> ConfigGauge { get; }
    ObservableGauge<int> NodeGauge { get; }
    Counter<long> PullAppConfigCounter { get; }
    ObservableGauge<int> ServiceGauge { get; }

    void Start();
}
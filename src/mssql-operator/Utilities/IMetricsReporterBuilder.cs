using App.Metrics.Builder;
using Microsoft.Extensions.Configuration;

namespace MSSqlOperator.Utilities
{
    public interface IMetricsReporterBuilder
    {
        void Chain(IMetricsReportingBuilder builder, IConfigurationSection configuration);
    }
}
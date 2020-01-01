using System;
using System.Collections.Generic;
using System.Text;
using App.Metrics;
using App.Metrics.Builder;
using App.Metrics.Formatters.Json;
using App.Metrics.Reporting.InfluxDB;
using Microsoft.Extensions.Configuration;

namespace MSSqlOperator.Utilities
{
    public class MetricsReporterBuilder : IMetricsReporterBuilder
    {
        private readonly Dictionary<string, IMetricsReporterBuilder> builders = new Dictionary<string, IMetricsReporterBuilder>
        {
            { "influxdb", new InfluxDbMetricsReporterBuilder() },
            { "console", new ConsoleMetricsReporterBuilder() }
        };

        public void Chain(IMetricsReportingBuilder builder, IConfigurationSection configuration)
        {
            var sections = configuration.GetChildren();

            foreach (var section in sections)
            {
                var reporterType = section.Key.ToLower();
                if (builders.ContainsKey(reporterType))
                {
                    builders[reporterType].Chain(builder, section);
                }
            }
        }
    }

    public class ConsoleMetricsReporterBuilder : IMetricsReporterBuilder
    {
        public void Chain(IMetricsReportingBuilder builder, IConfigurationSection configuration)
        {
            builder.ToConsole();
        }
    }

    public class InfluxDbMetricsReporterBuilder : IMetricsReporterBuilder
    {
        public void Chain(IMetricsReportingBuilder builder, IConfigurationSection configuration)
        {
            var options = GetOptions(configuration);
            builder.ToInfluxDb(options);
        }

        private MetricsReportingInfluxDbOptions GetOptions(IConfigurationSection configuration)
        {
            var options = new MetricsReportingInfluxDbOptions();
            options.InfluxDb.BaseUri = configuration.GetValue<Uri>("BaseUri");
            options.InfluxDb.Database = configuration.GetValue<string>("Database");
            options.InfluxDb.UserName = configuration.GetValue<string>("UserName");
            options.InfluxDb.Password = configuration.GetValue<string>("Password");
            options.InfluxDb.RetentionPolicy = configuration.GetValue<string>("RetentionPolicy");
            options.InfluxDb.CreateDataBaseIfNotExists = configuration.GetValue<bool>("CreateDatabaseIfNotExists");

            var retentionTime = configuration.GetValue<TimeSpan?>("RetentionPolicyTimespan");
            if (retentionTime != null)
            {
                options.InfluxDb.CreateDatabaseRetentionPolicy.Duration = retentionTime;
                options.InfluxDb.CreateDatabaseRetentionPolicy.Name = options.InfluxDb.RetentionPolicy;
            }

            return options;
        }
    }
}

using System.Collections.ObjectModel;
using Microsoft.Extensions.Logging;

namespace GrpcHost
{
    public class HostOptions
    {
        public HostOptions()
        {
        }

        public HostOptions(string[] args)
        {
            Host = args.Length >= 1 ? args[0] : Host;
            Port = args.Length >= 2 ? int.Parse(args[1]) : Port;
        }

        public string Host { get; set; } = "0.0.0.0";

        public int Port { get; set; } = 80;
    }

    public class LoggingOptions
    {
        public LogLevel MinimumLogLevel { get; set; } = LogLevel.Warning;

        public TypeLoggingOptions RequestLoggingOptions { get; set; } = new TypeLoggingOptions();

        public TypeLoggingOptions ResponseLoggingOptions { get; set; } = new TypeLoggingOptions();

        public class TypeLoggingOptions
        {
            public bool LogAll { get; set; } = false;

            public Collection<string> WellKnownProperties { get; set; } = new Collection<string>();

            public Collection<PropertyMetadata> PropertyFilter { get; set; } = new Collection<PropertyMetadata>();
        }

        public class PropertyMetadata
        {
            public string TypeName { get; set; }

            public Collection<string> PropertyNames { get; set; } = new Collection<string>();
        }
    }
}

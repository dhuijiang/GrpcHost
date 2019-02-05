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

        public string Host { get; set; } = "localhost";

        public int Port { get; set; } = 5000;
    }

    public class DiagnosticInterceptorOptions
    {
        public DiagnosticInterceptorOption[] ResponseLoggingFilters { get; set; }
    }

    public class DiagnosticInterceptorOption
    {
        public string ResponseTypeName { get; set; }

        public string[] PropertyNames { get; set; }
    }
}
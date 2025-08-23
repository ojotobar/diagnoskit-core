using Serilog;

namespace DiagnosKit.Core.Logging
{
    public class SerilogBootstrapper
    {
        public static void UseBootstrapLogger()
        {
            Log.Logger = new LoggerConfiguration()
                .Enrich.FromLogContext()
                .WriteTo.Console()
                .CreateBootstrapLogger();
        }
    }
}

using Microsoft.Extensions.Logging;

namespace MilkiDrugStore.Infrastructure.Logging;

public static class LoggerConfiguration
{
    public static void ConfigureLogger(ILoggingBuilder logging)
    {
        logging.ClearProviders();
        logging.AddConsole();
        logging.AddDebug();
    }
}

using Serilog.Events;
using Serilog.Formatting;

namespace ZODs.Api.Helpers;

public class AWSLogFormatter : ITextFormatter
{
    public void Format(LogEvent logEvent, TextWriter output)
    {
        // Check for null arguments to ensure safety
        if (logEvent == null)
        {
            throw new ArgumentNullException(nameof(logEvent));
        }

        if (output == null)
        {
            throw new ArgumentNullException(nameof(output));
        }

        try
        {
            output.WriteLine("[{0}]: {1}", logEvent.Level, logEvent.RenderMessage());

            // If there is an associated exception, log it
            if (logEvent.Exception != null)
            {
                output.WriteLine("Exception: {0}", logEvent.Exception);
            }
        }
        catch (Exception ex)
        {
            // In case of an error during formatting, write an error message
            output.WriteLine($"An error occurred while formatting the log event: {ex.Message}");
        }
    }
}

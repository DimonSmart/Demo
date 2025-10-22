using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using DimonSmart.PdfCropper;

namespace Demo.PdfProcessing;

internal sealed class InMemoryLogger : IPdfCropLogger
{
    private readonly List<string> _messages = new();
    private readonly Func<string, Task>? _callback;

    public InMemoryLogger(Func<string, Task>? callback)
    {
        _callback = callback;
    }

    public IReadOnlyList<string> Messages => _messages;

    public Task LogInfoAsync(string message) => AddMessageAsync("INFO", message);

    public Task LogWarningAsync(string message) => AddMessageAsync("WARN", message);

    public Task LogErrorAsync(string message) => AddMessageAsync("ERROR", message);

    private Task AddMessageAsync(string level, string message)
    {
        var formatted = $"{level}: {message}";
        _messages.Add(formatted);

        if (_callback != null)
        {
            _ = InvokeCallbackAsync(_callback, formatted);
        }

        return Task.CompletedTask;
    }

    private static async Task InvokeCallbackAsync(Func<string, Task> callback, string message)
    {
        try
        {
            await callback(message);
        }
        catch
        {
            // Intentionally swallow callback failures to avoid breaking the logger.
        }
    }
}

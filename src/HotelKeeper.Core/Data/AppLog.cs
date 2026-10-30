using System.Diagnostics;
using System.Text.RegularExpressions;

namespace HotelKeeper.Data;

/// <summary>
/// Writes unexpected failures to a local log file. User-facing messages never include these details.
/// Connection-string passwords are redacted before anything is persisted.
/// </summary>
public static class AppLog
{
    static readonly object Gate = new();
    static readonly Regex SecretPattern = new(@"(?i)\b(password|pwd)\s*=\s*[^;]*", RegexOptions.Compiled);

    public static string LogDirectory => Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "HotelKeeper", "logs");

    public static string LogFile => Path.Combine(LogDirectory, $"hotelkeeper-{DateTime.Today:yyyyMMdd}.log");

    public static void Error(string context, Exception exception)
    {
        var text = $"[{DateTimeOffset.Now:O}] {context}{Environment.NewLine}{exception}{Environment.NewLine}{Environment.NewLine}";
        Write(text);
    }

    static void Write(string text)
    {
        try
        {
            lock (Gate)
            {
                Directory.CreateDirectory(LogDirectory);
                File.AppendAllText(LogFile, SecretPattern.Replace(text, "$1=***"));
            }
        }
        catch (Exception logFailure)
        {
            // Logging must never take the application down. The debug output is for developers only.
            Debug.WriteLine("HotelKeeper logging failed: " + logFailure.Message);
        }
    }
}

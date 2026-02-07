using System;
using System.IO;
using System.Linq;

namespace MeuGestorVODs.Security;

public static class SecurityValidator
{
    private static readonly string[] AllowedSchemes = { "http", "https" };

    public static ValidationResult ValidateM3UUrl(string? url)
    {
        if (string.IsNullOrWhiteSpace(url))
            return ValidationResult.Failure("URL cannot be empty");

        if (!Uri.TryCreate(url, UriKind.Absolute, out var uri))
            return ValidationResult.Failure("Invalid URL format");

        if (!AllowedSchemes.Contains(uri.Scheme.ToLowerInvariant()))
            return ValidationResult.Failure($"URL scheme '{uri.Scheme}' is not allowed. Only HTTP and HTTPS are supported.");

        return ValidationResult.Success();
    }

    public static string SanitizeFileName(string? fileName)
    {
        if (string.IsNullOrWhiteSpace(fileName))
            return "unnamed";

        var invalid = Path.GetInvalidFileNameChars();
        foreach (var c in invalid)
        {
            fileName = fileName.Replace(c, '_');
        }

        fileName = fileName.Replace("..", "_");
        fileName = fileName.Trim();
        
        if (fileName.Length > 200)
            fileName = fileName[..200];

        if (string.IsNullOrWhiteSpace(fileName))
            fileName = "unnamed";

        return fileName;
    }
}

public class ValidationResult
{
    public bool IsValid { get; }
    public string? ErrorMessage { get; }

    private ValidationResult(bool isValid, string? errorMessage)
    {
        IsValid = isValid;
        ErrorMessage = errorMessage;
    }

    public static ValidationResult Success() => new(true, null);
    public static ValidationResult Failure(string message) => new(false, message);
}

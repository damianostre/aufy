using System.Collections.Concurrent;
using Aufy.Core;
using Aufy.Core.EmailSender;
using TestWebApi.Data;

namespace TestWebApi.IntegrationTests;

public class TestAufyEmailSenderManager : IAufyEmailSenderManager<TestUser>
{
    private static ConcurrentDictionary<(string?, string), int> _log = new();
    private static ConcurrentDictionary<string, string> _resetPasswordCodes = new();
    private static ConcurrentDictionary<string, string> _confirmEmailLinks = new();
    
    public Task SendEmailConfirmationAsync(TestUser user, string confirmationLink)
    {
        var value = _log.TryGetValue((user.Email, nameof(SendEmailConfirmationAsync)), out var count) ? count : 0;
        _log.AddOrUpdate((user.Email, nameof(SendEmailConfirmationAsync)), value + 1, (_, _) => value + 1);
        _confirmEmailLinks.TryAdd(user.Email ?? string.Empty , confirmationLink);
        
        return Task.CompletedTask;
    }

    public Task SendPasswordResetAsync(TestUser user, string resetLink)
    {
        var value = _log.TryGetValue((user.Email, nameof(SendPasswordResetAsync)), out var count) ? count : 0;
        _log.AddOrUpdate((user.Email, nameof(SendPasswordResetAsync)), value + 1, (_, _) => value + 1);
        _resetPasswordCodes.TryAdd(user.Email ?? string.Empty, resetLink);
        
        return Task.CompletedTask;
    }
    
    public static int GetSendPasswordForgotAsyncCount(string email)
    {
        return _log.TryGetValue((email, nameof(SendPasswordResetAsync)), out var count) ? count : 0;
    }
    
    public static int GetSendEmailConfirmationAsyncCount(string email)
    {
        return _log.TryGetValue((email, nameof(SendEmailConfirmationAsync)), out var count) ? count : 0;
    }
    
    public static string? GetResetPasswordLink(string email)
    {
        return _resetPasswordCodes.TryGetValue(email, out var link) ? link : null;
    }
    
    public static string? GetEmailConfirmationLink(string email)
    {
        return _confirmEmailLinks.TryGetValue(email, out var link) ? link : null;
    }
}
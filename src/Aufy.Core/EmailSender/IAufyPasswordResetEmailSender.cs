namespace Aufy.Core.EmailSender;

/// <summary>
/// Represents an email sender for Aufy
/// </summary>
/// <typeparam name="TUser"></typeparam>
public interface IAufyPasswordResetEmailSender<TUser> where TUser: IAufyUser
{
    /// <summary>
    /// Sends a password reset email to the user
    /// </summary>
    /// <param name="user"></param>
    /// <param name="resetLink"></param>
    /// <returns></returns>
    Task SendAsync(TUser user, string resetLink);
}
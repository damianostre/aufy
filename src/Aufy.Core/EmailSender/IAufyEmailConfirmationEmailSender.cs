namespace Aufy.Core.EmailSender;

/// <summary>
/// Represents an email sender for Aufy
/// </summary>
/// <typeparam name="TUser"></typeparam>
public interface IAufyEmailConfirmationEmailSender<in TUser> where TUser: IAufyUser
{
    /// <summary>
    /// Sends an email confirmation to the user
    /// </summary>
    /// <param name="user"></param>
    /// <param name="confirmationLink"></param>
    /// <returns></returns>
    Task SendAsync(TUser user, string confirmationLink);
}
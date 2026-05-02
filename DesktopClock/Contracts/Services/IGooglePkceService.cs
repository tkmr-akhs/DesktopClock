using Google.Apis.Auth.OAuth2;
using DesktopClock.Services;

namespace DesktopClock.Contracts.Services;

public interface IGooglePkceService
{
    /// <summary>
    /// Gets whether Google authentication should be used by the application.
    /// </summary>
    bool IsAuthenticationRequired { get; }

    /// <summary>
    /// Occurs when the Google authentication setting has changed.
    /// </summary>
    event GooglePkceService.AuthenticationRequiredChangedEventHandler? AuthenticationRequiredChanged;

    /// <summary>
    /// Gets a saved Google credential without starting an interactive authentication flow.
    /// </summary>
    /// <param name="cancellationToken">A token that cancels the credential loading operation.</param>
    /// <returns>A valid credential when one can be loaded or refreshed; otherwise, <c>null</c>.</returns>
    Task<UserCredential?> GetSavedCredentialAsync(CancellationToken cancellationToken);

    /// <summary>
    /// Initializes the authentication state from local settings.
    /// </summary>
    Task InitializeAsync();

    /// <summary>
    /// Gets a saved credential or starts an interactive authentication flow when needed.
    /// </summary>
    /// <param name="cancellationToken">A token that cancels the authentication operation.</param>
    /// <returns>A credential when authentication succeeds; otherwise, <c>null</c>.</returns>
    Task<UserCredential?> AuthenticateAsync(CancellationToken cancellationToken);

    /// <summary>
    /// Updates whether Google authentication should be used by the application.
    /// </summary>
    /// <param name="needToAuthenticated">True to require Google authentication; false to clear it.</param>
    /// <param name="cancellationToken">A token that cancels the update operation.</param>
    Task SetAuthenticationRequiredAsync(bool needToAuthenticated, CancellationToken cancellationToken);
}

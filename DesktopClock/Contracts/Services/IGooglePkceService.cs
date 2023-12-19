using Google.Apis.Auth.OAuth2;
using DesktopClock.Services;

namespace DesktopClock.Contracts.Services;

public interface IGooglePkceService
{
    bool IsAuthenticationRequired { get; }
    event GooglePkceService.AuthenticationRequiredChangedEventHandler? AuthenticationRequiredChanged;
    Task InitializeAsync();
    Task<UserCredential> AuthenticateAsync(CancellationToken cancellationToken);
    Task SetAuthenticationRequiredAsync(bool needToAuthenticated, CancellationToken cancellationToken);
}

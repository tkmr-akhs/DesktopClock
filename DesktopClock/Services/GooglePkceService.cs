using Microsoft.Extensions.Options;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Auth.OAuth2.Flows;
using Google.Apis.Calendar.v3;
using Google.Apis.Util;
using DesktopClock.Models;
using DesktopClock.Contracts.Services;
using DesktopClock.ViewModels;
using System.Diagnostics;

namespace DesktopClock.Services;

public class GooglePkceService : IGooglePkceService
{
    private const string AuthenticationRequiredSettingsKey = "GooglePkceIsAuthenticated";
    private static readonly string[] Scopes = new string[] { CalendarService.Scope.CalendarEventsReadonly, CalendarService.Scope.CalendarReadonly };

    public bool IsAuthenticationRequired { get; set; }
    public event AuthenticationRequiredChangedEventHandler? AuthenticationRequiredChanged;

    private readonly ILoggingService _loggingService;
    private readonly ILocalSettingsDataStoreService _localSettingsDataStoreService;
    private readonly PkceGoogleAuthorizationCodeFlow _flow;
    private readonly LocalSettingsOptions _options;

    public GooglePkceService(ILoggingService loggingService, ILocalSettingsDataStoreService localSettingsDataStoreService, IOptions<LocalSettingsOptions> options)
    {
        _loggingService = loggingService;
        _options = options.Value;
        _localSettingsDataStoreService = localSettingsDataStoreService;

        // PKCEに対応した認証コードフローの初期化を行います。
        var initializer = new GoogleAuthorizationCodeFlow.Initializer
        {
            ClientSecrets = new ClientSecrets
            {
                ClientId = _options.ClientId
            },
            Scopes = Scopes,
            DataStore = _localSettingsDataStoreService
        };

        _flow = new PkceGoogleAuthorizationCodeFlow(initializer);
    }

    public async Task InitializeAsync()
    {
        IsAuthenticationRequired = await LoadFromSettingsAsync();
        await Task.CompletedTask;
    }

    public async Task<UserCredential> AuthenticateAsync(CancellationToken cancellationToken)
    {
        await _loggingService.WriteLogAsync(nameof(GooglePkceService), nameof(AuthenticateAsync), "Authentication is invoked.");

        if (!IsAuthenticationRequired) return default;

        var token = await _flow.LoadTokenAsync("user", cancellationToken);

        if (token == null)
        {
            await _loggingService.WriteLogAsync(nameof(GooglePkceService), nameof(AuthenticateAsync), "The token was not saved in the DataStore.");
        }
        else
        {
            await _loggingService.WriteLogAsync(nameof(GooglePkceService), nameof(AuthenticateAsync), "A token was stored in the DataStore.");

            // TokenResponseからUserCredentialを作成します。
            var credential = new UserCredential(_flow, "user", token);

            if (!credential.Token.IsExpired(SystemClock.Default))
            {
                await _loggingService.WriteLogAsync(nameof(GooglePkceService), nameof(AuthenticateAsync), "The token has not expired.");
                return credential;
            }

            await _loggingService.WriteLogAsync(nameof(GooglePkceService), nameof(AuthenticateAsync), "The token has expired.");

            if (await credential.RefreshTokenAsync(cancellationToken))
            {
                await _loggingService.WriteLogAsync(nameof(GooglePkceService), nameof(AuthenticateAsync), "The token has been refreshed.");
                return credential;
            }
            else
            {
                await _loggingService.WriteLogAsync(nameof(GooglePkceService), nameof(AuthenticateAsync), "The token refresh failed.");
            }
        }

        await _loggingService.WriteLogAsync(nameof(GooglePkceService), nameof(AuthenticateAsync), "A new authentication process will be initiated.");
        var codeReceiver = new LocalServerCodeReceiver();
        var authCode = new AuthorizationCodeInstalledApp(_flow, codeReceiver);
        var newCredential = await authCode.AuthorizeAsync("user", cancellationToken);

        // 新たに取得したクレデンシャルを返します。
        return newCredential;
    }

    private async Task<bool> LoadFromSettingsAsync()
    {
        return await _localSettingsDataStoreService.GetAsync<bool>(AuthenticationRequiredSettingsKey);
    }

    private async Task SaveInSettingsAsync()
    {
        await _localSettingsDataStoreService.StoreAsync(AuthenticationRequiredSettingsKey, IsAuthenticationRequired);
    }

    public async Task SetAuthenticationRequiredAsync(bool needToAuthenticated, CancellationToken cancellationToken)
    {
        if (IsAuthenticationRequired == needToAuthenticated) return;
        var handler = AuthenticationRequiredChanged;

        IsAuthenticationRequired = needToAuthenticated;
        if (!IsAuthenticationRequired) await ClearAuthenticationAsync(cancellationToken);
        await SaveInSettingsAsync();

        handler?.Invoke(this, new AuthenticationRequiredChangedEventArgs(IsAuthenticationRequired));
    }

    private async Task ClearAuthenticationAsync(CancellationToken cancellationToken)
    {
        await _loggingService.WriteLogAsync(nameof(GooglePkceService), nameof(ClearAuthenticationAsync));

        // DataStoreから既存のトークンを取得しようと試みます。
        var token = await _flow.LoadTokenAsync("user", cancellationToken);

        if (token != null)
        {
            //await _localSettingsDataStoreService.ClearAsync();
            await _flow.RevokeTokenAsync("user", token.AccessToken, cancellationToken);
        }
    }

    public delegate void AuthenticationRequiredChangedEventHandler(object sender, AuthenticationRequiredChangedEventArgs e);
    public class AuthenticationRequiredChangedEventArgs : EventArgs
    {
        public bool IsAuthenticationRequired { get; private set; }

        public AuthenticationRequiredChangedEventArgs(bool isAuthenticationRequired)
        {
            IsAuthenticationRequired = isAuthenticationRequired;
        }
    }
}

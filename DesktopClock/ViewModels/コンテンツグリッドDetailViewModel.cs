using System.Windows.Input;

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

using DesktopClock.Contracts.Services;
using DesktopClock.Contracts.ViewModels;
using DesktopClock.Core.Contracts.Services;
using DesktopClock.Core.Models;

namespace DesktopClock.ViewModels;

public partial class コンテンツグリッドDetailViewModel : ObservableRecipient, INavigationAware
{
    private readonly ISampleDataService _sampleDataService;

    private readonly INavigationService _navigationService;

    [ObservableProperty]
    private SampleOrder? item;

    public ICommand GoBackCommand
    {
        get;
    }

    public コンテンツグリッドDetailViewModel(ISampleDataService sampleDataService, INavigationService navigationService)
    {
        _sampleDataService = sampleDataService;
        _navigationService = navigationService;

        GoBackCommand = new RelayCommand(OnGoBack);
    }

    public async void OnNavigatedTo(object parameter)
    {
        if (parameter is long orderID)
        {
            var data = await _sampleDataService.GetContentGridDataAsync();
            Item = data.First(i => i.OrderID == orderID);
        }
    }

    public void OnNavigatedFrom()
    {
    }

    private void OnGoBack()
    {
        if (_navigationService.CanGoBack)
        {
            _navigationService.GoBack();
        }
    }
}

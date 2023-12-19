using System.Collections.ObjectModel;

using CommunityToolkit.Mvvm.ComponentModel;

using DesktopClock.Contracts.ViewModels;
using DesktopClock.Core.Contracts.Services;
using DesktopClock.Core.Models;

namespace DesktopClock.ViewModels;

public partial class データグリッドViewModel : ObservableRecipient, INavigationAware
{
    private readonly ISampleDataService _sampleDataService;

    public ObservableCollection<SampleOrder> Source { get; } = new ObservableCollection<SampleOrder>();

    public データグリッドViewModel(ISampleDataService sampleDataService)
    {
        _sampleDataService = sampleDataService;
    }

    public async void OnNavigatedTo(object parameter)
    {
        Source.Clear();

        // TODO: Replace with real data.
        var data = await _sampleDataService.GetGridDataAsync();

        foreach (var item in data)
        {
            Source.Add(item);
        }
    }

    public void OnNavigatedFrom()
    {
    }
}

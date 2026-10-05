using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using bossfight_client.Requests;
using bossfight_client.Responses;
using bossfight_client.Services;
using StoreModel = bossfight_client.Store.Store;

namespace bossfight_client.ViewModels;

public partial class MainViewModel : ViewModelBase
{
    private readonly StoreModel _store;
    private readonly IPlayerProvider _playerProvider;

    [ObservableProperty]
    private string? _apiKeyError;

    [ObservableProperty]
    private string? _apiUrlError;

    [ObservableProperty]
    private string? _statusMessage;

    [ObservableProperty]
    private bool _isBusy;

    [ObservableProperty]
    private string _apiKey;

    [ObservableProperty]
    private Player? _selectedPlayer;

    [ObservableProperty]
    private string _apiUrl;

    public MainViewModel(StoreModel store, IPlayerProvider playerProvider)
    {
        _store = store;
        _playerProvider = playerProvider;

        _apiKey = store.ApiKey;
        _apiUrl = store.ApiUrl;

        AvailablePlayers = [];

        Validate();
    }

    public ObservableCollection<Player> AvailablePlayers { get; }

    /// <summary>
    /// The id written to the store, or 0 when nothing is selected.
    /// </summary>
    public int PlayerId => SelectedPlayer?.Id ?? 0;

    /// <summary>
    /// True once a save has succeeded, used to show the "saved" confirmation.
    /// </summary>
    public bool HasSaved { get; private set; }

    public async Task InitializeAsync()
    {
        await LoadPlayersAsync();
    }

    [RelayCommand]
    private async Task RefreshPlayersAsync()
    {
        await LoadPlayersAsync();
    }

    private async Task LoadPlayersAsync()
    {
        IsBusy = true;
        StatusMessage = "Loading players...";

        try
        {
            if (string.IsNullOrWhiteSpace(ApiKey) || string.IsNullOrWhiteSpace(ApiUrl))
            {
                AvailablePlayers.Clear();
                StatusMessage = "API key and API URL are required to load players.";
                return;
            }

            IReadOnlyList<Player> players = await _playerProvider.GetPlayersAsync(Snapshot());

            // Read the wanted id before the collection is rebuilt, otherwise the old
            // selection object is still hanging off SelectedPlayer.
            int wantedId = PlayerId != 0 ? PlayerId : _store.PlayerId;

            AvailablePlayers.Clear();

            foreach (Player player in players)
            {
                AvailablePlayers.Add(player);
            }

            SelectedPlayer = AvailablePlayers.FirstOrDefault(p => p.Id == wantedId);

            if (AvailablePlayers.Count == 0)
            {
                StatusMessage = "Failed to load players.";
            }
            else
            {
                StatusMessage = null;
            }
        }
        catch (Exception ex)
        {
            AvailablePlayers.Clear();
            StatusMessage = $"Could not load players: {ex.Message}";
        }
        finally
        {
            IsBusy = false;
        }
    }

    partial void OnApiKeyChanged(string value) => Validate();

    partial void OnApiUrlChanged(string value) => Validate();

    /// <summary>
    /// A throwaway copy of the form's current values, so validation and API calls work
    /// from what is typed in rather than from what has already been saved to disk.
    /// </summary>
    private StoreModel Snapshot() => new()
    {
        ApiKey = ApiKey.Trim(),
        PlayerId = PlayerId,
        ApiUrl = ApiUrl.Trim()
    };

    private void Validate()
    {
        IReadOnlyDictionary<string, string> errors = Snapshot().Validate();

        ApiKeyError = errors.GetValueOrDefault(nameof(StoreModel.ApiKey));
        ApiUrlError = errors.GetValueOrDefault(nameof(StoreModel.ApiUrl));

        SaveCommand.NotifyCanExecuteChanged();
    }

    [RelayCommand(CanExecute = nameof(CanSave))]
    private void Save()
    {
        try
        {
            _store.ApiKey = ApiKey.Trim();
            _store.PlayerId = PlayerId;
            _store.ApiUrl = ApiUrl.Trim();
            _store.Save();

            ApiKey = _store.ApiKey;
            ApiUrl = _store.ApiUrl;

            HasSaved = true;
            StatusMessage = "Saved.";
            OnPropertyChanged(nameof(HasSaved));
        }
        catch (Exception ex)
        {
            HasSaved = false;
            OnPropertyChanged(nameof(HasSaved));
            StatusMessage = $"Could not save: {ex.Message}";
        }
    }

    private bool CanSave() => !IsBusy && IsFormValid();

    private bool IsFormValid()
    {
        return string.IsNullOrEmpty(ApiKeyError)
            && string.IsNullOrEmpty(ApiUrlError);
    }

    partial void OnIsBusyChanged(bool value) => SaveCommand.NotifyCanExecuteChanged();
}
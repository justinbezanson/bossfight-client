using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
    private readonly IPlayerIdProvider _playerIdProvider;

    [ObservableProperty]
    private string? _apiKeyError;

    [ObservableProperty]
    private string? _playerIdError;

    [ObservableProperty]
    private string? _apiUrlError;

    [ObservableProperty]
    private string? _statusMessage;

    [ObservableProperty]
    private bool _isBusy;

    [ObservableProperty]
    private string _apiKey;

    [ObservableProperty]
    private string _playerId;

    [ObservableProperty]
    private string _apiUrl;

    public MainViewModel(StoreModel store, IPlayerIdProvider playerIdProvider)
    {
        _store = store;
        _playerIdProvider = playerIdProvider;

        _apiKey = store.ApiKey;
        _playerId = store.PlayerId;
        _apiUrl = store.ApiUrl;

        AvailablePlayerIds = [];

        Validate();
    }

    public ObservableCollection<string> AvailablePlayerIds { get; }

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
                AvailablePlayerIds.Clear();
                StatusMessage = "API key and API URL are required to load players.";
                return;
            }

            IReadOnlyList<string> ids = await _playerIdProvider.GetPlayerIdsAsync();
            AvailablePlayerIds.Clear();

            foreach (string id in ids)
            {
                AvailablePlayerIds.Add(id);
            }

            if (AvailablePlayerIds.Count == 0)
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
            AvailablePlayerIds.Clear();
            StatusMessage = $"Could not load players: {ex.Message}";
        }
        finally
        {
            IsBusy = false;
        }
    }

    partial void OnApiKeyChanged(string value) => Validate();

    partial void OnPlayerIdChanged(string value) => Validate();

    partial void OnApiUrlChanged(string value) => Validate();

    private void Validate()
    {
        // Build a throwaway snapshot of what would be written, so the form and the
        // startup path share one set of rules.
        var candidate = new StoreModel
        {
            ApiKey = ApiKey,
            PlayerId = PlayerId,
            ApiUrl = ApiUrl
        };

        IReadOnlyDictionary<string, string> errors = candidate.Validate();

        ApiKeyError = errors.GetValueOrDefault(nameof(StoreModel.ApiKey));
        PlayerIdError = errors.GetValueOrDefault(nameof(StoreModel.PlayerId));
        ApiUrlError = errors.GetValueOrDefault(nameof(StoreModel.ApiUrl));

        SaveCommand.NotifyCanExecuteChanged();
    }

    [RelayCommand(CanExecute = nameof(CanSave))]
    private void Save()
    {
        try
        {
            _store.ApiKey = ApiKey.Trim();
            _store.PlayerId = PlayerId.Trim();
            _store.ApiUrl = ApiUrl.Trim();
            _store.Save();

            ApiKey = _store.ApiKey;
            PlayerId = _store.PlayerId;
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
            && string.IsNullOrEmpty(PlayerIdError)
            && string.IsNullOrEmpty(ApiUrlError);
    }

    partial void OnIsBusyChanged(bool value) => SaveCommand.NotifyCanExecuteChanged();
}
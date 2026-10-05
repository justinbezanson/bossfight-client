using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;
using CommunityToolkit.Mvvm.ComponentModel;
using JsonSerializer = System.Text.Json.JsonSerializer;

namespace bossfight_client.Store;

public partial class Store : ObservableObject
{
    public const string FileName = "store.json";

    private static readonly JsonSerializerOptions ReadOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        ReadCommentHandling = JsonCommentHandling.Skip,
        AllowTrailingCommas = true
    };

    private static readonly JsonSerializerOptions WriteOptions = new()
    {
        WriteIndented = true
    };

    private string _apiKey = string.Empty;
    private int _playerId = 0;
    private string _apiUrl = string.Empty;
    private string? _path;

    [JsonPropertyName("api-key")]
    public string ApiKey
    {
        get => _apiKey;
        set => SetProperty(ref _apiKey, value);
    }

    [JsonPropertyName("player-id")]
    public int PlayerId
    {
        get => _playerId;
        set => SetProperty(ref _playerId, value);
    }

    [JsonPropertyName("api-url")]
    public string ApiUrl
    {
        get => _apiUrl;
        set => SetProperty(ref _apiUrl, value);
    }

    /// <summary>
    /// Absolute path this store was loaded from, or will be saved to.
    /// </summary>
    [JsonIgnore]
    public string Path => _path ?? DefaultPath;

    public static string DefaultPath => System.IO.Path.Combine(AppContext.BaseDirectory, FileName);

    public static Store Load()
    {
        return LoadFrom(DefaultPath);
    }

    public static Store LoadFrom(string path)
    {
        if (!File.Exists(path))
        {
            throw new FileNotFoundException(
                $"Could not find '{FileName}'. It must sit next to the executable. Expected at '{path}'.",
                path);
        }

        string json = File.ReadAllText(path);

        Store? store;
        try
        {
            store = JsonSerializer.Deserialize<Store>(json, ReadOptions);
        }
        catch (JsonException ex)
        {
            throw new InvalidOperationException($"'{path}' is not valid JSON: {ex.Message}", ex);
        }

        if (store is null)
        {
            throw new InvalidOperationException($"'{path}' did not contain a JSON object.");
        }

        store._path = path;
        store.ValidateOrThrow();
        return store;
    }

    /// <summary>
    /// Writes the current values back to <see cref="Path"/>. The write goes to a temp
    /// file that is then moved into place, so a crash mid-write cannot leave a
    /// truncated store.json behind.
    /// </summary>
    public void Save()
    {
        SaveTo(Path);
    }

    public void SaveTo(string path)
    {
        string json = JsonSerializer.Serialize(this, WriteOptions);

        string? directory = System.IO.Path.GetDirectoryName(path);
        if (!string.IsNullOrEmpty(directory))
        {
            Directory.CreateDirectory(directory);
        }

        string temp = path + ".tmp";
        File.WriteAllText(temp, json);
        File.Move(temp, path, overwrite: true);

        _path = path;
    }

    /// <summary>
    /// Replaces the in-memory values with those on disk, discarding unsaved edits.
    /// </summary>
    public void Reload()
    {
        Store reloaded = LoadFrom(Path);
        ApiKey = reloaded.ApiKey;
        PlayerId = reloaded.PlayerId;
        ApiUrl = reloaded.ApiUrl;
    }

    public IReadOnlyDictionary<string, string> Validate()
    {
        var errors = new Dictionary<string, string>(StringComparer.Ordinal);

        if (string.IsNullOrWhiteSpace(ApiKey))
        {
            errors[nameof(ApiKey)] = "API key is required.";
        }

        if (string.IsNullOrWhiteSpace(ApiUrl))
        {
            errors[nameof(ApiUrl)] = "API URL is required.";
        }
        else if (!Uri.TryCreate(ApiUrl, UriKind.Absolute, out var uri) ||
                 (uri.Scheme != Uri.UriSchemeHttp && uri.Scheme != Uri.UriSchemeHttps))
        {
            errors[nameof(ApiUrl)] = "API URL must be an absolute http or https URL.";
        }

        return errors;
    }

    [JsonIgnore]
    public bool IsValid => Validate().Count == 0;

    /// <summary>
    /// Startup validation: a missing or malformed store should fail loudly at launch
    /// rather than silently running with blank credentials.
    /// </summary>
    private void ValidateOrThrow()
    {
        IReadOnlyDictionary<string, string> errors = Validate();
        if (errors.Count == 0)
        {
            return;
        }

        var details = string.Join(
            Environment.NewLine,
            errors.Select(kv => $"  {kv.Key}: {kv.Value}"));

        throw new InvalidOperationException(
            $"'{Path}' is missing or has invalid values:{Environment.NewLine}{details}");
    }
}
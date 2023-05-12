using CodeOffer.Library.Exceptions;

namespace CodeOffer.Library.App;

public class AssetDirectory : List<Asset>
{
    private App InitiatedApp { get; }
    public AssetDirectory(App initiatedApp)
    {
        InitiatedApp = initiatedApp;
    }

    public AssetDirectory(IEnumerable<Asset> assetList, App initiatedApp)
    {
        AddRange(assetList);
        InitiatedApp = initiatedApp;
    }
    
    /// <summary>
    /// Returns an Asset defined by the identifier.
    /// </summary>
    /// <param name="identifier"></param>
    /// <returns>Asset</returns>
    /// <exception cref="AssetNotFoundException">Occurs when the asset was not found in the asset directory.</exception>
    public Asset GetAssetByIdentifier(string identifier)
    {
        var index = FindIndex(a => a.Identifier == identifier);
        if (index <= -1) throw new AssetNotFoundException("Asset not found in Asset directory.");
        return this[index];
    }
    
    /// <summary>
    /// Returns an Asset defined by the UUID.
    /// </summary>
    /// <param name="uuid"></param>
    /// <returns>Asset</returns>
    /// <exception cref="AssetNotFoundException">Occurs when the asset was not found in the asset directory.</exception>
    public Asset GetAssetByUuid(string uuid)
    {
        var index = FindIndex(a => a.Uuid == uuid);
        if (index <= -1) throw new AssetNotFoundException("Asset not found in Asset directory.");
        return this[index];
    }

    /// <summary>
    /// Reloads the current AssetDirectory.
    /// </summary>
    public async Task ReloadAsync()
    {
        Clear();
        var assets = await InitiatedApp.GetAssetDirectoryAsync();
        AddRange(assets);
    }
}
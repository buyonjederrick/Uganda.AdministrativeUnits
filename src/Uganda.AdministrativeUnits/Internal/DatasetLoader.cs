using System;
using System.IO;
using System.IO.Compression;
using System.Reflection;
using System.Text.Json;

namespace Uganda.AdministrativeUnits.Internal;

internal static class DatasetLoader
{
    private const string ResourceName = "Uganda.AdministrativeUnits.Data.uganda-administrative-units-2022-07.json.gz";

    public static DatasetDto LoadEmbedded()
    {
        Assembly assembly = typeof(DatasetLoader).Assembly;
        using Stream compressed = assembly.GetManifestResourceStream(ResourceName)
            ?? throw new InvalidOperationException($"Embedded resource '{ResourceName}' was not found in {assembly.GetName().Name}.");
        using var gzip = new GZipStream(compressed, CompressionMode.Decompress);

        return JsonSerializer.Deserialize(gzip, DatasetJsonContext.Default.DatasetDto)
            ?? throw new InvalidDataException("The embedded administrative-units dataset is empty.");
    }
}

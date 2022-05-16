using System.Globalization;
using Core.Domain;
using Core.Repositories;
using Microsoft.Extensions.Configuration;
using Repository.Util;

namespace Repository;

internal sealed class InFileScanRepository: IScanRepository
{
    private readonly string _file = ".plexnotifier.scan.dat"; 
    
    public InFileScanRepository(IConfiguration configuration)
    {
        if (!string.IsNullOrWhiteSpace(configuration["ScanRepositoryFile"]))
        {
            _file = configuration["ScanRepositoryFile"];
        }
    }
    
    public async Task<Scan> GetLastScan()
    {
        if (!File.Exists(_file))
        {
            await SetLastScan(new Scan(DateTime.UtcNow))
                .ConfigureAwait(false);
        }

        var result = await File
            .ReadAllTextAsync(_file)
            .ConfigureAwait(false);
        
        if(long.TryParse(result, out var ticks))
            return new Scan(ticks.UnixTimestampToDate());

        throw new ArgumentException($"Invalid file format {_file}");
    }

    public Task SetLastScan(Scan scan)
    {
        var text = scan
            .Time
            .ToUnixTimestamp()
            .ToString(CultureInfo.InvariantCulture);
            
        return File.WriteAllTextAsync( _file, text);
    }
}
using Core.Domain;

namespace Core.Repositories;

public interface IScanRepository
{
    /**
     * Retrieve information about the last media scan
     */
    Task<Scan> GetLastScan();
    
    /**
     * Updates the last scan information
     */
    Task SetLastScan(Scan scan);
}
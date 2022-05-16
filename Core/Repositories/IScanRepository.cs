using Core.Domain;

namespace Core.Repositories;

public interface IScanRepository
{
    Task<Scan> GetLastScan();
    Task SetLastScan(Scan scan);
}
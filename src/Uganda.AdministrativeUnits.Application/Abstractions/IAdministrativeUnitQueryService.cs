using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Uganda.AdministrativeUnits.Contracts.Dtos;
using Uganda.AdministrativeUnits.Contracts.Enums;
using Uganda.AdministrativeUnits.Contracts.Responses;

namespace Uganda.AdministrativeUnits.Application.Abstractions;

public interface IAdministrativeUnitQueryService
{
    Task<ApiResponse<PagedResult<DistrictDto>>> GetDistrictsAsync(
        int page,
        int pageSize,
        bool getAll = false,
        CancellationToken cancellationToken = default);

    Task<ApiResponse<DistrictDto>> GetDistrictByCodeAsync(string code, CancellationToken cancellationToken = default);

    Task<ApiResponse<IReadOnlyList<UnitSummaryDto>>> GetConstituenciesByDistrictCodeAsync(
        string districtCode,
        CancellationToken cancellationToken = default);

    Task<ApiResponse<IReadOnlyList<UnitSummaryDto>>> GetSubcountiesByConstituencyFullCodeAsync(
        string constituencyFullCode,
        CancellationToken cancellationToken = default);

    Task<ApiResponse<IReadOnlyList<UnitSummaryDto>>> GetParishesBySubcountyFullCodeAsync(
        string subcountyFullCode,
        CancellationToken cancellationToken = default);

    Task<ApiResponse<PagedResult<UnitSummaryDto>>> GetVillagesByParishFullCodeAsync(
        string parishFullCode,
        int page,
        int pageSize,
        bool getAll = false,
        CancellationToken cancellationToken = default);

    /// <summary>All districts as cascade-friendly summaries (no paging).</summary>
    Task<ApiResponse<IReadOnlyList<UnitSummaryDto>>> GetCascadeDistrictsAsync(
        CancellationToken cancellationToken = default);

    /// <summary>Immediate children of a unit by full code (cascade next step).</summary>
    Task<ApiResponse<IReadOnlyList<UnitSummaryDto>>> GetChildrenByFullCodeAsync(
        string parentFullCode,
        CancellationToken cancellationToken = default);

    Task<ApiResponse<UnitSummaryDto>> GetByFullCodeAsync(string fullCode, CancellationToken cancellationToken = default);

    Task<ApiResponse<IReadOnlyList<UnitSummaryDto>>> SearchAsync(
        string query,
        AdministrativeLevel? level,
        int maxResults,
        string? parentFullCode = null,
        CancellationToken cancellationToken = default);

    Task<ApiResponse<DatasetInfoDto>> GetDatasetInfoAsync(CancellationToken cancellationToken = default);

    Task<ApiResponse<DatasetStatisticsDto>> GetStatisticsAsync(CancellationToken cancellationToken = default);
}

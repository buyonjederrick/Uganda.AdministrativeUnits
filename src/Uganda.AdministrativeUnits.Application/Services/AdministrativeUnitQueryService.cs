using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Uganda.AdministrativeUnits.Application.Abstractions;
using Uganda.AdministrativeUnits.Application.Exceptions;
using Uganda.AdministrativeUnits.Application.Extensions;
using Uganda.AdministrativeUnits.Contracts.Dtos;
using Uganda.AdministrativeUnits.Contracts.Enums;
using Uganda.AdministrativeUnits.Contracts.Requests;
using Uganda.AdministrativeUnits.Contracts.Responses;
using Uganda.AdministrativeUnits.Domain.Entities;

namespace Uganda.AdministrativeUnits.Application.Services;

public sealed class AdministrativeUnitQueryService : IAdministrativeUnitQueryService
{
    private readonly IDbRepository _db;
    private readonly ILogger<AdministrativeUnitQueryService> _logger;

    public AdministrativeUnitQueryService(
        IDbRepository db,
        ILogger<AdministrativeUnitQueryService> logger)
    {
        _db = db ?? throw new ArgumentNullException(nameof(db));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public Task<ApiResponse<PagedResult<DistrictDto>>> GetDistrictsAsync(
        int page,
        int pageSize,
        bool getAll = false,
        CancellationToken cancellationToken = default) =>
        ExceptionHandlingExtensions.ExecuteWithExceptionHandlingAsync(
            () => GetDistrictsCoreAsync(page, pageSize, getAll, cancellationToken),
            _logger,
            "Failed to list districts.");

    public Task<ApiResponse<DistrictDto>> GetDistrictByCodeAsync(
        string code,
        CancellationToken cancellationToken = default) =>
        ExceptionHandlingExtensions.ExecuteWithExceptionHandlingAsync(
            () => GetDistrictByCodeCoreAsync(code, cancellationToken),
            _logger,
            $"Failed to get district '{code}'.");

    public Task<ApiResponse<IReadOnlyList<UnitSummaryDto>>> GetConstituenciesByDistrictCodeAsync(
        string districtCode,
        CancellationToken cancellationToken = default) =>
        ExceptionHandlingExtensions.ExecuteWithExceptionHandlingAsync(
            () => GetConstituenciesCoreAsync(districtCode, cancellationToken),
            _logger,
            $"Failed to list constituencies for district '{districtCode}'.");

    public Task<ApiResponse<IReadOnlyList<UnitSummaryDto>>> GetSubcountiesByConstituencyFullCodeAsync(
        string constituencyFullCode,
        CancellationToken cancellationToken = default) =>
        ExceptionHandlingExtensions.ExecuteWithExceptionHandlingAsync(
            () => GetSubcountiesCoreAsync(constituencyFullCode, cancellationToken),
            _logger,
            $"Failed to list subcounties for constituency '{constituencyFullCode}'.");

    public Task<ApiResponse<IReadOnlyList<UnitSummaryDto>>> GetParishesBySubcountyFullCodeAsync(
        string subcountyFullCode,
        CancellationToken cancellationToken = default) =>
        ExceptionHandlingExtensions.ExecuteWithExceptionHandlingAsync(
            () => GetParishesCoreAsync(subcountyFullCode, cancellationToken),
            _logger,
            $"Failed to list parishes for subcounty '{subcountyFullCode}'.");

    public Task<ApiResponse<PagedResult<UnitSummaryDto>>> GetVillagesByParishFullCodeAsync(
        string parishFullCode,
        int page,
        int pageSize,
        bool getAll = false,
        CancellationToken cancellationToken = default) =>
        ExceptionHandlingExtensions.ExecuteWithExceptionHandlingAsync(
            () => GetVillagesCoreAsync(parishFullCode, page, pageSize, getAll, cancellationToken),
            _logger,
            $"Failed to list villages for parish '{parishFullCode}'.");

    public Task<ApiResponse<IReadOnlyList<UnitSummaryDto>>> GetCascadeDistrictsAsync(
        CancellationToken cancellationToken = default) =>
        ExceptionHandlingExtensions.ExecuteWithExceptionHandlingAsync(
            () => GetCascadeDistrictsCoreAsync(cancellationToken),
            _logger,
            "Failed to list cascade districts.");

    public Task<ApiResponse<IReadOnlyList<UnitSummaryDto>>> GetChildrenByFullCodeAsync(
        string parentFullCode,
        CancellationToken cancellationToken = default) =>
        ExceptionHandlingExtensions.ExecuteWithExceptionHandlingAsync(
            () => GetChildrenCoreAsync(parentFullCode, cancellationToken),
            _logger,
            $"Failed to list children for '{parentFullCode}'.");

    public Task<ApiResponse<UnitSummaryDto>> GetByFullCodeAsync(
        string fullCode,
        CancellationToken cancellationToken = default) =>
        ExceptionHandlingExtensions.ExecuteWithExceptionHandlingAsync(
            () => GetByFullCodeCoreAsync(fullCode, cancellationToken),
            _logger,
            $"Failed to resolve unit '{fullCode}'.");

    public Task<ApiResponse<IReadOnlyList<UnitSummaryDto>>> SearchAsync(
        string query,
        AdministrativeLevel? level,
        int maxResults,
        string? parentFullCode = null,
        CancellationToken cancellationToken = default) =>
        ExceptionHandlingExtensions.ExecuteWithExceptionHandlingAsync(
            () => SearchCoreAsync(query, level, maxResults, parentFullCode, cancellationToken),
            _logger,
            $"Failed to search units for query '{query}'.");

    public Task<ApiResponse<DatasetInfoDto>> GetDatasetInfoAsync(CancellationToken cancellationToken = default) =>
        ExceptionHandlingExtensions.ExecuteWithExceptionHandlingAsync(
            () => GetDatasetInfoCoreAsync(cancellationToken),
            _logger,
            "Failed to load dataset metadata.");

    public Task<ApiResponse<DatasetStatisticsDto>> GetStatisticsAsync(CancellationToken cancellationToken = default) =>
        ExceptionHandlingExtensions.ExecuteWithExceptionHandlingAsync(
            () => GetStatisticsCoreAsync(cancellationToken),
            _logger,
            "Failed to load dataset statistics.");

    private async Task<PagedResult<DistrictDto>> GetDistrictsCoreAsync(
        int page,
        int pageSize,
        bool getAll,
        CancellationToken cancellationToken)
    {
        PagingRequest paging = NormalizePaging(page, pageSize, getAll);

        IQueryable<District> query = _db.GetDbSet<District>()
            .AsNoTracking()
            .OrderBy(x => x.FullCode.Length)
            .ThenBy(x => x.FullCode);

        IQueryable<DistrictDto> projected = query.Select(x => new DistrictDto
        {
            Code = x.FullCode,
            Name = x.Name,
            ConstituencyCount = x.Constituencies.Count,
        });

        if (paging.GetAll)
        {
            List<DistrictDto> allItems = await projected.ToListAsync(cancellationToken).ConfigureAwait(false);
            return CreateGetAllPage(allItems);
        }

        int totalCount = await query.CountAsync(cancellationToken).ConfigureAwait(false);
        List<DistrictDto> items = await projected
            .Skip((paging.Page - 1) * paging.PageSize)
            .Take(paging.PageSize)
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

        return new PagedResult<DistrictDto>
        {
            Items = items,
            Page = paging.Page,
            PageSize = paging.PageSize,
            TotalCount = totalCount,
        };
    }

    private async Task<DistrictDto> GetDistrictByCodeCoreAsync(string code, CancellationToken cancellationToken)
    {
        string normalizedCode = RequireNonEmpty(code, nameof(code));

        DistrictDto? district = await _db.GetDbSet<District>()
            .AsNoTracking()
            .Where(x => x.Code == normalizedCode)
            .Select(x => new DistrictDto
            {
                Code = x.FullCode,
                Name = x.Name,
                ConstituencyCount = x.Constituencies.Count,
            })
            .FirstOrDefaultAsync(cancellationToken)
            .ConfigureAwait(false);

        if (district is null)
        {
            throw new NotFoundException($"District '{normalizedCode}' was not found.");
        }

        return district;
    }

    private async Task<IReadOnlyList<UnitSummaryDto>> GetConstituenciesCoreAsync(
        string districtCode,
        CancellationToken cancellationToken)
    {
        string normalizedCode = RequireNonEmpty(districtCode, nameof(districtCode));
        await EnsureDistrictExistsAsync(normalizedCode, cancellationToken).ConfigureAwait(false);
        return await ListConstituenciesAsync(normalizedCode, cancellationToken).ConfigureAwait(false);
    }

    private async Task<IReadOnlyList<UnitSummaryDto>> GetSubcountiesCoreAsync(
        string constituencyFullCode,
        CancellationToken cancellationToken)
    {
        string normalizedFullCode = RequireNonEmpty(constituencyFullCode, nameof(constituencyFullCode));
        await EnsureConstituencyExistsAsync(normalizedFullCode, cancellationToken).ConfigureAwait(false);
        return await ListSubcountiesAsync(normalizedFullCode, cancellationToken).ConfigureAwait(false);
    }

    private async Task<IReadOnlyList<UnitSummaryDto>> GetParishesCoreAsync(
        string subcountyFullCode,
        CancellationToken cancellationToken)
    {
        string normalizedFullCode = RequireNonEmpty(subcountyFullCode, nameof(subcountyFullCode));
        await EnsureSubcountyExistsAsync(normalizedFullCode, cancellationToken).ConfigureAwait(false);
        return await ListParishesAsync(normalizedFullCode, cancellationToken).ConfigureAwait(false);
    }

    private async Task<PagedResult<UnitSummaryDto>> GetVillagesCoreAsync(
        string parishFullCode,
        int page,
        int pageSize,
        bool getAll,
        CancellationToken cancellationToken)
    {
        string normalizedFullCode = RequireNonEmpty(parishFullCode, nameof(parishFullCode));
        PagingRequest paging = NormalizePaging(page, pageSize, getAll);
        await EnsureParishExistsAsync(normalizedFullCode, cancellationToken).ConfigureAwait(false);

        IQueryable<UnitSummaryDto> projected = ProjectVillages(normalizedFullCode);

        if (paging.GetAll)
        {
            List<UnitSummaryDto> allItems = await projected.ToListAsync(cancellationToken).ConfigureAwait(false);
            return CreateGetAllPage(allItems);
        }

        int totalCount = await _db.GetDbSet<Village>()
            .AsNoTracking()
            .CountAsync(x => x.ParishFullCode == normalizedFullCode, cancellationToken)
            .ConfigureAwait(false);

        List<UnitSummaryDto> items = await projected
            .Skip((paging.Page - 1) * paging.PageSize)
            .Take(paging.PageSize)
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

        return new PagedResult<UnitSummaryDto>
        {
            Items = items,
            Page = paging.Page,
            PageSize = paging.PageSize,
            TotalCount = totalCount,
        };
    }

    private async Task<IReadOnlyList<UnitSummaryDto>> GetCascadeDistrictsCoreAsync(CancellationToken cancellationToken) =>
        await _db.GetDbSet<District>()
            .AsNoTracking()
            .OrderBy(x => x.FullCode.Length)
            .ThenBy(x => x.FullCode)
            .Select(x => new UnitSummaryDto
            {
                Code = x.FullCode,
                Name = x.Name,
                Level = AdministrativeLevel.District,
                ParentCode = null,
                Breadcrumb = x.Breadcrumb,
            })
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

    private async Task<IReadOnlyList<UnitSummaryDto>> GetChildrenCoreAsync(
        string parentFullCode,
        CancellationToken cancellationToken)
    {
        string normalizedFullCode = RequireNonEmpty(parentFullCode, nameof(parentFullCode));
        int segmentCount = CountSegments(normalizedFullCode);

        return segmentCount switch
        {
            1 => await GetDistrictChildrenAsync(normalizedFullCode, cancellationToken).ConfigureAwait(false),
            2 => await GetConstituencyChildrenAsync(normalizedFullCode, cancellationToken).ConfigureAwait(false),
            3 => await GetSubcountyChildrenAsync(normalizedFullCode, cancellationToken).ConfigureAwait(false),
            4 => await GetParishChildrenAsync(normalizedFullCode, cancellationToken).ConfigureAwait(false),
            5 => await GetVillageLeafAsync(normalizedFullCode, cancellationToken).ConfigureAwait(false),
            _ => throw new BadRequestException(
                $"Full code '{normalizedFullCode}' is invalid. Expected 1â€“5 hyphen-separated segments."),
        };
    }

    private async Task<IReadOnlyList<UnitSummaryDto>> GetDistrictChildrenAsync(
        string districtCode,
        CancellationToken cancellationToken)
    {
        await EnsureDistrictExistsAsync(districtCode, cancellationToken).ConfigureAwait(false);
        return await ListConstituenciesAsync(districtCode, cancellationToken).ConfigureAwait(false);
    }

    private async Task<IReadOnlyList<UnitSummaryDto>> GetConstituencyChildrenAsync(
        string constituencyFullCode,
        CancellationToken cancellationToken)
    {
        await EnsureConstituencyExistsAsync(constituencyFullCode, cancellationToken).ConfigureAwait(false);
        return await ListSubcountiesAsync(constituencyFullCode, cancellationToken).ConfigureAwait(false);
    }

    private async Task<IReadOnlyList<UnitSummaryDto>> GetSubcountyChildrenAsync(
        string subcountyFullCode,
        CancellationToken cancellationToken)
    {
        await EnsureSubcountyExistsAsync(subcountyFullCode, cancellationToken).ConfigureAwait(false);
        return await ListParishesAsync(subcountyFullCode, cancellationToken).ConfigureAwait(false);
    }

    private async Task<IReadOnlyList<UnitSummaryDto>> GetParishChildrenAsync(
        string parishFullCode,
        CancellationToken cancellationToken)
    {
        await EnsureParishExistsAsync(parishFullCode, cancellationToken).ConfigureAwait(false);
        return await ProjectVillages(parishFullCode).ToListAsync(cancellationToken).ConfigureAwait(false);
    }

    private async Task<IReadOnlyList<UnitSummaryDto>> GetVillageLeafAsync(
        string villageFullCode,
        CancellationToken cancellationToken)
    {
        await EnsureVillageExistsAsync(villageFullCode, cancellationToken).ConfigureAwait(false);
        return [];
    }

    private async Task<UnitSummaryDto> GetByFullCodeCoreAsync(string fullCode, CancellationToken cancellationToken)
    {
        string normalizedFullCode = RequireNonEmpty(fullCode, nameof(fullCode));
        int segmentCount = CountSegments(normalizedFullCode);

        UnitSummaryDto? unit = segmentCount switch
        {
            1 => await _db.GetDbSet<District>()
                .AsNoTracking()
                .Where(x => x.FullCode == normalizedFullCode)
                .Select(x => new UnitSummaryDto
                {
                    Code = x.FullCode,
                    Name = x.Name,
                    Level = AdministrativeLevel.District,
                    ParentCode = null,
                    Breadcrumb = x.Breadcrumb,
                })
                .FirstOrDefaultAsync(cancellationToken)
                .ConfigureAwait(false),
            2 => await _db.GetDbSet<Constituency>()
                .AsNoTracking()
                .Where(x => x.FullCode == normalizedFullCode)
                .Select(x => new UnitSummaryDto
                {
                    Code = x.FullCode,
                    Name = x.Name,
                    Level = AdministrativeLevel.Constituency,
                    ParentCode = x.DistrictCode,
                    Breadcrumb = x.Breadcrumb,
                })
                .FirstOrDefaultAsync(cancellationToken)
                .ConfigureAwait(false),
            3 => await _db.GetDbSet<Subcounty>()
                .AsNoTracking()
                .Where(x => x.FullCode == normalizedFullCode)
                .Select(x => new UnitSummaryDto
                {
                    Code = x.FullCode,
                    Name = x.Name,
                    Level = AdministrativeLevel.Subcounty,
                    ParentCode = x.ConstituencyFullCode,
                    Breadcrumb = x.Breadcrumb,
                })
                .FirstOrDefaultAsync(cancellationToken)
                .ConfigureAwait(false),
            4 => await _db.GetDbSet<Parish>()
                .AsNoTracking()
                .Where(x => x.FullCode == normalizedFullCode)
                .Select(x => new UnitSummaryDto
                {
                    Code = x.FullCode,
                    Name = x.Name,
                    Level = AdministrativeLevel.Parish,
                    ParentCode = x.SubcountyFullCode,
                    Breadcrumb = x.Breadcrumb,
                })
                .FirstOrDefaultAsync(cancellationToken)
                .ConfigureAwait(false),
            5 => await _db.GetDbSet<Village>()
                .AsNoTracking()
                .Where(x => x.FullCode == normalizedFullCode)
                .Select(x => new UnitSummaryDto
                {
                    Code = x.FullCode,
                    Name = x.Name,
                    Level = AdministrativeLevel.Village,
                    ParentCode = x.ParishFullCode,
                    Breadcrumb = x.Breadcrumb,
                })
                .FirstOrDefaultAsync(cancellationToken)
                .ConfigureAwait(false),
            _ => null,
        };

        if (unit is null)
        {
            throw new NotFoundException($"Administrative unit '{normalizedFullCode}' was not found.");
        }

        return unit;
    }

    private async Task<IReadOnlyList<UnitSummaryDto>> SearchCoreAsync(
        string query,
        AdministrativeLevel? level,
        int maxResults,
        string? parentFullCode,
        CancellationToken cancellationToken)
    {
        SearchRequest request = new()
        {
            Query = query,
            Level = level,
            MaxResults = maxResults,
            ParentCode = parentFullCode,
        };
        request.Normalize();

        if (string.IsNullOrWhiteSpace(request.Query))
        {
            throw new BadRequestException("Search query is required.");
        }

        int? minimumChildLevel = null;
        string? descendantPrefix = null;
        if (request.ParentCode is not null)
        {
            int parentSegments = CountSegments(request.ParentCode);
            if (parentSegments is < 1 or > 4)
            {
                throw new BadRequestException(
                    "parentCode must identify District, Constituency, Subcounty, or Parish (1-4 segments).");
            }

            minimumChildLevel = parentSegments + 1;
            descendantPrefix = request.ParentCode + "-";
        }

        string pattern = $"%{EscapeLikePattern(request.Query)}%";
        List<UnitSummaryDto> results = new(request.MaxResults);

        if (ShouldSearchLevel(AdministrativeLevel.District, level, minimumChildLevel))
        {
            await AppendSearchAsync(
                results,
                _db.GetDbSet<District>().AsNoTracking()
                    .Where(x => EF.Functions.Like(x.Name, pattern))
                    .OrderBy(x => x.FullCode.Length)
            .ThenBy(x => x.FullCode)
                    .Select(x => new UnitSummaryDto
                    {
                        Code = x.FullCode,
                        Name = x.Name,
                        Level = AdministrativeLevel.District,
                        ParentCode = null,
                        Breadcrumb = x.Breadcrumb,
                    }),
                request.MaxResults,
                cancellationToken).ConfigureAwait(false);
        }

        if (results.Count < request.MaxResults &&
            ShouldSearchLevel(AdministrativeLevel.Constituency, level, minimumChildLevel))
        {
            IQueryable<Constituency> constituencies = _db.GetDbSet<Constituency>().AsNoTracking()
                .Where(x => EF.Functions.Like(x.Name, pattern));
            if (descendantPrefix is not null)
            {
                constituencies = constituencies.Where(x => x.FullCode.StartsWith(descendantPrefix));
            }

            await AppendSearchAsync(
                results,
                constituencies.OrderBy(x => x.FullCode.Length).ThenBy(x => x.FullCode)
                    .Select(x => new UnitSummaryDto
                    {
                        Code = x.FullCode,
                        Name = x.Name,
                        Level = AdministrativeLevel.Constituency,
                        ParentCode = x.DistrictCode,
                        Breadcrumb = x.Breadcrumb,
                    }),
                request.MaxResults,
                cancellationToken).ConfigureAwait(false);
        }

        if (results.Count < request.MaxResults &&
            ShouldSearchLevel(AdministrativeLevel.Subcounty, level, minimumChildLevel))
        {
            IQueryable<Subcounty> subcounties = _db.GetDbSet<Subcounty>().AsNoTracking()
                .Where(x => EF.Functions.Like(x.Name, pattern));
            if (descendantPrefix is not null)
            {
                subcounties = subcounties.Where(x => x.FullCode.StartsWith(descendantPrefix));
            }

            await AppendSearchAsync(
                results,
                subcounties.OrderBy(x => x.FullCode.Length).ThenBy(x => x.FullCode)
                    .Select(x => new UnitSummaryDto
                    {
                        Code = x.FullCode,
                        Name = x.Name,
                        Level = AdministrativeLevel.Subcounty,
                        ParentCode = x.ConstituencyFullCode,
                        Breadcrumb = x.Breadcrumb,
                    }),
                request.MaxResults,
                cancellationToken).ConfigureAwait(false);
        }

        if (results.Count < request.MaxResults &&
            ShouldSearchLevel(AdministrativeLevel.Parish, level, minimumChildLevel))
        {
            IQueryable<Parish> parishes = _db.GetDbSet<Parish>().AsNoTracking()
                .Where(x => EF.Functions.Like(x.Name, pattern));
            if (descendantPrefix is not null)
            {
                parishes = parishes.Where(x => x.FullCode.StartsWith(descendantPrefix));
            }

            await AppendSearchAsync(
                results,
                parishes.OrderBy(x => x.FullCode.Length).ThenBy(x => x.FullCode)
                    .Select(x => new UnitSummaryDto
                    {
                        Code = x.FullCode,
                        Name = x.Name,
                        Level = AdministrativeLevel.Parish,
                        ParentCode = x.SubcountyFullCode,
                        Breadcrumb = x.Breadcrumb,
                    }),
                request.MaxResults,
                cancellationToken).ConfigureAwait(false);
        }

        if (results.Count < request.MaxResults &&
            ShouldSearchLevel(AdministrativeLevel.Village, level, minimumChildLevel))
        {
            IQueryable<Village> villages = _db.GetDbSet<Village>().AsNoTracking()
                .Where(x => EF.Functions.Like(x.Name, pattern));
            if (descendantPrefix is not null)
            {
                villages = villages.Where(x => x.FullCode.StartsWith(descendantPrefix));
            }

            await AppendSearchAsync(
                results,
                villages.OrderBy(x => x.FullCode.Length).ThenBy(x => x.FullCode)
                    .Select(x => new UnitSummaryDto
                    {
                        Code = x.FullCode,
                        Name = x.Name,
                        Level = AdministrativeLevel.Village,
                        ParentCode = x.ParishFullCode,
                        Breadcrumb = x.Breadcrumb,
                    }),
                request.MaxResults,
                cancellationToken).ConfigureAwait(false);
        }

        return results
            .OrderBy(x => x.Code.Length)
            .ThenBy(x => x.Code, StringComparer.Ordinal)
            .ToList();
    }

    private async Task<DatasetInfoDto> GetDatasetInfoCoreAsync(CancellationToken cancellationToken)
    {
        DatasetInfoDto? info = await _db.GetDbSet<DatasetMeta>()
            .AsNoTracking()
            .Where(x => x.Id == 1)
            .Select(x => new DatasetInfoDto
            {
                Title = x.Title,
                Edition = x.Edition,
                PublishedOn = x.PublishedOn,
                SourceNote = x.SourceNote,
                CorrectionsApplied = x.CorrectionsApplied,
            })
            .FirstOrDefaultAsync(cancellationToken)
            .ConfigureAwait(false);

        if (info is null)
        {
            throw new NotFoundException("Dataset metadata has not been seeded.");
        }

        return info;
    }

    private async Task<DatasetStatisticsDto> GetStatisticsCoreAsync(CancellationToken cancellationToken)
    {
        DbContext dbContext = _db.GetDbContext();
        string? providerName = dbContext.Database.ProviderName;

        if (providerName is not null &&
            providerName.Contains("SqlServer", StringComparison.OrdinalIgnoreCase))
        {
            return await dbContext.Database
                .SqlQueryRaw<DatasetStatisticsDto>(
                    """
                    SELECT
                        CAST((SELECT COUNT_BIG(*) FROM Districts) AS INT) AS Districts,
                        CAST((SELECT COUNT_BIG(*) FROM Constituencies) AS INT) AS Constituencies,
                        CAST((SELECT COUNT_BIG(*) FROM Subcounties) AS INT) AS Subcounties,
                        CAST((SELECT COUNT_BIG(*) FROM Parishes) AS INT) AS Parishes,
                        CAST((SELECT COUNT_BIG(*) FROM Villages) AS INT) AS Villages
                    """)
                .AsNoTracking()
                .FirstAsync(cancellationToken)
                .ConfigureAwait(false);
        }

        return new DatasetStatisticsDto
        {
            Districts = await _db.GetDbSet<District>().AsNoTracking().CountAsync(cancellationToken).ConfigureAwait(false),
            Constituencies = await _db.GetDbSet<Constituency>().AsNoTracking().CountAsync(cancellationToken).ConfigureAwait(false),
            Subcounties = await _db.GetDbSet<Subcounty>().AsNoTracking().CountAsync(cancellationToken).ConfigureAwait(false),
            Parishes = await _db.GetDbSet<Parish>().AsNoTracking().CountAsync(cancellationToken).ConfigureAwait(false),
            Villages = await _db.GetDbSet<Village>().AsNoTracking().CountAsync(cancellationToken).ConfigureAwait(false),
        };
    }

    private async Task<IReadOnlyList<UnitSummaryDto>> ListConstituenciesAsync(
        string districtCode,
        CancellationToken cancellationToken) =>
        await _db.GetDbSet<Constituency>()
            .AsNoTracking()
            .Where(x => x.DistrictCode == districtCode)
            .OrderBy(x => x.FullCode.Length)
            .ThenBy(x => x.FullCode)
            .Select(x => new UnitSummaryDto
            {
                Code = x.FullCode,
                Name = x.Name,
                Level = AdministrativeLevel.Constituency,
                ParentCode = x.DistrictCode,
                Breadcrumb = x.Breadcrumb,
            })
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

    private async Task<IReadOnlyList<UnitSummaryDto>> ListSubcountiesAsync(
        string constituencyFullCode,
        CancellationToken cancellationToken) =>
        await _db.GetDbSet<Subcounty>()
            .AsNoTracking()
            .Where(x => x.ConstituencyFullCode == constituencyFullCode)
            .OrderBy(x => x.FullCode.Length)
            .ThenBy(x => x.FullCode)
            .Select(x => new UnitSummaryDto
            {
                Code = x.FullCode,
                Name = x.Name,
                Level = AdministrativeLevel.Subcounty,
                ParentCode = x.ConstituencyFullCode,
                Breadcrumb = x.Breadcrumb,
            })
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

    private async Task<IReadOnlyList<UnitSummaryDto>> ListParishesAsync(
        string subcountyFullCode,
        CancellationToken cancellationToken) =>
        await _db.GetDbSet<Parish>()
            .AsNoTracking()
            .Where(x => x.SubcountyFullCode == subcountyFullCode)
            .OrderBy(x => x.FullCode.Length)
            .ThenBy(x => x.FullCode)
            .Select(x => new UnitSummaryDto
            {
                Code = x.FullCode,
                Name = x.Name,
                Level = AdministrativeLevel.Parish,
                ParentCode = x.SubcountyFullCode,
                Breadcrumb = x.Breadcrumb,
            })
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

    private IQueryable<UnitSummaryDto> ProjectVillages(string parishFullCode) =>
        _db.GetDbSet<Village>()
            .AsNoTracking()
            .Where(x => x.ParishFullCode == parishFullCode)
            .OrderBy(x => x.FullCode.Length)
            .ThenBy(x => x.FullCode)
            .Select(x => new UnitSummaryDto
            {
                Code = x.FullCode,
                Name = x.Name,
                Level = AdministrativeLevel.Village,
                ParentCode = x.ParishFullCode,
                Breadcrumb = x.Breadcrumb,
            });

    private async Task EnsureDistrictExistsAsync(string districtCode, CancellationToken cancellationToken)
    {
        bool exists = await _db.GetDbSet<District>()
            .AsNoTracking()
            .AnyAsync(x => x.Code == districtCode, cancellationToken)
            .ConfigureAwait(false);

        if (!exists)
        {
            throw new NotFoundException($"District '{districtCode}' was not found.");
        }
    }

    private async Task EnsureConstituencyExistsAsync(string fullCode, CancellationToken cancellationToken)
    {
        bool exists = await _db.GetDbSet<Constituency>()
            .AsNoTracking()
            .AnyAsync(x => x.FullCode == fullCode, cancellationToken)
            .ConfigureAwait(false);

        if (!exists)
        {
            throw new NotFoundException($"Constituency '{fullCode}' was not found.");
        }
    }

    private async Task EnsureSubcountyExistsAsync(string fullCode, CancellationToken cancellationToken)
    {
        bool exists = await _db.GetDbSet<Subcounty>()
            .AsNoTracking()
            .AnyAsync(x => x.FullCode == fullCode, cancellationToken)
            .ConfigureAwait(false);

        if (!exists)
        {
            throw new NotFoundException($"Subcounty '{fullCode}' was not found.");
        }
    }

    private async Task EnsureParishExistsAsync(string fullCode, CancellationToken cancellationToken)
    {
        bool exists = await _db.GetDbSet<Parish>()
            .AsNoTracking()
            .AnyAsync(x => x.FullCode == fullCode, cancellationToken)
            .ConfigureAwait(false);

        if (!exists)
        {
            throw new NotFoundException($"Parish '{fullCode}' was not found.");
        }
    }

    private async Task EnsureVillageExistsAsync(string fullCode, CancellationToken cancellationToken)
    {
        bool exists = await _db.GetDbSet<Village>()
            .AsNoTracking()
            .AnyAsync(x => x.FullCode == fullCode, cancellationToken)
            .ConfigureAwait(false);

        if (!exists)
        {
            throw new NotFoundException($"Village '{fullCode}' was not found.");
        }
    }

    private static async Task AppendSearchAsync(
        List<UnitSummaryDto> results,
        IQueryable<UnitSummaryDto> query,
        int maxResults,
        CancellationToken cancellationToken)
    {
        int remaining = maxResults - results.Count;
        if (remaining <= 0)
        {
            return;
        }

        List<UnitSummaryDto> page = await query
            .Take(remaining)
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

        results.AddRange(page);
    }

    private static bool ShouldSearchLevel(
        AdministrativeLevel candidate,
        AdministrativeLevel? requestedLevel,
        int? minimumChildLevel)
    {
        if (requestedLevel is not null && requestedLevel != candidate)
        {
            return false;
        }

        if (minimumChildLevel is not null && (int)candidate < minimumChildLevel.Value)
        {
            return false;
        }

        return true;
    }

    private static PagedResult<T> CreateGetAllPage<T>(IReadOnlyList<T> items) =>
        new()
        {
            Items = items,
            Page = 1,
            PageSize = items.Count,
            TotalCount = items.Count,
        };

    private static PagingRequest NormalizePaging(int page, int pageSize, bool getAll)
    {
        PagingRequest paging = new()
        {
            Page = page,
            PageSize = pageSize,
            GetAll = getAll,
        };
        paging.Normalize();
        return paging;
    }

    private static int CountSegments(string fullCode) => fullCode.Count(c => c == '-') + 1;

    private static string RequireNonEmpty(string value, string paramName)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new BadRequestException($"{paramName} is required.");
        }

        return value.Trim();
    }

    private static string EscapeLikePattern(string value) =>
        value
            .Replace("[", "[[]", StringComparison.Ordinal)
            .Replace("%", "[%]", StringComparison.Ordinal)
            .Replace("_", "[_]", StringComparison.Ordinal);
}

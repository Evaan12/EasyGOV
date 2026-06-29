using Application.Common.Pagination;
using Application.Interfaces;
using Domain.Entities;
using Domain.Repositories;
using Mediator;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Application.Features.SifarisApplications.Queries
{
    public record GetMySifarisApplicationsQuery(Guid CitizenId) : IRequest<IEnumerable<SifarisApplication>>;

    public class GetMySifarisApplicationsQueryHandler : IRequestHandler<GetMySifarisApplicationsQuery, IEnumerable<SifarisApplication>>
    {
        private readonly ISifarisApplicationRepository _repository;
        public GetMySifarisApplicationsQueryHandler(ISifarisApplicationRepository repository) => _repository = repository;
        
        public async Task<IEnumerable<SifarisApplication>> Handle(GetMySifarisApplicationsQuery request, CancellationToken ct) =>
            await _repository.GetByCitizenIdAsync(request.CitizenId, ct);
    }

    public record GetPendingSifarisApplicationsQuery(Guid WardId) : IRequest<IEnumerable<SifarisApplication>>;

    public class GetPendingSifarisApplicationsQueryHandler : IRequestHandler<GetPendingSifarisApplicationsQuery, IEnumerable<SifarisApplication>>
    {
        private readonly ISifarisApplicationRepository _repository;
        public GetPendingSifarisApplicationsQueryHandler(ISifarisApplicationRepository repository) => _repository = repository;
        
        public async Task<IEnumerable<SifarisApplication>> Handle(GetPendingSifarisApplicationsQuery request, CancellationToken ct) =>
            await _repository.GetPendingByWardIdAsync(request.WardId, ct);
    }

    public record GetSifarisApplicationByIdQuery(Guid Id) : IRequest<SifarisApplication?>;

    public class GetSifarisApplicationByIdQueryHandler : IRequestHandler<GetSifarisApplicationByIdQuery, SifarisApplication?>
    {
        private readonly ISifarisApplicationRepository _repository;
        public GetSifarisApplicationByIdQueryHandler(ISifarisApplicationRepository repository) => _repository = repository;
        
        public async Task<SifarisApplication?> Handle(GetSifarisApplicationByIdQuery request, CancellationToken ct) =>
            await _repository.GetByIdAsync(request.Id, ct);
    }

    public record GetPaginatedWardSifarisApplicationsQuery(Guid WardId, PaginationParameters Pagination) : IRequest<PagedResult<SifarisApplication>>;

    public class GetPaginatedWardSifarisApplicationsQueryHandler : IRequestHandler<GetPaginatedWardSifarisApplicationsQuery, PagedResult<SifarisApplication>>
    {
        private readonly ISifarisApplicationRepository _repository;
        public GetPaginatedWardSifarisApplicationsQueryHandler(ISifarisApplicationRepository repository) => _repository = repository;
        
        public async Task<PagedResult<SifarisApplication>> Handle(GetPaginatedWardSifarisApplicationsQuery request, CancellationToken ct)
        {
            var (items, count) = await _repository.GetPaginatedByWardIdAsync(request.WardId, request.Pagination.Skip, request.Pagination.PageSize, request.Pagination.SearchTerm, ct);
            return new PagedResult<SifarisApplication>(items, count, request.Pagination.PageNumber, request.Pagination.PageSize);
        }
    }
}
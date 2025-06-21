using ZODs.Common.Exceptions;
using ZODs.Api.Repository;
using ZODs.Api.Repository.Dtos;
using ZODs.Api.Repository.Entities;
using ZODs.Api.Repository.Interfaces;
using ZODs.Api.Repository.QueryParams;
using ZODs.Api.Service.Dtos;
using ZODs.Api.Service.InputDtos;
using ZODs.Api.Service.Mappers;
using ZODs.Api.Service.Validation.Interfaces;
using ZODs.Common.Extensions;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;
using ZODs.Api.Common.Extensions;

namespace ZODs.Api.Service.Services
{
    public sealed class SnippetTriggerPrefixService : ISnippetTriggerPrefixService
    {
        private readonly IUnitOfWork<ZodsContext> unitOfWork;
        private readonly IWorkspaceValidationService workspaceValidationService;

        public SnippetTriggerPrefixService(
            IUnitOfWork<ZodsContext> unitOfWork,
            IWorkspaceValidationService workspaceValidationService)
        {
            this.unitOfWork = unitOfWork;
            this.workspaceValidationService = workspaceValidationService;
        }

        private ISnippetTriggerPrefixesRepository SnippetTriggerPrefixesRepository => unitOfWork.GetRepository<ISnippetTriggerPrefixesRepository>();
        private ISnippetsRepository SnippetsRepository => unitOfWork.GetRepository<ISnippetsRepository>();

        public async Task<IEnumerable<SnippetTriggerPrefixDto>> GetWorkspaceSnippetTriggerPrefixesAsync(
             Guid workspaceId,
             Guid executingUserId,
             CancellationToken cancellationToken = default)
        {
            await workspaceValidationService.ValidateUserIsWorkspaceOwner(workspaceId, executingUserId, cancellationToken);

            var prefixes = await GetUserSnippetTriggerPrefixesAsync(
                               triggerPrefix => triggerPrefix.WorkspaceId == workspaceId,
                               cancellationToken);

            return prefixes;
        }

        public async Task<IEnumerable<SnippetTriggerPrefixDto>> GetUserSnippetTriggerPrefixesAsync(
             Guid userId,
             CancellationToken cancellationToken = default)
        {
            var prefixes = await GetUserSnippetTriggerPrefixesAsync(
                               triggerPrefix => triggerPrefix.UserId == userId,
                               cancellationToken);

            return prefixes;
        }

        public async Task<PagedResponse<SnippetTriggerPrefixDto>> GetPagedUserSnippetPrefixesAsync(
            GetUserSnippetTriggerPrefixesQuery query,
            Guid userId,
            CancellationToken cancellationToken)
        {
            var pagedEntities = await SnippetTriggerPrefixesRepository.GetPagedUserSnippetTriggerPrefixes(
                   query,
                   userId,
                   cancellationToken).NoSync();

            var pagedResponse = pagedEntities.ToPagedResponse();
            return pagedResponse;
        }

        public async Task<PagedResponse<SnippetTriggerPrefixDto>> GetPagedWorkspaceSnippetTriggerPrefixesAsync(
            GetWorkspaceSnippetTriggerPrefixesQuery query,
            Guid workspaceId,
            CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(query, nameof(query));

            var pagedEntities = await SnippetTriggerPrefixesRepository.GetPagedWorkspaceSnippetTriggerPrefixes(
                   query,
                   workspaceId,
                   cancellationToken).NoSync();

            var pagedResponse = pagedEntities.ToPagedResponse();
            return pagedResponse;
        }

        public async Task<SnippetTriggerPrefixDto> CreateSnippetTriggerPrefixAsync(
            SnippetTriggerPrefixInputDto inputDto,
            CancellationToken cancellationToken = default)
        {
            await ValidateUpsertAsync(inputDto, cancellationToken);

            var entity = inputDto.ToEntity();

            await SnippetTriggerPrefixesRepository.Insert(entity, cancellationToken).NoSync();
            await unitOfWork.CommitAsync(cancellationToken).NoSync();

            var dto = entity.ToDto();
            return dto;
        }

        public async Task<SnippetTriggerPrefixDto> UpdateSnippetTriggerPrefixAsync(
            SnippetTriggerPrefixInputDto inputDto,
            CancellationToken cancellationToken = default)
        {
            await ValidateUpsertAsync(inputDto, cancellationToken);

            var entity = inputDto.ToEntity();
            entity.Id = inputDto.Id!.Value;

            await SnippetTriggerPrefixesRepository.Update(entity, cancellationToken).NoSync();
            await unitOfWork.CommitAsync(cancellationToken).NoSync();

            await SnippetsRepository.IncrementUserSnippetsVersion(inputDto.UserId!.Value, cancellationToken).NoSync();

            var dto = entity.ToDto();
            return dto;
        }

        private async Task<IEnumerable<SnippetTriggerPrefixDto>> GetUserSnippetTriggerPrefixesAsync(
            Expression<Func<SnippetTriggerPrefix, bool>> predicate,
            CancellationToken cancellationToken = default)
        {
            var prefixes = await SnippetTriggerPrefixesRepository.FindByConditionProjectedAsync(
                    predicate,
                    triggerPrefix => new SnippetTriggerPrefixDto
                    {
                        Id = triggerPrefix.Id,
                        Prefix = triggerPrefix.Prefix,
                    },
                    cancellationToken: cancellationToken).NoSync();

            return prefixes;
        }

        private async Task ValidateUpsertAsync(
            SnippetTriggerPrefixInputDto inputDto,
            CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(inputDto, nameof(inputDto));
            var isUserPrefix = inputDto.UserId != null;

            if (!isUserPrefix && inputDto.WorkspaceId == null)
            {
                throw new BusinessValidationException("Snippet trigger prefix must be associated with either a user or a workspace.");
            }

            if (isUserPrefix && inputDto.WorkspaceId != null)
            {
                throw new BusinessValidationException("Snippet trigger prefix cannot be associated with both a user and a workspace.");
            }

            if (inputDto.Id.HasValue)
            {
                if (isUserPrefix)
                {
                    var exists = await SnippetTriggerPrefixesRepository.ExistsAsync(
                                           x => x.Id == inputDto.Id.Value &&
                                               x.UserId == inputDto.UserId!.Value,
                                           cancellationToken: cancellationToken).NoSync();

                    if (!exists)
                    {
                        throw new KeyNotFoundException(typeof(SnippetTriggerPrefix).NotFoundValidationMessage(inputDto.Id.Value));
                    }

                }

                var samePrefixExists = await SnippetTriggerPrefixesRepository.ExistsAsync(
                        x => x.Id != inputDto.Id.Value &&
                             (inputDto.UserId != null && x.UserId == inputDto.UserId.Value ||
                             inputDto.WorkspaceId != null && x.WorkspaceId == inputDto.WorkspaceId.Value) &&
                             EF.Functions.ILike(x.Prefix, inputDto.Prefix),
                        cancellationToken: cancellationToken).NoSync();

                if (samePrefixExists)
                {
                    string message = string.Empty;

                    if (inputDto.UserId != null)
                    {
                        message = $"Snippet with prefix '{inputDto.Prefix}' already exists for user with id '{inputDto.UserId.Value}'.";
                    }
                    else
                    {
                        message = $"Snippet with prefix '{inputDto.Prefix}' already exists for workspace with id '{inputDto.WorkspaceId.Value}'.";
                    }

                    throw new BusinessValidationException(message);
                }
            }
        }
    }
}
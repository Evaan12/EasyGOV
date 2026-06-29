using Application.Interfaces;
using Domain.Entities;
using Domain.Exceptions;
using Domain.Repositories;
using Mediator;
using System;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Application.Features.SifarisApplications.Commands
{
    public record ApproveSifarisApplicationCommand(Guid ApplicationId, string ReviewNotes, string SifarisDataJson) : IRequest<Guid>;

    public class ApproveSifarisApplicationCommandHandler : IRequestHandler<ApproveSifarisApplicationCommand, Guid>
    {
        private readonly ISifarisApplicationRepository _applicationRepository;
        private readonly ISifarisRepository _sifarisRepository;
        private readonly ICurrentUserService _currentUser;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IUserService _userService;
        private readonly ICitizenProfileRepository _profileRepository;

        public ApproveSifarisApplicationCommandHandler(
            ISifarisApplicationRepository applicationRepository, 
            ISifarisRepository sifarisRepository,
            ICurrentUserService currentUser, 
            IUnitOfWork unitOfWork,
            IUserService userService,
            ICitizenProfileRepository profileRepository)
        {
            _applicationRepository = applicationRepository;
            _sifarisRepository = sifarisRepository;
            _currentUser = currentUser;
            _unitOfWork = unitOfWork;
            _userService = userService;
            _profileRepository = profileRepository;
        }

        public async Task<Guid> Handle(ApproveSifarisApplicationCommand request, CancellationToken cancellationToken)
        {
            var application = await _applicationRepository.GetByIdAsync(request.ApplicationId, cancellationToken);
            if (application == null) throw new DomainException("Sifaris application not found.");

            var profile = await _profileRepository.GetByIdAsync(application.CitizenId, cancellationToken);
            if (profile == null) throw new DomainException("Citizen profile not found.");

            var userDetails = await _userService.GetUserDetailsAsync(_currentUser.UserId, cancellationToken);

            application.Approve(_currentUser.UserId, userDetails.FullName, userDetails.PrimaryRole, request.ReviewNotes ?? "Approved", DateTime.UtcNow);

            string hashString;
            using (var sha256 = SHA256.Create())
            {
                var hashBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(profile.FullName.ToLowerInvariant()));
                hashString = Convert.ToBase64String(hashBytes);
            }

            var sifaris = new Domain.Entities.Sifaris(
                Guid.NewGuid(),
                application.CitizenId,
                application.TargetWardId,
                application.TargetSifarisTemplateId,
                application.Id,
                request.SifarisDataJson,
                hashString,
                userDetails.FullName,
                userDetails.PrimaryRole,
                _currentUser.UserId
            );

            await _sifarisRepository.AddAsync(sifaris, cancellationToken);
            await _applicationRepository.UpdateAsync(application, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return sifaris.Id;
        }
    }
}
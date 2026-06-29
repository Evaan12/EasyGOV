using Application.Interfaces;
using Domain.Entities;
using Domain.Repositories;
using Mediator;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Application.Features.ApiConsent.Commands
{
    public record RequestApiConsentCommand(Guid CitizenId, string ThirdPartyClientId, string Scopes) : IRequest<Guid>;

    public class RequestApiConsentCommandHandler : IRequestHandler<RequestApiConsentCommand, Guid>
    {
        private readonly IApiConsentRequestRepository _repository;
        private readonly ICitizenProfileRepository _profileRepository;
        private readonly IOtpService _otpService;
        private readonly IUnitOfWork _unitOfWork;

        public RequestApiConsentCommandHandler(IApiConsentRequestRepository repository, ICitizenProfileRepository profileRepository, IOtpService otpService, IUnitOfWork unitOfWork)
        {
            _repository = repository;
            _profileRepository = profileRepository;
            _otpService = otpService;
            _unitOfWork = unitOfWork;
        }

        public async Task<Guid> Handle(RequestApiConsentCommand request, CancellationToken cancellationToken)
        {
            var profile = await _profileRepository.GetByIdAsync(request.CitizenId, cancellationToken);
            if (profile == null || profile.MobileNumber == null)
                throw new Domain.Exceptions.DomainException("Citizen profile not found or lacks a registered mobile number.");

            var activeRequest = await _repository.GetActivePendingRequestAsync(request.CitizenId, request.ThirdPartyClientId, cancellationToken);
            if (activeRequest != null) return activeRequest.Id; 

            var rawOtp = _otpService.GenerateCryptoSecureOtp();
            var hashedOtp = _otpService.ComputeHash(rawOtp);

            var consentRequest = new ApiConsentRequest(
                Guid.NewGuid(),
                request.CitizenId,
                request.ThirdPartyClientId,
                request.Scopes,
                hashedOtp,
                TimeSpan.FromMinutes(10),
                Guid.Empty
            );

            await _repository.AddAsync(consentRequest, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            _ = _otpService.SendOtpAsync(profile.MobileNumber.Value, rawOtp, cancellationToken);

            return consentRequest.Id;
        }
    }
}
using Application.Interfaces;
using Domain.Repositories;
using Mediator;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Application.Features.ApiConsent.Commands
{
    public record VerifyApiConsentCommand(Guid RequestId, string Otp) : IRequest<Unit>;

    public class VerifyApiConsentCommandHandler : IRequestHandler<VerifyApiConsentCommand, Unit>
    {
        private readonly IApiConsentRequestRepository _repository;
        private readonly IOtpService _otpService;
        private readonly IUnitOfWork _unitOfWork;

        public VerifyApiConsentCommandHandler(IApiConsentRequestRepository repository, IOtpService otpService, IUnitOfWork unitOfWork)
        {
            _repository = repository;
            _otpService = otpService;
            _unitOfWork = unitOfWork;
        }

        public async Task<Unit> Handle(VerifyApiConsentCommand request, CancellationToken cancellationToken)
        {
            var consentReq = await _repository.GetByIdAsync(request.RequestId, cancellationToken);
            if (consentReq == null) throw new Domain.Exceptions.DomainException("Consent request not found.");

            var hashedOtp = _otpService.ComputeHash(request.Otp);
            consentReq.GrantConsent(hashedOtp);

            await _repository.UpdateAsync(consentReq, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return Unit.Value;
        }
    }
}
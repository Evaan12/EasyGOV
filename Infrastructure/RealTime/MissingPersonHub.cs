using Application.Features.MissingPersons.Commands;
using Mediator;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using System;
using System.Threading.Tasks;

namespace Infrastructure.RealTime
{
    [Authorize]
    public class MissingPersonHub : Hub
    {
        private readonly IMediator _mediator;

        public MissingPersonHub(IMediator mediator)
        {
            _mediator = mediator;
        }

        public async Task ProcessFrame(byte[] frameData)
        {
            if (frameData == null || frameData.Length == 0) 
                return;

            try
            {
                await _mediator.Send(new ProcessVideoFrameCommand(Context.ConnectionId, frameData));
            }
            catch (Exception)
            {
            }
        }
    }
}


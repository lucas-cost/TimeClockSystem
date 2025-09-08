using MediatR;
using TimeClockSystem.Application.DTOs;

namespace TimeClockSystem.Application.UseCases.RegisterPoint
{
    public class RegisterPointCommand : IRequest<RegisterPointResult>
    {
        public RegisterPointRequestDto PointData { get; }

        public RegisterPointCommand(RegisterPointRequestDto pointData)
        {
            PointData = pointData;
        }
    }
}

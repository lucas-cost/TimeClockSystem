using TimeClockSystem.Infrastructure.Hardware.Abstractions;

namespace TimeClockSystem.Infrastructure.Hardware.Factories
{
    public interface IWebcamFactory
    {
        IVideoCaptureWrapper? CreateVideoCapture();
    }
}

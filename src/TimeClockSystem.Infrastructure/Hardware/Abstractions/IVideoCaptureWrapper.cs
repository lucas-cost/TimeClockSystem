using OpenCvSharp;

namespace TimeClockSystem.Infrastructure.Hardware.Abstractions
{
    public interface IVideoCaptureWrapper : IDisposable
    {
        bool IsOpened();
        void Read(Mat frame);
    }
}

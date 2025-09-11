using OpenCvSharp;
using TimeClockSystem.Infrastructure.Hardware.Abstractions;

namespace TimeClockSystem.Infrastructure.Hardware
{
    public class OpenCvCaptureWrapper : IVideoCaptureWrapper
    {
        private readonly VideoCapture _capture;

        public OpenCvCaptureWrapper(int index)
        {
            _capture = new VideoCapture(index);
        }

        public bool IsOpened() => _capture.IsOpened();
        public void Read(Mat frame) => _capture.Read(frame);
        public void Dispose() => _capture.Dispose();
    }
}

namespace TimeClockSystem.Application.Interfaces
{
    public interface IWebcamService
    {
        event Action<byte[]> FrameReady;

        bool IsWebcamAvailable();
        string CaptureAndSaveImage(string employeeId);
        void Start();
        void Stop();
    }
}

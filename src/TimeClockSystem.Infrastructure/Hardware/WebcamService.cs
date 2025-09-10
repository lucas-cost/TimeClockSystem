using OpenCvSharp;
using TimeClockSystem.Application.Interfaces;
using TimeClockSystem.Infrastructure.Hardware.Abstractions;

namespace TimeClockSystem.Infrastructure.Hardware
{
    public class WebcamService : IWebcamService, IDisposable
    {
        private readonly IVideoCaptureWrapper? _capture;
        private CancellationTokenSource? _cancellationTokenSource;
        private Task? _captureTask;

        public event Action<byte[]>? FrameReady;

        public WebcamService(IVideoCaptureWrapper? capture)
        {
            _capture = capture;
        }

        public bool IsWebcamAvailable() => _capture?.IsOpened() ?? false;

        public void Start()
        {
            if (_captureTask != null && _captureTask.Status == TaskStatus.Running)
                return;

            if (!IsWebcamAvailable())
                throw new InvalidOperationException("Nenhuma webcam funcional pôde ser inicializada.");

            _cancellationTokenSource = new CancellationTokenSource();
            CancellationToken token = _cancellationTokenSource.Token;

            _captureTask = Task.Run(async () =>
            {
                while (!token.IsCancellationRequested)
                {
                    using (var frame = new Mat())
                    {
                        _capture?.Read(frame);
                        if (frame != null && !frame.Empty())
                        {
                            Cv2.ImEncode(".jpg", frame, out byte[] buffer);
                            FrameReady?.Invoke(buffer);
                        }
                        await Task.Delay(33, token);
                    }
                }
            }, token);
        }

        public void Stop()
        {
            _cancellationTokenSource?.Cancel();
        }

        public string CaptureAndSaveImage(string employeeId)
        {
            if (!IsWebcamAvailable() || _capture == null)
                throw new InvalidOperationException("Webcam não disponível para captura de imagem.");

            using (var frame = new Mat())
            {
                _capture.Read(frame);
                if (frame.Empty())
                    throw new InvalidOperationException("Falha ao capturar frame da webcam.");

                string tempPath = Path.GetTempPath();
                string fileName = $"ponto_{employeeId}_{DateTime.Now:yyyyMMddHHmmss}.jpg";
                string fullPath = Path.Combine(tempPath, fileName);

                Cv2.ImWrite(fullPath, frame);
                return fullPath;
            }
        }

        public void Dispose()
        {
            Stop();
            _capture?.Dispose();
        }
    }
}

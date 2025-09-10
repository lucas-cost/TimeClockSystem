using OpenCvSharp;
using System.Diagnostics;
using TimeClockSystem.Application.Interfaces;

namespace TimeClockSystem.Infrastructure.Hardware
{
    public class WebcamService : IWebcamService, IDisposable
    {
        private VideoCapture? _capture;
        private CancellationTokenSource? _cancellationTokenSource;
        private Task? _captureTask;

        public event Action<byte[]>? FrameReady;

        public WebcamService()
        {
            int cameraIndex = -1;

            // Tenta encontrar uma câmera funcional nos índices de 0 a 9, medida realizada porque eu virtualizo a camera do meu celular como webcam usando o Iriun
            for (int i = 0; i < 10; i++)
            {
                try
                {
                    VideoCapture testCapture = new(i);
                    if (testCapture.IsOpened())
                    {
                        Debug.WriteLine($"Câmera encontrada e funcionando no índice: {i}");
                        cameraIndex = i;
                        testCapture.Dispose();
                        break;
                    }
                    testCapture.Dispose();
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"Erro ao testar câmera no índice {i}: {ex.Message}");
                }
            }

            if (cameraIndex != -1)
            {
                _capture = new VideoCapture(cameraIndex);
            }
            else
            {
                Debug.WriteLine("Nenhuma câmera funcional foi encontrada.");
                _capture = null;
            }
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
            if (!IsWebcamAvailable()) throw new InvalidOperationException("Webcam não disponível.");
            using (var frame = new Mat())
            {
                _capture.Read(frame);
                if (frame.Empty()) throw new InvalidOperationException("Falha ao capturar frame.");

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

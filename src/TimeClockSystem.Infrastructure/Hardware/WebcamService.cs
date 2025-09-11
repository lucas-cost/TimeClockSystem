using Microsoft.Extensions.Logging;
using OpenCvSharp;
using TimeClockSystem.Application.Interfaces;
using TimeClockSystem.Core.Exceptions;
using TimeClockSystem.Infrastructure.Hardware.Abstractions;

namespace TimeClockSystem.Infrastructure.Hardware
{
    public class WebcamService : IWebcamService, IDisposable
    {
        private readonly IVideoCaptureWrapper? _capture;
        private readonly ILogger<WebcamService> _logger;
        private CancellationTokenSource? _cancellationTokenSource;
        private Task? _captureTask;

        public event Action<byte[]>? FrameReady;

        public WebcamService(IVideoCaptureWrapper? capture, ILogger<WebcamService> logger)
        {
            _capture = capture;
            _logger = logger;
        }

        public bool IsWebcamAvailable() => _capture?.IsOpened() ?? false;

        public void Start()
        {
            if (_captureTask != null && _captureTask.Status == TaskStatus.Running)
                return;

            if (!IsWebcamAvailable())
            {
                _logger.LogError("Falha ao iniciar o feed: nenhuma webcam funcional pôde ser inicializada.");
                throw new InvalidOperationException("Nenhuma webcam funcional pôde ser inicializada.");
            }

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
            {
                _logger.LogError("Tentativa de capturar imagem, mas a webcam não está disponível.");
                throw new InvalidOperationException("Webcam não disponível para captura de imagem.");
            }

            using (var frame = new Mat())
            {
                _capture.Read(frame);
                if (frame.Empty())
                {
                    _logger.LogError("Falha ao capturar frame da webcam para o funcionário {EmployeeId}.", employeeId);
                    throw new InvalidOperationException("Falha ao capturar frame da webcam.");
                }

                ValidateImageQuality(frame);

                string tempPath = Path.GetTempPath();
                string fileName = $"ponto_{employeeId}_{DateTime.Now:yyyyMMddHHmmss}.jpg";
                string fullPath = Path.Combine(tempPath, fileName);

                Cv2.ImWrite(fullPath, frame);
                return fullPath;
            }
        }

        private void ValidateImageQuality(Mat image)
        {
            // --- Validação de Brilho ---
            // Converte a imagem para escala de cinza para calcular o brilho médio
            using (var grayImage = new Mat())
            {
                Cv2.CvtColor(image, grayImage, ColorConversionCodes.BGR2GRAY);
                // Calcula o valor médio dos pixels (0=preto, 255=branco)
                Scalar meanBrightness = Cv2.Mean(grayImage);

                // Define os limites. Estes valores podem ser ajustados.
                const double minBrightness = 25.0;
                const double maxBrightness = 150.0;

                if (meanBrightness.Val0 < minBrightness)
                {
                    _logger.LogWarning("Validação de qualidade falhou: imagem muito escura. Valor: {BrightnessValue}", meanBrightness.Val0);
                    throw new ImageQualityException("A imagem está muito escura. Por favor, melhore a iluminação.");
                }

                if (meanBrightness.Val0 > maxBrightness)
                {
                    _logger.LogWarning("Validação de qualidade falhou: imagem muito clara. Valor: {BrightnessValue}", meanBrightness.Val0);
                    throw new ImageQualityException("A imagem está muito clara (superexposta). Por favor, ajuste a iluminação.");
                }
            }

            // --- Validação de Foco (Nitidez) ---
            // Usa a variância do Laplaciano para medir a nitidez.
            // Um valor alto significa mais bordas e, portanto, mais nitidez.
            using (var laplacian = new Mat())
            {
                Cv2.Laplacian(image, laplacian, MatType.CV_64F);
                Cv2.MeanStdDev(laplacian, out _, out Scalar stdDev);
                double focusMeasure = stdDev.Val0 * stdDev.Val0;

                const double minFocus = 90.0;

                if (focusMeasure < minFocus)
                    throw new ImageQualityException("A imagem está sem foco (borrada). Por favor, fique parado e tente novamente.");

                _logger.LogInformation("Validação de qualidade de imagem aprovada.");
            }
        }

        public void Dispose()
        {
            _logger.LogInformation("Disposing WebcamService...");
            Stop();
            _capture?.Dispose();
        }
    }
}

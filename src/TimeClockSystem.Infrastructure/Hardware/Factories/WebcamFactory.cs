using System.Diagnostics;
using TimeClockSystem.Infrastructure.Hardware.Abstractions;

namespace TimeClockSystem.Infrastructure.Hardware.Factories
{
    public class WebcamFactory : IWebcamFactory
    {
        // Tenta encontrar uma câmera funcional nos índices de 0 a 9, medida realizada porque eu virtualizo a camera do meu celular como webcam usando o Iriun
        public IVideoCaptureWrapper? CreateVideoCapture()
        {
            for (int i = 0; i < 10; i++)
            {
                try
                {
                    OpenCvCaptureWrapper testCapture = new(i);
                    if (testCapture.IsOpened())
                    {
                        Debug.WriteLine($"Câmera encontrada e funcionando no índice: {i}");
                        return testCapture;
                    }
                    testCapture.Dispose();
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"Erro ao testar câmera no índice {i}: {ex.Message}");
                }
            }
            Debug.WriteLine("Nenhuma câmera funcional foi encontrada.");
            return null;
        }
    }
}

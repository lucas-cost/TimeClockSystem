using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MediatR;
using System.IO;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using TimeClockSystem.Application.DTOs;
using TimeClockSystem.Application.Interfaces;
using TimeClockSystem.Application.UseCases.RegisterPoint;
using TimeClockSystem.Core.Enums;

namespace TimeClockSystem.UI.ViewModels
{
    public partial class MainViewModel : ObservableObject, IDisposable
    {
        private readonly IMediator _mediator;
        private readonly IWebcamService _webcamService;

        [ObservableProperty]
        private string _employeeId = string.Empty;

        [ObservableProperty]
        private string _statusMessage = "Pronto para registrar.";

        [ObservableProperty]
        private ImageSource? _webcamFeed;

        [ObservableProperty]
        private bool _isWebcamAvailable;

        public MainViewModel(IMediator mediator, IWebcamService webcamService)
        {
            _mediator = mediator;
            _webcamService = webcamService;

            _webcamService.FrameReady += OnFrameReady;
                        try
            {
                _webcamService.Start();
                IsWebcamAvailable = true;
                StatusMessage = "Webcam iniciada com sucesso.";
            }
            catch (Exception ex)
            {
                IsWebcamAvailable = false;
                StatusMessage = $"Aviso: {ex.Message}";
            }
        }

        private void OnFrameReady(byte[] frameData)
        {
            if (frameData == null || frameData.Length == 0)
                return;

            System.Windows.Application.Current.Dispatcher.Invoke(() =>
            {
                using (MemoryStream stream = new(frameData))
                {
                    BitmapImage bitmapImage = new();
                    bitmapImage.BeginInit();
                    bitmapImage.StreamSource = stream;
                    bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
                    bitmapImage.EndInit();
                    bitmapImage.Freeze();

                    WebcamFeed = bitmapImage;
                }
            });
        }

        [RelayCommand(CanExecute = nameof(IsWebcamAvailable))]
        private async Task RegisterPoint()
        {
            if (string.IsNullOrWhiteSpace(EmployeeId))
            {
                StatusMessage = "Por favor, informe a matrícula.";
                return;
            }

            StatusMessage = "Registrando ponto...";

            RegisterPointRequestDto requestDto = new RegisterPointRequestDto
            {
                EmployeeId = this.EmployeeId
            };

            RegisterPointCommand command = new RegisterPointCommand(requestDto);

            RegisterPointResult result = await _mediator.Send(command);

            if (result.Success)
            {
                string recordTypeMessage = GetMessageForRecordType(result.CreatedRecordType);
                StatusMessage = $"{recordTypeMessage} para '{EmployeeId}' com sucesso!";
                EmployeeId = string.Empty;
            }
            else
            {
                StatusMessage = result.ErrorMessage;
            }
        }

        private string GetMessageForRecordType(RecordType? recordType)
        {
            return recordType switch
            {
                RecordType.Entry => "Entrada registrada",
                RecordType.Exit => "Saída registrada",
                RecordType.BreakStart => "Início de pausa registrado",
                RecordType.BreakEnd => "Fim de pausa registrado",
                _ => "Ponto registrado"
            };
        }

        public void Dispose()
        {
            _webcamService.FrameReady -= OnFrameReady;
            _webcamService.Stop();
        }
    }
}

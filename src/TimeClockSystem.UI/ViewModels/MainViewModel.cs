using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MediatR;
using TimeClockSystem.Application.DTOs;
using TimeClockSystem.Application.UseCases.RegisterPoint;

namespace TimeClockSystem.UI.ViewModels
{
    public partial class MainViewModel : ObservableObject
    {
        private readonly IMediator _mediator;

        [ObservableProperty]
        private string _employeeId = string.Empty;

        [ObservableProperty]
        private string _statusMessage = "Pronto para registrar.";

        public MainViewModel(IMediator mediator)
        {
            _mediator = mediator;
        }


        [RelayCommand]
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


            //Envio do comando via MediatR
        }
    }
}

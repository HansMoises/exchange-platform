using System;
using System.Threading;
using System.Threading.Tasks;
using ExchangePlatform.Application.Features.Reports.Commands.CrearReporte;
using ExchangePlatform.Application.Common.Exceptions;
using ExchangePlatform.Domain.Entities;
using ExchangePlatform.Domain.Interfaces;
using ExchangePlatform.Domain.Interfaces.Repositories;
using FluentAssertions;
using Moq;
using Xunit;

namespace ExchangePlatform.UnitTests.Application.Reports;

public class CrearReporteCommandTests
{
    [Fact]
    public void Validator_Rechaza_EntidadTipo_Invalido()
    {
        var cmd = new CrearReporteCommand(Guid.NewGuid(), "Invalido", Guid.NewGuid(), "Spam", null);
        var validator = new CrearReporteCommandValidator();

        var result = validator.Validate(cmd);

        result.IsValid.Should().BeFalse();
    }

    [Fact]
    public async Task Handler_Lanza_ConflictException_Si_ya_existe_reporte_activo()
    {
        var uowMock = new Mock<IUnitOfWork>();
        var repoMock = new Mock<IReporteRepository>();

        repoMock.Setup(r => r.ExisteReporteActivoAsync(It.IsAny<Guid>(), It.IsAny<Guid>()))
            .ReturnsAsync(true);

        uowMock.SetupGet(u => u.Reportes).Returns(repoMock.Object);

        var handler = new CrearReporteCommandHandler(uowMock.Object);

        var cmd = new CrearReporteCommand(Guid.NewGuid(), "Objeto", Guid.NewGuid(), "Spam", "detalle");

        Func<Task> act = async () => await handler.Handle(cmd, CancellationToken.None);

        await act.Should().ThrowAsync<ConflictException>();
    }

    [Fact]
    public async Task Handler_Crea_reporte_y_guarda_en_uow()
    {
        var uowMock = new Mock<IUnitOfWork>();
        var repoMock = new Mock<IReporteRepository>();

        repoMock.Setup(r => r.ExisteReporteActivoAsync(It.IsAny<Guid>(), It.IsAny<Guid>()))
            .ReturnsAsync(false);

        // Capture the added reporte
        Reporte? captured = null;
        repoMock.Setup(r => r.AddAsync(It.IsAny<Reporte>()))
            .Returns<Reporte>(r => { captured = r; return Task.CompletedTask; });

        uowMock.SetupGet(u => u.Reportes).Returns(repoMock.Object);
        uowMock.Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

        var handler = new CrearReporteCommandHandler(uowMock.Object);

        var reportanteId = Guid.NewGuid();
        var entidadId = Guid.NewGuid();
        var cmd = new CrearReporteCommand(reportanteId, "Objeto", entidadId, "Spam", "detalle");

        var result = await handler.Handle(cmd, CancellationToken.None);

        result.Should().NotBe(Guid.Empty);
        captured.Should().NotBeNull();
        captured!.ReportanteId.Should().Be(reportanteId);
        captured.EntidadId.Should().Be(entidadId);
        repoMock.Verify(r => r.AddAsync(It.IsAny<Reporte>()), Times.Once);
        uowMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }
}

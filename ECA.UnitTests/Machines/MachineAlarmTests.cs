using ECA.Domain.Machines.Alarms;

namespace ECA.UnitTests.Machines
{
    public class MachineAlarmTests
    {
        [Fact]
        public void Create_Should_Create_Valid_Machine_Alarm()
        {
            // Arrange
            // Definimos los datos con los que crearemos la alarma.
            const string code = "TEMP-001";
            const string message = "Temperatura del husillo demasiado alta";
            const AlarmSeverity severity = AlarmSeverity.Critical;

            // Act
            var alarm = MachineAlarm.Create(code, message, severity);

            // Assert
            // Comprobamos que se ha generado una identidad válida.
            Assert.NotEqual(default, alarm.Id);

            // Verificamos que los datos proporcionados se conservan.
            Assert.Equal(code, alarm.Code);
            Assert.Equal(message, alarm.Message);
            Assert.Equal(severity, alarm.Severity);

            // Una alarma nueva debe comenzar activa.
            Assert.True(alarm.IsActive);
        }


        [Fact]
        public void Create_Should_Throw_When_Severity_Is_Invalid()
        {
            const string code = "TEMP-002";
            const string message = "Temperatura del husillo demasiado baja";
            const AlarmSeverity invalidSeverity = (AlarmSeverity)99; // Valor inválido

            // Act  & Assert
            var exception = Assert.Throws<ArgumentOutOfRangeException>(
                () => MachineAlarm.Create(code, message, invalidSeverity));
        }
    }
}

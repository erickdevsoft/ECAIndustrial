using ECA.Domain.Machines.Alarms;

namespace ECA.UnitTests.Machines
{
    public class MachineAlarmTests
    {
        [Fact]
        public void Create_Should_Create_Valid_Machine_Alarm()
        {
            //arrange
            const string Code = "TEMP-001";
            const string Message = "Temperatura del husillo demasiado alta";
            const AlarmSeverity Severity = AlarmSeverity.Critical;

            //act
            var alarm = MachineAlarm.Create(Code, Message, Severity);

            //assert
            Assert.NotNull(alarm);
        }
    }
}

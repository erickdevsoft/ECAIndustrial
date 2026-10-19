# Contexto de continuidad — ECA Industrial

> Actualizar al final de cada sesión. Este documento registra el estado de trabajo más reciente; `README.md` describe el producto y `ROADMAP.md` contiene el plan por sprints.

## Objetivo del proyecto

ECA Industrial es una plataforma industrial ficticia para demostrar competencias avanzadas en .NET. Arquitectura objetivo: Clean Architecture + DDD, API ASP.NET Core, persistencia SQL Server, HMI con WPF/MVVM, simulador industrial y web SCADA/MES con Angular + TypeScript.

## Decisiones confirmadas

- .NET 10 y C#.
- WPF + MVVM para el HMI de escritorio.
- Angular + TypeScript para la web SCADA/MES.
- xUnit para tests y Moq para simular dependencias en tests de Application.
- `Directory.Packages.props` para versiones NuGet centralizadas.
- Preferencia por opciones de compilación proyecto por proyecto, evitando cambios globales innecesarios.
- Identificadores técnicos en inglés; explicaciones, comentarios didácticos y mensajes funcionales en español.
- En las sesiones de aprendizaje: explicar, mostrar ejemplos, proponer un único ejercicio, esperar la implementación, revisar y corregir antes de avanzar.

## Estructura del repositorio

Los proyectos están directamente en la raíz del repositorio (no bajo `src/` y `tests/`). Entre otros, existen `ECA.Domain`, `ECA.Application`, `ECA.Infrastructure`, `ECA.Api`, `ECA.Hmi`, `ECA.Simulator`, `ECA.UnitTests`, `ECA.IntegrationTests` y `ECA.ArchitectureTests`.

## Trabajo implementado hasta ahora

### Dominio

- `Machine`, `MachineId`, `MachineCode`, `MachineType` y `MachineStatus`.
- Ciclo de vida y validaciones de la máquina.
- `MachineAlarm`, `MachineAlarmId` y `AlarmSeverity`.
- Alarmas gestionadas desde `Machine` como Aggregate Root.
- `IDomainEvent`, evento de cambio de estado, evento de alarma generada y evento de alarma desactivada.
- `GetDomainEvents()` devuelve una copia de los eventos pendientes y limpia la colección interna.
- Al limpiar una alarma ya inactiva no se genera otro evento.
- Modelo inicial de `Plant` y `ProductionLine` con referencias por identificador.
- Tests unitarios asociados; el usuario ha confirmado que pasan en las últimas iteraciones del dominio.

## Application

- `IMachineRepository` define `GetByIdAsync` y `SaveAsync`.
- `ClearMachineAlarmUseCase` recibe `IMachineRepository` por constructor y coordina la desactivación de una alarma.
- `RaiseMachineAlarmUseCase` recibe `IMachineRepository`, recupera la máquina, delega la creación de la alarma al agregado, guarda la máquina y devuelve el ID de la alarma; devuelve `null` si la máquina no existe.
- Moq está añadido como dependencia de `ECA.UnitTests` mediante gestión centralizada de versiones NuGet.

### Tests de ClearMachineAlarmUseCase

Se han implementado y el usuario ha confirmado que pasan estos escenarios:

1. `ExecuteAsync_Should_Return_False_When_Machine_Does_Not_Exist`
   - Devuelve `false` si no existe la máquina.
   - Comprueba que `SaveAsync` no se invoca.

2. `ExecuteAsync_Should_Return_True_When_Machine_Exists_And_Alarm_Is_Cleared`
   - Devuelve `true` cuando existe la máquina y se limpia correctamente la alarma.
   - Comprueba que la alarma queda inactiva.
   - Comprueba que `SaveAsync` se invoca exactamente una vez con la máquina correspondiente.

3. `ExecuteAsync_Should_Propagate_DomainException_When_Alarm_Does_Not_Belong_To_Machine`
   - Comprueba que se propaga `DomainException` cuando la alarma no pertenece a la máquina.
   - Comprueba que `SaveAsync` no se invoca si la operación falla.

## Punto exacto para retomar

**Sprint 4 — ECA.Application.**

`RaiseMachineAlarmUseCaseTests` cubre el camino exitoso y el caso en que la máquina no existe. El test de máquina inexistente verifica que `ExecuteAsync` devuelve `null`, que consulta el repositorio con el mismo identificador y que no invoca `SaveAsync`.

La implementación del caso de uso está en el commit `9a4c914` (`feat(aplicacion): añadir caso de uso para generar alarmas`). La ejecución más reciente de `dotnet test ECA.UnitTests/ECA.UnitTests.csproj` terminó correctamente: 63 tests, 0 fallidos.

El siguiente ejercicio pendiente es probar que se propaga `ArgumentException` cuando se intenta generar una alarma con un código vacío, y que `SaveAsync` no se invoca.

## Metodología de aprendizaje

Trabajar un solo ejercicio cada vez:

1. Explicar el concepto y el problema que se quiere resolver.
2. Mostrar ejemplos guiados cuando sean necesarios.
3. Proponer un único ejercicio.
4. Esperar la implementación del usuario.
5. Revisar el código, explicar los errores y comparar alternativas.
6. Confirmar los tests antes de avanzar.

Utilizar español en las explicaciones, los comentarios educativos y los mensajes funcionales. Mantener los identificadores técnicos en inglés y aplicar Clean Code, evitando comentarios redundantes.

Respetar la configuración actual de los proyectos. No introducir cambios globales de compilación ni dependencias nuevas sin una necesidad justificada.

## Próxima actualización

Al finalizar la siguiente sesión, registrar:

- Archivos creados o modificados.
- Ejercicio completado y decisiones tomadas.
- Resultado confirmado de `dotnet test` y `dotnet build`, cuando se ejecuten.
- Commit realizado, si corresponde.
- Siguiente ejercicio único pendiente.

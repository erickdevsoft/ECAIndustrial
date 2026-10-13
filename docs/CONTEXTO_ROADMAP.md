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

### Application

- `IMachineRepository` define `GetByIdAsync` y `SaveAsync`.
- `ClearMachineAlarmUseCase` recibe `IMachineRepository` por constructor, obtiene la máquina, devuelve `false` si no existe, llama a `ClearAlarm`, guarda y devuelve `true` si existe.
- Moq añadido como dependencia de `ECA.UnitTests` usando gestión centralizada de versiones NuGet.
- El test `ExecuteAsync_Should_Return_False_When_Machine_Does_Not_Exist` está implementado y pasa. Comprueba también que `SaveAsync` no se invoca cuando la máquina no existe.
- La solución fue recompilada correctamente después de corregir comentarios XML y nomenclatura; verificar de nuevo al continuar si se han realizado cambios desde entonces.

## Punto exacto para retomar

**Sprint 4 — ECA.Application.** El siguiente ejercicio pendiente es crear el test del camino exitoso de `ClearMachineAlarmUseCase`: el repositorio devuelve una máquina, el caso de uso invoca `ClearAlarm` a través de esa máquina, invoca `SaveAsync` con la máquina y devuelve `true`.

Se trabaja un ejercicio cada vez. No adelantar más tareas hasta revisar la solución del usuario y comprobar los tests.

## Próxima actualización

Al cerrar la próxima sesión, anotar los archivos modificados, resultado de `dotnet test`/`dotnet build`, commit realizado (si lo hay) y siguiente ejercicio único.

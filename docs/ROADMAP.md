# Roadmap de ECA Industrial

Este documento contiene el plan de implementación y aprendizaje de ECA Industrial. Los sprints son hitos de entrega; no representan una duración fija. Cada hito debe cumplir sus criterios de aceptación antes de considerarse completado.

## Visión del producto

ECA Industrial simulará una fábrica con plantas, líneas y máquinas. Evolucionará hacia una plataforma con API, simulador industrial, HMI de escritorio con WPF y cliente web SCADA/MES con Angular.

El propósito es construir un portfolio .NET profesional y, al mismo tiempo, aprender arquitectura y desarrollo mediante ejercicios incrementales. Las tecnologías se incorporarán cuando resuelvan una necesidad real.

## Stack confirmado

- C# y .NET 10.
- Clean Architecture y DDD aplicados de forma pragmática.
- WPF + MVVM para el HMI de escritorio.
- Angular + TypeScript para SCADA/MES web.
- xUnit para tests; Moq para simular dependencias de Application.
- `Directory.Packages.props` para gestionar versiones NuGet de forma centralizada.
- Visual Studio 2026.

Tecnologías previstas, aún no implementadas: ASP.NET Core, persistencia efectiva con EF Core y SQL Server, SignalR, RabbitMQ, Redis cuando se justifique, Serilog, OpenTelemetry, Docker, GitHub Actions y Azure si aporta valor al portfolio. El proveedor EF Core SQL Server ya está referenciado en `ECA.Infrastructure`.

## Estructura actual del repositorio

Los proyectos se encuentran en la raíz, no dentro de carpetas `src/` y `tests/`:

```text
ECAIndustrial/
├── ECA.Api/
├── ECA.Application/
├── ECA.ArchitectureTests/
├── ECA.Domain/
├── ECA.Hmi/
├── ECA.Infrastructure/
├── ECA.IntegrationTests/
├── ECA.Simulator/
├── ECA.UnitTests/
├── docs/
├── Directory.Build.props
├── Directory.Packages.props
└── README.md
```

## Sprint 0 — Fundación de la solución

**Objetivo:** disponer de una solución organizada y compilable.

Tareas:
- Crear los proyectos principales y de pruebas.
- Configurar .NET, nullable reference types e implicit usings.
- Configurar la gestión centralizada de versiones NuGet.
- Revisar las opciones de compilación y análisis por proyecto, según la convención acordada.
- Eliminar archivos de ejemplo que no tengan una función real.

**Estado:** completado a nivel de estructura inicial.

**Criterios de aceptación:** la solución compila; los proyectos y referencias son intencionados; no se conservan clases de plantilla sin uso.

## Sprint 1 — Modelo inicial de máquinas

**Objetivo:** modelar una máquina y proteger su ciclo de vida.

Tareas:
- `Machine`, `MachineId`, `MachineCode`, `MachineType` y `MachineStatus`.
- Validar los datos de creación.
- Expresar las transiciones de estado mediante métodos de dominio.
- Actualizar `LastStatusChangedAt` al cambiar el estado.
- Probar transiciones válidas e inválidas.

**Estado:** implementado inicialmente; se mantendrán y ampliarán las pruebas cuando surjan reglas nuevas.

**Criterios de aceptación:** las invariantes principales están en el dominio y cubiertas por pruebas unitarias.

## Sprint 2 — Alarmas y Domain Events

**Objetivo:** modelar alarmas y registrar los hechos importantes de la máquina.

Tareas:
- `MachineAlarm`, `MachineAlarmId` y `AlarmSeverity`.
- Añadir y limpiar alarmas a través de `Machine`, el Aggregate Root.
- Implementar `IDomainEvent` y eventos para cambios de estado, alarma generada y alarma desactivada.
- Validar la gravedad del enum.
- Evitar generar eventos duplicados cuando se intenta limpiar una alarma ya inactiva.
- Probar pertenencia de alarmas, estado activo e información de los eventos.

**Estado:** implementado inicialmente; los tests correspondientes se han ejecutado correctamente según la última confirmación.

**Criterios de aceptación:** las alarmas se modifican a través del agregado, las transiciones respetan las reglas y los eventos describen el hecho correcto.

## Sprint 3 — Plantas y líneas de producción

**Objetivo:** representar la estructura básica de la fábrica sin crear agregados gigantes.

Tareas:
- `Plant` y `PlantId`.
- `ProductionLine` y `ProductionLineId`.
- Referencias entre agregados por identidad.
- Validación de IDs vacíos, asociaciones duplicadas y eliminaciones inválidas.
- Tests de sus reglas principales.

**Estado:** modelo inicial implementado.

**Criterios de aceptación:** cada agregado controla sus propias invariantes y no expone sus colecciones internas para modificación directa.

## Sprint 4 — Casos de uso de Application

**Objetivo:** coordinar el dominio sin acoplar los casos de uso a la persistencia concreta.

Tareas:
- Definir `IMachineRepository` como contrato de Application.
- Implementar `ClearMachineAlarmUseCase`.
- Comprobar el comportamiento cuando la máquina no existe.
- Probar con Moq las llamadas a las dependencias.
- Cubrir el camino exitoso y la propagación de errores de dominio.
- Mantener el dominio libre de dependencias de infraestructura.

**Estado:** completado a nivel inicial.

**Implementado:** `IMachineRepository`, `ClearMachineAlarmUseCase` y `RaiseMachineAlarmUseCase`. Los tests de Application cubren los caminos exitosos, máquina inexistente, errores de validación y guardado, además de la propagación del `CancellationToken`. La última ejecución de `dotnet test ECA.UnitTests/ECA.UnitTests.csproj` terminó con 68 tests correctos.

**Criterios de aceptación:** Application coordina el caso de uso, el dominio aplica las reglas y los tests verifican resultados e interacciones relevantes.

## Sprint 5 — Infrastructure y persistencia

**Objetivo:** guardar y recuperar el dominio mediante una implementación real.

Tareas:
- Configurar EF Core y SQL Server.
- Implementar el repositorio de máquinas y los repositorios que realmente se necesiten.
- Configurar mapeos y migraciones.
- Resolver la persistencia de las colecciones de alarmas y otros conceptos del dominio.
- Definir la frontera transaccional y el procesamiento de Domain Events sin pérdida de consistencia.
- Crear tests de integración de persistencia.

**Estado:** en curso.

**Implementado:** referencia al proveedor EF Core SQL Server en `ECA.Infrastructure`, con la versión gestionada desde `Directory.Packages.props`, y esqueleto de `EcaDbContext` que recibe `DbContextOptions<EcaDbContext>` y expone `DbSet<Machine>`. La compilación de `ECA.Infrastructure` y los 68 tests unitarios se ejecutaron correctamente. La persistencia todavía no está implementada.

**Siguiente tarea:** configurar la conversión de `MachineId` a `Guid` para que EF Core pueda usarlo como clave persistida de `Machine`.

**Criterios de aceptación:** las entidades pueden guardarse y recuperarse conservando sus identidades, estados y relaciones; las operaciones de persistencia se cubren con tests de integración.

## Sprint 6 — API y seguridad

**Objetivo:** exponer los casos de uso a través de HTTP de manera mantenible y segura.

Tareas:
- Definir endpoints REST y contratos de entrada y salida.
- Implementar validación de solicitudes y tratamiento consistente de errores.
- Configurar OpenAPI.
- Incorporar autenticación y autorización cuando existan los primeros flujos de usuario.
- Gestionar secretos y configuración de forma segura.
- Probar endpoints, errores y permisos.

**Estado:** pendiente.

**Criterios de aceptación:** los casos de uso pueden ejecutarse por HTTP, las respuestas de error son coherentes y los accesos no autorizados se rechazan correctamente.

## Sprint 7 — Simulador industrial

**Objetivo:** generar comportamiento industrial reproducible sin maquinaria física.

Tareas:
- Implementar un Worker Service de simulación.
- Simular cambios de estado, telemetría y alarmas.
- Incluir escenarios configurables de operación normal y fallo.
- Mantener la simulación desacoplada del dominio y de los clientes.
- Probar las reglas relevantes del simulador.

**Estado:** pendiente.

**Criterios de aceptación:** se pueden ejecutar escenarios reproducibles que alimentan la API y las interfaces sin PLC físico.

## Sprint 8 — Tiempo real e integración

**Objetivo:** transmitir los cambios relevantes a los consumidores apropiados.

Tareas:
- Incorporar SignalR para actualizar HMI y web en tiempo real.
- Diferenciar Domain Events de Integration Events.
- Introducir RabbitMQ si se necesita comunicación asíncrona entre procesos.
- Introducir Redis solo para un caso de caché o estado compartido justificado.
- Diseñar reintentos, idempotencia, errores y observabilidad de los mensajes.
- Probar los comportamientos de integración.

**Estado:** pendiente.

**Criterios de aceptación:** los consumidores reciben los eventos destinados a ellos; duplicados, fallos y reintentos tienen un comportamiento explícito y probado.

## Sprint 9 — HMI de escritorio con WPF

**Objetivo:** construir una interfaz de operador industrial profesional.

Tareas:
- WPF y MVVM.
- Cliente de API y conexión en tiempo real.
- Vista de planta y estados de máquinas.
- Detalle de máquina y panel de alarmas.
- Controles reutilizables, estilos, estados visuales y accesibilidad.
- Mantener reglas de negocio fuera de la interfaz.

**Estado:** pendiente.

**Criterios de aceptación:** el operador puede visualizar máquinas, estados y alarmas generadas por el simulador desde el HMI.

## Sprint 10 — Angular Web SCADA/MES

**Objetivo:** crear una interfaz web para supervisión y procesos básicos de fabricación.

Tareas:
- Angular + TypeScript.
- Organizar funcionalidades, componentes y servicios.
- Integrar API y SignalR.
- Vista general SCADA, detalle de máquina y alarmas.
- Órdenes de producción, trabajos y asignaciones.
- Paradas y motivos de inactividad.
- Calidad y trazabilidad.
- Cálculo inicial de OEE con sus definiciones documentadas.
- Tests de componentes y servicios relevantes.

**Estado:** tecnología decidida; implementación pendiente.

**Criterios de aceptación:** se pueden monitorizar máquinas desde el navegador y seguir una orden de producción simulada hasta consultar sus resultados principales.

## Sprint 11 — Calidad, observabilidad y entrega del portfolio

**Objetivo:** mejorar la fiabilidad y presentar el proyecto de forma profesional.

Tareas:
- Ampliar tests unitarios, de integración y arquitectura.
- Añadir logging estructurado y observabilidad.
- Revisar seguridad, rendimiento, resiliencia y manejo de errores.
- Preparar Docker y GitHub Actions.
- Desplegar en Azure si aporta valor al portfolio.
- Documentar diagramas, decisiones arquitectónicas (ADR), configuración, ejecución local y demostración.

**Estado:** pendiente.

**Criterios de aceptación:** otro desarrollador puede clonar, configurar, ejecutar y probar el proyecto siguiendo la documentación, y el pipeline valida los cambios de forma automática.

## Metodología de aprendizaje

Cada concepto se trabajará con este ciclo:

1. Explicar el problema y la teoría necesaria.
2. Implementar un ejemplo guiado.
3. Proponer **un único ejercicio**.
4. Esperar la solución del alumno.
5. Revisar y explicar la solución, con mejoras concretas.
6. Comparar alternativas y ejecutar tests.
7. No avanzar hasta cerrar el ejercicio actual.

Los identificadores técnicos del código estarán en inglés. Las explicaciones, los comentarios didácticos y los mensajes funcionales estarán en español. Los comentarios deben ayudar a aprender, pero mantener Clean Code y evitar repetir literalmente lo que ya expresa el código.

## Convención de commits

Se utilizarán tipos de commit convencionales y una descripción en español:

```text
feat(dominio): validar gravedad de alarmas
fix(dominio): evitar eventos duplicados al limpiar alarmas
test(aplicacion): cubrir máquina inexistente en limpieza de alarmas
```

## Comandos de validación

Desde la raíz del repositorio:

```bash
dotnet build
dotnet test
```

Antes de dar por cerrado un hito, revisar también el estado de Git y los cambios pendientes, y documentar el siguiente paso concreto.

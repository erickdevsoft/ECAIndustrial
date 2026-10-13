# ECA Industrial

**ECA Industrial** es una plataforma industrial ficticia desarrollada como proyecto práctico y portfolio para aprender y demostrar competencias avanzadas en .NET. Su objetivo es modelar una fábrica, simular máquinas y procesos de producción, y evolucionar hacia una solución con API, HMI de escritorio y supervisión web SCADA/MES.

> **Estado:** desarrollo activo. El dominio inicial de máquinas y alarmas está implementado; se está construyendo la capa `ECA.Application` y sus casos de uso.

## Objetivos

- Construir una aplicación industrial coherente, comprobable y extensible.
- Aprender arquitectura y diseño mediante problemas reales, no añadiendo patrones o tecnologías por moda.
- Aplicar Clean Architecture, DDD, testing, persistencia, seguridad y observabilidad de forma incremental.
- Crear un proyecto de GitHub que pueda explicarse y defenderse en una entrevista técnica.

## Escenario industrial

El escenario inicial es una fábrica con plantas, líneas de producción y máquinas como `CNC-01`, `CNC-02`, `ROBOT-01` e `INSPECTION-01`.

A lo largo del proyecto se incorporarán progresivamente:

- Estados operativos, alarmas y eventos.
- Sensores y telemetría, por ejemplo temperatura, velocidad y contadores.
- Órdenes de producción y trabajos asignados.
- Paradas, motivos de inactividad y disponibilidad.
- Calidad, trazabilidad y métricas OEE.
- Usuarios, roles, permisos y auditoría.

Se desarrollará un simulador para probar la plataforma sin necesitar PLC ni maquinaria física.

## Arquitectura objetivo

```text
ECA Industrial
├── ECA.Domain          Modelo y reglas de negocio
├── ECA.Application     Casos de uso y coordinación
├── ECA.Infrastructure  Persistencia e integraciones técnicas
├── ECA.Api             API HTTP y composición de servicios
├── ECA.Hmi             HMI de escritorio con WPF y MVVM
├── ECA.Simulator       Simulador de máquinas y telemetría
└── Angular Web         Cliente web SCADA/MES con Angular y TypeScript
```

### Responsabilidades

- **Domain:** entidades, Value Objects, Aggregate Roots, invariantes y Domain Events. No depende de la API, la persistencia ni las interfaces de usuario.
- **Application:** coordina casos de uso y define los contratos que necesita para colaborar con recursos externos.
- **Infrastructure:** implementará repositorios, persistencia y adaptadores técnicos.
- **Api:** expondrá los casos de uso mediante endpoints HTTP.
- **Hmi:** interfaz de operador de escritorio con WPF y MVVM.
- **Simulator:** generará estados y telemetría de máquinas de manera reproducible.
- **Angular Web:** interfaz web SCADA/MES para monitorización, alarmas y procesos de fabricación.

Principios de diseño:

1. Las reglas de negocio se protegen en el dominio.
2. Application coordina; Domain decide si una operación de negocio es válida; Infrastructure resuelve detalles técnicos.
3. Los Aggregate Roots protegen las invariantes de sus agregados.
4. Las referencias entre agregados se modelan por identidad cuando corresponde.
5. Los Domain Events describen hechos ocurridos en el dominio. Las integraciones externas se diseñarán por separado cuando sean necesarias.
6. Las abstracciones y tecnologías se introducen cuando existe una necesidad que las justifica.

## Tecnologías

### Decisiones confirmadas

- C# y .NET 10.
- Clean Architecture y conceptos de Domain-Driven Design (DDD).
- WPF + MVVM para el HMI de escritorio.
- Angular + TypeScript para el cliente web SCADA/MES.
- xUnit para pruebas y Moq para simular dependencias cuando sea útil.
- `Directory.Packages.props` para gestionar versiones de paquetes NuGet de forma centralizada.
- Visual Studio 2026.

### Previstas, pendientes de implementar

| Tecnología | Uso previsto | Estado |
|---|---|---|
| ASP.NET Core | API HTTP | Proyecto creado; casos de uso y endpoints pendientes de completar |
| Entity Framework Core | Persistencia y mapeo | Pendiente |
| SQL Server | Base de datos principal | Pendiente |
| SignalR | Actualizaciones en tiempo real para HMI y web | Pendiente |
| RabbitMQ | Mensajería entre procesos, si está justificada | Pendiente |
| Redis | Caché o estado compartido, si está justificado | Pendiente |
| Serilog / OpenTelemetry | Logging y observabilidad | Pendiente |
| Docker | Entorno reproducible | Pendiente |
| GitHub Actions | Integración y entrega continua | Pendiente |
| Azure | Despliegue, si aporta valor al portfolio | Pendiente |

No se incorporarán todas las tecnologías a la vez. Cada una tendrá un caso de uso y pruebas que justifiquen su presencia.

## Estructura actual del repositorio

Los proyectos están actualmente en la raíz del repositorio; no están organizados bajo carpetas `src/` y `tests/`.

```text
ECAIndustrial/
├── ECA.Api/
├── ECA.Application/
│   └── Machines/
├── ECA.ArchitectureTests/
├── ECA.Domain/
│   ├── Common/
│   ├── Machines/
│   ├── Plants/
│   └── ProductionLines/
├── ECA.Hmi/
├── ECA.Infrastructure/
├── ECA.IntegrationTests/
├── ECA.Simulator/
├── ECA.UnitTests/
├── docs/
│   ├── ROADMAP.md
│   ├── CONTEXTO_ROADMAP.md  (documento de continuidad por mantener)
│   └── decisions/
├── Directory.Build.props
├── Directory.Packages.props
└── README.md
```

La estructura podrá evolucionar mediante un cambio deliberado y documentado; no se moverán carpetas solo para hacer coincidir el repositorio con un ejemplo de documentación.

## Roadmap

El plan detallado, los objetivos, criterios de aceptación y estado de cada hito se mantienen en [`docs/ROADMAP.md`](docs/ROADMAP.md).

| Hito | Objetivo | Estado aproximado |
|---|---|---|
| Sprint 0 | Fundación de solución y proyectos | Completado a nivel inicial |
| Sprint 1 | Modelo de máquinas y ciclo de vida | Implementado inicialmente |
| Sprint 2 | Alarmas y Domain Events | Implementado inicialmente; tests existentes |
| Sprint 3 | Plantas y líneas de producción | Modelo inicial implementado |
| Sprint 4 | Casos de uso de Application | En curso |
| Sprint 5 | Persistencia e Infrastructure | Pendiente |
| Sprint 6 | API y seguridad | Pendiente |
| Sprint 7 | Simulador industrial | Pendiente |
| Sprint 8 | Tiempo real e integración | Pendiente |
| Sprint 9 | HMI WPF + MVVM | Pendiente |
| Sprint 10 | Web SCADA/MES con Angular | Pendiente |
| Sprint 11 | Calidad, observabilidad, CI/CD y portfolio | Pendiente |

Los sprints representan hitos de aprendizaje y entrega, no duraciones fijas. Un hito se considera terminado cuando sus criterios de aceptación, pruebas y documentación están cubiertos.

## Forma de trabajar y aprender

El proyecto también funciona como roadmap de aprendizaje. Cada concepto seguirá este ciclo:

1. Explicar el problema y la teoría necesaria.
2. Mostrar uno o varios ejemplos guiados.
3. Proponer **un único ejercicio cada vez**.
4. Esperar la implementación del alumno.
5. Revisar la solución, explicar mejoras y comparar alternativas.
6. Ejecutar las pruebas y cerrar el concepto antes de avanzar.

Las explicaciones, los comentarios educativos y los mensajes funcionales se escribirán en español. Los nombres de clases, métodos, propiedades y variables se mantendrán en inglés siguiendo convenciones habituales de C#/.NET. Los comentarios serán detallados cuando faciliten el aprendizaje, evitando comentarios redundantes.

## Estado actual y siguiente paso

### Implementado inicialmente

- Solución y proyectos base creados.
- Modelo de dominio de máquinas, identificadores, estados, código de máquina y tipos.
- Modelo inicial de plantas y líneas de producción mediante referencias por identificador.
- Alarmas de máquina y gravedad.
- Domain Events para cambio de estado, generación de alarma y desactivación de alarma.
- Tests del dominio para reglas de estado, alarmas y eventos.
- `ECA.Application` con el contrato `IMachineRepository` y el caso de uso `ClearMachineAlarmUseCase`.
- Moq incorporado a `ECA.UnitTests`.
- El test del caso en que una máquina no existe devuelve `false` y verifica que no se llama al guardado.

### Siguiente paso inmediato

Completar las pruebas de `ClearMachineAlarmUseCase` para el camino exitoso: máquina encontrada, alarma limpiada, `SaveAsync` invocado con la máquina y resultado `true`. Después probar la propagación de errores de dominio y continuar con la implementación real de Infrastructure.

El estado exacto de cada sesión debe mantenerse en `docs/CONTEXTO_ROADMAP.md`. Si la documentación y el código difieren, el código y sus pruebas son la fuente de verdad del comportamiento implementado.

## Convención de commits

Usaremos tipos convencionales de commit y descripciones en español:

```text
feat(dominio): validar gravedad de alarmas
fix(dominio): evitar eventos duplicados al limpiar alarmas
test(aplicacion): cubrir máquina inexistente en limpieza de alarmas
```

Cada commit debe representar un cambio coherente y, siempre que sea posible, dejar la solución compilable y las pruebas relevantes pasando.

## Compilar y probar

Desde la raíz de la solución:

```bash
dotnet build
dotnet test
```

Los comandos deben ejecutarse con un SDK compatible con la versión de .NET utilizada por la solución.

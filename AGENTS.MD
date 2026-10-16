# Instrucciones para Codex — ECA Industrial

## 1. Objetivo del proyecto

ECA Industrial es una plataforma industrial ficticia de aprendizaje y portfolio para desarrollar competencias avanzadas en .NET, arquitectura, pruebas y sistemas industriales.

La arquitectura prevista utiliza Clean Architecture y DDD, con .NET 10, C#, ASP.NET Core, SQL Server, WPFMVVM para el HMI de escritorio y Angular + TypeScript para la interfaz web SCADAMES.

## 2. Documentación obligatoria

Antes de realizar cambios, lee

- `README.md` objetivo y arquitectura general.
- `docsROADMAP.md` sprints y planificación.
- `docsCONTEXTO_ROADMAP.md` estado de aprendizaje y siguiente paso pendiente.

Contrasta la documentación con el código y los tests actuales. Si existen discrepancias, señálalas antes de continuar y no inventes implementaciones que no hayas verificado.

## 3. Metodología de aprendizaje

El usuario quiere aprender mientras construye el proyecto. No debes actuar como un generador autónomo de código.

Sigue siempre este ciclo

1. Explica el problema y el concepto necesario.
2. Muestra ejemplos guiados cuando sean útiles.
3. Propón un único ejercicio cada vez.
4. Espera a que el usuario implemente su solución.
5. Revisa el código y explica los errores, aciertos y mejoras.
6. Muestra una solución de referencia después de revisar el intento.
7. Comprueba los tests antes de avanzar al siguiente ejercicio.

No propongas varios ejercicios juntos ni continúes si el ejercicio actual sigue pendiente.

## 4. Idioma y estilo

- Explicaciones, mensajes funcionales y comentarios educativos en español.
- Nombres de clases, métodos, propiedades y variables en inglés.
- Mantén Clean Code y evita comentarios redundantes.
- Los comentarios deben explicar conceptos, reglas de negocio o decisiones que ayuden a aprender.
- Respeta las convenciones de nomenclatura y los analizadores configurados en cada proyecto.

## 5. Arquitectura

- `ECA.Domain` contiene el modelo y las reglas del negocio.
- `ECA.Application` coordina los casos de uso.
- `ECA.Infrastructure` implementa persistencia e integraciones técnicas.
- `ECA.Api` expone los casos de uso mediante HTTP.
- `ECA.Hmi` utilizará WPF y MVVM.
- La interfaz web SCADAMES utilizará Angular + TypeScript.
- `ECA.Simulator` proporcionará simulación industrial.

No añadas patrones, paquetes ni abstracciones sin explicar qué problema resuelven.

## 6. Estructura real del repositorio

Los proyectos están directamente en la raíz del repositorio. No asumas que siguen una estructura `src` y `tests`.

Respeta `Directory.Packages.props` para las versiones NuGet centralizadas. No cambies la configuración de compilación global ni la estructura de proyectos sin una razón técnica explicada y aprobada.

## 7. Tests y calidad

- Utiliza xUnit para pruebas.
- Utiliza Moq en Application cuando sea apropiado para simular dependencias.
- Prioriza pruebas del comportamiento de negocio y de los casos de uso.
- No afirmes que una prueba pasa sin haberlo comprobado o sin que el usuario lo haya confirmado.
- Explica los errores de compilación y de pruebas en lugar de limitarte a corregirlos.

## 8. Git y control de cambios

- No hagas commits ni pushes sin autorización expresa del usuario.
- Antes de modificar archivos, revisa el estado del repositorio y los cambios existentes.
- No sobrescribas cambios locales del usuario.
- Explica qué archivos propones modificar y por qué.
- Separa los cambios en commits coherentes cuando el usuario los solicite.

## 9. Próximo paso

Lee `docsCONTEXTO_ROADMAP.md` y verifica el estado actual de `ECA.Application`.

El último ejercicio acordado es crear la estructura inicial de `ECA.ApplicationMachinesRaiseMachineAlarmUseCase.cs` declaración de la clase, dependencia `IMachineRepository`, constructor y firma de `ExecuteAsync`.

No implementes todavía la lógica ni los tests. Primero confirma el estado real del repositorio y presenta al usuario el siguiente paso. Continúa con un único ejercicio cada vez.
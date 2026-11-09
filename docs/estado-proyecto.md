# Estado del proyecto Web Scout

Versión 0.1 · Propuesta de arranque · 6 de noviembre de 2026.

El proyecto parte de la propuesta de requisitos y arquitectura 0.1, basada en Core 1.29.0. El trabajo disponible es documental. El siguiente incremento es E0, que cerrará las decisiones y probará las capacidades técnicas necesarias para iniciar E1.

## Situación al 6 de noviembre de 2026

| Área | Estado | Evidencia o condición de salida |
| --- | --- | --- |
| Línea base de producto | Preparada para revisión | Documento base de 42 páginas y catálogo de 71 RF, 12 RNF y 19 PA |
| Organización documental | Preparada | Índice, referencias temáticas, backlog, políticas y procedimientos de este paquete |
| Decisiones de inicio | Pendientes | DC01 a DC10 en el registro de decisiones |
| E0 Descubrimiento y prototipo | Pendiente de iniciar | IN01 a IN06 con responsables y evidencia definidos |
| Solución y código .NET | Pendiente | Creación prevista en E1 |
| Pruebas funcionales de la nueva aplicación | Pendientes | Deben ejecutarse sobre un candidato construido |
| Importación WordPress | Pendiente | Requiere copia autorizada de base y archivos reales y conciliación |
| Infraestructura y despliegue | Pendientes | Proveedor, coste y configuración por decidir |
| Piloto y aceptación operativa | Pendientes | Checklist ejecutado y conciliación de datos |

La revisión documental del paquete se registra en [verificación documental](referencias/verificacion-documental.md). Sus comprobaciones no validan una aplicación todavía no creada.

## Trabajo inmediato

1. Asignar los responsables de producto, técnica, revisión funcional y operación.
2. Cerrar las decisiones de alcance e infraestructura que condicionan E0.
3. Confirmar actividades abiertas en `signed_pdf` y `legacy_form`, volumen y versiones reales del sitio de origen.
4. Preparar datos sintéticos y un receptor de correo aislado para prototipos.
5. Ejecutar IN01 a IN06 y registrar los resultados antes de declarar E0 completada.

El detalle de tareas pertenece al [backlog](planificacion/plan-entregas-y-backlog.md); las decisiones y riesgos al [registro](planificacion/decisiones-y-riesgos.md). Esta página refleja únicamente sus cambios de estado.

## Registro de avances

| Fecha | Entrega o cambio | Candidato y entorno | Resultado | Evidencia | Siguiente acción |
| --- | --- | --- | --- | --- | --- |
| 2026-11-06 | Base y paquete de arranque | Documentación 0.1 | Preparados para revisión | Base incluida y manifiesto del paquete | Resolver decisiones de E0 |
| 2026-11-08 | Refinamiento de arquitectura multitenant SCLM | Documentación 1.0 (Plataforma SCLM) | Incorporada arquitectura multitenant, motor de tematización de pañoleta/colores con WCAG 2.2 AA y desacoplamiento de solución a WebScout.* | Propuesta arquitectónica, ADR10, ADR11 y actualización de visión multigrupo | Prototipar resolución de tenant y tematización en E0 |
| 2026-11-08 | Adopción de Vertical Slice Architecture (ADR12) | Arquitectura técnica | Aprobada transición a Vertical Slices (`Features/`), eliminando la sobre-ingeniería de capas horizontales en el monolito | ADR12, arquitectura-modular.md v1.1 | Crear solución WebScout.slnx y skeleton de Vertical Slices |
| 2026-11-08 | Implementación Paso 1 (Identidad y Tenancy) y Definición Formal MVP | Solución .NET 10 y Documentación v1.1 | Paso 1 completado (16/16 tests pasando). Documentación de dominio modernizada: Censo familiar duradero, Ficha Médica Anual, Mesa de Revisión por Rama y definición formal del MVP para Lestonnac 330. | Commit 95d173e, definicion-mvp-piloto.md | Implementar Paso 2: Dominio de Educandos (ScoutParticipant) y Ficha Médica (ParticipantHealthRecord) |
| 2026-11-08 | Implementación Paso 2: Dominio de Educandos y Ficha Médica Anual | src/WebScout.Web (v0.3.0) y PostgreSQL | Entidades ScoutParticipant y ParticipantHealthRecord, migración EF Core aplicada, aislamiento multitenant estricto verificado, interfaz MudBlazor en /familias/educandos con edición de ficha médica y 21/21 tests pasando. | Migración 20261108211739_AddParticipantsAndHealthRecords, ParticipantHealthRecordTests.cs | Implementar Paso 3: Publicación de Actividades y Campamentos por Ramas |
| 2026-11-08 | Implementación Paso 3: Publicación y Gestión de Actividades por Ramas | src/WebScout.Web (.NET 10) y PostgreSQL | Publicación de campamentos y salidas por ramas pedagógicas, panel de Kraal y Familias, filtrado reactivo y migración EF Core. (28/28 tests pasando). | Issue #21, PR #25, Rama `feat/actividades-campamentos-ramas-3` | Implementar Paso 4: Inscripciones y Justificantes |
| 2026-11-09 | Implementación Paso 4: Inscripciones Familiares y Justificantes Bancarios | src/WebScout.Web (.NET 10) y PostgreSQL | Flujo completo de inscripción familiar, subida y almacenamiento privado de justificantes bancarios, endpoint seguro de streaming y mesa de revisión del Kraal (PA04). (32/32 tests pasando). | Issue #22, PR #26, Rama `feat/inscripciones-justificantes-4` | Implementar Paso 5: Dossier de Campamento y Asistencia |
| 2026-11-09 | Implementación Paso 5: Dossier de Campamento, Salud y Asistencia | src/WebScout.Web (.NET 10) y PostgreSQL | Dossier operativo unificado para scouters, fichero sanitario completo, alertas de intendencia/cocina, hoja de asistencia por secciones y exportación CSV con UTF-8 BOM. (35/35 tests pasando). | Issue #23, PR #27, Rama `feat/dossier-asistencia-campamentos-5` | Implementar Paso 6: Subdominios y SuperAdmin |
| 2026-11-09 | Implementación Paso 6: Subdominios URL, Login Profesional y SuperAdmin | src/WebScout.Web (.NET 10) y PostgreSQL | Multitenencia estricta por subdominio (`lestonnac.localhost`, `sanvicente.localhost`), eliminación de selectores en UI, login profesional contextual y rol SuperAdmin con panel central en `/admin/grupos` y bypass automático en Global Query Filters. (47/47 tests pasando). | Issue #24, PR #28, Rama `feat/subdominios-login-superadmin-6` | Auditoría de Seguridad, Outbox y Protección de Datos |

Cada avance de implementación añadirá commit, configuración, pruebas ejecutadas y enlace a su evidencia. Si cambia el código después de probarlo, la evidencia anterior no se atribuirá al candidato nuevo.

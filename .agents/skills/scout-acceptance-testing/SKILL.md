---
name: scout-acceptance-testing
description: >-
  Use this skill to run, verify, and document the 19 Acceptance Tests (PA01-PA19)
  for WebScout SCLM. Guides synthetic dataset generation, tenant isolation verification,
  security boundary tests, and compliance reporting.
---

# Scout Acceptance Testing (PA01 - PA19)

Esta skill proporciona la guía de ejecución y verificación de la suite de pruebas de aceptación formal (PA01 a PA19) de la plataforma **WebScout SCLM**.

## Matriz de Pruebas de Aceptación

| Código | Objetivo | Requisitos Vinculados | Condición Crítica de Éxito |
| :--- | :--- | :--- | :--- |
| **PA01** | Familia A frente a Familia B | RF03–RF05, RF28, RF35–RF38 | Aislamiento total: La Familia B no puede ver ni acceder a documentos, fichas médicas ni justificantes de la Familia A mediante manipulación de ID o URLs. |
| **PA02** | Revisor de Sección y Revocación | RF04–RF06, RF32, RF34 | Un Scouter solo revisa participantes de su sección asignada. Si su permiso se revoca en sesión activa, la siguiente petición debe ser rechazada inmediatamente (403 Forbidden). |
| **PA03** | Contenido Público de Grupo | RF03, RF18, RF32 | La sección de noticias/blog es pública dentro del grupo, pero no expone respuestas privadas ni listados de menores. |
| **PA04** | Dos Hermanos y Corrección | RF26–RF33 | En una actividad con dos hermanos, un único justificante bancario puede vincularse a ambos, pero la corrección o anulación de la inscripción de uno no invalida al otro. |
| **PA05** | Modelo Sustituido y Plazo | RF24–RF29 | Se rechaza el envío de inscripciones fuera de plazo o con modelos de formulario antiguos sin crear registros huérfanos. |
| **PA06** | Huellas y Archivos Históricos | RF26, RF29, RF31, RF38 | Los documentos guardados conservan su hash SHA-256 inmutable; nuevas versiones añaden un registro histórico sin sobrescribir el original. |
| **PA07** | Pedido Sencillo | RF48–RF54 | Flujo de pedidos simplificado (Core 1.29): entrega de material sin forzar bloqueo financiero rígido; registro de estados trazable. |
| **PA08** | Rectificación Administrativa | RF50, RF52, RF60 | Un administrador puede rectificar estados de pedidos con historial de auditoría inmutable. |
| **PA09** | Concurrencia y Fallo de Red | RF38, RF40, RF44, RNF07 | La subida doble por fallo de conexión no genera inscripciones duplicadas ni corrompe el estado. |
| **PA10** | Carga Masiva de Fotos (100 fotos) | RF41–RF47 | Carga progresiva en álbum de campamento: pausa, reintento ante microcortes y almacenamiento privado en S3/MinIO. |
| **PA11** | Caída de SMTP y Worker Outbox | RF10, RF14, RF40, RNF07 | La transacción de inscripción finaliza con éxito aunque el servidor de correo esté caído; el Outbox reintenta el envío cuando el servicio se restablece. |
| **PA12** | Comentarios y Autorización | RF19–RF23 | Moderación de comentarios por recurso; prevención de filtraciones de datos privados. |
| **PA13** | Consentimiento y Versiones Legales | RF55–RF58 | Consentimiento explícito sin casillas premarcadas; almacenamiento de la versión exacta de la política aceptada. |
| **PA14** | Supresión y Conservación | RF31, RF38, RF59, RF60 | Respeto a periodos de retención legal (ej. justificación de subvenciones); bloqueo de borrado para documentos vinculados a expedientes vigentes. |
| **PA15** | Compatibilidad Histórica | RF15, RF16, RF54, RF68–RF70 | Verificación de datos migrados de WordPress (caso Lestonnac 330) sin pérdida de historial ni autorías. |
| **PA16** | Configuración Visual y Reset | RF61–RF64 | Modificar colores y pañoleta de un grupo no afecta a otros grupos; opción de restaurar a la paleta oficial de SCLM. |
| **PA17** | Accesibilidad y Navegación Teclado | RF66–RF67, RNF02 | Navegación 100% operable por teclado, foco visible, etiquetas ARIA y contraste WCAG 2.2 AA. |
| **PA18** | Aislamiento Multitenant Total | RF05, RF34, RF53, RNF03, RNF08 | Peticiones a `lestonnac.scoutsclm.org` jamás devuelven entidades de `sanvicente.scoutsclm.org` en ninguna circunstancia. |
| **PA19** | Corte, Respaldo y Restauración | RF68–RF71, RNF06 | Copia de seguridad integral de PostgreSQL y MinIO con prueba de restauración exitosa y validación de sumas de verificación. |

## Ejecución de Pruebas Automatizadas

```powershell
# 1. Levantar servicios locales de prueba (PostgreSQL, Mailpit, MinIO)
docker compose -f docker-compose.dev.yml up -d

# 2. Ejecutar toda la batería de tests
dotnet test WebScout.slnx

# 3. Ejecutar exclusivamente los tests de arquitectura (reglas de aislamiento)
dotnet test tests/WebScout.ArchitectureTests

# 4. Ejecutar tests de integración contra PostgreSQL real (migraciones Npgsql, una base clonada por test).
#    Sin variable de entorno, el fixture levanta postgres:17-alpine con Testcontainers (requiere Docker).
#    Para reutilizar el PostgreSQL de docker-compose.dev.yml (puerto 5433):
#    $env:ConnectionStrings__DefaultConnection = "Host=localhost;Port=5433;Database=webscout_dev;Username=scout_admin;Password=<password local>"
dotnet test tests/WebScout.IntegrationTests

# 5. Ejecutar tests de aceptación de multitenancy (PA01, PA18)
dotnet test tests/WebScout.IntegrationTests --filter "FullyQualifiedName~TenantIsolation"
```

## Registro de Evidencia

Tras la ejecución de cualquier batería de pruebas, documentar los resultados en `docs/estado-proyecto.md`:
- Commit SHA probado.
- Entorno (Local Docker, Staging, etc.).
- Batería ejecutada (PA01-PA19 o subconjunto).
- Estado: Pasó, Falló, o Bloqueada (con enlace a la incidencia).

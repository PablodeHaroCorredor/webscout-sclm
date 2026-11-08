# Modelo organizativo y de participantes

Versión 1.0 · Refinamiento multitenant SCLM · 8 de noviembre de 2026.

La organización representa a un **grupo scout federado en Scouts de Castilla-La Mancha (SCLM)** y constituye la frontera estricta de aislamiento (tenant). La sección define una audiencia y un ámbito de responsabilidad dentro de ese grupo. La cuenta autentica a una persona usuaria; el participante identifica a quien corresponde una entrega concreta, sin crear todavía un expediente personal permanente.

| Concepto | Regla en la plataforma multitenant SCLM |
| --- | --- |
| Organización (Tenant) | Grupo scout con identificador único, slug (ej. `lestonnac`), subdominio o dominio propio, identidad visual y zona horaria (Europe/Madrid). |
| Identidad de Grupo | Configuración de pañoleta (fondo, ribete, cinta), paleta de colores con contraste WCAG 2.2 AA, escudo/logo y datos legales del grupo (CIF, sede). |
| Membresía | Vínculo activo de una cuenta con una organización concreta y un perfil funcional; admite múltiples membresías por usuario. |
| Federación SCLM | Ámbito regional asociativo; proporciona aprovisionamiento de tenants y soporte técnico justificado sin acceso ordinario a expedientes. |
| Sección / Rama | Castores, Manada, Sección Scout, Unidad Esculta y Clan Rover; catálogo configurable con nombres propios de cada grupo. |
| Scouters | Audiencia y equipo de responsables del grupo; no es sinónimo del rol técnico de la cuenta. |
| Todo el grupo | Audiencia general implícita de ese grupo; no concede gestión global ni acceso a respuestas de otras familias. |
| Cuenta familiar | Puede actuar en varias secciones del grupo y presentar varios participantes. |
| Participante | Identidad declarada de una respuesta; su nombre no es clave única en la base. |
| Sección responsable | Una o varias asignadas a una respuesta dentro de las permitidas para esa actividad y cuenta. |

## Invariantes

- Todas las relaciones de negocio y consultas se limitan estrictamente a la misma organización mediante filtros globales en base de datos.
- El contexto de organización procede del servidor a través de la resolución del host/subdominio y de la membresía activa del usuario.
- Sin sección específica, la cuenta ve publicaciones generales y sus propios expedientes dentro de su grupo; nunca obtiene alcance global ni de otros grupos.
- El propietario de una respuesta conserva acceso a su propia documentación según el estado de la cuenta y la política de acceso del grupo.
- Un revisor requiere concesión y sección responsable; ver una actividad general no le permite abrir todos sus documentos.
- El soporte asociativo de SCLM requiere una concesión de emergencia temporal (*break-glass*), motivo explícito y registro inmutable en auditoría.
- No se deduce tutela, parentesco o identidad personal por nombre o por compartir una cuenta.

## Decisiones diferidas

El censo permanente unificado de educandos en SCLM, las relaciones de tutela verificadas documentalmente, las rondas solares y la transición automática al perfil adulto exigirán requisitos propios y una migración específica.

Referencias: RF01 a RF06, RF28, RF31 y RF32 en el [catálogo](../requisitos/catalogo-requisitos.md); [visión multitenant SCLM](../evolucion/vision-multigrupo.md); [decisiones de arquitectura](decisiones-arquitectura.md).

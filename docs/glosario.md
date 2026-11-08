# Glosario de Términos del Proyecto Web Scout

**Grupo Scout Lestonnac 330 · Scouts de Castilla-La Mancha (SCLM)**  
Versión 1.1 · Actualización de Arquitectura y Alcance · 8 de noviembre de 2026.

Los términos que figuran a continuación fijan el lenguaje ubicuo (*Ubiquitous Language*) compartido entre la jefatura de grupo, los scouters, las familias y el equipo de ingeniería de software.

---

| Término | Definición y Significado en la Plataforma |
| --- | --- |
| **Organización o Grupo Scout** | Ámbito federado en SCLM (ej. Grupo Scout Lestonnac 330) que delimita la frontera estricta de aislamiento de datos (*Tenant*). |
| **Rama o Sección** | Unidad educativa del grupo dividida por edades: *Castores* (6-8 años), *Manada* (8-11 años), *Tropa* (11-14 años), *Escultas* (14-17 años) y *Clan* (17-21 años). |
| **Cuenta de Usuario** | Identidad digital autenticada mediante ASP.NET Core Identity (`ScoutUser`), perteneciente a una organización concreta. |
| **Familia (Tutor Legal)** | Perfil de usuario que ostenta la patria potestad o tutela de uno o varios educandos censados en el grupo. |
| **Educando** | Miembro infantil o juvenil (`ScoutParticipant`). Registro permanente dependiente de su tutor legal y adscrito a una rama específica del grupo. |
| **Ficha Médica Anual** | Expediente sanitario persistente (`ParticipantHealthRecord`) del educando: CIP/Seguridad Social, alergias, intolerancias, medicación activa, natación y teléfonos de emergencia. |
| **Scouter** | Educador scout voluntario con asignación a una rama concreta y facultades para publicar actividades y revisar inscripciones de su sección. |
| **Kraal** | Conjunto de todos los scouters y responsables del grupo scout. |
| **Coordinación / Secretaría** | Equipo directivo del grupo scout con facultades de supervisión global, aprobación de ingresos y gestión asociativa. |
| **Administración Técnica** | Perfil responsable del mantenimiento, configuración de identidad visual, gestión de roles y auditoría de la plataforma. |
| **Actividad / Campamento** | Evento scout (`ScoutActivity`) convocado para una o varias ramas con fechas civiles, fecha límite de inscripción y cuota económica. |
| **Inscripción** | Expediente formal (`ActivityInscription`) que formaliza la asistencia de un educando a una actividad concreta tras validar salud, autorizaciones y pago. |
| **Mesa de Revisión de Rama** | Bandeja de tramitación donde los scouters de una rama revisan, validan o solicitan subsanaciones para las inscripciones de sus educandos. |
| **Subsanación Requerida** | Estado de una inscripción cuando un scouter detecta un error (ej. justificante ilegible) y requiere que la familia aporte una corrección motivada. |
| **Justificante de Pago** | Comprobante documental de transferencia bancaria aportado por la familia para acreditar el abono de la cuota de una actividad. |
| **Pago Compartido de Hermanos** | Capacidad del sistema para vincular un único justificante bancario de pago agrupado a las inscripciones independientes de varios hermanos. |
| **Informes de Campamento** | Documentos operativos generados automáticamente antes de una salida: lista de asistencia con teléfonos de emergencia, informe de cocina (alergias/dietas) y dossier de botiquín. |
| **Tenant Isolation** | Garantía arquitectónica de que ninguna consulta o mutación pueda traspasar los datos de un grupo scout a otro. |
| **Ronda Solar** | Curso scout anual (habitualmente de octubre a septiembre del año siguiente). |

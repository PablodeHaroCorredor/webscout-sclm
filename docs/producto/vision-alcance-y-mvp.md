# Visión, Alcance y Piloto de la Web Scout

**Grupo Scout Lestonnac 330 · Scouts de Castilla-La Mancha (SCLM)**  
Versión 1.1 · Actualización de Arquitectura y Alcance · 8 de noviembre de 2026.

La aplicación Web Scout permite a familias y scouters gestionar la vida asociativa, sanitaria y documental del grupo desde una experiencia web privada, segura, accesible y adaptada a dispositivos móviles.

La definición operativa detallada y los criterios de aceptación del piloto están formalizados en la [Definición del MVP y Alcance Operativo del Piloto](definicion-mvp-piloto.md).

---

## 1. Usuarios y Necesidades Principales

| Usuario / Rol | Necesidad Principal | Resultado Observable |
| --- | --- | --- |
| **Familia (Tutor Legal)** | Disponer de un censo permanente de sus educandos, mantener su ficha médica al día y autorizar salidas fácilmente. | Cada educando tiene su expediente persistente; una inscripción o subsanación afecta únicamente a ese educando; soporte de justificante compartido para hermanos. |
| **Scouter (Responsable de Rama)** | Convocar actividades de su sección, revisar inscripciones y disponer de información sanitaria inmediata. | Publica actividades segmentadas por rama; revisa inscripciones solo dentro de su sección responsable; genera listas de campamento y resúmenes de cocina al instante. |
| **Coordinación y Secretaría** | Supervisar el censo general, validar autorizaciones globales y controlar altas. | Bandejas con estados claros, filtros por rama y exportaciones operativas con control de acceso. |
| **Administración Técnica** | Mantener accesos, gobernanza multitenant y trazabilidad. | Aislamiento estricto por organización (`Tenant`), auditoría funcional inmutable y copias de seguridad verificables. |
| **Visitante** | Conocer el grupo scout y solicitar información o ingreso. | Portal público y formulario de contacto/solicitud sin creación automática de cuenta. |

---

## 2. Piloto Operativo y Entregas

El piloto operativo se concentra en el **núcleo de identidad, censo familiar, salud y gestión de actividades**:
1. **Identidad y Acceso Multitenant**: Autenticación segura con ASP.NET Core Identity y aislamiento estricto por grupo scout (`OrganizationId`).
2. **Censo Familiar y Ficha Médica Anual**: Expedientes permanentes de educandos (`ScoutParticipant`) vinculados a tutores legales (`ScoutUser`), con ficha médica anual actualizada (`ParticipantHealthRecord`) que recoge alergias, intolerancias, medicación y contactos de emergencia.
3. **Publicación y Gestión de Actividades**: Convocatorias por rama con control de fechas límite, cuotas en céntimos y condiciones de transferencia bancaria.
4. **Flujo de Inscripción y Mesa de Revisión**: Inscripción guiada con confirmación de salud, aceptación legal y justificación de pago con soporte de hermanos; mesa de revisión segmentada por rama con flujo de subsanaciones.
5. **Salidas de Campamento**: Nóminas de asistencia, resúmenes de alérgenos para cocina e informes médicos para botiquín.

---

## 3. Fuera del Alcance Inicial (Exclusiones Explícitas del MVP)

Para asegurar la viabilidad y fiabilidad del lanzamiento con el Grupo Scout Lestonnac 330, las siguientes capacidades quedan excluidas del MVP inicial:
- **Pasarelas de pago telemático directo (Redsys, Stripe, Bizum)**: El pago se acredita mediante justificante documental de transferencia bancaria verificado manualmente.
- **Facturación mercantil y contabilidad de doble partida**: Gestión de cuotas asociativas sin módulo de facturación comercial.
- **Firma biométrica avanzada o validación de certificados DNIe**: Validez basada en autenticación de la cuenta, trazabilidad de sesión (IP, UTC) y aceptación contractual de condiciones.
- **Gestión de inventario y almacén de material de acampada**.
- **Chat o mensajería instantánea interna en tiempo real**.
- **Tienda online de merchandising avanzada con stock en tiempo real**.

---

## 4. Medidas de Aceptación del Piloto

- Una familia registra a dos educandos en ramas distintas y cumplimenta sus fichas sanitarias.
- Un scouter publica una actividad acotada a ramas específicas y con fecha de cierre.
- La familia inscribe a ambos participantes utilizando un único justificante bancario compartido.
- Un revisor de Manada no puede consultar ni validar inscripciones de educandos de Tropa o Escultas.
- Un scouter requiere una subsanación; la familia reemplaza el justificante sin perder el resto del expediente ni afectar al hermano.
- El Kraal exporta en segundos la nómina de teléfonos de emergencia y el desglose de intolerancias y alergias para la cocina del campamento.
- Ninguna operación o consulta permite saltar el aislamiento entre organizaciones multitenant.

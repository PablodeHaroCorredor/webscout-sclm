# Glosario de la web scout

Versión 0.1 · Propuesta de arranque · 6 de noviembre de 2026.

Los términos siguientes fijan el significado del proyecto. Una etiqueta visible puede variar con una decisión editorial; los identificadores de estado y contrato deben permanecer estables.

| Término | Significado |
| --- | --- |
| Organización o grupo | Ámbito Lestonnac 330 del piloto; no una empresa del ERP |
| Sección | Unidad del grupo usada en pertenencia, audiencia y responsabilidad |
| Cuenta | Identidad autenticable; puede presentar varias respuestas por actividad |
| Membresía | Relación activa de una cuenta con la organización y su perfil |
| Familia | Perfil de cuenta; no implica un censo permanente de menores |
| Mayor de edad | Perfil para participante adulto con gestiones propias |
| Participante declarado | Persona indicada en una respuesta, sin identidad permanente deducida |
| Scouter | Responsable con alcance propio y concesiones adicionales explícitas |
| Coordinación | Perfil de gestión global dentro del grupo |
| Administración | Perfil de configuración y facultades sensibles del grupo |
| Capacidad o concesión | Facultad concreta, independiente de otras como publicar o revisar |
| Audiencia | Destinatarios que pueden leer un contenido; no una facultad de gestión |
| Propietario o titular | Cuenta a la que pertenece una gestión concreta |
| Sección responsable | Alcance usado para autorizar revisión de una respuesta |
| Actividad | Evento que puede requerir autorización documental y justificante de actividad |
| Modelo de autorización | PDF que se descarga y firma fuera de la web; sus versiones son inmutables |
| Respuesta o expediente | Entrega de un participante a una actividad, con estado y versiones |
| Recibida | Entrega confirmada pendiente del tratamiento de revisión aplicable |
| Necesita corrección | Estado que permite corregir ese expediente dentro de plazo |
| Firma declarada | Método indicado por quien presenta el PDF; no validación criptográfica |
| Justificante de actividad | Documento exigido si hay coste positivo, compartible solo en misma cuenta/actividad |
| Justificante de pedido | Adjunto opcional, independiente de estado y entrega; no pago verificado |
| Solicitado | Estado inicial de pedido; corresponde al estado técnico pendiente del origen |
| Revisado | Estado de pedido sin aceptación económica; destino de ciertos estados antiguos |
| Pedido al proveedor | Estado operativo del encargo, sin imponer una secuencia obligatoria |
| Entregado | Estado operativo que no exige justificante ni verificación de pago |
| Rechazado o Cancelado | Estados sin nuevas subidas de justificante de pedido |
| Legacy form | Modo de formulario histórico que se conserva si tiene expedientes abiertos |
| Archivo original | Bytes recibidos, conservados sin modificar cuando constituyen evidencia |
| Derivado | Miniatura o versión de presentación; conserva el permiso del original privado |
| Vínculo compartido | Relación explícita de varios expedientes con un objeto; no deduplicación global por hash |
| Idempotencia | Repetir una misma operación confirmada no crea otra gestión o foto |
| Concurrencia | Dos cambios sobre un recurso se detectan por versión, evitando sobrescritura silenciosa |
| Outbox | Intención durable de notificar guardada con la transacción de negocio |
| Conservación | Permanencia justificada por finalidad y responsabilidad; distinta de publicar o archivar |
| Bloqueo | Tratamiento restringido en los términos aplicables; no simple ocultación de UI |
| RPO y RTO | Pérdida temporal máxima objetivo y tiempo objetivo para recuperar servicio |
| RF y RNF | Requisito funcional y no funcional de la base |
| PA | Escenario de aceptación; necesita ejecución y evidencia sobre candidato |
| ADR y DC | Decisión técnica de arquitectura y decisión de arranque/producto |
| P0 y P1 | Prioridad de la regla o función; no estado de implementación |
| E0 a E5 | Entregas propuestas de descubrimiento a piloto |

La equivalencia de nombres técnicos antiguos se registrará en el importador y en su manifiesto; no se inferirá a partir de traducciones de etiquetas. Véanse [modelo de datos](architecture/modelo-datos.md), [ciclo de vida](architecture/politica-ciclo-vida-y-versiones.md) y [migración](migracion/plan-migracion-wordpress.md).

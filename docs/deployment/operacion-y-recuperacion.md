# Operación recuperación y continuidad

Versión 0.1 · Propuesta de arranque · 6 de noviembre de 2026.

El objetivo inicial es disponibilidad interna mensual del 99,5%, RPO de una hora y RTO de cuatro horas. Son objetivos propuestos RNF05 y RNF06, sujetos a aceptación de coste y servicio en DC08; no son un SLA contratado.

## Responsables y observación

Designar titular y suplente de operación, responsable funcional de guardia en ventanas críticas y canal de incidencia. Registrar contactos en un sistema de acceso restringido, sin incluir teléfonos personales ni secretos en este repositorio. Acordar horario de atención y cuándo se escala al proveedor.

| Señal | Detección propuesta | Acción inicial |
| --- | --- | --- |
| Aplicación o recorrido esencial caído | Sonda externa y operación sintética | Revisar alcance, último cambio y dependencias; abrir incidencia |
| Errores o p95 elevados | Métricas por operación y entorno | Correlacionar con candidato, carga y consultas; limitar trabajo no esencial |
| Avisos pendientes antiguos o fallidos | Edad, reintentos y estado de outbox | Comprobar proveedor y worker; reintentar de forma controlada |
| Objetos sin vínculo o vínculo sin objeto | Conciliación de preparación y referencias | Investigar lote; no borrar huérfanos automáticamente sin margen y política |
| Copia fallida o demasiado antigua | Fecha, integridad y verificación de copia | Restaurar cobertura del RPO y registrar exposición |
| Acceso anómalo o filtración | Auditoría minimizada y alertas | Contener acceso, conservar evidencia y activar procedimiento de incidente |

Los umbrales se fijarán con una carga de referencia; no se crea una alerta inútil por cada evento. No registrar nombres de participantes, PDFs, tokens de acceso ni datos de salud en métricas o logs.

## Copias y restauración

Copiar base, objetos originales/derivados necesarios, claves de protección, configuración y manifiesto de versión de forma coordinada. Documentar cifrado, control de acceso, ubicación, retención, rotación y verificación de integridad. Una copia de base sin sus documentos o claves no demuestra recuperación del sistema.

Ensayar en un entorno aislado: elegir punto, restaurar componentes compatibles, impedir correo real, aplicar configuración de ensayo, comprobar login, PDF/versiones/hash, recibo compartido, foto privada, pedido y outbox. Medir pérdida máxima de operaciones y tiempo hasta servicio utilizable; contrastar con RPO/RTO. El ensayo incluye caída del worker y repetición de avisos sin nuevas intenciones duplicadas.

Aplicar las supresiones y bloqueos que deban mantenerse tras restaurar; una copia antigua no puede reabrir indefinidamente datos retirados. El calendario de expiración y las excepciones se enlazan a [conservación](../requisitos/privacidad-y-conservacion.md).

## Procedimientos iniciales

**Correo detenido:** confirmar que el trámite está guardado, inspeccionar edad y error depurado, comprobar credenciales/proveedor y recuperar worker. Reintentar por identificador y vigilar duplicados de proveedor. No volver a crear la gestión de negocio para producir otro aviso.

**Fallo de archivo:** distinguir subida preparada, objeto guardado sin confirmación y vínculo roto. Usar token persistido y manifiesto; verificar hash y permisos. Reparar con operación registrada y no sustituir un original firmado por una imagen reconstruida.

**Despliegue fallido:** cerrar escritura afectada si procede, identificar esquema y candidato, decidir regreso compatible o restauración coordinada. Contabilizar escrituras posteriores al punto de copia antes de volver a WordPress o a una base anterior.

**Incidente de privacidad:** contener el canal, revocar acceso afectado, conservar evidencias mínimas y comunicar internamente al responsable del tratamiento. Este decide evaluación, notificaciones y plazos aplicables conforme al procedimiento ratificado; el repositorio no sustituye ese proceso.

## Rutina propuesta

Revisión automática diaria de copias, colas, capacidad y errores; revisión periódica de permisos privilegiados, versiones soportadas, retención y proveedores; ensayo de recuperación al inicio y tras cambios materiales de persistencia. La frecuencia final y el coste se acuerdan en DC08. Cada ensayo guarda candidato, duración, datos de prueba, resultado y acciones pendientes.

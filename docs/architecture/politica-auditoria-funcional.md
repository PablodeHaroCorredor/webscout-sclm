# Política de auditoría funcional

Versión 0.1 · Propuesta de arranque · 6 de noviembre de 2026.

La auditoría debe explicar quién realizó una operación sensible, cuándo, sobre qué recurso y con qué resultado. El historial de versiones conserva lo recibido; los logs diagnostican fallos técnicos; el evento de auditoría registra la acción de negocio. No se usará una copia completa de cada formulario como auditoría.

## Modelo de evento

| Campo | Uso |
| --- | --- |
| Id y OccurredAtUtc | Identificación e instante del evento |
| OrganizationId | Ámbito resuelto en servidor |
| ActorUserId o ActorService | Cuenta o trabajo que actuó |
| ActionCode | Acción estable, separada del texto mostrado |
| ResourceType y ResourceId | Recurso afectado |
| Result | Success, Denied, Error o Partial según resultado real |
| Origin | Web, API, Migration o Worker |
| CorrelationId y RequestId | Relación con solicitud, lote y reintento |
| FromState y ToState | Cambios de estado cuando proceda |
| ReasonCode o referencia al motivo | Razón necesaria sin duplicar contenido sensible |

## Acciones iniciales

Concesión o revocación de facultades; cambio de secciones; publicación o retirada; sustitución de modelo; entrega o corrección; revisión; cambio de estado de pedido; sustitución de justificante; descarga privada; exportación; importación; eliminación o aplicación de una retención. Los intentos denegados relevantes tendrán campos mínimos y protección frente a inundación de logs.

Un archivo identificado por SHA256 conserva una huella de integridad de sus bytes. Esa huella no convierte el registro de auditoría en un sello cualificado ni demuestra la validez de la firma.

## Integridad y minimización

Las operaciones confirmadas guardan su evento y outbox en la misma transacción local. Un rollback no deja un evento de éxito. Los fallos técnicos de una transacción revertida se registran por correlación en logs; no se fuerza una segunda transacción que invente un resultado funcional.

El rol ordinario de la aplicación no modifica eventos ya registrados. Una identidad administrativa de conservación aplica las políticas aprobadas. El evento no contiene contraseña, token de sesión, cadena de conexión, cuerpo de PDF, datos médicos, coordenadas o IP cruda por defecto. Cualquier motivo de texto libre se limitará y se revisará para no convertirlo en un expediente paralelo.

## Consulta y pruebas

Administración consulta y exporta el registro global con permiso específico, dejando evento de la exportación. Coordinación no obtiene esa capacidad por defecto. Se probará que un cambio sensible confirmado deja un evento, que una operación rechazada no aparece como éxito y que las exportaciones no añaden datos excluidos. RF60 y PA02, PA06, PA08, PA14 y PA19 proporcionan escenarios iniciales.

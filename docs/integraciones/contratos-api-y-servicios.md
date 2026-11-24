# Contratos de API archivos y servicios externos

Versión 0.1 · Propuesta de arranque · 6 de noviembre de 2026.

La API bajo el mismo origen del portal y los formularios HTML llamarán a los mismos casos de uso. La tabla define el contrato de diseño; su implementación y OpenAPI se incorporarán al crear cada módulo.

| Operación | Entrada de negocio | Regla decisiva |
| --- | --- | --- |
| POST /public/contact | Contacto, mensaje y versión legal | Aceptación necesaria separada de comunicaciones opcionales |
| POST /public/join-requests | Participante, nacimiento, contacto y facultad declarada | Solicitud pública no crea cuenta |
| POST /api/v1/communications | Tipo, contenido, audiencia y estado | Publicación y edición se autorizan por separado |
| POST /api/v1/activities/{id}/model-versions | PDF y confirmación | Nueva versión y conservación de anteriores |
| POST /api/v1/activities/{id}/responses | Participante, secciones, modelo y documentos | Plazo, modelo vigente, titular y recibo si la actividad cuesta |
| POST /api/v1/responses/{id}/corrections | Nueva entrega y versión leída | Mismo participante y solo respuesta en corrección |
| POST /api/v1/responses/{id}/review | Estado, motivo y versión leída | Concesión y sección responsable |
| POST /api/v1/albums/{id}/photos | Una foto y token | Confirmación persistida antes de avanzar la cola |
| POST /api/v1/albums/{id}/finalize | Estado y decisión de fin parcial | Publicar requiere al menos una foto |
| POST /api/v1/orders | Producto, talla, cantidad y notas | Instantánea de solicitud sin proceso económico |
| POST /api/v1/orders/{id}/status | Uno de seis estados y versión | Gestión de pedidos; se admiten rectificaciones |
| POST /api/v1/orders/{id}/receipt-versions | Archivo y clave | Titular; no rechazada ni cancelada; estado intacto |
| GET /api/v1/files/{bindingId} | Vínculo opaco | Autorización actual sobre el recurso de negocio |
| GET /api/v1/exports/activity-responses | Filtros | CSV filtrado y restringido al alcance del revisor |
| GET /api/v1/exports/orders | Solicitud de exportación | Todos los pedidos accesibles del grupo, anunciado en la UI |
| PUT /api/v1/me/tours/{id} | Progreso y versión de guía | Solo usuario de sesión y guía permitida |

## Identidad e idempotencia

OrganizationId y OwnerUserId no son entradas de autoridad. El servidor los obtiene del contexto y valida las referencias. If-Match o un campo equivalente representa la versión leída; una edición obsoleta devuelve 412 o conflicto de negocio 409 sin sobrescribir.

Idempotency-Key tiene ámbito de organización, actor y operación, junto con hash normalizado de datos y archivos. Repetir clave y contenido devuelve la operación original, sin un nuevo archivo ni respuesta. La misma clave con otro contenido produce conflicto. La autorización actual también se comprueba al recuperar el resultado previo.

## Subida preparada

La operación lógica puede recibirse por multipart o mediante UploadId privado de preparación; E0 validará el transporte con el proxy elegido. En ambos casos el contrato de negocio es el mismo y la preparación nunca es una URL pública elegida por el cliente.

La aplicación distingue recibido, comprobando y confirmado. Un 202 devuelve OperationId consultable si falta comprobación; un 201 final solo identifica el recurso ya confirmado. La lista no muestra documento disponible por tener una ruta o un nombre sin bytes validados. Un fallo conserva el adjunto activo anterior y permite reintento con la misma intención.

## Errores

ProblemDetails incluirá código estable, descripción útil, errores de campo y correlación. Se contemplan `model_changed`, `deadline_closed`, `invalid_file`, `stale_version`, `operation_pending` y `session_expired`. JSON usa 401 para sesión perdida sin devolver una página de login dentro de la respuesta de subida. 413 identifica tamaño, 429 frecuencia y 503 indisponibilidad recuperable. Se aplicará una política coherente de 404/403 que no revele datos ajenos.

## Correo y trabajos

`IEmailSender` se implementará con el proveedor seleccionado en DC03. La outbox guarda el aviso con el trámite y un worker lo procesa con reintentos. Avisos de documentos y pedidos contienen referencia y enlace al portal, sin adjuntos ni datos bancarios. Contacto conserva su finalidad de entregar el mensaje al buzón configurado; su retención temporal en la outbox se incluye en privacidad.

Se garantiza persistencia de la intención y reintento, no entrega exactamente una vez por SMTP. Una caída después de enviar puede repetir el aviso; se usará identificador de mensaje y deduplicación si el proveedor la ofrece. Un correo fallido no cambia el trámite a no recibido ni duplica el expediente.

Los puertos `IPrivateFileStore`, `IPublicAssetStore`, `IBackgroundJobStore`, `IAuthorizationPolicy` e `IClock` permiten sustituir infraestructura y probar errores.

# Catálogo de requisitos funcionales y no funcionales

Versión 0.1 · Propuesta de arranque · 6 de noviembre de 2026.

Este catálogo transcribe los requisitos y criterios del documento base sin alterar su alcance. Se conserva la prioridad P0/P1 y la naturaleza indicada en la base. La aprobación de esta propuesta sigue pendiente. El [original íntegro](base-requisitos-arquitectura-v0.1.docx) aporta el contexto de análisis y las matrices originales.

Cada tarea cita identificadores RF/RNF; las políticas desarrollan cómo aplicarlos. Los criterios siguientes no equivalen a pruebas ejecutadas. La [estrategia de validación](../testing/estrategia-y-trazabilidad.md) conserva los PA y el [backlog](../planificacion/plan-entregas-y-backlog.md) cubre todos los RF.

Resumen: 71 RF, con 40 P0 y 31 P1; 12 RNF. La naturaleza distingue paridad, endurecimiento propuesto y otros matices de la base. Cualquier cambio aprobado deberá versionar este catálogo y actualizar trazabilidad y decisiones; el DOCX histórico se conserva.

## Cuentas perfiles y acceso

La cuenta autentica a una persona usuaria. En la línea base, una cuenta familiar puede presentar documentación de varios participantes sin crear un censo permanente de hijos.

### RF01 Alta administrada de cuentas

Prioridad: **P0**. Naturaleza: Paridad.

El sistema permitirá crear, activar, desactivar y recuperar cuentas mediante administración. El acceso público servirá para solicitar ingreso; no generará una cuenta automáticamente.

**Aceptación:** Una solicitud aceptada no permite iniciar sesión hasta que una persona autorizada cree o invite la cuenta. Una cuenta desactivada deja de acceder a datos privados.

Fuente en la base: C01 153–237; C05 274–297; D10 Crear las cuentas. Véase el [inventario de fuentes](../referencias/fuentes.md).

### RF02 Perfiles de usuario

Prioridad: **P0**. Naturaleza: Paridad.

Se distinguirán Administración, Coordinación, Scouter, Familia y Educando mayor de edad. Las capacidades de publicación, revisión, pedidos y configuración se evaluarán de forma independiente.

**Aceptación:** La matriz de permisos de la sección 3 se verifica en servidor y produce el mismo resultado al invocar directamente la operación.

Fuente en la base: C01 153–237, 294–381. Véase el [inventario de fuentes](../referencias/fuentes.md).

### RF03 Asignación de varias secciones

Prioridad: **P0**. Naturaleza: Paridad.

Una cuenta podrá tener varias secciones. Todo el grupo será una audiencia general implícita; carecer de sección específica no concederá acceso global.

**Aceptación:** Una familia asignada a Manada y Castores ve ambas secciones y el contenido general, y no ve Clan Rover. Sin sección solo ve contenido general.

Fuente en la base: C03 6–30; C14 397–425. Véase el [inventario de fuentes](../referencias/fuentes.md).

### RF04 Concesiones individuales

Prioridad: **P0**. Naturaleza: Paridad.

Administración podrá conceder y revocar a Scouters revisión de documentos, publicación pública directa y gestión de pedidos. El último permiso abarca los pedidos del grupo completo.

**Aceptación:** Revocar revisión impide abrir documentos de otras cuentas en la siguiente operación. Conceder pedidos no abre solicitudes de ingreso ni autorizaciones.

Fuente en la base: C01 177–216; C08 17–20, 123–132; C14 415–425. Véase el [inventario de fuentes](../referencias/fuentes.md).

### RF05 Acceso según contexto

Prioridad: **P0**. Naturaleza: Paridad.

Lectura y descarga aplicarán una política única que considere organización, usuario activo, propiedad, tipo de recurso, publicación, audiencia y capacidad específica.

**Aceptación:** Cambiar un identificador de URL a un documento de otra familia no devuelve contenido ni metadatos del archivo, aunque exista una sesión válida.

Fuente en la base: C03 18–58; C10 211–258. Véase el [inventario de fuentes](../referencias/fuentes.md).

### RF06 Edición del contenido propio

Prioridad: **P0**. Naturaleza: Paridad.

El Scouter editará su contenido privado. Coordinación y Administración podrán gestionar el conjunto. La lectura por sección no concederá edición de publicaciones ajenas.

**Aceptación:** Un Scouter puede editar su álbum y recibe denegación al modificar un álbum ajeno de la misma sección. Coordinación puede modificar ambos.

Fuente en la base: C07 1080–1089. Véase el [inventario de fuentes](../referencias/fuentes.md).

### RF07 Perfil personal

Prioridad: **P1**. Naturaleza: Paridad.

La persona usuaria actualizará nombre, apellidos, nombre visible, teléfono y fotografía privada. Los cambios de correo, perfil y secciones se tramitarán mediante administración.

**Aceptación:** Modificar el teléfono conserva roles y secciones. La foto no es accesible sin sesión; se acepta JPG, PNG o WebP con un máximo inicial de 2 MB.

Fuente en la base: C04 536–569; C10 261–305. Véase el [inventario de fuentes](../referencias/fuentes.md).

### RF08 Acceso y recuperación de sesión

Prioridad: **P0**. Naturaleza: Paridad y mejora.

Se ofrecerán inicio y cierre de sesión, recuperación de contraseña e invitación de un solo uso. Se mostrará el aviso de confidencialidad y el login respetará la identidad visual.

**Aceptación:** Un token usado o caducado no restablece la contraseña; una subida con sesión caducada conserva las fotos confirmadas y permite recuperar el acceso.

Fuente en la base: C04 230–243; C17 98–105; W04. Véase el [inventario de fuentes](../referencias/fuentes.md).

### RF09 Resumen y navegación por permisos

Prioridad: **P1**. Naturaleza: Paridad.

El área privada reunirá Resumen, Comunicaciones, Fotos, Mis gestiones, Productos y Mis datos. La gestión y configuración se mostrarán según capacidades.

**Aceptación:** Los contadores y próximos plazos se calculan sobre recursos accesibles. La navegación y sus enlaces funcionan con teclado y en pantalla de 320 px.

Fuente en la base: C04 29–126, 162–204, 244–294. Véase el [inventario de fuentes](../referencias/fuentes.md).

## Contacto ingreso y registros históricos

Contacto y solicitud de ingreso son procesos públicos diferentes. El plugin envía Contacto por correo y conserva la solicitud de ingreso como registro privado.

### RF10 Formulario de contacto

Prioridad: **P1**. Naturaleza: Paridad.

Se recogerán nombre, email, asunto, mensaje y teléfono opcional, con información de privacidad y elección separada de comunicaciones no esenciales.

**Aceptación:** Sin aceptar el tratamiento necesario no se envía; con la opción comercial desmarcada se acepta. Un fallo del proveedor no se presenta como correo entregado.

Fuente en la base: C05 36–58, 257–272; C17 69–96. Véase el [inventario de fuentes](../referencias/fuentes.md).

### RF11 Solicitud pública de ingreso

Prioridad: **P0**. Naturaleza: Paridad.

Se recogerán participante, nacimiento, persona de contacto, relación, email, teléfono y observaciones, con declaración de facultad para facilitar los datos. No se pedirán DNI ni salud en esta fase.

**Aceptación:** Una fecha futura o una relación no permitida se rechaza. La opción de solicitante por sí mismo exige mayoría de edad según la fecha de negocio.

Fuente en la base: C05 60–88, 274–297, 578–594. Véase el [inventario de fuentes](../referencias/fuentes.md).

### RF12 Seguimiento de solicitudes

Prioridad: **P0**. Naturaleza: Paridad.

Coordinación y Administración gestionarán solicitudes con los estados Pendiente, Contactado, Lista de espera, Aceptado y Rechazado, conservando el registro y su contexto legal.

**Aceptación:** Un Scouter ordinario no consulta la bandeja. Cambiar a Lista de espera registra autor y fecha sin crear una inscripción o una cuenta.

Fuente en la base: C07 421–444, 858–872, 1093–1109. Véase el [inventario de fuentes](../referencias/fuentes.md).

### RF13 Protección de formularios públicos

Prioridad: **P0**. Naturaleza: Paridad y mejora.

Los formularios incorporarán protección CSRF, límites de longitud, validación de servidor, señuelo antispam y limitación de frecuencia configurable.

**Aceptación:** Una petición sin protección válida se rechaza. El límite de frecuencia devuelve un resultado recuperable y no incorpora datos recibidos a un registro privado.

Fuente en la base: C05 559–565; W07. Véase el [inventario de fuentes](../referencias/fuentes.md).

### RF14 Avisos de recepción

Prioridad: **P0**. Naturaleza: Paridad y mejora.

Las nuevas solicitudes y documentos generarán avisos al equipo destinatario sin adjuntar archivos privados. El estado de notificación se distinguirá del estado del trámite.

**Aceptación:** Tras guardar una solicitud, un fallo de correo conserva el registro y deja el aviso pendiente o fallido para reintento. No se duplica el trámite al reintentar.

Fuente en la base: C05 297, 465, 600; C08 94–109. Véase el [inventario de fuentes](../referencias/fuentes.md).

### RF15 Consulta de altas anteriores

Prioridad: **P1**. Naturaleza: Compatibilidad.

Se conservarán las altas generales históricas, sus campos, consentimientos, evidencias de aceptación y PDF. Se consultarán separadas de las autorizaciones de actividad.

**Aceptación:** Una ficha antigua mantiene datos y etiquetas aunque la plantilla actual cambie. No aparece un formulario de reinscripción general a todas las familias.

Fuente en la base: C05 90–130, 299–311; C07 15–17; D10 Alta al grupo. Véase el [inventario de fuentes](../referencias/fuentes.md).

### RF16 Plantillas de formulario y compatibilidad

Prioridad: **P1**. Naturaleza: Compatibilidad.

Se importarán y mantendrán las plantillas anteriores, con creación, edición, orden y duplicación de campos. Las actividades abiertas en modo legacy_form conservarán la captura mediante adaptador; las nuevas autorizaciones priorizarán PDF.

**Aceptación:** Se admiten los 11 tipos existentes, hasta 40 campos y 30 opciones por campo de selección. Modificar la plantilla no altera etiquetas, opciones ni respuestas anteriores.

Fuente en la base: C15 95–117, 189–265, 318–397; D10 Compatibilidad. Véase el [inventario de fuentes](../referencias/fuentes.md).

## Comunicaciones y publicación

Un creador común distinguirá noticia pública, aviso privado, actividad informativa y actividad con autorización. La exposición pública deberá ser una decisión explícita.

### RF17 Editor de comunicaciones

Prioridad: **P0**. Naturaleza: Paridad.

Se crearán borradores con título, contenido enriquecido, destinatarios y estado. El sistema permitirá editar, publicar, archivar y enviar a papelera según tipo y permiso.

**Aceptación:** Archivar retira la publicación del consumo habitual y conserva sus documentos y respuestas. Una actualización inválida no deja un contenido publicado a medias.

Fuente en la base: C07 161–232, 725–776, 942–968. Véase el [inventario de fuentes](../referencias/fuentes.md).

### RF18 Audiencias privadas

Prioridad: **P0**. Naturaleza: Paridad.

Avisos, actividades y álbumes podrán destinarse a una o varias secciones o a todo el grupo. La audiencia se aplicará también a imágenes y adjuntos.

**Aceptación:** Un cambio de sección modifica el acceso al contenido y a sus miniaturas en la siguiente petición, sin depender de ocultar tarjetas en la interfaz.

Fuente en la base: C03 18–30; C07 679–684. Véase el [inventario de fuentes](../referencias/fuentes.md).

### RF19 Conversación en avisos

Prioridad: **P1**. Naturaleza: Paridad y mejora.

Los avisos privados admitirán respuestas de usuarios autorizados, con escape del contenido y moderación administrativa. La política se aplicará también a la escritura del comentario.

**Aceptación:** Una cuenta que no puede leer el aviso tampoco puede comentar invocando el endpoint. No se publican conversaciones en feeds ni búsquedas públicas.

Fuente en la base: C04 22–27, 403–414. Véase el [inventario de fuentes](../referencias/fuentes.md).

### RF20 Imágenes y adjuntos en comunicaciones

Prioridad: **P1**. Naturaleza: Paridad.

El editor insertará imágenes entre párrafos con texto alternativo y pie opcional. Avisos y actividades tendrán imagen principal; PDF y adjuntos antiguos seguirán accesibles.

**Aceptación:** Reabrir y guardar no anida imágenes ni persiste URLs temporales. Una imagen de otra comunicación no se puede vincular sin una operación autorizada.

Fuente en la base: C13 21–81, 88–127; D03. Véase el [inventario de fuentes](../referencias/fuentes.md).

### RF21 Publicación pública revisada

Prioridad: **P1**. Naturaleza: Paridad.

Scouters redactarán borradores y los enviarán a revisión; publicar directamente exigirá capacidad. Se validarán contenido, resumen, categoría pública, imagen, texto alternativo y revisión de privacidad.

**Aceptación:** Sin uno de esos elementos la noticia permanece sin publicar. Un permiso de publicación directa no concede automáticamente edición de noticias públicas ajenas.

Fuente en la base: C07 778–812, 970–978, 1083–1089; D07. Véase el [inventario de fuentes](../referencias/fuentes.md).

### RF22 Copia de privado a público

Prioridad: **P1**. Naturaleza: Paridad.

Se creará un borrador público independiente desde un aviso privado, con referencia interna al origen y sin documentos ni imágenes privados.

**Aceptación:** La copia no publica nada y no contiene referencias de medios privados. Repetir la acción devuelve la copia existente mientras siga válida.

Fuente en la base: C07 1027–1039; C13 61–63; D07. Véase el [inventario de fuentes](../referencias/fuentes.md).

### RF23 Listado y destaque de noticias públicas

Prioridad: **P1**. Naturaleza: Paridad y mejora.

La web pública dispondrá de noticias por categorías, paginación y noticia destacada. Las categorías internas y recursos privados se excluirán de listados, búsquedas, sitemap y feeds.

**Aceptación:** Un recurso privado no aparece ni por su URL directa ni mediante APIs públicas. Una noticia retirada deja de estar destacada y se invalida la caché pública.

Fuente en la base: C26; T01 functions.php 324–393; D10 Contenido público. Véase el [inventario de fuentes](../referencias/fuentes.md).

## Actividades y autorizaciones

El flujo preferente exige firmar el PDF fuera de la plataforma. La elección de certificado es una declaración del usuario; no demuestra una validación criptográfica de la firma.

### RF24 Datos y modalidad de actividad

Prioridad: **P0**. Naturaleza: Paridad y mejora.

La actividad tendrá fechas, lugar, plazo, precio informativo y modalidad Informativa o PDF firmado, preservando el modo histórico. Un precio mayor que cero exige PDF y justificante de actividad.

**Aceptación:** No se publica una actividad que exige PDF sin modelo vigente. Las fechas imposibles o un final anterior al inicio se rechazan con mensajes de campo.

Fuente en la base: C07 814–829, 849–855, 979–981; C05 500–531. Véase el [inventario de fuentes](../referencias/fuentes.md).

### RF25 Vigencia del plazo

Prioridad: **P0**. Naturaleza: Paridad.

Las entregas y correcciones se admitirán hasta el fin del día de plazo en la zona horaria configurada. Los registros seguirán consultables después.

**Aceptación:** En Europe/Madrid, el día de cierre acepta entregas y el día siguiente las rechaza. El resultado se mantiene al cruzar el cambio de horario de verano.

Fuente en la base: C05 132–184, 313–324; D02. Véase el [inventario de fuentes](../referencias/fuentes.md).

### RF26 Modelos PDF con versiones

Prioridad: **P0**. Naturaleza: Paridad.

Cada sustitución creará una versión inmutable del modelo, previa confirmación explícita. Se conservarán las versiones anteriores, la actividad y todas sus respuestas.

**Aceptación:** Cambiar el modelo no invalida las autorizaciones existentes ni exige una nueva firma automáticamente. El modelo vigente se identifica sin depender de una posición en una lista.

Fuente en la base: C06 39–66; C07 849–855; D02. Véase el [inventario de fuentes](../referencias/fuentes.md).

### RF27 Control del modelo abierto

Prioridad: **P0**. Naturaleza: Paridad.

La entrega indicará la versión de modelo que se mostró. Si ya no es vigente, se pedirá descargar el modelo actual antes de presentar el PDF.

**Aceptación:** Un formulario abierto con la versión 2 no se acepta después de publicar la versión 3 y no crea un expediente parcial.

Fuente en la base: C05 322–325; D02. Véase el [inventario de fuentes](../referencias/fuentes.md).

### RF28 Autorizaciones independientes por participante

Prioridad: **P0**. Naturaleza: Paridad.

Una cuenta podrá enviar varias autorizaciones a la misma actividad, cada una con identidad declarada del participante, PDF propio y secciones responsables.

**Aceptación:** Dos hermanos de la misma sección generan dos registros. El nombre no actúa como clave única; dos altas intencionadas con distinto identificador de petición son independientes.

Fuente en la base: C05 384–466; C06 11–34; D02. Véase el [inventario de fuentes](../referencias/fuentes.md).

### RF29 Entrega de documentos firmados

Prioridad: **P0**. Naturaleza: Paridad.

Se exigirá un PDF y la confirmación de que corresponde a la actividad y contiene la firma necesaria. Se registrarán método declarado, usuario, fecha UTC, modelo y huella SHA256.

**Aceptación:** Un JPG presentado como autorización se rechaza. El PDF aceptado conserva exactamente sus bytes; la interfaz no lo etiqueta como certificado validado.

Fuente en la base: C05 185–213, 384–466; C10 67–132. Véase el [inventario de fuentes](../referencias/fuentes.md).

### RF30 Justificante de actividad

Prioridad: **P0**. Naturaleza: Paridad.

Cuando el precio de la actividad sea positivo se exigirá justificante PDF, JPG, PNG o WebP, o referencia a un justificante compartido válido. Su recepción no acredita un pago comprobado.

**Aceptación:** Una actividad con coste no acepta una entrega sin justificante. Una actividad gratuita no obliga a subirlo. Esto es independiente del flujo sencillo de pedidos.

Fuente en la base: C05 384–409, 432–462, 500–502. Véase el [inventario de fuentes](../referencias/fuentes.md).

### RF31 Justificante compartido entre participantes

Prioridad: **P0**. Naturaleza: Paridad.

Se permitirá reutilizar un justificante únicamente entre respuestas de la misma cuenta y actividad, con referencia común y un único objeto físico.

**Aceptación:** Otra cuenta o actividad no puede seleccionar ni descargar el justificante. Borrar una referencia conserva el archivo mientras otra referencia vigente o histórica lo necesite.

Fuente en la base: C06 35–42; C05 412–433; C10 340–357. Véase el [inventario de fuentes](../referencias/fuentes.md).

### RF32 Revisión documental por alcance

Prioridad: **P0**. Naturaleza: Paridad.

Administración y Coordinación revisarán todas las respuestas. El Scouter con concesión individual revisará solo respuestas asignadas a sus secciones, aunque no sea autor de la actividad.

**Aceptación:** Todo el grupo no abre por sí mismo las respuestas privadas generales sin sección responsable. Los registros antiguos sin alcance suficiente quedan para revisión global.

Fuente en la base: C01 354–373; C03 33–47; D06. Véase el [inventario de fuentes](../referencias/fuentes.md).

### RF33 Estados y corrección individual

Prioridad: **P0**. Naturaleza: Paridad y mejora.

Se distinguirán Recibida, Revisada, Necesita corrección, Validada y Rechazada. Corregir una respuesta conservará participante y versiones anteriores, y devolverá solo esa respuesta a Recibida.

**Aceptación:** Pedir corrección a un hermano no cambia al otro. La versión corregida conserva evidencia y permite consultar el archivo sustituido. Se propone exigir motivo al solicitar corrección.

Fuente en la base: C06 15–19; C05 386–391, 439–459; C07 1093–1109. Véase el [inventario de fuentes](../referencias/fuentes.md).

### RF34 Bandeja y exportación de respuestas

Prioridad: **P0**. Naturaleza: Paridad.

La bandeja filtrará por actividad, sección, estado, plazo y presencia documental, con acciones individuales y por lote. El CSV incluirá método, modelo, referencia compartida y estados, sin archivos.

**Aceptación:** El CSV respeta el alcance del revisor y los filtros anunciados. Una acción por lote devuelve resultado por registro, sin actualizar elementos fuera de permiso.

Fuente en la base: C07 446–615, 874–935. Véase el [inventario de fuentes](../referencias/fuentes.md).

## Archivos privados y conservación técnica

Los archivos son un recurso del dominio con autorización propia. Original, miniaturas e históricos deben mantener el mismo ámbito de privacidad.

### RF35 Almacenes público y privado separados

Prioridad: **P0**. Naturaleza: Paridad y mejora.

Los archivos privados se guardarán fuera de la raíz pública o en un contenedor privado. Ningún nombre, ruta física o miniatura concederá acceso directo anónimo.

**Aceptación:** Pedir original, miniatura o versión anterior sin autorización devuelve denegación; el servidor estático no sirve el directorio privado.

Fuente en la base: C10 16–43, 211–258; W07. Véase el [inventario de fuentes](../referencias/fuentes.md).

### RF36 Validación y límites de subida

Prioridad: **P0**. Naturaleza: Paridad y mejora.

El servidor validará formato real, extensión, tamaño y finalidad. El límite inicial será 8 MB configurable entre 1 y 20 MB por archivo, mostrando el menor límite de aplicación y proxy.

**Aceptación:** Una extensión permitida con contenido incompatible se rechaza. El cliente recibe una causa útil para tamaño, formato, sesión, cuota o fallo temporal.

Fuente en la base: C10 21–25, 67–132; C11 6–13. Véase el [inventario de fuentes](../referencias/fuentes.md).

### RF37 Acceso y descarga controlados

Prioridad: **P0**. Naturaleza: Paridad.

Visualización y descarga pasarán por autorización actual, cabeceras privadas y registro mínimo de eventos. El enlace no sustituirá la comprobación de permiso.

**Aceptación:** Un enlace antes válido deja de funcionar para un revisor revocado. Se devuelven no-store, noindex y nosniff en archivos privados y no se emiten URLs públicas.

Fuente en la base: C03 49–58; C10 211–258; C20 42–61. Véase el [inventario de fuentes](../referencias/fuentes.md).

### RF38 Versiones e integridad

Prioridad: **P0**. Naturaleza: Paridad y mejora.

Sustituir un documento creará una versión con huella, autor y fecha, conservando las anteriores. La eliminación física dependerá de todas las referencias y de la política de conservación.

**Aceptación:** Una sustitución fallida mantiene el documento activo anterior. Dos correcciones concurrentes no sobrescriben silenciosamente una versión.

Fuente en la base: C05 439–465; C08 73–99; C10 340–367. Véase el [inventario de fuentes](../referencias/fuentes.md).

### RF39 Previsualizaciones protegidas

Prioridad: **P1**. Naturaleza: Paridad.

Las fotografías tendrán una previsualización de hasta 960 px en su lado mayor, sin modificar el original almacenado. Se regenerarán las históricas mediante trabajos limitados.

**Aceptación:** Una cuadrícula pide miniaturas y el visor pide el original. El trabajo interrumpido continúa sin duplicarlas y conserva los mismos permisos.

Fuente en la base: C10 307–338; C22 55–76; D09. Véase el [inventario de fuentes](../referencias/fuentes.md).

### RF40 Gestión de archivos incompletos

Prioridad: **P0**. Naturaleza: Mejora.

Las subidas pasarán por preparación, comprobación y asociación. Se recuperarán fallos entre almacenamiento y base de datos, y se limpiarán objetos huérfanos tras el plazo técnico acordado.

**Aceptación:** Simular una caída después de subir y antes de asociar permite reintentar o limpiar el objeto; no crea una respuesta válida sin documento ni elimina un archivo referenciado.

Fuente en la base: C05 399–424; C11 32–50; W07. Véase el [inventario de fuentes](../referencias/fuentes.md).

## Biblioteca y subida de fotografías

La robustez móvil se basa en una petición por foto y confirmación del servidor. El lote pendiente permanece en memoria en la versión actual.

### RF41 Carpetas y álbumes

Prioridad: **P1**. Naturaleza: Paridad.

Se gestionarán carpetas jerárquicas, álbumes, títulos, destinatarios y publicación, con navegación de ruta y recuentos limitados al contenido visible.

**Aceptación:** Mover o archivar un álbum conserva fotos. Una carpeta no revela nombres ni recuentos de álbumes inaccesibles a la cuenta.

Fuente en la base: C02 11–36; C04 432–505; C07 77–157, 992–999. Véase el [inventario de fuentes](../referencias/fuentes.md).

### RF42 Cola de fotografías

Prioridad: **P1**. Naturaleza: Paridad.

Se acumularán selecciones y arrastrados hasta 100 fotos por lote. La interfaz explicará las excluidas y evitará repetir en la cola una misma selección por nombre, tamaño y fecha.

**Aceptación:** Seleccionar 101 fotos añade como máximo 100 e informa de la restante. Repetir la selección no duplica la cola; no se promete detección visual de duplicados.

Fuente en la base: C12; D04 Lotes. Véase el [inventario de fuentes](../referencias/fuentes.md).

### RF43 Envío individual y progreso real

Prioridad: **P1**. Naturaleza: Paridad.

Cada fotografía se enviará de forma independiente. Se distinguirá progreso de transporte, procesamiento y confirmación, con resumen de guardadas, pendientes y fallidas.

**Aceptación:** El 100 por ciento de transmisión no marca Guardada hasta confirmación. Un error en una foto conserva las anteriores y no descarta todo el lote.

Fuente en la base: C11 32–55; C12; D04. Véase el [inventario de fuentes](../referencias/fuentes.md).

### RF44 Reintento sin duplicación

Prioridad: **P0**. Naturaleza: Paridad y mejora.

Cada foto tendrá un identificador estable durante el lote y una confirmación persistente en servidor. Los reintentos de una operación ya guardada devolverán el mismo resultado.

**Aceptación:** Perder la respuesta tras guardar y reintentar no duplica foto ni archivo. Reutilizar la clave con contenido distinto produce conflicto.

Fuente en la base: C11 32–50; C12; D04. Véase el [inventario de fuentes](../referencias/fuentes.md).

### RF45 Pausa y finalización parcial

Prioridad: **P1**. Naturaleza: Paridad.

Se permitirá pausar tras la foto actual, reintentar las fallidas y terminar explícitamente con las guardadas. El cliente avisará antes de abandonar una cola pendiente.

**Aceptación:** Cerrar la página conserva fotos confirmadas; volver no promete recuperar la cola local. Finalizar parcialmente respeta título, destinatarios y estado elegidos.

Fuente en la base: C07 661–677; C12; D04. Véase el [inventario de fuentes](../referencias/fuentes.md).

### RF46 Optimización y formatos de fotos

Prioridad: **P1**. Naturaleza: Paridad.

Se admitirán JPG, PNG y WebP, con optimización opcional en dispositivo a 2560 px y calidad orientativa 0,85 en JPEG y WebP. Se procesará una foto cada vez.

**Aceptación:** La optimización no modifica el archivo del dispositivo. HEIC o HEIF muestran necesidad de conversión; el servidor valida igualmente el resultado.

Fuente en la base: C12 1–50; D04 Formatos. Véase el [inventario de fuentes](../referencias/fuentes.md).

### RF47 Visibilidad durante los lotes

Prioridad: **P1**. Naturaleza: Paridad.

Un álbum nuevo seguirá como borrador hasta finalizar y exigirá al menos una foto para publicarse. Las fotos confirmadas de un álbum ya publicado podrán verse durante la subida.

**Aceptación:** El equipo conoce que la finalización de un lote no es una publicación atómica de todas las fotos. El fallo de una foto no retira las ya visibles.

Fuente en la base: C07 616–677, 986–989; D04. Véase el [inventario de fuentes](../referencias/fuentes.md).

## Catálogo y pedidos internos

La línea base es 1.29. El justificante es un adjunto independiente del estado del encargo y no existe un proceso de aceptación económica o confirmación de pago.

### RF48 Catálogo interno

Prioridad: **P1**. Naturaleza: Paridad.

Los responsables crearán productos con imagen, descripción, precio informativo y tallas permitidas. Se publicarán, archivarán y editarán mediante permisos de contenido.

**Aceptación:** Un producto sin imagen, precio o tallas no se publica. Cambiar su precio no modifica las solicitudes ya recibidas.

Fuente en la base: C07 831–847, 982–985. Véase el [inventario de fuentes](../referencias/fuentes.md).

### RF49 Solicitud de producto

Prioridad: **P1**. Naturaleza: Paridad.

La cuenta solicitará un producto publicado, una talla válida y entre 1 y 10 unidades, con notas. Se conservará una instantánea de producto, precio unitario y total informativo.

**Aceptación:** Una talla inventada o una cantidad de 11 se rechaza. La ficha histórica mantiene el precio de la solicitud aunque el catálogo cambie.

Fuente en la base: C05 468–482. Véase el [inventario de fuentes](../referencias/fuentes.md).

### RF50 Estados sencillos de pedido

Prioridad: **P1**. Naturaleza: Paridad.

El responsable elegirá Solicitado, Revisado, Pedido al proveedor, Entregado, Rechazado o Cancelado. Entregar no exigirá justificante ni comprobación de pago.

**Aceptación:** La operación de gestión admite cualquiera de los seis estados, incluso una rectificación o reapertura, y conserva historial. No se impone una secuencia económica heredada.

Fuente en la base: C08 29–32, 56–99; D01. Véase el [inventario de fuentes](../referencias/fuentes.md).

### RF51 Justificante adjunto al pedido

Prioridad: **P1**. Naturaleza: Paridad.

El propietario podrá subir, consultar, descargar y sustituir un justificante desde Solicitado, sin aceptación previa. No se admitirán nuevas subidas en Rechazado o Cancelado.

**Aceptación:** Subir un justificante no cambia el estado. Un pedido Entregado puede conservar o recibir adjunto mientras no esté rechazado o cancelado; la versión anterior sigue disponible.

Fuente en la base: C08 56–80; C09; D01. Véase el [inventario de fuentes](../referencias/fuentes.md).

### RF52 Cancelación por el solicitante

Prioridad: **P1**. Naturaleza: Paridad.

El propietario podrá cancelar un pedido Solicitado o Revisado indicando motivo. En Pedido al proveedor o Entregado se le dirigirá al equipo responsable.

**Aceptación:** La cancelación sin motivo se rechaza. El responsable mantiene su operación de gestión de estados con auditoría, independiente de las restricciones del solicitante.

Fuente en la base: C08 64–85; D01. Véase el [inventario de fuentes](../referencias/fuentes.md).

### RF53 Bandeja y exportación de pedidos

Prioridad: **P1**. Naturaleza: Paridad.

Los gestores verán filtros, ficha, historial e indicador de justificante. La exportación completa incluirá referencia, producto, talla, cantidad, precio informativo, estado y solicitante.

**Aceptación:** La interfaz indica que el CSV abarca todos los pedidos y no solo el filtro. No exporta datos bancarios, archivos, enlaces privados ni supuestas comprobaciones de pago.

Fuente en la base: C08 134–146; C09; D01. Véase el [inventario de fuentes](../referencias/fuentes.md).

### RF54 Compatibilidad con pedidos 1 28

Prioridad: **P0**. Naturaleza: Compatibilidad.

Aceptado y Justificante recibido se mostrarán como Revisado. Se conservarán datos originales, archivos y flujo anterior en un archivo privado, sin exponer instrucciones bancarias en el nuevo flujo.

**Aceptación:** Una importación y posterior edición no pierde el original 1.28 ni transforma sus datos en un pago verificado. Los estados Entregado y Rechazado se conservan.

Fuente en la base: C08 22–27, 87–94; D01; D15. Véase el [inventario de fuentes](../referencias/fuentes.md).

## Privacidad textos legales y auditoría

Los textos aprobados son contenido versionado y separado del diseño. El traslado a una nueva aplicación necesita confirmar que describen sus tratamientos y proveedores reales.

### RF55 Biblioteca legal versionada

Prioridad: **P0**. Naturaleza: Paridad y mejora.

Se publicarán Aviso legal, Privacidad, Cookies, Compromiso y Estatutos según versión aprobada. Las revisiones conservarán texto, fuente, huella y vigencia; no se reescribirán al cambiar el diseño.

**Aceptación:** Publicar una nueva revisión conserva la anterior. El recurso aprobado de septiembre de 2026 puede compararse íntegramente sin modificar su contenido por maquetación.

Fuente en la base: C16 150–212; C17 16–22; C18; D05. Véase el [inventario de fuentes](../referencias/fuentes.md).

### RF56 Evidencia de consentimientos

Prioridad: **P0**. Naturaleza: Paridad.

Las elecciones obligatorias y opcionales se registrarán de forma separada y sin premarcar. La evidencia incluirá texto presentado, versión, elección, fecha UTC y contexto.

**Aceptación:** Un envío de formulario antiguo conserva esa condición y no recibe un consentimiento nuevo retrospectivo. Rechazar comunicaciones opcionales no bloquea la solicitud.

Fuente en la base: C05 283–295; C17 69–96; D05. Véase el [inventario de fuentes](../referencias/fuentes.md).

### RF57 Avisos y Estatutos públicos

Prioridad: **P1**. Naturaleza: Paridad.

Se mostrarán avisos de uso por módulo y confidencialidad antes del acceso. Los enlaces legales permanecerán visibles en el pie y los Estatutos podrán verse y descargarse si se aporta el PDF.

**Aceptación:** Cambiar la ruta de una página legal no rompe el enlace. Sin PDF se informa de que está pendiente y no se genera un documento ficticio.

Fuente en la base: C16 130–212, 254–267; C17 98–117; T01; D05. Véase el [inventario de fuentes](../referencias/fuentes.md).

### RF58 Cookies y servicios externos

Prioridad: **P0**. Naturaleza: Paridad y mejora.

La configuración inicial prescindirá de servicios no esenciales. El aviso informativo de cookies técnicas no se tratará como gestor de consentimiento para terceros.

**Aceptación:** Si se añade analítica o un contenido que requiere consentimiento, se revisa la política y se incorpora bloqueo previo de ese servicio antes de habilitarlo.

Fuente en la base: C17 55–67; D05 Cookies; W10. Véase el [inventario de fuentes](../referencias/fuentes.md).

### RF59 Derechos y conservación

Prioridad: **P0**. Naturaleza: Paridad y mejora.

Se ofrecerán exportación y supresión administradas, con revisión de identidad, alcance y retenciones justificadas. Las políticas distinguirán solicitudes, documentos, fotos, auditoría y copias.

**Aceptación:** El borrado aprobado retira datos y archivos aplicables, conserva referencias compartidas necesarias y registra las excepciones. Una política caducada se ejecuta mediante trabajo programado verificable.

Fuente en la base: C19 27–83; C16 20–43; C20 63–69; W10. Véase el [inventario de fuentes](../referencias/fuentes.md).

### RF60 Registro de acciones sensibles

Prioridad: **P0**. Naturaleza: Paridad y mejora.

Se registrarán cambios de permisos, revisión, estados, publicaciones, exportaciones, descargas y borrados con actor, fecha, recurso y resultado, evitando contenido de formularios o documentos.

**Aceptación:** Un evento permite identificar la operación y su resultado sin exponer datos médicos, contraseñas ni archivos. Solo Administración consulta el registro global.

Fuente en la base: C20 18–80; C14; C08 94. Véase el [inventario de fuentes](../referencias/fuentes.md).

## Web pública configuración y ayuda

La equivalencia pública completa requiere cubrir funciones aportadas conjuntamente por plugin y tema. Puede implantarse después del núcleo privado manteniendo la web WordPress durante la transición.

### RF61 Páginas y navegación públicas

Prioridad: **P1**. Naturaleza: Paridad ampliada al tema.

Se ofrecerán Inicio, presentación, secciones, equipo, Noticias, Contacto, Solicitud de ingreso y páginas legales, con navegación jerárquica y pie configurables.

**Aceptación:** Añadir, ordenar u ocultar un enlace actualiza el mismo modelo de navegación. Se detectan destinos inexistentes, privados y duplicados.

Fuente en la base: C25; C26; C27; T01. Véase el [inventario de fuentes](../referencias/fuentes.md).

### RF62 Identidad y composición visual

Prioridad: **P1**. Naturaleza: Paridad ampliada al tema.

Se configurarán logotipos, paleta, imágenes, fondos por zona, portada, tarjetas, bloques, textos, redes y pie mediante componentes seguros y vista previa adaptable.

**Aceptación:** Guardar solo el login conserva las demás personalizaciones. Los ajustes se almacenan fuera del código y sobreviven al despliegue.

Fuente en la base: C23 62–466, 527–548; D03; D10 Diseño; T01. Véase el [inventario de fuentes](../referencias/fuentes.md).

### RF63 Diseño por página y propuestas de sección

Prioridad: **P1**. Naturaleza: Paridad ampliada al tema.

Cada página podrá tener cabecera, subtítulo, anchura, imagen y llamada final. Las propuestas guiadas de sección serán borradores independientes antes de aplicar un cambio confirmado.

**Aceptación:** Aplicar una propuesta conserva identidad, dirección y navegación de la página y guarda una revisión recuperable. Un reset visual no elimina contenido.

Fuente en la base: C24; C25 171–274; D10. Véase el [inventario de fuentes](../referencias/fuentes.md).

### RF64 Configuración por áreas

Prioridad: **P1**. Naturaleza: Paridad.

Los ajustes se separarán en General, Comunicación, Legal, Identidad, Ayuda y Operación, manteniendo un único origen para cada dato y guardado parcial.

**Aceptación:** Cambiar un contacto no resetea conservación ni fondos. Las validaciones bloquean valores inválidos y el historial identifica el cambio.

Fuente en la base: C14; C16; C21; C22; C23. Véase el [inventario de fuentes](../referencias/fuentes.md).

### RF65 Diagnóstico y mantenimiento

Prioridad: **P1**. Naturaleza: Paridad y mejora.

Administración consultará límites de subida, cuota, espacio, disponibilidad de procesamiento de imágenes, trabajos pendientes, correo y estado de copias y restauraciones.

**Aceptación:** Una cola detenida o almacenamiento no disponible genera una señal operativa verificable; las pantallas no muestran secretos o rutas de acceso sensibles.

Fuente en la base: C22; C10 369–418. Véase el [inventario de fuentes](../referencias/fuentes.md).

### RF66 Recorridos y ayuda contextual

Prioridad: **P1**. Naturaleza: Paridad.

Se ofrecerán recorridos por permisos y pantalla, progreso por usuario y versión, bienvenida, lista de primeros pasos y repetición manual. La ayuda estará desactivada por defecto hasta configurarla.

**Aceptación:** Cerrar o terminar una guía no la reinicia al recargar. No se descargan recorridos de otros perfiles y ninguna guía guarda, publica o valida datos automáticamente.

Fuente en la base: C21 15–65, 111–310, 343–460; D08; D09. Véase el [inventario de fuentes](../referencias/fuentes.md).

### RF67 Ayuda práctica accesible

Prioridad: **P1**. Naturaleza: Paridad.

Los pasos prácticos podrán detectar una selección, un filtro o una entrada en formulario, con omisión, foco visible, control por teclado y reducción de movimiento.

**Aceptación:** Una persona puede cerrar o saltar cualquier paso sin ejecutar la acción de negocio. Un control ausente o una ruta incorrecta no bloquean la aplicación.

Fuente en la base: C21 488–559; D08; D09. Véase el [inventario de fuentes](../referencias/fuentes.md).

## Migración y portabilidad

La migración trasladará datos y reglas verificables. Un ZIP de código no contiene por sí mismo las cuentas, solicitudes, archivos subidos ni configuración real de la instalación.

### RF68 Importación repetible

Prioridad: **P0**. Naturaleza: Propuesta necesaria.

Se importarán usuarios, permisos, secciones, contenidos, respuestas, modelos, archivos, referencias compartidas, pedidos, consentimientos y ajustes con mapa de identificadores originales.

**Aceptación:** Reejecutar el mismo lote no duplica entidades. Cada objeto importado conserva procedencia, versión del importador y resultado verificable.

Fuente en la base: C02; C05; C08; C10; C14; C18; C19. Véase el [inventario de fuentes](../referencias/fuentes.md).

### RF69 Conciliación e históricos desconocidos

Prioridad: **P0**. Naturaleza: Propuesta necesaria.

Se contrastarán recuentos, relaciones y huellas con el origen. Los campos ausentes se marcarán como desconocidos sin inventar firma, versión del modelo o pago.

**Aceptación:** Un documento sin fecha o modelo conocido sigue así después de migrar. Toda diferencia bloqueante se resuelve antes de habilitar la función afectada.

Fuente en la base: C06 62–65; C08 22–27, 87–94; D01; D02. Véase el [inventario de fuentes](../referencias/fuentes.md).

### RF70 Transición de credenciales y enlaces

Prioridad: **P0**. Naturaleza: Propuesta necesaria.

Las credenciales se renovarán mediante invitación o recuperación segura, conservando la correspondencia de usuarios. Se mantendrán rutas públicas con redirecciones y una tabla de enlaces históricos privados.

**Aceptación:** Los hashes de WordPress no se copian como si fueran hashes de Identity. Un enlace privado antiguo exige autenticación y vuelve a comprobar propiedad y permiso.

Fuente en la base: C01; C04 39–50; W04. Véase el [inventario de fuentes](../referencias/fuentes.md).

### RF71 Salida y reversibilidad

Prioridad: **P0**. Naturaleza: Propuesta necesaria.

La organización podrá exportar datos y archivos con manifiesto de relaciones, versiones y huellas. El corte tendrá copia coordinada, congelación de escrituras y procedimiento de reversión.

**Aceptación:** Una restauración ensayada recupera base de datos, objetos y claves de cifrado del mismo punto. No se permite reabrir dos sistemas editables sin reconciliar sus cambios.

Fuente en la base: D14 Principios; D02 Recuperación; D10 Recuperación. Véase el [inventario de fuentes](../referencias/fuentes.md).

## Requisitos no funcionales

| Identificador | Objetivo propuesto | Verificación |
| --- | --- | --- |
| RNF01 Seguridad | Autorización por recurso en todas las lecturas y escrituras; MFA obligatoria para Administración y Coordinación | Matriz negativa de permisos y revocación durante sesión activa |
| RNF02 Accesibilidad | WCAG 2.2 AA como objetivo del producto | Teclado, lector de pantalla, foco, errores, contraste y reflujo a 320 px; W09 |
| RNF03 Latencia | p95 menor de 600 ms de servidor para listados de 25 registros, sin transferencia de archivos | Carga de 50 sesiones con dataset de referencia y observabilidad |
| RNF04 Experiencia web | LCP menor de 2,5 s como objetivo en páginas principales bajo perfil móvil acordado | Prueba de navegador con dispositivo y red documentados; revisar también uso real |
| RNF05 Disponibilidad | Objetivo interno mensual de 99,5 por ciento, sujeto al servicio contratado | Medición externa de acceso y funciones esenciales; registro de interrupciones |
| RNF06 Recuperación | RPO de 1 hora y RTO de 4 horas como objetivo inicial | Restauración coordinada de base, objetos y claves; coste a validar |
| RNF07 Consistencia | Ninguna pérdida silenciosa de versiones ni duplicación por reintento | Pruebas de caída, concurrencia e idempotencia con base real |
| RNF08 Escalabilidad | Paginación 25, 50 y 100; exportaciones en lotes; miniaturas y trabajos con límites | Carga sobre históricos sin materializar todos los archivos o registros en memoria |
| RNF09 Privacidad operativa | Sin documentos ni datos médicos en logs, métricas o correos de aviso | Inspección de eventos y mensajes sintéticos; pruebas de redacción |
| RNF10 Mantenibilidad | Reglas de negocio separadas de HTML, EF e infraestructura | Dependencias verificadas y pruebas sobre casos de uso reales |
| RNF11 Portabilidad | Exportación completa con manifiesto y almacenamiento sustituible | Importación de prueba en entorno limpio y conciliación de huellas |
| RNF12 Actualización | Runtime, paquetes y SO dentro de soporte; parches reproducibles | Versiones fijadas, inventario de dependencias y actualización ensayada; W01 |

Los volúmenes y objetivos son hipótesis de dimensionamiento a ratificar en DC08; no son resultados medidos. Cada RNF debe tener perfil y evidencia de aceptación.

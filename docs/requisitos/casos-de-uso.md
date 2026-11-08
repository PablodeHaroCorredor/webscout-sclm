# Casos de uso prioritarios del piloto

Versión 0.1 · Propuesta de arranque · 6 de noviembre de 2026.

Los casos de uso convierten los requisitos en recorridos que Producto, Coordinación y desarrollo puedan revisar juntos. Conservan el comportamiento de 1.29 y se ejecutarán sobre datos sintéticos en las primeras demostraciones.

## CU01 Solicitar ingreso

Actor: visitante. Resultado: solicitud privada Pendiente, con datos mínimos y evidencia legal. RF10 a RF14; PA11 y PA13.

1. Facilita participante, nacimiento, relación y persona de contacto.
2. Declara facultad para aportar los datos, acepta el tratamiento necesario y decide por separado las comunicaciones opcionales.
3. El servidor valida valores, antispam y versión legal y confirma la recepción.
4. El equipo consulta la solicitud y puede pasar a Contactado, Lista de espera, Aceptado o Rechazado.

Un fallo de correo mantiene la solicitud guardada y el aviso pendiente. No se crea cuenta ni se solicitan DNI o salud en esta fase. Si la persona se representa a sí misma se comprueba mayoría de edad conforme al contrato del formulario.

## CU02 Dar acceso y retirar una concesión

Actor: Administración. Resultado: cuenta habilitada con secciones y capacidades determinadas. RF01 a RF08; PA01 a PA03.

1. Crea o invita una cuenta, asigna perfil y secciones.
2. Concede de forma independiente revisión, publicación pública o pedidos cuando corresponda.
3. Comprueba el resultado con una cuenta sin facultades globales.
4. Revoca una capacidad y verifica que el siguiente acceso directo al recurso deja de estar autorizado sin depender de cerrar el navegador.

Asignar Todo el grupo no equivale a conceder revisión global. El permiso de pedidos del Scouter abarca los pedidos del grupo y mantiene cerrados los otros trámites.

## CU03 Preparar una comunicación

Actor: Scouter o Coordinación. Resultado: aviso o actividad publicado para su audiencia. RF17 a RF24; PA12.

Elige tipo, crea borrador, redacta y añade imágenes con texto alternativo. Configura secciones y comprueba la vista previa. Una actividad con precio positivo exige modelo y modalidad PDF. Publica con capacidad suficiente. Otra persona con lectura por sección no adquiere edición del contenido ajeno.

Para difundir un aviso en público crea una copia independiente en borrador. No se trasladan documentos o imágenes privados; añade únicamente activos públicos autorizados y completa revisión editorial antes de publicar.

## CU04 Presentar documentos de dos hermanos

Actor: familia. Resultado: dos respuestas independientes y un solo justificante compartido. RF25 a RF31; PA04 a PA06.

1. Descarga el modelo vigente y firma el PDF fuera de la aplicación.
2. Presenta la primera autorización con participante y secciones responsables; añade justificante si la actividad tiene coste.
3. Presenta al segundo participante, aunque sea de la misma sección, con su propio PDF.
4. Selecciona el justificante anterior si cubre también esa entrega.
5. Consulta estados y documentos de cada respuesta por separado.

Si el modelo cambió se exige descargar el vigente. Fuera de plazo se rechaza también la corrección. Otra cuenta o actividad no puede reutilizar el justificante. Los nombres iguales no se fusionan automáticamente y el doble envío se controla por clave de operación.

## CU05 Revisar y corregir una autorización

Actores: revisor habilitado y propietario. Resultado: una nueva versión del expediente corregido, con anteriores conservadas. RF32 a RF34 y RF38; PA02, PA04 y PA09.

El revisor filtra respuestas dentro de sus secciones responsables, consulta el documento y solicita corrección con motivo. La familia abre esa respuesta, mantiene participante, sustituye el PDF y confirma. Solo esa respuesta vuelve a Recibida; su hermano mantiene estado. La versión anterior sigue accesible según permiso. Dos revisores que actúan sobre la misma revisión reciben conflicto, sin sobreescritura.

## CU06 Subir fotos desde un móvil

Actor: responsable del álbum. Resultado: fotos confirmadas y estado del álbum según decisión final. RF39 a RF47; PA09 y PA10.

Acumula selecciones hasta 100 fotos, activa o desactiva optimización y guarda. Observa envío y confirmación por foto. Puede pausar tras la foto actual, reintentar fallidas o terminar parcialmente de forma explícita. La foto ya confirmada no se duplica tras perder su respuesta de red.

Cerrar la página conserva lo confirmado y pierde la cola pendiente en memoria; no se promete reanudación posterior. El nuevo álbum permanece en borrador hasta finalizar y tener foto. En uno ya publicado, las fotos confirmadas pueden ser visibles antes de terminar todo el lote.

## CU07 Solicitar y entregar un producto

Actores: familia y gestor de pedidos. Resultado: pedido con seguimiento y adjunto opcional independiente. RF48 a RF54; PA07 y PA08.

La familia solicita producto publicado, talla válida y 1 a 10 unidades. El pedido conserva el precio informativo del momento. Puede adjuntar o sustituir justificante desde Solicitado. El gestor cambia entre los seis estados y puede marcar Entregado sin documento ni verificación de pago.

El propietario cancela desde Solicitado o Revisado, con motivo; después contacta con el equipo. No puede subir nuevos adjuntos en Rechazado o Cancelado. La exportación de pedidos abarca el conjunto anunciado y no incluye archivos, URLs privadas o datos económicos antiguos de 1.28.

## CU08 Sustituir el modelo PDF

Actor: responsable de actividad. Resultado: modelo vigente nuevo e histórico conservado. RF26 y RF27; PA05 y PA06.

Selecciona nuevo PDF, confirma sustitución y guarda con la versión de actividad leída. Los nuevos formularios usan esa versión. Las respuestas existentes conservan su modelo conocido y no se invalidan automáticamente. El responsable decide si alguna requiere corrección; un histórico sin versión permanece desconocido.

## CU09 Atender exportación o supresión

Actor: responsable autorizado de privacidad. Resultado: operación limitada al ámbito acreditado y con excepciones justificadas. RF55 a RF60; PA13 y PA14.

Verifica identidad y alcance, inventaría datos y vínculos, revisa conservación y reclamaciones y prepara la operación. Ejecuta exportación mínima o supresión/bloqueo aplicable, comprueba archivos compartidos y registra resultado sin copiar información sensible. La política incluye réplicas y caducidad de copias, no solo las filas de la base.

## CU10 Migrar y habilitar el piloto

Actores: migración, operación y validación funcional. Resultado: un único sistema escritor con datos conciliados. RF68 a RF71; PA15 y PA19.

Ensaya importación con manifiesto y correspondencias, valida permisos y archivos, congela escritura del módulo, hace importación final y concilia. Habilita .NET solo cuando se cumplen los criterios de salida. Volver a WordPress después de nuevas entregas exige reconciliar esas operaciones, no solo reinstalar el ZIP anterior.

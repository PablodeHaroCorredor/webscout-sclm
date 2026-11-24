# Privacidad conservación y contenido legal

Versión 0.1 · Propuesta de arranque · 6 de noviembre de 2026.

El piloto tratará datos de familias, participantes y responsables, incluidos documentos que pueden contener salud o identificación. El grupo debe ratificar finalidades, bases, proveedores y periodos para la aplicación real. La política se concretará con el responsable del tratamiento y su asesoría antes de usar datos reales.

## Criterios de diseño

El formulario público de ingreso utiliza datos mínimos y no solicita DNI o salud. Los documentos se limitan a propietario y equipo autorizado; listados, correo y logs evitan el contenido sensible. Las elecciones opcionales se separan y no se premarcan. El texto presentado, versión, elección y fecha quedan vinculados a su contexto.

Estos criterios siguen la [protección de datos por defecto de la AEPD](https://www.aepd.es/derechos-y-deberes/cumple-tus-deberes/medidas-de-cumplimiento/proteccion-de-datos-por-defecto). El [RGPD](https://www.boe.es/buscar/doc.php?id=DOUE-L-2016-80807) fundamenta minimización, conservación limitada y seguridad; el diseño debe concretar qué datos necesita cada finalidad y quién accede.

## Matriz que debe completar el grupo

| Categoría | Finalidad prevista | Acceso operativo | Inicio y duración a ratificar | Cierre |
| --- | --- | --- | --- | --- |
| Contacto y outbox temporal | Responder a consulta y asegurar envío | Buzón configurado y operador acotado | Recepción y envío; plazo técnico por fijar | Eliminar payload temporal; revisar buzón y proveedor |
| Solicitud no continuada | Tramitar ingreso o espera | Coordinación y Administración | Cierre de solicitud; plazo por fijar | Eliminar o bloquear cuando corresponda |
| Cuenta y asignaciones | Autenticación y acceso | Titular y administración autorizada | Fin de relación y responsabilidades aplicables | Desactivar acceso y revisar conservación |
| PDF y justificantes de actividad | Gestión y acreditación documental | Titular y revisores por alcance | Cierre de actividad y responsabilidades aplicables | Revisar versiones, vínculos y motivo de retención |
| Imágenes privadas | Documentar actividad | Audiencias autorizadas | Fin de finalidad y revisión de autorizaciones | Retirar exposición y aplicar destino aprobado |
| Imágenes públicas | Difusión expresamente autorizada | Público | Vigencia de finalidad y autorizaciones | Retirar y revisar réplicas o proveedores |
| Pedido y justificantes | Tramitar encargo interno | Titular y gestores de pedidos | Cierre y responsabilidades aplicables | Conservar solo lo necesario |
| Auditoría y logs | Seguridad y trazabilidad | Administración u operación limitada | Evento y plazo por categoría | Purgar o anonimizar según política |
| Copias de seguridad | Recuperación | Operación restringida | Punto de copia y calendario de caducidad | Expirar copias y aplicar supresiones al restaurar |

Un plazo técnico por defecto, como 730 días de documentos o auditoría, no se adopta como obligación jurídica universal. No se adopta tampoco el plazo de documentos laborales del ERP para la web scout.

## Bloqueo supresión y derechos

La supresión puede tener excepciones por obligaciones o responsabilidades y, cuando proceda, un régimen de bloqueo. El [artículo 32 de la LOPDGDD](https://www.boe.es/buscar/act.php?id=BOE-A-2018-16673#a32) regula el bloqueo en sus términos. El procedimiento del grupo debe distinguirlo de desactivar una cuenta, archivar un álbum o cancelar una solicitud.

El expediente de atención de derechos registrará identidad acreditada, alcance, decisión, excepción, responsable, ejecución y resultado. Revisará referencias compartidas y versiones, sin destruir el documento de otro expediente vigente. Se comprobarán almacenamiento, derivados, caché, proveedor de correo y copias. La restauración no debe reactivar indefinidamente datos que ya debían retirarse.

## Textos y cookies

La revisión de septiembre de 2026 se conserva como fuente versionada del sistema anterior. El traslado a otro hosting, correo o proveedor puede exigir cambios en la información. La asesoría decidirá la versión que se publica; una actualización visual no debe modificar textos aprobados.

La configuración inicial evita servicios no esenciales. El aviso informativo para cookies técnicas no es un gestor de consentimiento de analítica, mapas, vídeos o servicios de terceros. Si se incorporan, se diseña su bloqueo y tratamiento antes de habilitarlos.

## Salida de DC06

Se requiere inventario real de tratamientos y proveedores, matriz de acceso y conservación completada, información legal aplicable, procedimiento de derechos, revisión de datos de salud y de menores y evaluación de riesgos/impacto cuando corresponda. Esas decisiones se registran con versión, fecha y responsable en el [registro de arranque](../planificacion/decisiones-y-riesgos.md).

Referencias funcionales: RF10 a RF16, RF29 a RF38 y RF55 a RF60. Las fuentes oficiales anteriores se consultaron el 6 de noviembre de 2026.

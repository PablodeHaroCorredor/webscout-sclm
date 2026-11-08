# Experiencia de uso y accesibilidad

Versión 0.1 · Propuesta de arranque · 6 de noviembre de 2026.

La interfaz debe ayudar a saber qué hay que hacer, con qué participante y qué resultado está confirmado. Usará la terminología del [glosario](../glosario.md), navegación por permisos y acciones claras en móvil.

## Navegación

Mi espacio: Resumen, Comunicaciones, Fotos, Mis gestiones, Productos y Mis datos. Gestión del grupo: comunicaciones, documentos recibidos, álbumes, catálogo, pedidos, solicitudes y altas anteriores según permisos. Configuración agrupa las áreas estables y evita duplicar un ajuste en varias pantallas.

Una cuenta con varias secciones ve etiquetas comprensibles. Los estados vacíos no revelan la existencia de recursos ajenos. Los contadores se calculan dentro del alcance de lectura y no con totales globales filtrados después en el navegador.

## Patrones principales

| Pantalla | Elementos mínimos | Decisión de interacción |
| --- | --- | --- |
| Actividad | Fechas, plazo, coste, modelo vigente y resumen por participante | Presentar nueva autorización y corregir una existente son acciones distintas |
| Mis gestiones | Participante, actividad, fecha y estado | El enlace de corrección pertenece al expediente concreto |
| Revisión | Filtros, lista paginada, detalle y acción con versión | No confirmar un estado sobre datos obsoletos |
| Pedido | Instantánea, estado y presencia de justificante | El adjunto no se muestra como pago verificado |
| Subida de fotos | Cola, progreso por archivo y resumen | Enviado no equivale a Guardado; fin parcial explícito |
| Configuración | Área, ayuda contextual y vista previa | Guardar solo el área editada |

## Estados y errores

Un cambio confirmado se diferencia de un envío pendiente, un conflicto o un correo fallido. La recuperación de sesión conserva las fotos confirmadas; volver a abrir la página no promete restaurar la cola local. El mensaje de modelo obsoleto indica volver a descargar; el de plazo explica el cierre sin borrar lo entregado.

Los formularios conservan entradas seguras después de un error, llevan el foco a un resumen enlazado a campos y no dependen exclusivamente del color. No se anuncia un éxito por una respuesta HTTP que no haya confirmado la operación de negocio.

## Accesibilidad y adaptación

WCAG 2.2 AA será el objetivo de producto. Se verifican teclado, orden y visibilidad de foco, nombres de controles, contraste, errores, ampliación, reflujo a 320 px, avisos de progreso y reducción de movimiento. El visor y la ayuda permiten cerrar con teclado y devuelven el foco al origen. La [recomendación oficial WCAG 2.2](https://www.w3.org/TR/WCAG22/) orienta la revisión.

Se revisarán 320, 390 y 1440 px y al menos un teléfono físico con navegador representativo de las familias. La emulación cubre anchuras y algunos fallos de red; no demuestra comportamiento bajo suspensión real del dispositivo.

## Sistema de tematización e identidad de grupo multitenant

Cada grupo scout federado en SCLM cuenta con su propia identidad visual, centrada en los colores oficiales de su pañoleta, su heráldica y su paleta de comunicación:

1. **Tokens de diseño inyectados en runtime:** La aplicación inyecta variables CSS semánticas en el elemento raíz (`:root`) a partir de la configuración validada del tenant activo:
   - `--scout-primary` y `--scout-primary-hover`: Color principal de marca e interacción.
   - `--scout-primary-contrast`: Color de texto calculado automáticamente (blanco o carbón) para garantizar legibilidad.
   - `--scout-secondary` y `--scout-accent`: Colores secundarios de apoyo y avisos.
   - `--panoleta-base`, `--panoleta-ribete` y `--panoleta-cinta`: Colores exactos que definen el pañuelo scout del grupo.
   - `--header-bg` y `--header-text`: Superficie de navegación superior del grupo.
2. **Validación automática de accesibilidad (WCAG 2.2 AA):**
   - El panel de administración de cada grupo incluye un motor en servidor que calcula el ratio de contraste de luminancia relativa sobre los colores seleccionados.
   - Si la combinación elegida no alcanza el umbral de 4.5:1 para texto normal o 3:1 para elementos de control, el sistema sugiere un ajuste de luminosidad automático o fija un contraste alternativo seguro antes de guardar.
3. **Componente de pañoleta vectorial SVG:** Un componente paramétrico renderiza la pañoleta en SVG con los colores del grupo en el encabezado, el perfil de usuario y las cabeceras de notificación por correo.
4. **Seguridad visual:** Se prohíbe taxativamente la inyección de hojas de estilo (CSS) o scripts externos arbitrarios. La personalización se restringe a valores cromáticos validados y archivos de imagen optimizados (PNG/WebP/SVG verificados en el almacén público de activos).
5. **Selector de grupo (Multi-tenant):** Si un usuario dispone de membresías en más de un grupo (o en el equipo regional SCLM), la cabecera muestra un selector accesible que permite cambiar de contexto organizativo manteniendo la misma sesión autenticada.

## Ayuda y marca

Guías declarativas con anclajes estables, filtradas por permisos, versión y ruta; progreso propio y repetición manual. Ninguna guía publica o valida por el usuario. La identidad usa tokens y componentes seguros, sin CSS arbitrario ni imágenes privadas en el fondo previo al login. Los textos legales se presentan legibles y permanecen separados del diseño.

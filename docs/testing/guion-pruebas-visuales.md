# Guion de pruebas visuales y móviles

Versión 0.1 · Propuesta de arranque · 6 de noviembre de 2026.

Los recorridos se ejecutarán cuando exista UI, sobre candidato identificado, con datos sintéticos y en las anchuras acordadas. Capturar estados inicial, pendiente, confirmado y error cuando sean relevantes. Las capturas acompañan la comprobación de datos y permisos; no la sustituyen.

## GV01 Acceso y navegación

Entrar como familia, Scouter, revisor y gestor de pedidos. Comprobar menú y contadores propios, volver con URL copiada y revocar concesión en otra sesión. Recorrer con teclado, ampliar y revisar foco. Esperado: se distingue el alcance, no se revela contenido ajeno y la denegación tiene una salida comprensible. PA01 a PA03 y PA17.

## GV02 Dos participantes y corrección

Presentar dos respuestas con el mismo titular y nombres incluso coincidentes. Adjuntar un único justificante compartido en una actividad con coste; comprobar que cada PDF pertenece al participante correcto. Solicitar corrección de una sola respuesta y volver a entregarla. Esperado: el otro expediente permanece igual, existe versión anterior y el formulario muestra el modelo/plazo correcto. PA04 a PA06.

## GV03 Modelo y plazo cambiados

Dejar formulario abierto, sustituir modelo en otra sesión e intentar enviar; después probar el cierre de día civil y cambio horario con reloj de ensayo. Esperado: mensaje accionable, sin respuesta parcial ni confirmación falsa. La entrega anterior no queda invalidada por sustituir el modelo. PA05.

## GV04 Fotos en móvil físico

Acumular varias selecciones hasta 100 fotos, optimizar, iniciar y pausar. Cortar red después de confirmar una foto y antes de recibir su respuesta, reintentar y finalizar parcialmente. Bloquear el teléfono y volver. Cerrar/reabrir la página como caso separado. Esperado: contadores coherentes, sin duplicar confirmadas; se explica que la cola pendiente se pierde al cerrar. Comprobar borrador nuevo frente a álbum ya publicado. PA09 y PA10.

## GV05 Pedidos

Solicitar producto/talla con 1 y 10 unidades y rechazar cantidades fuera del límite. Adjuntar justificante desde Solicitado. Gestionar los seis estados en orden no secuencial, entregar sin adjunto y cancelar como titular en estados permitido/prohibido. Esperado: adjunto separado del estado, sin aceptación económica ni pago verificado. Conservar historial de rectificación. PA07 y PA08.

## GV06 Publicación y archivos

Preparar comunicación con imágenes privadas y copiar a público con el permiso correspondiente. Abrir sin sesión, revisar contenido, código y solicitudes de red; retirar publicación y revisar caché. Esperado: solo activos expresamente públicos, sin enlaces de versiones privadas. Probar comentario inválido, CSRF y contenido saneado. PA12.

## GV07 Configuración ayuda y formularios públicos

Cambiar una sección de ajustes y restaurar su aspecto, verificando que otras áreas y datos no cambian. Abrir ayuda desde varias rutas/roles, repetirla y revisar progreso después de volver a entrar. Probar contacto/ingreso con errores, consentimientos opcionales vacíos y proveedor de correo caído. Esperado: campos accesibles, estado real del trámite y ningún texto legal modificado por diseño. PA11, PA13, PA16 y PA17.

## Revisión común

Probar 320, 390 y 1440 px CSS, zoom 200%/400%, foco visible, lector de pantalla acordado, etiquetas, contraste, mensajes anunciados y cierre de diálogos con devolución de foco. Registrar navegador/versión, sistema, dispositivo físico, red y limitaciones. Ningún caso está ejecutado todavía; DC07 fija mejoras de control y el equipo acuerda la matriz de QA en E0.

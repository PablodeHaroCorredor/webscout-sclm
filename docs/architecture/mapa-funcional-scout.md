# Mapa funcional de la web scout

Versión 0.1 · Propuesta de arranque · 6 de noviembre de 2026.

Los módulos se organizan por responsabilidad de negocio y conservan el vocabulario del grupo. El menú no determina por sí solo los permisos ni la propiedad de los datos.

| Módulo | Capacidades | RF principales | Propiedad de datos |
| --- | --- | --- | --- |
| Acceso | Cuenta, membresía, secciones y concesiones | RF01 a RF09 | Identidad funcional y asignaciones |
| Comunicaciones | Avisos, conversación y noticias públicas | RF17 a RF23 | Comunicación, audiencia, comentario y borrador público |
| Actividades y documentos | Modelos, participantes, entregas y revisión | RF24 a RF34 | Actividad, respuesta, versión y justificante compartido |
| Biblioteca | Carpetas, álbumes y fotos | RF39 y RF41 a RF47 | Álbum y confirmación de cada foto |
| Catálogo y pedidos | Productos, solicitudes y seguimiento | RF48 a RF53 | Producto, talla, instantánea e historial del pedido |
| Ingreso y formularios | Solicitudes, contacto y formularios de actividad | RF10 a RF14 y RF16 | Solicitud, plantilla versionada y respuestas |
| Sitio y configuración | Páginas, menú, identidad y ajustes | RF61 a RF65 | Página, revisión, navegación y configuración |
| Privacidad y operación | Textos, evidencias, retención y auditoría | RF35 a RF40 y RF55 a RF60 | Archivos transversales, consentimiento, evento y trabajo |
| Ayuda | Guías contextualizadas y progreso | RF66 y RF67 | Estado individual y versión de guía |
| Portabilidad | Exportación completa y restauración | RF71 | Manifiesto de datos, archivos y huellas |

## Recorridos compartidos

Una actividad usa el editor y la audiencia de comunicaciones y gobierna su propio modelo y respuestas. La biblioteca de archivos conserva bytes; actividades o pedidos deciden el acceso a cada vínculo. Una foto de álbum usa el almacén privado y hereda la audiencia del álbum. Un cambio de alcance afecta a originales, miniaturas y versiones anteriores.

Un caso de uso puede coordinar datos de una misma transacción local cuando una invariante lo exige, por ejemplo respuesta, vínculos documentales, auditoría y aviso pendiente. La biblioteca de archivos y la auditoría son servicios transversales de esa unidad. No se introduce una separación física de bases que impida esa consistencia.

## Referencias de diseño

La [arquitectura modular](arquitectura-modular.md) fija las dependencias técnicas; el [modelo de datos](modelo-datos.md), las relaciones; y [casos de uso](../requisitos/casos-de-uso.md), la secuencia para el usuario. El catálogo RF sigue siendo la referencia de aceptación.

# Plan de migración desde WordPress

Versión 0.1 · Propuesta de arranque · 6 de noviembre de 2026.

El ZIP acredita código y semántica de referencia; no contiene los datos completos de la instalación. Antes del importador se necesita una copia autorizada y coherente de base, configuración, uploads, almacén privado, perfiles y versiones reales. Identificar Elementor, galerías o eventos ajenos al Core que continúen en uso.

## Inventario de entrada

Medir usuarios, roles, secciones, concesiones individuales, registros por tipo/estado, actividades abiertas, `legacy_form`, referencias a archivos, tamaños, faltantes y zona horaria. Registrar prefijo real de tablas, codificación, versiones y fecha de captura. Decodificar valores serializados sin ejecutar PHP ni objetos del volcado. Conservar copia original protegida y un manifiesto de procedencia.

## Correspondencias principales

| Origen | Destino propuesto | Control |
| --- | --- | --- |
| Usuarios y capacidades de usermeta | Identity, membresías y concesiones | Mapa de identidad; invitación/renovación de acceso |
| Taxonomía de secciones | Section y asignaciones | Slug, pertenencia y responsabilidad diferenciadas |
| Comunicaciones y noticias | Communication y versión pública | Autor, audiencia y archivos sin exposición implícita |
| Actividades y PDFs modelo | Activity y ModelVersion | Plazo, coste, modo y modelo desconocido preservados |
| Registros de actividad | ActivityResponse y ResponseVersion | Varias respuestas por cuenta; no fusionar por nombre |
| Índices de archivos y grupos de recibo | FileAsset, FileBinding y ActivityReceipt | Traducir índices posicionales; mismo objeto con todas sus referencias |
| Pedidos y flujo histórico | Order, adjuntos y archivo privado 1.28 | Mapear estados antiguos a Revisado cuando procede, conservar estado bruto |
| Carpetas, álbumes y fotos | AlbumFolder, Album y Photo | Jerarquía, autor, audiencia, bytes y derivados |
| Formularios y respuestas históricas | LegacyFormTemplate y LegacyRegistration | Etiquetas/payload de la versión original; captura abierta si se usa |
| Textos, consentimiento, ayuda y auditoría | Versiones legales, evidencia, progreso y eventos | Origen, versión, fechas conocidas y minimización |
| Páginas, navegación y ajustes | Contenido, revisiones y SiteSettings | Inventario editorial y redirecciones; no trasladar PHP ejecutable |

La denominación física final de las entidades se cerrará con el [modelo](../architecture/modelo-datos.md). `LegacyIdMap` identifica organización, tipo, ID de origen, ID destino y lote. Reejecutar el lote no duplica registros. Un hash igual no permite fusionar documentos entre cuentas u organizaciones sin evidencia de referencia compartida.

## Ensayos obligatorios

1. Importación completa en entorno aislado, correo capturado y sin modificar el origen.
2. Conciliación de todos los registros incluidos: recuentos por tipo/estado, relaciones, permisos y archivos.
3. Comparación SHA-256 y tamaño de originales; muestra visual de texto enriquecido, tildes, fechas y galerías.
4. Importación incremental con cambios, nuevo adjunto, eliminación y corrección representativos.
5. Pruebas de familia, revisor y gestor sobre datos sintéticos derivados o anonimizados autorizados.

Cada ejecución conserva versión del importador, fuentes, correspondencias, incidencias y resultado. Cero referencias rotas o discrepancias de bytes sin resolver es condición del lote aceptado. Un dato ausente queda desconocido; no se inventan modelo firmado, fecha, verificación de firma o pago.

Los hashes de contraseña WordPress no se copian como si fueran hashes Identity. La renovación propuesta usará invitaciones o restablecimiento controlado. El procedimiento, remitente y comunicación a usuarios se aprueban antes de envío real.

## Corte y convivencia

Un solo sistema escribe cada dominio. Si WordPress conserva la web pública, los formularios migrados llevan a .NET sin crear copia de la gestión en ambos. Acordar ventana, contacto, congelación, copia coherente, importación final, conciliación, prueba de permisos y decisión de habilitación. Conservar WordPress en la modalidad acordada, protegido y sin escrituras trasladadas.

La vuelta antes de escrituras .NET puede restaurar el punto coordinado probado. Después de nuevas operaciones exige conciliarlas o usar un importador inverso probado; reinstalar el ZIP perdería esas gestiones. Los criterios de retorno, tiempo máximo, autoridad de corte y registro se fijan antes de abrir el piloto.

PA15, PA18 y PA19 aportan aceptación. DC02, DC05, DC06 y DC08 condicionan el plan. El [checklist de piloto](../testing/checklist-piloto.md) registra su ejecución, todavía pendiente.

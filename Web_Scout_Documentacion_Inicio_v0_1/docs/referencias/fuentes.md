# Fuentes y procedencia del paquete

Versión 0.1 · Propuesta de arranque · 6 de noviembre de 2026.

## Línea base conservada

[Requisitos y arquitectura versión 0.1](../requisitos/base-requisitos-arquitectura-v0.1.docx) es copia binaria del documento entregado antes de este paquete. SHA-256: `90da2fd32df44a97bb621e8fa38eb93419dfa5638a3a3f92fa4205926170dc5b`. Incluye análisis, 71 RF, 12 RNF, 19 PA y trazabilidad a código y documentos originales. No se ha reescrito para generar las guías de arranque.

La fuente de código es `C:\Users\PabloLocal\Documents\Codex\2026-08-19\te\outputs\lestonnac-330-core-1.29.0.zip`, SHA-256 `3c30a96b61ab29aaa2f6e7056f6bc6f9d91313a1d06c0cc467ff22fb1f74c792`. La carpeta outputs de origen aporta documentación histórica y tema complementario. La base explica las discrepancias entre versiones y por qué 1.29 prevalece sobre el flujo de pedidos de 1.28.

## Evidencia local del documento base

Los códigos siguientes conservan la identificación de la base. Las rutas C son internas a `lestonnac-330-core/` en el ZIP; D y T son documentación y tema en la carpeta de origen. Los rangos corresponden al artefacto examinado, no al futuro proyecto .NET. El paquete no redistribuye los ZIP de código ni la documentación interna del ERP.

| Código | Archivo de origen | Localización | Evidencia |
| --- | --- | --- | --- |
| C01 | lestonnac-330-core.php | 25–80, 153–237, 294–381 | Carga de módulos, roles, capacidades, migración y alcance de revisión |
| C02 | includes/class-l330-content-types.php | 7–115 | Tipos de registros, contenidos, taxonomías y secciones iniciales |
| C03 | includes/class-l330-access.php | 6–58 | Política de lectura, revisión y descarga por contexto |
| C04 | includes/class-l330-portal.php | 29–126, 162–204, 403–414, 536–569, 594–624 | Navegación, conversación, perfil y gestiones propias |
| C05 | includes/class-l330-forms.php | 257–499, 504–531, 559–600 | Contacto, ingreso, altas históricas, actividades y solicitudes de producto |
| C06 | includes/class-l330-documents.php | 11–66 | Autorizaciones por participante, justificante compartido y modelos PDF |
| C07 | includes/class-l330-front-management.php | 421–615, 616–689, 725–856, 858–990, 1027–1109 | Gestión diaria, filtros, revisión, exportaciones y publicación |
| C08 | includes/class-l330-orders.php | 8–146 | Pedidos vigentes, permisos individuales, adjuntos, concurrencia e histórico 1.28 |
| C09 | includes/class-l330-orders-view.php | 5–103 | Listado, ficha, justificante y acciones de pedidos |
| C10 | includes/class-l330-files.php | 16–132, 172–258, 261–367, 369–418 | Almacén privado, descarga, miniaturas, perfil y referencias compartidas |
| C11 | includes/class-l330-album-uploads.php | 6–55 | Subida por fotografía, bloqueo e identificador de reintento |
| C12 | assets/js/album-uploads.js | 1–fin | Cola móvil, límite de lote, optimización, pausa y reintentos |
| C13 | includes/class-l330-communication-images.php | 7–127 | Imágenes embebidas, portada privada y publicación explícita de imágenes |
| C14 | includes/class-l330-admin.php | 397–425, 429–444 | Secciones, concesiones individuales y exportaciones generales |
| C15 | includes/class-l330-form-builder.php | 11–75, 95–117, 189–265, 318–397 | Plantillas históricas, validaciones y captura de etiquetas |
| C16 | includes/class-l330-legal.php | 20–43, 86–112, 150–212, 214–267 | Configuración legal, conservación declarada, páginas y Estatutos |
| C17 | includes/class-l330-reviewed-legal.php | 16–22, 55–121 | Recurso aprobado, aviso de cookies, consentimientos y avisos contextuales |
| C18 | assets/legal/audidat-2026-09.json | recurso completo | Textos legales incorporados en la revisión de septiembre de 2026 |
| C19 | includes/class-l330-privacy.php | 5–83 | Exportación, supresión, anonimización y eliminación de archivos |
| C20 | includes/class-l330-audit.php | 9–80 | Eventos sensibles, tabla propia, limpieza y anonimización |
| C21 | includes/class-l330-onboarding.php | 15–65, 111–310, 343–460, 488–559 | Ayuda versionada, audiencias, progreso propio y configuración |
| C22 | includes/class-l330-performance.php | 6–98 | Diagnóstico, miniaturas históricas y procesamiento por lotes |
| C23 | includes/class-l330-design.php | 62–345, 355–466, 527–548, 678–797 | Identidad, portada, login y personalización conservadora |
| C24 | includes/class-l330-page-design.php | 20–64, 137–165 | Ajustes visuales individuales de páginas |
| C25 | includes/class-l330-section-pages.php | 23–63, 171–274 | Páginas de sección y propuestas que preservan la página existente |
| C26 | includes/class-l330-public-content.php | 16–27, 50–104, 270–395 | Centro editorial y categorías públicas e internas |
| C27 | includes/class-l330-menu-manager.php | 74–240, 255–296 | Menú jerárquico, validación de enlaces y diagnóstico |
| D01 | PEDIDOS-SENCILLOS-1.29.0.md | Familias, Scouters y Cambios respecto a 1.28 | Regla de pedidos que prevalece sobre la entrega 1.28 |
| D02 | ENTREGA-1-AUTORIZACIONES-1.25.0.md | Familias, Correcciones y Sustituir el modelo | Participantes independientes, modelo vigente y justificantes conjuntos |
| D03 | ENTREGA-2-IMAGENES-LOGIN-1.26.0.md | Insertar imágenes y Personalizar el fondo | Comunicaciones con imágenes y login personalizable |
| D04 | ENTREGA-3-GALERIA-MOVIL-1.27.0.md | Lotes, Pausar y Formatos | Cola de 100 fotos, ausencia de reanudación tras cerrar y límites |
| D05 | REVISION-LEGAL-1.24.0.md | Ubicación, Consentimientos y Cookies | Condiciones de implantación de textos legales y alcance del aviso |
| D06 | MATRIZ-PERMISOS-ENTREGA-0-1.19.1.md | tabla y nota final | Separación de publicación, revisión documental y alcance de sección |
| D07 | ENTREGA-A1-COMUNICACIONES-PUBLICAS-1.21.0.md | Publicación y Copiar un aviso | Revisión editorial pública y copia independiente |
| D08 | ENTREGA-B1-AYUDA-CONTEXTUAL-1.22.1.md | Ayuda y Recorridos incluidos | Ayuda contextual y persistencia de progreso |
| D09 | ENTREGA-B2-ONBOARDING-RENDIMIENTO-1.23.0.md | Aprender, Fotografías y Caché | Ayuda práctica, previsualizaciones y caché privada |
| D10 | GUIA-INSTALACION-LESTONNAC-330.md | Roles, Flujos, Diseño y Plugins | Guía acumulada hasta Core 1.19 y dependencias del ecosistema |
| D11 | TUTORIAL-ADMINISTRACION-LESTONNAC-330.md | Partes A, B y D | Manual acumulado hasta 1.19.1 y referencia de experiencia administrativa |
| D12 | PLAN-TRABAJO-LESTONNAC-330-2026.md | 1, 4 y 11 | Plan histórico y funcionalidades previstas que requieren contrastar con código |
| D13 | ROADMAP-DEBATIBLE-LESTONNAC-330-2026.md | Decisiones y estados | Alternativas y propuestas históricas; no acredita implementación |
| D14 | VISION-MULTITENANT-FUTURA-LESTONNAC-330.md | Hipótesis y Decisiones pendientes | Visión de producto multigrupo separada del plugin |
| D15 | ENTREGA-PEDIDOS-PAGOS-1.28.0.md | flujo completo | Antecedente sustituido en 1.29; útil para migración |
| T01 | lestonnac-330-theme-3.9.1.zip | README.md, functions.php y plantillas PHP | Referencia complementaria de web pública, patrones, navegación y pie |

## Referencia organizativa

[Correspondencia con GTS.ERP](correspondencia-gts-erp.md) registra documentos de `C:\backUp\gts.erp` que inspiraron los tipos y su adaptación. [Huellas de la lectura](fuentes-gts-erp.json) identifica los archivos seleccionados disponibles al preparar el paquete. No sustituye el estado Git del proyecto de origen.

## Referencias técnicas

Consultadas o conservadas de la base con fecha 6 de noviembre de 2026. Respaldan capacidades de plataforma; el diseño concreto sigue siendo una propuesta local.

- W01: [Política oficial de soporte de NET](https://dotnet.microsoft.com/en-us/platform/support/policy/dotnet-core). Elección de NET 10 LTS y mantenimiento de parches.
- W02: [Arquitecturas de aplicaciones web en NET](https://learn.microsoft.com/en-us/dotnet/architecture/modern-web-apps-azure/common-web-application-architectures). Separación lógica de una aplicación desplegada como unidad.
- W03: [Autorización basada en recursos de ASP NET Core](https://learn.microsoft.com/en-us/aspnet/core/security/authorization/resource-based?view=aspnetcore-10.0). Comprobación de autorización sobre el recurso concreto.
- W04: [Introducción a ASP NET Core Identity](https://learn.microsoft.com/en-us/aspnet/core/security/authentication/identity?view=aspnetcore-10.0). Cuentas, credenciales, roles, claims y tokens.
- W05: [Concurrencia optimista en EF Core](https://learn.microsoft.com/en-us/ef/core/saving/concurrency). Token de concurrencia y resolución de conflictos.
- W06: [Filtros globales de EF Core](https://learn.microsoft.com/en-us/ef/core/querying/filters). Filtros de organización y borrado lógico como defensa adicional.
- W07: [Subida de archivos en ASP NET Core](https://learn.microsoft.com/en-us/aspnet/core/mvc/models/file-uploads?view=aspnetcore-10.0). Almacenamiento separado, límites y validación en servidor.
- W08: [Npgsql EF Core 10](https://www.npgsql.org/efcore/release-notes/10.0.html). Disponibilidad del proveedor PostgreSQL para EF Core 10.
- W09: [WCAG 2 2](https://www.w3.org/TR/WCAG22/). Objetivo propuesto de accesibilidad AA.
- W10: [Protección de datos por defecto de la AEPD](https://www.aepd.es/derechos-y-deberes/cumple-tus-deberes/medidas-de-cumplimiento/proteccion-de-datos-por-defecto). Minimización, acceso y conservación desde el diseño.

Referencias adicionales usadas en las guías: [formato slnx por defecto en .NET 10](https://learn.microsoft.com/en-us/dotnet/core/compatibility/sdk/10.0/dotnet-new-sln-slnx-default), [aplicación de migraciones EF Core](https://learn.microsoft.com/en-us/ef/core/managing-schemas/migrations/applying), [protección de datos por defecto AEPD](https://www.aepd.es/derechos-y-deberes/cumple-tus-deberes/medidas-de-cumplimiento/proteccion-de-datos-por-defecto), [RGPD](https://www.boe.es/buscar/doc.php?id=DOUE-L-2016-80807) y [bloqueo en artículo 32 LOPDGDD](https://www.boe.es/buscar/act.php?id=BOE-A-2018-16673#a32). La revisión legal del tratamiento real continúa pendiente en DC06.

## Límites de evidencia

No se ha ejecutado el plugin en un WordPress de producción ni se dispone de un volcado real de esa instalación. No se ha creado ni desplegado una aplicación .NET. Las comprobaciones de esta entrega son documentales, de integridad de archivos y de maquetación, descritas en [verificación](verificacion-documental.md).

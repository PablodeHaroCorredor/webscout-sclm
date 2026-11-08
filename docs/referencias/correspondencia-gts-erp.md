# Correspondencia documental con GTS ERP

Versión 0.1 · Propuesta de arranque · 6 de noviembre de 2026.

La referencia local consultada es `C:\backUp\gts.erp`, identificada como GTS.ERP en los proyectos de Codex. Se revisaron su índice, guía de trabajo y documentos temáticos representativos. No se modificó ese proyecto ni se trasladaron datos de negocio.

Se adapta la organización documental: una responsabilidad por documento, reglas en su tema y avances en Estado. Los títulos de algunos tipos se comprobaron en el inventario; la matriz no afirma haber auditado ni ejecutado todo el ERP.

| Tipo en GTS.ERP | Documento Scout | Propósito compartido | Adaptación |
| --- | --- | --- | --- |
| `docs/README.md` | [README](../README.md) | Índice y rutas de lectura | Catálogo scout por responsabilidad y propietario |
| `AGENTS.md` | [AGENTS](../../AGENTS.md) | Guía del repositorio | Reglas para el futuro trabajo; sin copiar estados del ERP |
| `docs/estado-proyecto.md` | [estado-proyecto](../estado-proyecto.md) | Estado central y evidencia | Solo documentación preparada; aplicación pendiente |
| `docs/glosario.md` | [glosario](../glosario.md) | Terminología única | Grupo, sección, participante, capacidad y expediente |
| `docs/architecture/clean-architecture-modular.md` | [arquitectura-modular](../architecture/arquitectura-modular.md) | Capas y límites modulares | Cuatro proyectos; Razor/MVC; sin XAF ni DevExpress |
| `docs/architecture/mapa-funcional-erp.md` | [mapa-funcional-scout](../architecture/mapa-funcional-scout.md) | Mapa de áreas | Gestión scout y sitio público, sin módulos económicos del ERP |
| `docs/architecture/politica-modelo-organizativo.md` | [modelo-organizativo](../architecture/modelo-organizativo.md) | Ámbito organizativo | Un grupo y secciones; sin jerarquía host/tenant/empresa |
| `docs/architecture/politica-seguridad-roles-empresa.md` | [politica-seguridad-y-permisos](../architecture/politica-seguridad-y-permisos.md) | Roles y autorización contextual | Propiedad, sección y concesiones independientes por recurso |
| `docs/architecture/politica-ciclo-vida-entidades.md` | [politica-ciclo-vida-y-versiones](../architecture/politica-ciclo-vida-y-versiones.md) | Retirada y conservación | Estados de negocio, modelos y originales inmutables |
| `docs/architecture/politica-auditoria-funcional.md` | [politica-auditoria-funcional](../architecture/politica-auditoria-funcional.md) | Eventos de negocio | Minimización y operación con menores y documentos |
| `docs/architecture/migraciones-ef-core-host-tenants.md` | [migraciones-ef-core](../architecture/migraciones-ef-core.md) | Propiedad del esquema | Un contexto y línea de migraciones propuestos, no dos bases |
| `docs/architecture/plan-migracion-strangler.md` | [plan-migracion-wordpress](../migracion/plan-migracion-wordpress.md) | Sustitución gradual | Inventario WordPress y un escritor por dominio |
| `docs/architecture/validation-ux-xaf-blazor.md` | [experiencia-y-accesibilidad](../diseno/experiencia-y-accesibilidad.md) | Patrones de validación y mensajes | Formularios Razor/MVC y errores accesibles; sin dependencia XAF |
| `docs/architecture/pwa-y-acceso-recordado.md` | [vision-multigrupo](../evolucion/vision-multigrupo.md) | Separación de mejoras posteriores | PWA no incluida; evaluar caché privada antes de proponerla |
| `docs/multitenant/esquema-multitenant-aplicacion.md` | [vision-multigrupo](../evolucion/vision-multigrupo.md) | Visión de producto compartido | Aislamiento preparado; SaaS y licencias requieren proyecto propio |
| `docs/requisitos/marco-normativo-y-privacidad.md` | [privacidad-y-conservacion](../requisitos/privacidad-y-conservacion.md) | Finalidades y conservación | Tratamiento scout; sin trasladar normativa y plazos laborales |
| `docs/desarrollo/onboarding-desarrolladores.md` | [onboarding-desarrolladores](../desarrollo/onboarding-desarrolladores.md) | Preparación del entorno | SDK soportado, servicios sintéticos y código aún por crear |
| `docs/deployment/guia-despliegue.md` | [guia-despliegue](../deployment/guia-despliegue.md) | Promoción y evidencias | Topología por decidir, artefacto único y archivos privados |
| `docs/testing/checklist-piloto-control-jornada.md` | [checklist-piloto](../testing/checklist-piloto.md) | Aceptación por candidato | PA scout, conciliación, copia y salida de piloto |
| `docs/testing/guion-pruebas-visuales-alta-empleados.md` | [guion-pruebas-visuales](../testing/guion-pruebas-visuales.md) | Recorridos manuales de interfaz | Familias, revisión, fotos, pedidos y publicación |

## Documentos propios añadidos

El paquete añade catálogo de los 71 RF y 12 RNF, casos de uso, modelo de datos, ADR, plan/backlog, riesgos, contratos de integración, estrategia de pruebas y operación/recuperación. Son necesarios para pasar del análisis de un plugin al inicio de una aplicación con datos y archivos privados.

El documento base permanece íntegro. Las nuevas políticas desarrollan la propuesta; la aprobación del alcance y de la arquitectura sigue pendiente. El catálogo es una transcripción controlada para trabajar, y cualquier cambio posterior deberá versionar la línea base.

## Decisiones que no se importan

XAF, DevExpress, Blazor como requisito de UI, empresa/tenant/host, varias líneas de migraciones, conectores Acucobol/Win, licencias y normativa laboral responden al ERP. La web scout propone Razor/MVC, organización y secciones, una base del piloto, permisos por recurso y migración desde WordPress.

La lista de elementos implementados o pruebas aprobadas del ERP tampoco acredita nada en Scout. Los nuevos checklists permanecen pendientes y los comandos se condicionan a que exista código.

## Identificación de la lectura

La referencia de Git legible fue la rama `bugfix/justificantes-incidencias-56`, HEAD `46ebd8dc8aca67ab80d6e26cc8132b28e0c407cd`. Identifica contexto local, no certifica que el árbol estuviera limpio ni que todos los archivos consultados estuvieran confirmados. Las huellas de archivos seleccionados constan en [fuentes-gts-erp.json](fuentes-gts-erp.json).

Fecha de consulta: 6 de noviembre de 2026. El índice de [fuentes](fuentes.md) separa código WordPress, documentación base, referencia organizativa y documentación técnica oficial.

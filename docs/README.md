# Índice documental del proyecto Web Scout

Versión 0.1 · Propuesta de arranque · 6 de noviembre de 2026.

Este índice organiza la documentación de arranque por responsabilidad. Todos los documentos nuevos son propuestas versión 0.1. El estado de ejecución vive en [Estado del proyecto](estado-proyecto.md); los documentos temáticos fijan reglas y criterios.

## Ruta recomendada

1. [Definición del MVP y alcance operativo](producto/definicion-mvp-piloto.md), [visión del producto](producto/vision-alcance-y-mvp.md) y [decisiones](planificacion/decisiones-y-riesgos.md).
2. [Catálogo RF/RNF](requisitos/catalogo-requisitos.md), [casos de uso](requisitos/casos-de-uso.md) y [privacidad](requisitos/privacidad-y-conservacion.md).
3. [Arquitectura](architecture/arquitectura-modular.md), [seguridad](architecture/politica-seguridad-y-permisos.md), [datos](architecture/modelo-datos.md) y [ADR](architecture/decisiones-arquitectura.md).
4. [Desarrollo](desarrollo/onboarding-desarrolladores.md), [pruebas](testing/estrategia-y-trazabilidad.md), [migración](migracion/plan-migracion-wordpress.md) y [operación](deployment/operacion-y-recuperacion.md).

## Documentos por responsabilidad

### Entrada del paquete

| Documento | Propósito | Responsable propuesto |
| --- | --- | --- |
| [Inicio del proyecto Web Scout SCLM](../README.md) | Entrada al paquete y orden recomendado para empezar. | Promotor y Coordinación |
| [Guía de trabajo del proyecto Web Scout](../AGENTS.md) | Reglas para agentes y personas que incorporen el paquete a un repositorio. | Responsable técnico |

### Estado y lenguaje común

| Documento | Propósito | Responsable propuesto |
| --- | --- | --- |
| [Estado del proyecto Web Scout](estado-proyecto.md) | Estado inicial, pendientes y evidencias sin confundir diseño e implementación. | Responsable de producto |
| [Glosario de la web scout](glosario.md) | Términos comunes para producto, código, pruebas y operación. | Producto y responsable técnico |

### Producto

| Documento | Propósito | Responsable propuesto |
| --- | --- | --- |
| [Definición del MVP y alcance operativo](producto/definicion-mvp-piloto.md) | Alcance formal, 4 módulos clave, salidas de campamento y exclusiones del MVP con Lestonnac 330. | Producto y Kraal |
| [Visión alcance y piloto de la web scout](producto/vision-alcance-y-mvp.md) | Objetivo general, actores, alcance inicial y medidas de éxito. | Promotor y Coordinación |

### Arquitectura y políticas

| Documento | Propósito | Responsable propuesto |
| --- | --- | --- |
| [Mapa funcional de la web scout](architecture/mapa-funcional-scout.md) | Fronteras de módulos, capacidades y propiedad funcional. | Responsable técnico |
| [Arquitectura modular de la web scout](architecture/arquitectura-modular.md) | Capas, dependencias, transacciones y composición del monolito. | Responsable técnico |
| [Modelo organizativo y de participantes](architecture/modelo-organizativo.md) | Diferencia entre grupo, sección, cuenta y participante declarado. | Responsable técnico |
| [Política de seguridad y permisos por recurso](architecture/politica-seguridad-y-permisos.md) | Autorización efectiva, concesiones, revocación y pruebas negativas. | Responsable técnico |
| [Política de ciclo de vida y versiones](architecture/politica-ciclo-vida-y-versiones.md) | Estados, archivo, correcciones, papelera y eliminación física. | Responsable técnico |
| [Política de auditoría funcional](architecture/politica-auditoria-funcional.md) | Eventos mínimos de negocio separados de logs y versiones documentales. | Responsable técnico |
| [Modelo de datos y contratos de integridad](architecture/modelo-datos.md) | Agregados, claves, versiones, importes y referencias compartidas. | Responsable técnico |
| [Migraciones EF Core y evolución del esquema](architecture/migraciones-ef-core.md) | Una línea de esquema, revisión y recuperación con datos existentes. | Responsable técnico |
| [Registro de decisiones de arquitectura](architecture/decisiones-arquitectura.md) | Alternativas, consecuencias y evidencia para ratificar las ADR iniciales. | Responsable técnico |

### Planificación

| Documento | Propósito | Responsable propuesto |
| --- | --- | --- |
| [Decisiones de arranque riesgos y responsabilidades](planificacion/decisiones-y-riesgos.md) | Registro único de DC01 a DC10 y riesgos que condicionan las entregas. | Responsable de producto |
| [Plan de entregas y backlog inicial](planificacion/plan-entregas-y-backlog.md) | Fases E0 a E5, épicas, primeras tareas y condiciones de cierre. | Responsable de producto |

### Diseño

| Documento | Propósito | Responsable propuesto |
| --- | --- | --- |
| [Experiencia de uso y accesibilidad](diseno/experiencia-y-accesibilidad.md) | Navegación, patrones de pantalla, mensajes y revisión móvil. | Diseño y QA |
| [Manual de identidad corporativa y guía de estilo](diseno/manual-identidad-corporativa.md) | Identidad visual SCLM y multitenant, pañoletas, tokens CSS, componentes UI, accesibilidad y tono de voz. | Diseño y Comunicación |

### Contratos e integraciones

| Documento | Propósito | Responsable propuesto |
| --- | --- | --- |
| [Contratos de API archivos y servicios externos](integraciones/contratos-api-y-servicios.md) | Casos de uso HTTP, archivos preparados, correo y comportamiento de error. | Responsable técnico |

### Requisitos y privacidad

| Documento | Propósito | Responsable propuesto |
| --- | --- | --- |
| [Casos de uso prioritarios del piloto](requisitos/casos-de-uso.md) | Recorridos de negocio con variantes y requisitos asociados. | Producto y Coordinación |
| [Privacidad conservación y contenido legal](requisitos/privacidad-y-conservacion.md) | Tratamientos, minimización, decisiones de conservación y revisión previa al piloto. | Responsable del tratamiento y asesoría |
| [Catálogo de requisitos funcionales y no funcionales](requisitos/catalogo-requisitos.md) | Transcripción trazable de los 71 RF y 12 RNF de la base. | Producto y responsable técnico |

### Desarrollo

| Documento | Propósito | Responsable propuesto |
| --- | --- | --- |
| [Incorporación al desarrollo](desarrollo/onboarding-desarrolladores.md) | Preparación del entorno y primera contribución al futuro repositorio. | Responsable técnico |
| [Convenciones de desarrollo y flujo de cambios](desarrollo/convenciones-y-flujo.md) | Reglas de código, revisión, configuración y documentación del repositorio. | Responsable técnico |

### Migración

| Documento | Propósito | Responsable propuesto |
| --- | --- | --- |
| [Plan de migración desde WordPress](migracion/plan-migracion-wordpress.md) | Inventario, correspondencias, ensayos y corte con conciliación. | Responsable de migración y Secretaría |

### Despliegue y operación

| Documento | Propósito | Responsable propuesto |
| --- | --- | --- |
| [Guía de despliegue y promoción](deployment/guia-despliegue.md) | Entornos, artefactos e instalación controlada del futuro sistema. | Operación y responsable técnico |
| [Operación recuperación y continuidad](deployment/operacion-y-recuperacion.md) | Señales de salud, copias, incidentes y restauración verificable. | Operación |

### Evolución posterior

| Documento | Propósito | Responsable propuesto |
| --- | --- | --- |
| [Plataforma multitenant para Scouts de Castilla-La Mancha (SCLM)](evolucion/vision-multigrupo.md) | Modelo multitenant, aislamiento en EF Core, tematización grupal y gobernanza federativa. | Promotor y responsable técnico |
| [Propuesta técnica de arquitectura multitenant SCLM](evolucion/propuesta-arquitectura-multitenant-sclm.md) | Especificación detallada de aislamiento, resolución por subdominio y motor de pañoleta/colores. | Responsable técnico |

### Validación

| Documento | Propósito | Responsable propuesto |
| --- | --- | --- |
| [Estrategia de pruebas y trazabilidad](testing/estrategia-y-trazabilidad.md) | Capas de comprobación, escenarios de aceptación y evidencia requerida. | QA y responsable técnico |
| [Checklist de aceptación del piloto](testing/checklist-piloto.md) | Plantilla de ejecución para un candidato concreto y decisión de salida. | Coordinación, QA y operación |
| [Guion de pruebas visuales y móviles](testing/guion-pruebas-visuales.md) | Recorridos manuales de interfaz, accesibilidad y recuperación de errores. | QA y diseño |

### Referencias y verificación

| Documento | Propósito | Responsable propuesto |
| --- | --- | --- |
| [Correspondencia documental con GTS ERP](referencias/correspondencia-gts-erp.md) | Tipos adaptados del proyecto de referencia y límites de reutilización. | Responsable técnico |
| [Fuentes y procedencia del paquete](referencias/fuentes.md) | Identificación de la base, código de origen y documentación de referencia. | Responsable técnico |
| [Verificación de la entrega documental](referencias/verificacion-documental.md) | Comprobaciones efectivamente realizadas sobre este paquete. | Autoría documental |

## Línea base y lectura Word

El [documento base conservado](requisitos/base-requisitos-arquitectura-v0.1.docx) recoge el análisis original completo. La [guía Word de arranque](../Guia_Inicio_Proyecto_Web_Scout_v0_1.docx) resume la lectura y la primera sesión de trabajo. El catálogo Markdown permite consultar los RF sin depender de Word.

No hay código de aplicación incluido. Incorporar estos archivos al futuro repositorio y registrar sus referencias en la solución será una tarea de E1. Los artefactos de evidencia de cada ejecución se añaden cuando existan, evitando información personal.

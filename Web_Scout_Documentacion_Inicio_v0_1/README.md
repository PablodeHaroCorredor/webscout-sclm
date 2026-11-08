# Proyecto Web Scout SCLM

Versión 1.0 · Refinamiento multitenant para Scouts de Castilla-La Mancha · 8 de noviembre de 2026.

La plataforma **Web Scout SCLM** es una solución web multitenant en .NET 10 LTS concebida para que **todos los grupos scout de Scouts de Castilla-La Mancha (SCLM)** dispongan de su propia aplicación web de gestión, completamente personalizada con su temática, escudo y colores de pañoleta, con aislamiento estricto de datos. El **Grupo Scout Lestonnac 330** actúa como grupo piloto de validación funcional y caso de referencia para la migración de datos.

El [índice documental](docs/README.md) organiza las referencias por responsabilidad. El [documento base](docs/requisitos/base-requisitos-arquitectura-v0.1.docx) conserva la propuesta inicial completa y el [catálogo navegable](docs/requisitos/catalogo-requisitos.md) reproduce sus requisitos y criterios de aceptación. El estado efectivo del trabajo se concentra en [Estado del proyecto](docs/estado-proyecto.md).

## Lectura para empezar

1. Producto y Coordinación: [visión multitenant SCLM](docs/evolucion/vision-multigrupo.md), [visión y alcance](docs/producto/vision-alcance-y-mvp.md), [plan de entregas y backlog](docs/planificacion/plan-entregas-y-backlog.md) y [decisiones y riesgos](docs/planificacion/decisiones-y-riesgos.md).
2. Equipo técnico: [arquitectura modular](docs/architecture/arquitectura-modular.md), [decisiones de arquitectura (ADR)](docs/architecture/decisiones-arquitectura.md), [mapa funcional](docs/architecture/mapa-funcional-scout.md), [seguridad](docs/architecture/politica-seguridad-y-permisos.md) y [modelo de datos](docs/architecture/modelo-datos.md).
3. Experiencia y diseño: [experiencia, accesibilidad y tematización grupal](docs/diseno/experiencia-y-accesibilidad.md).
4. Incorporación: [guía de desarrollo](docs/desarrollo/onboarding-desarrolladores.md) y [guía de trabajo del repositorio](AGENTS.md).
5. Preparación del piloto: [estrategia de pruebas](docs/testing/estrategia-y-trazabilidad.md), [checklist](docs/testing/checklist-piloto.md), [migración WordPress](docs/migracion/plan-migracion-wordpress.md) y [despliegue](docs/deployment/guia-despliegue.md).

## Criterios de producto que se mantienen

- Plataforma multitenant con aislamiento lógico estricto por `OrganizationId` y filtros globales en EF Core.
- Personalización de identidad scout (pañoleta, paleta y escudo) mediante tokens CSS y validación de contraste WCAG 2.2 AA.
- Pedidos sencillos de la versión 1.29: un justificante adjunto no confirma un pago ni bloquea una entrega.
- Autorización independiente por participante; firma del PDF fuera de la web y versiones conservadas.
- Un justificante de actividad puede compartirse solo dentro de la misma cuenta y actividad.
- La capacidad de publicar, revisar documentos y gestionar pedidos se concede por separado.
- El piloto valida el sistema con Lestonnac 330 antes del despliegue masivo a los demás grupos de SCLM.

## Primera sesión de trabajo

La sesión inicial debe resolver alcance del piloto, modalidad de web pública, proveedor de despliegue y responsables. El resultado será el cierre de DC01, DC02, DC03 y la preparación de DC05, DC06 y DC08 en el [registro de decisiones](docs/planificacion/decisiones-y-riesgos.md). Después se puede abrir la tarea IN01 de E0. No hace falta completar todas las decisiones editoriales para probar el acceso y el almacenamiento privado.

Este paquete contiene documentación, una copia íntegra de la base y una guía Word de lectura. La solución agnóstica `WebScout.slnx`, los proyectos, las migraciones y las pruebas de aplicación son entregables previstos de E1, no archivos incluidos aquí.

## Cómo mantenerlo

Una regla tiene un propietario documental; los demás archivos la enlazan. Requisitos y prioridades viven en el catálogo; decisiones y riesgos, en su registro; pruebas y resultados de cada ejecución, en el checklist y en el estado. La [correspondencia con GTS ERP](docs/referencias/correspondencia-gts-erp.md) explica qué tipos documentales se han adaptado.

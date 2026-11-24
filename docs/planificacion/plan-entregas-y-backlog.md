# Plan de entregas y backlog inicial

Versión 0.1 · Propuesta de arranque · 6 de noviembre de 2026.

El plan empieza por probar acceso y documentos y completa después gestión cotidiana y web pública. Cada incremento debe demostrar un recorrido útil, no solo una pantalla o una clase.

## Entregas

| Entrega | Resultado | Salida exigida | Referencia orientativa |
| --- | --- | --- | --- |
| E0 Descubrimiento y prototipo | Decisiones críticas, inventario y pruebas técnicas | IN01 e IN03 a IN06 con evidencia y riesgos actualizados | 1 a 2 semanas |
| E1 Base y acceso | Solución, identidad, políticas, archivos, auditoría y cola | Recurso privado, revocación, persistencia y recuperación | 2 a 3 semanas |
| E2 Actividades y documentos | Comunicación privada, modelos y respuestas | Dos participantes, corrección, recibo compartido y revisión restringida | 3 a 4 semanas |
| E3 Gestión cotidiana | Fotos, pedidos, ingreso, perfil y ayuda | Lotes resistentes, pedido sencillo y trámites completos | 3 a 4 semanas |
| E4 Web pública y configuración | Páginas, noticias, marca, navegación y textos | Equivalencia pública y ajustes realmente necesarios | 2 a 4 semanas |
| E5 Piloto | Alta de datos reales, formación y puesta en marcha | PA aplicables y restauración ensayada | 1 a 3 semanas |

La referencia total de la base es 12 a 20 semanas con dos desarrolladores experimentados y apoyo parcial de QA, diseño y operación. Es una hipótesis para estimar tras E0, condicionada por infraestructura y alcance editorial. No asigna fechas ni presupuesto comprometidos.

## Épicas y cobertura

| Épica | Capacidad | Entrega | Requisitos | Aceptación principal | Resultado demostrable |
| --- | --- | --- | --- | --- | --- |
| BL01 | Base de acceso y archivos | E1 | RF01–RF08, RF35–RF38, RF40, RF60 | PA01–PA03, PA09, PA14 | Cuenta activa, revocación y documento privado recuperable |
| BL02 | Comunicaciones y resumen | E2 y E4 | RF09, RF17–RF23 | PA12, PA17 | Publicación privada y copia pública sin activos privados |
| BL03 | Contacto y solicitudes | E3 | RF10–RF14 | PA11, PA13 | Recepción persistida y seguimiento sin alta automática |
| BL04 | Formularios dinámicos en actividades | E2 | RF16, RF24, RF34 | PA04–PA05 | Preguntas propias por actividad con respuestas inmutables y exportables |
| BL05 | Actividades y respuestas | E2 | RF24–RF34 | PA02–PA06 | Dos participantes, corrección individual y recibo compartido |
| BL06 | Fotos y miniaturas | E3 | RF39, RF41–RF47 | PA09–PA10 | Cien fotos con confirmación, pausa y fin parcial |
| BL07 | Catálogo y pedidos sencillos | E3 | RF48–RF53 | PA07–PA08 | Entrega independiente del adjunto |
| BL08 | Privacidad y textos | E1 y E4 | RF55–RF59 | PA13–PA14 | Elecciones versionadas y proceso de conservación definido |
| BL09 | Sitio público y diseño | E4 | RF61–RF63 | PA12, PA16–PA17 | Páginas, navegación, portada y revisiones sin pérdida |
| BL10 | Configuración y operación | E1 y E3 | RF64–RF65 | PA11, PA16, PA19 | Ajustes parciales y diagnóstico de trabajos y copias |
| BL11 | Ayuda contextual | E3 | RF66–RF67 | PA17 | Guías por permiso y progreso persistente |

Cada BL empieza Pendiente y se desglosará en tareas revisables de una capacidad. Los rangos RF remiten al [catálogo](../requisitos/catalogo-requisitos.md); PA remite a [validación](../testing/estrategia-y-trazabilidad.md). Los RNF se aplican transversalmente y no forman una entrega final separada.

## Trabajo concreto de E0

| ID | Trabajo | Responsable propuesto | Entrega revisable | Condición de cierre |
| --- | --- | --- | --- | --- |
| IN01 | Taller de alcance, actores y prioridades | Producto y Coordinación | DC01 y responsables registrados; lista de funciones del piloto | Alcance y aceptación entendidos por quien valida |
| IN03 | Prototipo de autorización | Técnica | Cuenta A/B, dos secciones y dos organizaciones sintéticas | Invocación directa y revocación no cruzan recursos |
| IN04 | Prototipo documental y móvil | Técnica y QA | PDF inmutable, archivo preparado, miniatura y reintento de foto | Huella y referencias correctas; caída no pierde lo confirmado |
| IN05 | Prototipo de transacción y operación | Técnica y operación | Estado, auditoría y outbox con proveedor fallido | Reinicio recupera aviso y conflicto no sobreescribe |
| IN06 | Estimación y plan de entorno | Técnica, operación y promotor | DC03, DC08; propuesta de DC06 y backlog dimensionado | Alternativa desplegable, coste y ensayos de recuperación previstos |

IN03 a IN05 son trabajo de implementación futuro sobre un prototipo con datos sintéticos. En esta versión se define su resultado y todavía no se da por ejecutado.

## Condición de tarea preparada

Debe tener RF y PA relacionados, actor, entrada, resultado, permisos, límites, errores, dependencia de datos y criterio de aceptación. Si cambia esquema debe identificar dueño, migración, compatibilidad y prueba de conservación. Si cambia UI debe identificar recorrido y estados de pantalla.

## Condición de tarea terminada

Código revisado; pruebas pertinentes sobre candidato identificado; sin pérdida de datos o ampliación de permiso; recorrido visual cuando cambie interacción; documentación temática actualizada; y evidencia registrada. Terminar desarrollo no equivale a desplegar ni a aceptar un piloto.

## Primera demostración vertical

La primera demostración después de E1 será CU04 y CU05 sobre una actividad sintética: dos participantes, justificante común, revisor por sección, corrección individual y enlace denegado a otra cuenta. Concentra las invariantes de mayor riesgo antes de completar todas las opciones del CMS.

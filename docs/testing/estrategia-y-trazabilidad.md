# Estrategia de pruebas y trazabilidad

Versión 0.1 · Propuesta de arranque · 6 de noviembre de 2026.

La aceptación demostrará reglas, aislamiento, integridad y recuperación sobre una versión identificada. El paquete documental no incluye código ni resultados de pruebas de aplicación. Todos los PA permanecen Sin ejecutar.

## Capas y propósito

| Capa | Qué verifica | Datos y entorno |
| --- | --- | --- |
| Dominio | Plazo civil, límites de cancelación, versiones y vínculos | Reloj controlado y ejemplos sintéticos de reglas |
| Integración | Autorización efectiva, transacciones, concurrencia, migración, outbox y archivo | PostgreSQL y almacenamiento real o equivalente del proveedor seleccionado |
| Navegador | Formularios, errores, navegación, descarga, cola móvil y accesibilidad | Candidato completo y matriz de navegadores acordada |
| Operación | Reinicio, carga, restauración y corte | Topología representativa, copias y proveedor de correo de prueba |

No sustituir el proveedor relacional por una base en memoria para concluir que funcionan FKs, transacciones o concurrencia. Los dobles de correo y objetos sirven para aislar reglas; la integración final debe probar también contratos del proveedor real.

## Datos de referencia

Dos organizaciones sintéticas, con familias A/B, una cuenta sin sección, un adulto, un Scouter normal, uno revisor, otro gestor de pedidos, Coordinación y Administración. Dos secciones, tres participantes declarados, una actividad gratuita, otra con coste, una cerrada y una con modelo sustituido. Respuestas históricas sin modelo conocido, dos hermanos con recibo común y pedido heredado 1.28.

Añadir archivos de prueba válidos, tipo/contenido discrepante, tamaño límite, PDF de bytes conocidos y fotografías JPG/PNG/WebP. Preparar 100 fotos y casos de respuesta de red perdida, caída tras guardar objeto y conflicto entre dos sesiones. No usar datos personales reales en pruebas ordinarias.

## Pruebas de aceptación de la base

| Prueba de aceptación | Requisitos vinculados | Resultado exigido |
| --- | --- | --- |
| PA01 Familia A frente a familia B | RF03–RF05, RF28, RF35–RF38 | No cruzan documentos, metadatos, miniaturas ni justificantes |
| PA02 Revisor con dos secciones y revocación | RF04–RF06, RF32, RF34 | Solo revisa su alcance; la revocación afecta a la siguiente operación |
| PA03 Contenido de todo el grupo | RF03, RF18, RF32 | Es visible como comunicación general sin abrir respuestas privadas generales |
| PA04 Dos hermanos y corrección | RF26–RF33 | Dos respuestas y un solo justificante compartido; corregir una conserva la otra |
| PA05 Modelo sustituido y plazo | RF24–RF29 | Rechaza formulario obsoleto y fuera de plazo sin crear registros parciales |
| PA06 Huellas y archivos históricos | RF26, RF29, RF31, RF38 | Bytes idénticos, versiones conservadas y borrado seguro de referencias |
| PA07 Pedido sencillo | RF48–RF54 | Recibo desde Solicitado; entrega sin recibo; no existen acciones económicas 1.28 |
| PA08 Rectificación administrativa de pedido | RF50, RF52, RF60 | El responsable cambia entre los seis estados con historial; el titular tiene sus límites |
| PA09 Respuesta perdida y concurrencia | RF38, RF40, RF44; RNF07 | Reintento no duplica; edición obsoleta no sobreescribe |
| PA10 Cien fotos y error parcial | RF41–RF47 | Selecciones acumulables, pausa, reintento y finalización parcial conservan avances |
| PA11 SMTP y trabajador interrumpidos | RF10, RF14, RF40; RNF07 | Trámite guardado, aviso reintentable y cola recuperable tras reinicio |
| PA12 Copia pública y comentarios | RF19–RF23 | No filtra archivos privados, requiere capacidad y valida escritura por recurso |
| PA13 Consentimiento y versiones legales | RF55–RF58 | Opcional sin premarcar; texto y versión correctos; aviso técnico no activa terceros |
| PA14 Supresión y conservación | RF31, RF38, RF59, RF60 | No borra archivo aún referenciado y explica excepciones de retención |
| PA15 Compatibilidad histórica | RF15, RF16, RF54, RF68–RF70 | Sin pérdida de etiquetas, estados, evidencias ni atribuciones inventadas |
| PA16 Configuración y restauración visual | RF61–RF64 | Guardado parcial y reset no alteran datos o ajustes de otras áreas |
| PA17 Ayuda y accesibilidad | RF66–RF67; RNF02 | Guías por permisos, progreso persistente, teclado, foco y reflujo correcto |
| PA18 Carga y aislamiento de organización | RF05, RF34, RF53; RNF03, RNF08 | Objetivos medidos sin cruces entre dos organizaciones sintéticas |
| PA19 Corte y recuperación | RF68–RF71; RNF06 | Conciliación completa y restauración dentro de objetivos acordados |

Las relaciones RF/PA de esta tabla se conservan de la base. Los PA no sustituyen todos los criterios individuales del [catálogo](../requisitos/catalogo-requisitos.md): cada RF necesita sus comprobaciones positivas, negativas y de límite pertinentes. Las [épicas](../planificacion/plan-entregas-y-backlog.md) asignan los 71 RF al trabajo, incluidos los que no aparecen directamente en un PA de extremo a extremo.

## Comprobaciones transversales

Repetir autorización mediante URL directa, API, CSV, miniatura, versión anterior, enlace copiado y tarea. Revocar una concesión con sesión activa y comprobar la siguiente operación. Probar referencias de otra organización, payload manipulado, doble envío y versión obsoleta.

Para RNF03 usar 50 sesiones y listados de 25 elementos sin transferencia de archivos; medir p95 de servidor menor de 600 ms en el perfil acordado. Para RNF04 documentar dispositivo/red/caché y LCP objetivo inferior a 2,5 s. El dataset de referencia propuesto es 500 cuentas, 20.000 respuestas, 50.000 fotos y 250 GB; DC08 contrasta producción y coste. No generar ni almacenar 250 GB sin necesidad para una prueba que solo consulta metadatos.

Medir recuperación RNF06 con copia coherente y tiempos observados. Revisar logs/correos para RNF09 y exportación/importación para RNF11. RNF02 requiere teclado y lector de pantalla además de herramientas automáticas; usar el [guion visual](guion-pruebas-visuales.md).

## Evidencia y salida

Cada ejecución registra commit, artefacto, esquema, entorno, versiones de servicios, datos, pasos, resultado esperado/real, logs depurados e incidencia. Distinguir Pasó, Falló, Bloqueada y No ejecutada. Un caso omitido no pasa por ausencia de error.

La salida exige P0 del alcance sin incidencias bloqueantes, P1 diferidos explícitos, políticas ratificadas, migración conciliada, envío de prueba autorizado y restauración medida. La equivalencia completa incluye los P1 aplicables. El [checklist](checklist-piloto.md) se completa sobre el candidato y se resume en [Estado](../estado-proyecto.md).

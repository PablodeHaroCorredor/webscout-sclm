# Checklist de aceptación del piloto

Versión 0.1 · Propuesta de arranque · 6 de noviembre de 2026.

**Estado inicial: sin ejecutar.** Esta plantilla se copia por candidato, conserva los resultados e incidencias y enlaza la evidencia. Marcar una casilla requiere comprobar su resultado; no acredita ejecución por estar escrita aquí.

## Identificación de la ejecución

| Campo | Valor inicial |
| --- | --- |
| Fecha y personas que ejecutan | Por completar |
| Commit y hash del artefacto | Por completar |
| Entorno, URL y versión de esquema | Por completar |
| Alcance y RF/P1 diferidos aprobados | Por completar |
| Proveedor de base, objetos y correo de prueba | Por completar |
| Dataset, navegadores y móvil físico | Por completar |
| Referencias de copias y plan de retorno | Por completar |

## Preparación

- [ ] Alcance DC01/DC02 y responsables definidos; se sabe qué permanece en WordPress.
- [ ] DC03, DC05, DC06, DC08 y DC10 resueltos para los servicios y datos del piloto.
- [ ] Datos de ensayo y destinatarios autorizados; no se envían mensajes reales por un error de entorno.
- [ ] Usuarios y concesiones representan familias, secciones, revisor y gestor de pedidos independientes.
- [ ] Archivo privado sin sesión, miniatura y URL antigua denegados; no hay credenciales en logs.

## Aceptación funcional y técnica

- [ ] **PA01 Familia A frente a familia B** (RF03–RF05, RF28, RF35–RF38): No cruzan documentos, metadatos, miniaturas ni justificantes. Resultado y evidencia: por completar.
- [ ] **PA02 Revisor con dos secciones y revocación** (RF04–RF06, RF32, RF34): Solo revisa su alcance; la revocación afecta a la siguiente operación. Resultado y evidencia: por completar.
- [ ] **PA03 Contenido de todo el grupo** (RF03, RF18, RF32): Es visible como comunicación general sin abrir respuestas privadas generales. Resultado y evidencia: por completar.
- [ ] **PA04 Dos hermanos y corrección** (RF26–RF33): Dos respuestas y un solo justificante compartido; corregir una conserva la otra. Resultado y evidencia: por completar.
- [ ] **PA05 Modelo sustituido y plazo** (RF24–RF29): Rechaza formulario obsoleto y fuera de plazo sin crear registros parciales. Resultado y evidencia: por completar.
- [ ] **PA06 Huellas y archivos históricos** (RF26, RF29, RF31, RF38): Bytes idénticos, versiones conservadas y borrado seguro de referencias. Resultado y evidencia: por completar.
- [ ] **PA07 Pedido sencillo** (RF48–RF54): Recibo desde Solicitado; entrega sin recibo; no existen acciones económicas 1.28. Resultado y evidencia: por completar.
- [ ] **PA08 Rectificación administrativa de pedido** (RF50, RF52, RF60): El responsable cambia entre los seis estados con historial; el titular tiene sus límites. Resultado y evidencia: por completar.
- [ ] **PA09 Respuesta perdida y concurrencia** (RF38, RF40, RF44; RNF07): Reintento no duplica; edición obsoleta no sobreescribe. Resultado y evidencia: por completar.
- [ ] **PA10 Cien fotos y error parcial** (RF41–RF47): Selecciones acumulables, pausa, reintento y finalización parcial conservan avances. Resultado y evidencia: por completar.
- [ ] **PA11 SMTP y trabajador interrumpidos** (RF10, RF14, RF40; RNF07): Trámite guardado, aviso reintentable y cola recuperable tras reinicio. Resultado y evidencia: por completar.
- [ ] **PA12 Copia pública y comentarios** (RF19–RF23): No filtra archivos privados, requiere capacidad y valida escritura por recurso. Resultado y evidencia: por completar.
- [ ] **PA13 Consentimiento y versiones legales** (RF55–RF58): Opcional sin premarcar; texto y versión correctos; aviso técnico no activa terceros. Resultado y evidencia: por completar.
- [ ] **PA14 Supresión y conservación** (RF31, RF38, RF59, RF60): No borra archivo aún referenciado y explica excepciones de retención. Resultado y evidencia: por completar.
- [ ] **PA15 Compatibilidad histórica** (RF15, RF16, RF54, RF68–RF70): Sin pérdida de etiquetas, estados, evidencias ni atribuciones inventadas. Resultado y evidencia: por completar.
- [ ] **PA16 Configuración y restauración visual** (RF61–RF64): Guardado parcial y reset no alteran datos o ajustes de otras áreas. Resultado y evidencia: por completar.
- [ ] **PA17 Ayuda y accesibilidad** (RF66–RF67; RNF02): Guías por permisos, progreso persistente, teclado, foco y reflujo correcto. Resultado y evidencia: por completar.
- [ ] **PA18 Carga y aislamiento de organización** (RF05, RF34, RF53; RNF03, RNF08): Objetivos medidos sin cruces entre dos organizaciones sintéticas. Resultado y evidencia: por completar.
- [ ] **PA19 Corte y recuperación** (RF68–RF71; RNF06): Conciliación completa y restauración dentro de objetivos acordados. Resultado y evidencia: por completar.

Cada PA se detalla en la [estrategia](estrategia-y-trazabilidad.md), el [catálogo](../requisitos/catalogo-requisitos.md) y los [casos de uso](../requisitos/casos-de-uso.md). Conservar capturas depuradas y evidencia de persistencia; evitar documentos reales en tickets de incidencias.

## Corte y operación

- [ ] Inventario y manifiestos completos; registros, estados y relaciones conciliados sin discrepancias abiertas.
- [ ] Hash y tamaño de los originales trasladados comprobados; históricos desconocidos siguen identificados.
- [ ] Un escritor por dominio, congelación acordada, redirecciones y retorno documentados.
- [ ] Copia coordinada y restauración ensayada; RPO/RTO medidos y aceptados.
- [ ] Outbox persiste avisos ante caída; destinatario, remitente y correo de prueba revisados.
- [ ] Políticas legales/conservación ratificadas y personal responsable formado.
- [ ] Móvil físico, teclado y recorridos esenciales completados; incidencias vinculadas.

## Decisión

Registrar resultado como Aceptado, Aceptado con limitaciones explícitas o No aceptado, con alcance, incidencias, riesgo residual, persona y fecha. Una limitación no permite dar por cumplida una regla P0 del módulo sin acordar un cambio de alcance que la resuelva.

En el estado inicial la decisión es **Pendiente**. No se ha desplegado la aplicación ni se han ejecutado estos casos. Tras ejecutar, actualizar [Estado del proyecto](../estado-proyecto.md) con la referencia de la checklist cumplimentada y la versión que realmente se probó.

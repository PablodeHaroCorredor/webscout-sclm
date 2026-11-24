# Decisiones de arranque riesgos y responsabilidades

Versión 0.1 · Propuesta de arranque · 6 de noviembre de 2026.

Las decisiones se resolverán al llegar a la entrega que depende de ellas. Una recomendación inicial permite preparar alternativas, pero no registra una aprobación que todavía no se haya producido. El estado inicial de DC01 a DC10 es Pendiente.

## Registro de decisiones

| ID | Decisión | Recomendación inicial | Responsable propuesto | Momento límite |
| --- | --- | --- | --- | --- |
| DC01 | Alcance asociativo y multitenancy | Plataforma multitenant para grupos de SCLM con tematización y aislamiento lógico; Lestonnac 330 como piloto de implantación | Promotor y SCLM | Antes de cerrar E0 |
| DC02 | Web pública en el piloto | **Cerrada por DC11:** la web pública se construye en la nueva plataforma (E4) | Producto y Coordinación | Resuelta |
| DC03 | Hosting, base, archivos y correo | .NET 10 soportado, PostgreSQL, objetos privados y proveedor observable | Técnica y operación | Selección en E0; configuración antes de E1 |
| DC04 | Censo de personas y familias | Participantes declarados por actividad; censo fuera del piloto | Coordinación | Antes de cambiar el modelo de respuestas |
| DC05 | Formularios históricos abiertos | **Cerrada por DC11:** no aplica; no se importan formularios anteriores | Secretaría | Resuelta |
| DC06 | Privacidad y conservación | Ratificar finalidades, proveedores, textos y periodos | Responsable del tratamiento y asesoría | Antes de usar datos reales |
| DC07 | Mejoras de control | Motivo de corrección, MFA y validación editorial unificada | Administración y Coordinación | Antes de aceptar sus casos de uso |
| DC08 | Volumen, disponibilidad y recuperación | Medir datos y coste de objetivos RNF | Operación y promotor | Antes de contratar y validar carga |
| DC09 | Personalización y tematización | Tokens CSS validados, pañoleta SVG y contraste WCAG 2.2 AA por tenant | Equipo editorial y diseño | Antes de cerrar E4 |
| DC10 | Presupuesto, repositorio y aceptación | Entregas pequeñas, responsables de revisión y flujo Git acordado | Promotor y responsables | Antes de planificar implementación comprometida |
| DC11 | Migración y convivencia con WordPress | **Aprobada el 24/11/2026:** la plataforma arranca con datos nuevos; no se migran datos ni se convive con WordPress. Se retiran RF15, RF54, RF68–RF70, PA15, IN02, BL12, el plan de migración y los riesgos R03, R04 y R06; DC02 y DC05 quedan cerradas (issue #54) | Promotor | Resuelta |

Para cerrar una DC se registrarán respuesta, fecha, persona, motivo, RF/RNF afectados y documentos actualizados. Las ADR técnicas se mantienen en su [registro](../architecture/decisiones-arquitectura.md).

## Riesgos iniciales

| ID | Riesgo y efecto | Prevención | Señal y respuesta | Prioridad |
| --- | --- | --- | --- | --- |
| R01 | Introducir pasos económicos rígidos que bloqueen la entrega de pedidos | RF50 a RF53 y PA07 | Aparecen importe confirmado o verificación de pago: corregir alcance | Alta |
| R02 | Acceso a documentos de otra familia, sección u organización | Política por recurso y pruebas negativas | Descarga o metadato ajeno: detener habilitación afectada y corregir | Crítica |
| R05 | Duplicación o pérdida al reintentar | Clave persistida, versión y preparación | Foto duplicada o reemplazo perdido: añadir regresión y corregir | Alta |
| R07 | Hosting sin soporte o recuperación suficiente | Prototipo y ensayo de restauración | RPO/RTO o privacidad no demostrados: cambiar propuesta o servicio | Alta |
| R08 | Retención o consentimiento asumidos | DC06 y evidencia legal versionada | Campo sin origen o plazo universal sin base: devolver a revisión | Alta |
| R09 | Construcción del CMS retrasa portal | Separar E4 y concretar DC09 | Aumenta backlog editorial sin uso real: repriorizar | Media |
| R10 | Correo falla sin aviso operativo | Outbox, reintentos y panel de fallos | Tareas antiguas o fallidas: alerta y recuperación | Alta |
| R11 | PWA, censo o SaaS amplían el piloto | Cambio de alcance explícito | Nuevos actores o flujos sin RF: registrar incremento | Media |
| R12 | Código verde sin prueba móvil real | Guion visual y dispositivo físico | Solo emulación o tests auxiliares: mantener PA pendiente | Alta |

## Criterios para tratar incidencias

Bloquean una función la exposición de datos, corrupción o pérdida documental, incapacidad de tramitar el recorrido esencial, falta de conciliación o imposibilidad de recuperar el sistema dentro del objetivo acordado. Los defectos visuales menores se registran con su impacto; los que impiden teclado, comprensión o completar un formulario son funcionales y pueden bloquear el recorrido.

## Responsabilidad de los cambios

Producto acepta cambios de alcance y prioridad; Coordinación valida los recorridos; técnica revisa arquitectura e integridad; operación acepta recuperación y despliegue; privacidad ratifica tratamientos. Un cambio documentará beneficio, coste, RF afectados, prueba, migración y decisión antes de incorporarlo al incremento comprometido.

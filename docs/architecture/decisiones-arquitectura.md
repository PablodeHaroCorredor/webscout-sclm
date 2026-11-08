# Registro de decisiones de arquitectura

Versión 0.1 · Propuesta de arranque · 6 de noviembre de 2026.

Las ADR01 a ADR08 desarrollan las decisiones iniciales de la base 0.1. ADR09 concreta la propiedad del contexto y la línea de migraciones. Todas parten como Propuestas; su ratificación registrará persona responsable, fecha y evidencia. Las decisiones de producto y presupuesto siguen en el [registro DC](../planificacion/decisiones-y-riesgos.md).

| ADR | Decisión propuesta | Alternativa considerada | Consecuencia | Cómo se valida |
| --- | --- | --- | --- | --- |
| ADR01 | Monolito modular | Microservicios iniciales | Una unidad de aplicación y transacciones locales; límites por carpetas y contratos | Caso de uso de autorización completo y tests de dependencias |
| ADR02 | .NET 10 LTS | Iniciar con versiones próximas al fin de soporte o preview | Ventana de mantenimiento y paquetes compatibles | SDK fijado, restore reproducible y proveedor compatible |
| ADR03 | Razor Pages o MVC y JS progresivo | SPA o interfaz que dependa de circuito persistente | Formularios simples; interacción compleja localizada | Prueba móvil con subida interrumpida y navegación normal |
| ADR04 | Base relacional y objetos privados separados | Guardar todas las fotos en base o bajo raíz web pública | Dos almacenes coordinados, claves opacas y proceso de conciliación | Archivo privado, sustitución y caída entre almacenes |
| ADR05 | Capacidades y autorización por recurso | Roles genéricos sin propiedad ni alcance | Una política reutilizada por interfaz, API, CSV y archivos | PA01 a PA03 con invocación directa y revocación |
| ADR06 | OrganizationId desde el piloto | Aislamiento añadido después | Claves y relaciones contextualizadas sin construir un SaaS comercial pesado | Dos organizaciones sintéticas y referencias cruzadas rechazadas |
| ADR07 | Outbox y trabajos duraderos | Enviar correo en la transacción web o usar cola solo en memoria | Reintentos observables y operación explícita del worker | Caída tras guardar, reinicio y deduplicación de intención |
| ADR08 | Versiones inmutables | Sobrescribir el documento anterior | Más almacenamiento y conservación precisa | Huellas iguales, históricos y referencias compartidas |
| ADR09 | Un contexto propietario y una línea de esquema | Contexto por módulo o doble mecanismo de creación de tablas | Migración serializada y seeds separados; identidad de despliegue distinta | Crear desde cero, actualizar anterior y restaurar |
| ADR10 | Resolución de tenant por subdominio y filtros globales EF Core | Base de datos por grupo o esquema por grupo | Un único cluster relacional SCLM, bajo coste operativo; aislamiento forzado en DbContext | Tests de integración con dos organizaciones sintéticas sin fugas de datos |
| ADR11 | Motor de tematización dinámica (pañoleta y tokens CSS) con WCAG 2.2 AA | Subida de archivos CSS libres o tema estático sin personalización | Identidad visual scout por grupo segura contra XSS; legibilidad y contraste garantizados | Cálculo de ratio de contraste >= 4.5:1 en tests unitarios y renderizado SVG |

## Ratificación y cambios

Cada decisión ratificada añade estado, fecha, responsable y vínculo al prototipo o prueba. Un cambio de proveedor, interfaz o aislamiento debe explicar qué hipótesis falló, qué RF/RNF afecta y cómo se migrarán datos y operación. El registro conserva la decisión anterior como Sustituida y enlaza la nueva; no reescribe la historia como si la alternativa nunca se hubiera usado.

## Pruebas de decisión en E0

El prototipo debe demostrar login y revocación por recurso; almacenamiento privado sin ruta anónima; un PDF inmutable y un reintento; concurrencia de dos revisores; y persistencia de un aviso pendiente al fallar el correo. Esa evidencia decide el diseño físico y la combinación de proveedor y transporte; no se exige construir el catálogo completo para validar la arquitectura.

## Revisión técnica de la versión

La [política oficial de .NET](https://dotnet.microsoft.com/en-us/platform/support/policy/dotnet-core), consultada el 6 de noviembre de 2026, mantiene .NET 10 como LTS hasta noviembre de 2028. La versión concreta de SDK y paquetes se fijará en E1 con `global.json`, herramientas locales y versiones centralizadas. No se incorporará una preview por el mero hecho de ser más nueva.

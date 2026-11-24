# Auditoría UI/UX · Área de gestión (Kraal, Administración y SuperAdmin)

**Fecha:** 24 de noviembre de 2026 · **Candidato:** `develop` @ `7855a3d` · **Entorno:** local (`lestonnac.localhost`, `sclm.localhost`), PostgreSQL de `docker-compose.dev.yml`, datos sintéticos del seed.

**Método:**

- Recorrido en navegador a 1366×768 y a 375 px (móvil) con tres perfiles: coordinador (Lestonnac), scouter con doble rol de madre (Manada) y SuperAdmin.
- Revisión del código de las páginas y diálogos de gestión, del layout, del tema y de `app.css`.

Cada hallazgo indica cómo se ha comprobado: **[N]** visto en el navegador, **[C]** comprobado en el código. Las rutas son relativas a `src/WebScout.Web/`.

> **Estado (24/11/2026):** la fase F1 (C1–C8 y A1–A2) está corregida en la rama `fix/f1-auditoria-ui-gestion`, pendiente de revisión e integración.

Esta auditoría continúa la de [UI/UX técnica](auditoria-ui-ux-tecnica-webscout.md), cuyas mejoras M1 a M6 ya están integradas. No repite esos puntos salvo cuando la implementación no los cumple.

---

## 1. Diagnóstico

La base visual ha mejorado mucho con M1–M6: badges de estado centralizados, vista de tarjetas en móvil, paginación y estados vacíos. Sin embargo, la UI esconde errores funcionales que afectan a la seguridad de los niños y a la confianza en los datos:

- Alertas sanitarias que faltan o que se invierten.
- Acciones irreversibles de un solo clic.
- La acción principal de la Mesa de Inscripciones da 404.
- El panel de diagnóstico muestra estados inventados.

En el plano de sistema faltan cuatro piezas: la cultura `es-ES`, una cabecera de página común, que el color del grupo llegue al CSS propio y una estructura accesible (no hay `h1` y el título de la pestaña del navegador se queda congelado).

---

## 2. Críticos: corregir antes del piloto

### C1. Las alertas sanitarias del Censo y del Dossier no son fiables [N][C]

- **Un niño "sin alergias" sale como alérgico.** El texto libre "Sin alergias alimentarias conocidas" se pinta como chip rojo de alergia y suma en el KPI. El Censo muestra **2** alertas de alergia cuando solo hay una niña alérgica (Mateo sale en rojo). Código: `Features/Participants/Pages/CensoEducandosKraal.razor:92` y `:259-275`. El mismo fallo aparece en el dashboard familiar.
- **Faltan datos clínicos.** Las **alergias a medicamentos y ambientales** y las **enfermedades crónicas** no aparecen ni en el Censo ni en el fichero sanitario del Dossier, ni en pantalla ni en la versión impresa (`Features/Dossier/Pages/KraalActivityDossier.razor:274-324` y `:513-540`). Solo salen en `VerFichaSanitariaDialog`. Un scouter que repase el censo antes de salir no verá una alergia a la penicilina.
- **Datos inventados.** Para educandos sin ficha, el servicio rellena "SESCAM", "No sabe nadar" y "Sin vacunas" (`Features/Dossier/Services/ServicioDossierAsistencia.cs:74-77`). Un dato que falta se presenta como dato real.
- **Etiqueta ambigua.** La columna "Nadar" muestra el chip "Nada", que puede leerse como "nada" o como "nada (sabe nadar)".

**Corrección:**

- Crear un único helper `AlertasSanitarias(ficha)` que usen el Censo, el Dossier, el impreso y los KPI, y que incluya todas las alergias y las enfermedades crónicas.
- Guardar "sin alergias" como un booleano estructurado, no como texto libre.
- Mostrar "Sin ficha médica" cuando falte la ficha.
- Usar las etiquetas "Sabe nadar" y "No sabe nadar".

### C2. "Ver Justificante" de la Mesa de Inscripciones da 404 [C]

El enlace apunta a `/api/justificantes/{id}/archivo` (`Features/Inscriptions/Pages/MesaInscripcionesKraal.razor:302` y `:474`), pero el único endpoint es `/api/justificantes/{id:guid}` (`Features/Inscriptions/Endpoints/JustificanteEndpoints.cs:19`). La tarea principal de la Mesa, validar el pago, no funciona. El resto de pantallas usa la ruta correcta.

### C3. Acciones críticas de un solo clic, sin confirmación

- **Gestor de Actividades [N][C].** "Publicar para Familias", "Cancelar Actividad" y "Volver a Borrador" se ejecutan al instante (`Features/Activities/Pages/KraalActivities.razor:144-163` y `:285-296`). El menú es denso (32 px por opción) y "Publicar" está justo debajo de "Editar". Durante esta auditoría, un clic algo desviado publicó el campamento de verano; se revirtió y no llegó a encolarse ningún correo. Cancelar una actividad con inscripciones debería pedir confirmación y avisar de a cuántas familias afecta.
- **Confirmar plaza en la Mesa [C].** No pide confirmación ni bloquea el doble clic (`MesaInscripcionesKraal.razor:334`, `:499` y `:665`).
- **"Forzar reintento" del Outbox [C].** Se ofrece en cualquier estado y sin confirmación, así que puede reenviar correos a las familias (`Features/Administration/Pages/MonitorOutbox.razor:337-345`).
- **Check-in del Dossier [C].** El botón muestra el estado y no la acción: "Ausente" es el botón para marcar presente. No tiene `aria-pressed`.

**Corrección:** pedir confirmación en las transiciones que afectan a familias (publicar, cancelar, reenviar), separar visualmente las acciones destructivas y dar a la acción principal de cada estado un botón propio (ver M2).

### C4. La subsanación envía a la familia un motivo inventado [C]

"Requerir Subsanación", lanzado desde el diálogo de Inscripciones de una actividad, no pide motivo. Envía siempre "El comprobante adjuntado no es legible o no refleja el importe correcto de la cuota" (`Features/Inscriptions/Components/KraalInscripcionesDialog.razor:213-229`). La Mesa sí pide el motivo, con `RequerirSubsanacionDialog`, y conviene reutilizarlo aquí.

### C5. El panel de Diagnóstico y Backups muestra estados ficticios [N][C]

- "Almacén Documental: Operativo" y "Worker Outbox: En Ejecución" están escritos a mano, con un check verde fijo (`Features/Administration/Pages/DiagnosticoYBackups.razor:66-105`).
- "Ejecutar Simulación de Respaldo" registra "Snapshot PostgreSQL 17 íntegro" sin hacer ninguna copia (`:201-208`).
- "Verificado" sale con el icono verde aunque el texto de debajo diga "Pendiente de primer ensayo" (`:150-162`). La etiqueta dice "Fecha y hora UTC", pero muestra la hora local.
- El chip "Despachador Activo" del Monitor Outbox también es fijo (`MonitorOutbox.razor:22-27`).

**Corrección:** hacer comprobaciones reales, o retirar la pantalla o marcarla claramente como demostración. Un administrador no debe ver "Verificado" sobre copias que no existen.

### C6. Las horas de las actividades se desplazan en cada guardado [C]

Al cargar se aplica `ToLocalTime()` (`Features/Activities/Components/EditarActividadDialog.razor:225-228`). Al guardar, la hora local se marca como UTC con `DateTime.SpecifyKind(..., Utc)` (`:272-278`). Con el servidor en hora de Madrid, cada edición suma 1 h (2 h en verano), aunque no se toque la fecha.

**Corrección:** convertir en los dos sentidos con `TimeZoneInfo` "Europe/Madrid" desde un helper común.

### C7. Ajustes del grupo: el email de contacto lleva el nombre del grupo [N][C]

El campo "Email Oficial de Contacto" aparece precargado con "Grupo Scout Lestonnac 330". Al crear la configuración, `Features/Administration/Services/ServicioConfiguracionGrupo.cs:39` usa `GroupName` en lugar de `EmailContacto`. Además, el formulario no valida email, IBAN, CIF, teléfono ni que el fin de ronda sea posterior al inicio.

### C8. Buzón de Admisiones: se pueden crear educandos duplicados [C]

"Incorporar" aparece en todas las solicitudes, también en las ya admitidas, rechazadas o archivadas (`Features/Admissions/Pages/BuzonAdmisionesKraal.razor:192` y `:298`). La conversión del servicio no comprueba el estado.

---

## 3. Prioridad alta

### A1. Roles y navegación incoherentes [N][C]

- **Administración ve un enlace que no puede abrir.** El Buzón no admite el rol `Administracion` (`BuzonAdmisionesKraal.razor:9`), pero el menú se lo enseña (`Components/Layout/MainLayout.razor:193-207`), así que ese usuario recibe acceso denegado.
- **SuperAdmin tiene permisos sueltos.** Mesa, Censo y Pedidos lo admiten, pero el menú no los muestra. Actividades y Dossier no lo admiten, así que el botón "Gestor de Actividades" de la Mesa le lleva a acceso denegado.
- **SuperAdmin ve ajustes de grupo.** En el tenant de la federación aparecen "Ajustes del Grupo", "Auditoría del Grupo" y similares, y el menú dice "Portal de Grupo Scout".
- **El acceso rápido de SuperAdmin solo funciona desde el dominio de la federación.** Abrir `/auth/quick-login` del SuperAdmin en el subdominio de un grupo devuelve un 404 en texto plano ("Usuario '…' no encontrado o inactivo"), porque el filtro de tenant oculta a ese usuario (`IdentityEndpoints.cs:66`). El login de los grupos no muestra ese botón; afecta a quien use el enlace directo.
- **No hay página de inicio del Kraal.** El coordinador entra en Actividades, el scouter en la Mesa y el scouter-padre en el portal familiar.
- **Corrección:**
  - Crear constantes de política (`RolesKraal`, `RolesAdministracion`) y usarlas tanto en `[Authorize]` como en los `AuthorizeView`.
  - Crear un **Inicio Kraal** con tareas pendientes: inscripciones en revisión, solicitudes nuevas, pedidos por revisar y próximas salidas con fichas médicas caducadas.

> El endpoint `/auth/quick-login` sigue sin limitarse a Development. Es un problema de seguridad conocido y está fuera del alcance de esta auditoría.

### A2. El título de la pestaña del navegador no cambia al navegar [N][C]

`<HeadOutlet />` no tiene render mode interactivo (`Components/App.razor:13`), mientras que `Routes` sí lo tiene. Por eso los `PageTitle` no se actualizan con la navegación interna: tras pasar a modo Kraal, la pestaña seguía diciendo "Mi Espacio Familiar". La corrección es una línea: `<HeadOutlet @rendermode="InteractiveServer" />`.

### A3. Menú lateral [N][C]

- **Todo en mayúsculas.** Todos los enlaces salen en MAYÚSCULAS de 0,72 rem en gris, así que las cabeceras de grupo y las opciones no se distinguen. La causa es que el selector `.scout-drawer .mud-nav-group .mud-nav-link` (`wwwroot/app.css:588-596`) también alcanza a los hijos. Debería limitarse a la cabecera del grupo.
- **Datos repetidos.** El nombre del grupo aparece **cuatro veces**: barra superior, subtítulo del usuario, cabecera del menú y pie del menú.
- **Usuario sin nombre.** El usuario se muestra por su email (`Identity.Name`) y no por su nombre y rol, por ejemplo "Roberto · Coordinación".
- **Pie que ocupa espacio.** A 768 px de alto, el pie fijo tapa la última opción ("Diagnóstico y Backups") y repite "Contacto y Admisiones", que no aporta nada en gestión.
- "Administración" y "Federación SCLM" usan el mismo icono.
- **Selector de entorno redundante.** El entorno del scouter-padre se indica tres veces: chip "Modo Kraal" en la barra, chip "Scouter-Padre" y el selector segmentado. Además, siempre se entra en modo Familia; convendría recordar el último modo usado.

### A4. Móvil, 375 px [N]

- **Barra superior.** El título se parte en dos líneas y el botón "Salir" queda cortado.
- **Tarjetas de la Mesa.** La fila de acciones se sale de la tarjeta (unos 50 px) y "Confirmar" queda cortado.
- **Primer registro fuera de pantalla.** La cabecera, los tres filtros y cuatro filas de chips ocupan más de una pantalla. Conviene agrupar los filtros tras un botón "Filtros" y poner los chips en scroll horizontal.
- **Botones de check-in pequeños.** En el Dossier miden **30 px** de alto (medido); la auditoría anterior fijó 44 px como objetivo.

### A5. Contraste y tema [N][C]

- **Colores que no cumplen AA.** Con texto blanco, Warning `#D97706` da **3,2:1** e Info `#0284C7` **4,1:1** (`Infrastructure/Tenancy/ScoutMudTheme.cs`). Afecta a chips rellenos como "Pendientes de Revisión", Castores o "Medicación activa". El anillo de foco `#FBC02D` sobre blanco da **1,7:1**. Propuesta: Warning `#B45309` (5,0:1), Info `#0369A1` (5,9:1) y un anillo de foco oscuro.
- **El color del grupo no llega al CSS propio.** `--scout-primary` está fijo en `#1B5E20` (`app.css:8`) y hay unos 350 colores hex en línea en las páginas de gestión (678 en todos los `.razor`). Un grupo con otro color principal seguirá viendo verdes el enlace activo y los importes. Conviene usar `var(--mud-palette-primary)` y `Color.*`.
- **Fuentes.** El tema declara Inter, que no se carga, mientras `App.razor` descarga Roboto de Google Fonts, que no se usa. El texto acaba en la fuente del sistema, distinta según el dispositivo. Además, cargar fuentes de Google envía la IP del usuario a un tercero, algo delicado en una plataforma con datos de menores. Conviene servir la fuente desde el propio servidor.

### A6. Estructura accesible [C]

- **No hay ningún `h1`.** Los títulos de página son `h4` o `h5` y el layout empieza con un `h6`. `FocusOnNavigate Selector="h1"` (`Components/Routes.razor`) nunca encuentra destino, así que el foco se pierde tras cada navegación. Las cifras de los KPI también están marcadas como encabezados.
- Unos 20 botones de solo icono no tienen `aria-label`: el botón del menú, aprobar y subsanar, las vistas de tabla o tarjetas y los menús de tres puntos. Los tooltips solo aparecen al pasar el ratón.
- Las tarjetas KPI clicables de Pedidos y Outbox son `div` sin rol ni soporte de teclado, y la selección solo se distingue por el color.
- `ScoutStatusBadge` lleva `role="status"`, así que el lector de pantalla anuncia cada badge de la tabla.

---

## 4. Prioridad media: coherencia, textos y flujo

| ID | Hallazgo | Corrección |
| --- | --- | --- |
| M1 | **Cuatro variantes de cabecera de página**: chip "KRAAL DE SCOUTERS", nombre del grupo o ningún chip; `h4` o `h5`; acciones a la derecha o debajo; `MaxWidth` Large o ExtraLarge, con distinto margen superior. El sufijo del título de pestaña también varía ("· Grupo…", "· Kraal", "· WebScout SCLM"). [N] | Componente `ScoutPageHeader` (título, subtítulo, chip de alcance, acciones, enlace de vuelta). |
| M2 | **El botón principal de la cabecera navega a otra página**: "Gestor de Actividades" en la Mesa, "Mesa de Inscripciones" en el Censo. Compite con las acciones reales. En el Gestor, "Publicar", que es la acción principal de un borrador, está escondida en el menú de tres puntos con otras seis opciones. [N] | Enlaces secundarios; mostrar la acción principal del estado como botón en la fila. |
| M3 | **Códigos internos y jerga técnica a la vista**: "(RF12 / RF13)", "(PA07)", "Estado (Core 1.29)", "(RF64 / PA16)", "Alcance restringido a Manada (PA02)", "Simulación de Respaldo PA19", "guardado parcial atómico", "Outbox", "Despachar Cola", "Worker", "SMTP", "wwwroot", "Multitenancy". [N] | Quitar los códigos. Renombrar: "Monitor Outbox" → "Correos enviados", "Despachar cola" → "Enviar pendientes ahora". Llevar Diagnóstico al área de SuperAdmin. |
| M4 | **Valores internos sin traducir y errores técnicos**: "Estado cambiado a InscripcionesCerradas", "PedidoAlProveedor", "EnListaEspera", "Reunion" sin tilde. Más de 30 snackbars muestran `ex.Message`; borrar una actividad con inscripciones enseña el error de EF en inglés. [N][C] | Usar `ObtenerNombre()` en todos los enums y traducir los errores a mensajes legibles. |
| M5 | **Sin cultura `es-ES`** (`Program.cs`) y **cuatro formatos de moneda** (`C2`, `N2 €`, `F2 €` y un literal). Los importes se parten ("245,00 / €"), las fechas ocupan cuatro líneas en la tabla de actividades y los teléfonos se cortan. [N][C] | `RequestLocalization` en es-ES, un formateador común y `white-space: nowrap` en importes, fechas y teléfonos. |
| M6 | **Formulario de actividad**: largo y sin secciones. "Ramas (dejar vacío para Grupo Completo)" depende de una regla implícita. El selector de Estado permite saltarse las transiciones del menú. El importe tiene flechas de incremento. El IBAN no se rellena desde Ajustes. El concepto de pago incluye el literal "Nombre Educando". La validación de fechas sale en un snackbar y no junto al campo. Un clic fuera del diálogo lo cierra y se pierde todo. [N][C] | Secciones (General, Fechas, Logística, Pago, Información para familias), opción explícita "Grupo completo", estado fuera del formulario, `BackdropClick=false`. |
| M7 | **Mesa de Inscripciones**: al confirmar se guarda una nota que la tabla pinta en rojo cursiva, así que una plaza confirmada parece un error. Los chips no siguen el ciclo de vida ("Esperando Justificante" va el último). Hay un chip "En Revisión" redundante junto al título. "Pendiente de justificante" no ofrece ninguna acción, como recordar a la familia. La tabla de altura fija deja un hueco vacío y genera doble scroll. [N][C] | No guardar nota al confirmar, ordenar los chips por el ciclo de vida, acción "Recordar". |
| M8 | **Los colores de rama cambian entre páginas**: el Buzón invierte Tropa y Clan, Manada y Clan comparten color, y Escultas sale en rojo, igual que el chip de alergia. [C] | Usar los tokens `--branch-*` que ya existen en `app.css` y alinearlos con los colores oficiales de las ramas de la federación. |
| M9 | **Estados vacíos que culpan a los filtros cuando no hay ninguno activo** ("…con los filtros seleccionados" con "Todos" y "Todas"). "Exportar CSV" y "Despachar Cola" siguen activos con 0 elementos. [N] | Distinguir "aún no hay datos" de "sin resultados" y deshabilitar las acciones sin datos. |
| M10 | **Diálogos que se cierran antes de guardar** (`RequerirSubsanacionDialog`, `IncorporarEducandoDialog`, `CambiarEstadoPedidoDialog`): el spinner no llega a verse y, si falla, se pierde lo escrito. [C] | Llamar al servicio dentro del diálogo y cerrarlo solo si va bien. |
| M11 | **Ajustes**: un botón de guardar por pestaña, sin aviso de cambios pendientes al cambiar de pestaña. Si la carga falla, el formulario se muestra con valores por defecto y se puede guardar encima. [C] | Aviso de cambios sin guardar y bloquear el formulario si la carga falla. |
| M12 | **Outbox**: los KPI no coinciden con el filtro que aplican ("Pendientes" e "Incidencias" cuentan distinto de lo que listan). El estado usa una pastilla propia en vez de `ScoutStatusBadge`. [C] | Unificar el cálculo y el componente. |
| M13 | **Buzón**: en móvil no se puede admitir ni rechazar (el diálogo "Gestionar" no tiene selector de estado). "Admitir en el Grupo" e "Incorporar" se confunden. "Responder" solo existe en móvil. [C] | Un único flujo "Admitir e incorporar" con los mismos controles en todas las vistas. |
| M14 | **Pedidos**: no hay forma de abrir el comprobante adjunto. El motivo de rechazo se pide pero no se exige. Se puede guardar sin cambiar de estado. [C] | Endpoint de descarga autorizado y validación en el diálogo. |
| M15 | **Panel SuperAdmin**: los nombres de grupo se parten en cuatro líneas y aparece scroll horizontal a 1366 px. Los KPI cuentan mal ("Actividades Publicadas" incluye borradores, "Educandos" incluye inactivos). No hay acción "Nuevo grupo". [N][C] | Columnas más anchas, consultas de conteo correctas, acción de alta de grupo. |
| M16 | **Auditoría**: dice que registra las consultas de fichas sanitarias, pero abrir una ficha no deja traza. Los filtros son de texto libre y piden códigos internos. [C] | Registrar la consulta y usar un `MudSelect` con las acciones conocidas. |
| M17 | **Dossier**: las fechas impresas salen rotas, porque `@x.ToLocalTime():dd/MM/yyyy` imprime el patrón literal (`KraalActivityDossier.razor:112`). Las observaciones se guardan en cada salida del campo, con un snackbar de éxito aunque no cambien. La asistencia mezcla inscripciones sin confirmar sin indicar su estado. [C] | `@(x.ToString("dd/MM/yyyy HH:mm"))`, guardar solo si cambia, mostrar el estado de la inscripción. |

---

## 5. Prioridad baja

- "v1.0.0" escrito a mano en el menú (`MainLayout.razor:254`); el csproj no define `<Version>`.
- Los buscadores solo tienen placeholder y ninguna etiqueta. En la Mesa, el Censo y el Dossier solo filtran al perder el foco; en Pedidos, Outbox y Auditoría filtran mientras se escribe, con debounce, que es el patrón a generalizar.
- Mayúsculas iniciales inconsistentes en los menús ("Editar Convocatoria", "Eliminar Definitivamente"). En español lo habitual es solo la inicial: "Editar convocatoria".
- La paginación móvil no vuelve a la página 1 al filtrar.
- `DetalleActividadDialog` rellena los campos vacíos con texto inventado ("Llevar uniforme scout, pañoleta…"), lo que oculta al Kraal que falta el dato.
- El snackbar aparece arriba a la derecha, encima del bloque del usuario.
- "Check-in" es un anglicismo; mejor "Salida" y "Regreso".

---

## 6. Bien resuelto: mantener

- `ScoutStatusBadge` con resolución centralizada (texto, punto y color, sin depender solo del color). Conviene extenderlo al Outbox.
- Patrón tabla/tarjetas con selector de vista y objetivos táctiles de 44 px en Mesa, Censo, Pedidos y Buzón.
- Paginador traducido de forma uniforme y estados vacíos con "Limpiar filtros" en Pedidos, Mesa y Censo.
- Diálogos con acciones fijas abajo (`scout-dialog-scrollable`) y `_guardando` con spinner en la edición de actividades y en Ajustes.
- Confirmación al eliminar una actividad y al cancelar una inscripción.
- Enlace de vuelta y estilos de impresión en el Dossier; chip de alcance por rama para scouters ("Alcance restringido a Manada").
- `lang="es"`, enlaces `tel:` y `mailto:` con `aria-label` en el Buzón, `role="tablist"` con `aria-selected` en el selector Kraal/Familia.

---

## 7. Plan propuesto

| Fase | Contenido | Esfuerzo |
| --- | --- | --- |
| **F1 · Errores y datos sensibles** | C1–C8, A2 (una línea), roles de A1 | Medio |
| **F2 · Sistema visual** | `ScoutPageHeader` (M1, M2), selector CSS del menú y cabecera de A3, cultura y formateadores (M5), traducción de enums y errores (M4), retirada de códigos y jerga (M3) | Medio |
| **F3 · Accesibilidad y móvil** | `h1` y foco tras navegar, `aria-label`, contraste del tema, teclado en KPI (A5, A6), filtros plegables y acciones de tarjeta (A4), check-in de 44 px | Medio |
| **F4 · Flujos** | Inicio Kraal con tareas pendientes (A1), formulario de actividad por secciones (M6), flujo único de admisión (M13), renombrado del área de Administración (M3) | Alto |

Se recomienda abrir un issue por fase (o por hallazgo crítico) antes de crear las ramas `fix/…-XX` desde `develop`, según `AGENTS.md`.

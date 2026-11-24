# Casos de Uso Prioritarios del Piloto

**Grupo Scout Lestonnac 330 · Scouts de Castilla-La Mancha (SCLM)**  
Versión 1.1 · Actualización de Arquitectura y Alcance · 8 de noviembre de 2026.

Los casos de uso formalizan los recorridos que Producto, Kraal de Scouters y Desarrollo validan conjuntamente. Se apoyan en el modelo relacional persistente de **Censo Familiar, Ficha Médica Anual y Mesa de Revisión por Rama**, eliminando el modelo efímero de «participante declarado» del sistema legado.

---

## CU01 Solicitar ingreso e incorporar al censo

**Actor:** Visitante (familiar o tutor) / Secretaría.  
**Resultado:** Solicitud privada registrada; tras aceptación, habilitación de cuenta familiar y alta de educando en censo.  
**Trazabilidad:** RF10 a RF14; PA11 y PA13.

1. El visitante facilita datos básicos del menor (nombre, apellidos, fecha de nacimiento), datos de contacto del tutor y rama scout orientativa.
2. Declara potestad legal para aportar los datos, acepta la política de privacidad obligatoria y decide por separado las comunicaciones informativas.
3. El servidor valida valores, mecanismo antispam y versión legal, emitiendo acuse de recibo.
4. Secretaría evalúa la solicitud y cambia su estado (`Pendiente`, `Lista de espera`, `Aceptada` o `Rechazada`).
5. Al marcar `Aceptada`, el sistema remite invitación al tutor legal para crear su cuenta familiar en el portal, incorporar formalmente al educando al censo de su rama y cumplimentar su primera Ficha Sanitaria Anual.

---

## CU01b Gestión del censo familiar y Ficha Médica Anual

**Actor:** Familia (Tutor legal autenticado).  
**Resultado:** Expediente duradero del menor (`ScoutParticipant`) y Ficha Médica Anual (`ParticipantHealthRecord`) vigentes.

1. El tutor legal accede al *Portal de Familias* (`/familias`).
2. Da de alta o edita el perfil de sus hijos (nombre, apellidos, fecha de nacimiento civil, DNI si dispone de él, sexo y rama asignada por el grupo).
3. Cumplimenta o actualiza el expediente sanitario anual de cada educando:
   - Identificación sanitaria (CIP / Nº Seguridad Social, seguro privado).
   - Alergias e intolerancias (alimentarias, medicamentosas, ambientales; dietas especiales como celiaquía o exclusión de cerdo).
   - Tratamientos crónicos y medicación activa pautada para salidas.
   - Estado de vacunación según calendario oficial.
   - Habilidad natatoria (*«¿Sabe nadar?»* — indispensable para campamentos acuáticos).
   - Teléfonos de emergencia priorizados (Tutor principal, segundo tutor, familiar de reserva).
4. La información queda salvaguardada bajo cifrado y aislamiento estricto por organización (`Tenant`).

---

## CU02 Dar acceso y gestionar permisos por rama

**Actor:** Administración del grupo / Coordinación.  
**Resultado:** Cuenta habilitada con roles y asignación de ramas responsables.  
**Trazabilidad:** RF01 a RF08; PA01 a PA03.

1. Invita o da de alta a un scouter, asignándole el rol `Scouter` o `Coordinacion` y su rama operativa (`Castores`, `Manada`, `Tropa`, `Escultas`, `Clan`).
2. Concede de forma independiente facultades: revisión de inscripciones de rama, publicación de avisos/actividades, gestión económica o administración.
3. Verifica el alcance: un scouter asignado exclusivamente a Manada solo puede consultar y validar expedientes de educandos de Manada.
4. Revoca una concesión o desactiva la cuenta, comprobando que el siguiente acceso directo al recurso queda denegado inmediatamente sin depender del reinicio de sesión.

---

## CU03 Publicar una actividad o campamento

**Actor:** Scouter responsable o Coordinación.  
**Resultado:** Convocatoria de actividad publicada para las ramas destinatarias.  
**Trazabilidad:** RF17 a RF24; PA12.

1. Crea borrador de actividad: título, descripción operativa, lugar/ubicación, recomendaciones de equipo y fechas civiles (salida, regreso y **fecha límite de inscripción**).
2. Selecciona las **ramas destinatarias**: una sola sección (ej. Manada), varias (ej. Tropa + Escultas) o *Todo el Grupo* (Campamentos de Navidad, San Jorge o Verano).
3. Fija condiciones económicas: cuota por participante en céntimos (0 si es gratuita) e instrucciones de transferencia bancaria (entidad, IBAN, titular y formato del concepto).
4. Asocia las cláusulas de autorización legal requeridas (pernocta, salidas fuera del municipio, transporte y asistencia sanitaria de urgencia).
5. Publica la actividad. Las familias con educandos en las ramas convocadas reciben aviso y visualizan la actividad en su portal privado.

---

## CU04 Inscribir a educandos (hermanos) con justificante compartido

**Actor:** Familia (Tutor legal autenticado).  
**Resultado:** Dos expedientes de inscripción independientes vinculados al mismo justificante de transferencia bancaria.  
**Trazabilidad:** RF25 a RF31; PA04 a PA06.

1. La familia accede a la convocatoria de la actividad desde su portal. El sistema detecta automáticamente qué educandos de su censo son aptos por pertenecer a las ramas convocadas (ej. Lucía en Manada y Marcos en Tropa).
2. **Inscripción del primer educando (Lucía)**:
   - Revisa el resumen de la Ficha Médica vigente y pulsa: *«Confirmo que los datos sanitarios y de emergencia están actualizados»* (con acceso directo a edición si hubiera cambios).
   - Lee y acepta las autorizaciones legales de la salida con evidencia inmutable (timestamp UTC, IP y usuario autenticado).
   - Si la actividad tiene cuota, sube el archivo del justificante de transferencia bancaria (PDF, JPG, PNG).
   - Confirma la inscripción; el expediente de Lucía pasa a estado `PendienteRevision`.
3. **Inscripción del segundo educando (Marcos)**:
   - El sistema detecta que el tutor ya aportó un justificante bancario reciente para esta misma actividad.
   - El tutor valida la ficha médica de Marcos y acepta sus cláusulas legales individuales.
   - En el paso de pago, selecciona la opción: *«Asociar justificante ya aportado (Transferencia conjunta para hermanos)»*.
   - Confirma la inscripción; el expediente de Marcos pasa a estado `PendienteRevision` de forma independiente.
4. Cada hermano conserva su propio ciclo de vida, estado de revisión y trazabilidad.

---

## CU05 Revisar inscripciones y gestionar subsanaciones

**Actores:** Scouter revisor de rama y Familia.  
**Resultado:** Expediente validado formalmente o rectificado sin pérdida de información.  
**Trazabilidad:** RF32 a RF34, RF38; PA02, PA04 y PA09.

1. El scouter de Manada accede a la mesa de revisión de su sección y visualiza la inscripción de Lucía.
2. Si la documentación y el importe bancario son correctos, pulsa **Aprobar inscripción** (pasa a `Confirmada`).
3. Si el comprobante bancario resulta ilegible o el importe no coincide con la cuota establecida:
   - El scouter selecciona **Requerir subsanación**, indicando un motivo claro (ej. *«El comprobante bancario está cortado y no se lee el importe total»*).
   - El expediente de Lucía pasa a `SubsanacionRequerida`. La inscripción de su hermano Marcos en Tropa permanece inalterada en su propio estado.
4. La familia recibe notificación, accede a la ficha de inscripción de Lucía, sustituye exclusivamente el archivo cuestionado y pulsa **Reenviar subsanación**.
5. La inscripción vuelve automáticamente a `PendienteRevision` para su validación final por el scouter.

---

## CU05b Generar salidas operativas de campamento (Cocina, Botiquín y Emergencias)

**Actor:** Scouter responsable de sección o Coordinación.  
**Resultado:** Informes operativos generados al instante para la intendencia y seguridad del campamento.

1. Finalizado el plazo de inscripción, el equipo de Kraal accede a la actividad en estado de cierre.
2. **Nómina de Asistencia y Emergencias**: Genera la lista nominal de educandos confirmados por rama, con edad cumplida y teléfonos de emergencia de sus tutores ordenados por prioridad de llamada.
3. **Informe de Cocina e Intendencia**: Genera la matriz agregada de necesidades dietéticas para el equipo de intendentes y cocineros (recuento de celíacos, alérgenos específicos: huevo, frutos secos, crustáceos; intolerancias a la lactosa y menús adaptados).
4. **Dossier de Botiquín y Salud**: Genera el informe confidencial para el scouter responsable de salud del campamento: números de CIP / Seguridad Social, pautas de medicación activa que deben administrarse durante la acampada, patologías crónicas y nivel de natación.

---

## CU06 Subir fotos de actividad desde móvil

**Actor:** Scouter responsable de álbum / Comunicación.  
**Resultado:** Fotografías cargadas, optimizadas y publicadas para las familias autorizadas.  
**Trazabilidad:** RF39 a RF47; PA09 y PA10.

1. Selecciona o crea el álbum asociado a la actividad o campamento.
2. Selecciona un lote de fotos desde el dispositivo móvil (hasta 100 imágenes por lote).
3. El sistema optimiza las imágenes en cliente, preserva permisos y envía la cola con barra de progreso.
4. El scouter confirma la subida; las fotos quedan accesibles únicamente para las familias de los educandos participantes y el equipo de responsables.

---

## CU07 Solicitar prendas o insignias del grupo

**Actores:** Familia y Responsable de Pedidos.  
**Resultado:** Solicitud de material de grupo (pañoleta, polos, insignias) con seguimiento de entrega.  
**Trazabilidad:** RF48 a RF53; PA07 y PA08.

1. La familia consulta los artículos disponibles (pañoleta oficial, camiseta de rama, polar de grupo) y sus tallas.
2. Registra una solicitud indicando educando destinatario, producto y unidades.
3. Adjunta de manera voluntaria el justificante de pago si realizó transferencia previa.
4. El responsable de pedidos gestiona los estados (`Solicitado`, `Revisado`, `Pedido al proveedor`, `Entregado`) y entrega las prendas en mano durante una reunión de grupo.

---

## CU08 Atender derechos de privacidad, exportación o supresión

**Actor:** Responsable de Tratamiento / Delegado de Protección de Datos.  
**Resultado:** Ejercicio de derechos ARCO/RGPD conforme a la normativa vigente.  
**Trazabilidad:** RF55 a RF60; PA13 y PA14.

1. El responsable recibe una solicitud formal de acceso, rectificación o supresión de un titular.
2. Identifica al usuario y sus educandos vinculados en la base de datos.
3. Emite un volcado estructurado de sus datos o aplica el bloqueo/supresión de los expedientes que no deban conservarse por obligaciones legales o de seguro de accidentes.
4. Registra el evento en la auditoría inmutable del sistema.

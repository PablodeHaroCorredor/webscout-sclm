# Política de seguridad y permisos por recurso

Versión 0.1 · Propuesta de arranque · 6 de noviembre de 2026.

Cada operación privada requiere usuario autenticado y activo, membresía de organización válida y una autorización sobre el recurso concreto. La interfaz muestra las opciones disponibles, pero el servidor determina el permiso al ejecutar la operación.

## Orden de evaluación

1. Resolver organización y cuenta desde el contexto del servidor.
2. Comprobar cuenta y membresía activas.
3. Cargar solo los metadatos necesarios del recurso dentro de la organización.
4. Evaluar propiedad, perfil, concesión y secciones según la acción.
5. Validar estado, plazo y versión de concurrencia cuando corresponda.
6. Ejecutar y registrar el resultado mínimo necesario.

Administración y Coordinación tienen alcance global de gestión del grupo dentro de las capacidades definidas. Ese alcance se evalúa después del ámbito de organización; nunca permite saltar a otro grupo.

## Matriz funcional

| Acción | Familia o adulto | Scouter | Coordinación | Administración |
| --- | --- | --- | --- | --- |
| Leer comunicaciones y fotos | Audiencia propia | Audiencia y autoría propia | Grupo completo | Grupo completo |
| Editar contenido privado | No | Propio | Todo | Todo |
| Revisar respuestas ajenas | No | Concesión individual y sección responsable | Todas | Todas |
| Gestionar solicitudes y altas | No | No | Sí | Sí |
| Gestionar pedidos ajenos | No | Concesión para todos los pedidos del grupo | Sí | Sí |
| Publicar noticias públicas | No | Concesión individual | Sí | Sí |
| Conceder facultades sensibles | No | No | No por defecto | Sí |
| Consultar auditoría global | No | No | No por defecto | Sí |

El Scouter con publicación pública directa no obtiene por ello edición de noticias públicas ajenas o ya publicadas. Crear borrador, publicar y gestionar el conjunto son facultades distintas.

## Archivos y vínculos

`ReadCommunication`, `EditOwnContent`, `ReviewActivityResponse`, `ManageOrders`, `PublishPublicNews`, `ManageGroupSettings` y `ManageSensitiveGrants` serán políticas explícitas. Las descargas se autorizan mediante el vínculo de archivo y su recurso, no mediante la coincidencia de hash ni un ID opaco.

El propietario puede consultar su expediente. Un revisor solo abre las respuestas de sus secciones responsables; un registro histórico general sin alcance suficiente queda para gestión global. La concesión de pedidos permite gestionar los pedidos del grupo, sin abrir solicitudes de ingreso u otros formularios.

Los originales, miniaturas y versiones anteriores usan el mismo control. La respuesta privada será no-store y noindex. Las proyecciones y CSV respetarán el permiso y limitarán campos; las denegaciones evitarán confirmar la existencia de recursos ajenos.

## Sesión y revocación

Identity gestiona credenciales y recuperación. La propuesta exige MFA a Administración y Coordinación, cookies seguras y CSRF en mutaciones. Las concesiones sensibles se revalidarán en cada operación mediante datos vigentes o una versión de seguridad; la información de un claim antiguo no mantendrá acceso después de una revocación.

Los servicios y workers recibirán un contexto explícito con permisos de sistema acotados.

## Pruebas mínimas

PA01, PA02, PA03, PA12 y PA18 deben incluir invocación directa, CSV, miniaturas, versiones históricas y revocación con sesión activa. La política se implementará en Application y en el servidor de archivos, conforme a [autorización por recurso de ASP.NET Core](https://learn.microsoft.com/en-us/aspnet/core/security/authorization/resource-based?view=aspnetcore-10.0).

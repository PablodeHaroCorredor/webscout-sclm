# Convenciones de desarrollo y flujo de cambios

Versión 0.1 · Propuesta de arranque · 6 de noviembre de 2026.

Se propone un repositorio para la nueva aplicación y su documentación. El flujo de ramas, revisores y automatización se cerrará en DC10. Como punto de partida: rama principal protegida, ramas de tarea breves, revisión antes de integrar y artefactos identificados por commit. No se impone Gitflow por usar documentos de GTS.ERP como referencia.

## Código y contratos

Namespaces por capa y módulo. Tipos C# en inglés coherente; textos de usuario en español separados de reglas. Identificadores estables, DTOs específicos y validación de servidor. Las entidades no se serializan directamente como contratos públicos. Las fechas instantáneas se guardan en UTC; plazos civiles se calculan en la zona de la organización. Usar reloj inyectable cuando la regla dependa del tiempo.

Las operaciones sensibles reciben organización y actor de un contexto acreditado, no de campos confiados al cliente. La versión de concurrencia y la clave de reintento forman parte del contrato donde proceda. Las consultas paginan en servidor y evitan cargar archivos completos para contar o listar. La transacción incluye estado, auditoría e intención de notificar; los proveedores externos quedan fuera de su confirmación.

El manejo de errores distingue validación, permiso, recurso inexistente, conflicto y dependencia temporalmente fallida. El usuario recibe una acción útil; los logs, una correlación y datos mínimos. No guardar cuerpos de PDFs ni secretos en excepciones, test snapshots o telemetría.

## Configuración y dependencias

Centralizar versiones, fijar SDK y revisar licencias, soporte y alertas de los paquetes realmente incorporados. Los valores por entorno no cambian las reglas de negocio de forma oculta. Las características aplazadas se deshabilitan de forma consistente en navegación, endpoint, tareas y permisos.

Una migración tiene dueño, cambio revisado, compatibilidad y verificación; no editar una ya aplicada para ocultar historia. No introducir SQL de negocio en Web. Se pueden usar consultas o scripts especializados justificados en Infrastructure con autorización, parametrización y pruebas pertinentes.

## Flujo de una tarea

1. Verificar contexto real, RF y política afectada; preparar datos y prueba del comportamiento.
2. Implementar una capacidad completa, incluyendo fallos y límites de acceso.
3. Compilar y ejecutar comprobaciones pertinentes sobre el candidato.
4. Revisar diferencia y documentación; describir problema, nuevo comportamiento y evidencia.
5. Integrar por el procedimiento acordado. Publicación y despliegue tienen su autorización y registro propios.

Los commits deben describir el cambio concreto. El idioma y prefijos se acuerdan en DC10; no se requieren cambios masivos de estilo ajenos a la tarea. Conservar trabajo ajeno y no incluir volcados, binarios de usuario, claves, exportaciones personales ni resultados temporales de pruebas.

## Regla documental

Cada regla vive en su política temática; cada RF y RNF en el catálogo. Los demás documentos enlazan y concretan su aplicación. Estado concentra entregas reales y límites de evidencia. Un cambio aprobado actualiza requisitos, ADR o decisiones de producto cuando corresponda; no se reescribe el documento base histórico.

Registrar la documentación en la solución cuando exista, siguiendo las capacidades de `.slnx`, sin crear proyectos ficticios. Los archivos de este paquete son versión inicial; su mera incorporación no certifica implementación ni conformidad legal.

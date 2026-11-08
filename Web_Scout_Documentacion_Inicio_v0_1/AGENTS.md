# Guía de trabajo del proyecto Web Scout

Versión 0.1 · Propuesta de arranque · 6 de noviembre de 2026.

Esta guía se aplicará al futuro repositorio de la web scout. La documentación establece la propuesta; el estado del código, las pruebas y los despliegues debe demostrarse con evidencia del candidato real.

## Lectura previa

Leer [Estado del proyecto](docs/estado-proyecto.md), el [índice](docs/README.md) y los documentos del área afectada. El contrato funcional procede del [catálogo RF](docs/requisitos/catalogo-requisitos.md). No tratar un estado Propuesto como Implementado ni copiar componentes de GTS.ERP por su mera aparición en una referencia documental.

## Límites del proyecto

- Monolito modular .NET 10 para un grupo piloto, con HTML en servidor y JavaScript progresivo.
- Domain conserva invariantes; Application orquesta y autoriza; Infrastructure implementa persistencia, archivos y correo; Web adapta la interacción.
- Los módulos se agrupan por carpetas y namespaces. No crear un proyecto por cada entidad o módulo.
- Organización, propiedad, sección y capacidad se comprueban en servidor, también en descargas, exportaciones y trabajos.
- No reintroducir el flujo económico de pedidos 1.28, un censo familiar permanente o la validación de certificados sin un cambio de alcance.
- Proteger originales firmados, versiones y referencias compartidas. Una corrección añade evidencia y no reescribe la anterior.

## Antes de modificar

Comprobar raíz, rama, HEAD y cambios locales. Respetar trabajo del usuario y de otras tareas. Buscar implementaciones y contratos similares antes de crear abstracciones. Delimitar las operaciones externas por el encargo y por las autorizaciones vigentes; seguir instrucciones más específicas cuando existan.

La documentación no concede permiso para enviar correos reales, importar datos de producción, publicar contenido, hacer push o desplegar. Las acciones ya autorizadas en la sesión no requieren una segunda confirmación por esta guía.

## Desarrollo y comprobación

- C# con nombres técnicos coherentes y comentarios que expliquen invariantes y decisiones.
- No incluir credenciales, documentos reales, datos de menores o cadenas de conexión en Git ni fixtures.
- Una operación sensible conserva contexto, control de versión y evento mínimo de auditoría.
- Probar comportamiento de negocio y errores relevantes. No sustituir PostgreSQL por EF InMemory cuando se verifica integridad o aislamiento.
- Un cambio de permisos requiere casos negativos y revocación con sesión activa. Un cambio de archivos requiere reintento, huella y versiones.
- Un cambio solo documental requiere enlaces, coherencia y revisión del contenido; no la batería funcional completa.
- No afirmar prueba visual por un resultado unitario, despliegue por una compilación o aceptación por un HTTP 200.

## Documentación y cierre

Actualizar la referencia temática y, si cambia entrega, pendiente o evidencia, [Estado del proyecto](docs/estado-proyecto.md). Registrar nuevos documentos en [docs/README.md](docs/README.md) y, cuando exista la solución, en su agrupación documental. Reportar qué cambió, candidato comprobado, resultados y limitaciones materiales. Las instrucciones de comandos se mantienen en [onboarding](docs/desarrollo/onboarding-desarrolladores.md).

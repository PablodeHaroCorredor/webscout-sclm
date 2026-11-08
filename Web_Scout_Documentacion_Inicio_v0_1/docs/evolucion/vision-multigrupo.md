# Plataforma multitenant para Scouts de Castilla-La Mancha (SCLM)

Versión 1.0 · Refinamiento de arquitectura multitenant · 8 de noviembre de 2026.

La visión evoluciona la aplicación web scout hacia una **plataforma multitenant asociativa** donde todos los grupos scout integrados en Scouts de Castilla-La Mancha (SCLM) disponen de su propia aplicación web de gestión, conservando su plena autonomía organizativa, su privacidad y su identidad visual propia (pañoleta, colores de grupo, heráldica y lema). El Grupo Scout Lestonnac 330 se mantiene como el **grupo piloto de referencia** para el prototipo, validación y migración inicial.

## Principios del modelo multitenant asociativo

1. **Aislamiento lógico estricto:** Los datos de familias, educandos, documentos y fotos de cada grupo scout están completamente aislados mediante `OrganizationId` obligatorio y filtros globales en el contexto de datos (`Global Query Filters` en EF Core). Un responsable de un grupo no puede consultar ni inferir datos de otro.
2. **Resolución por subdominio o dominio propio:** Cada grupo accede a través de su identidad web: `{slug}.scoutsclm.org` (por ejemplo, `lestonnac.scoutsclm.org`, `sanvicente.scoutsclm.org`) o mediante un dominio personalizado configurado (`CNAME`).
3. **Personalización visual y temática de cada grupo:** Cada tenant define su identidad visual scout:
   - **Colores de la pañoleta:** Color base, ribete, cinta y distintivos.
   - **Paleta de interfaz:** Color primario, secundario, contraste accesible de texto (validación automática WCAG 2.2 AA) y cabecera.
   - **Activos gráficos:** Escudo, logotipo en versión clara/oscura y favicon.
   - **Configuración de ramas:** Catálogo y nombres propios de sus secciones (ej. Manada Seeonee vs Manada Waingunga).
4. **Soporte y gobernanza federativa SCLM:**
   - La asociación regional (SCLM) actúa como entidad facilitadora y proveedora del servicio técnico, sin acceso directo e indiscriminado a expedientes privados de menores.
   - Las intervenciones de soporte técnico inter-grupo requieren una política de delegación temporal (*break-glass*), acotada en tiempo, con motivo justificado y auditoría funcional inmutable.
5. **Cuentas y usuarios multi-membresía:**
   - Un usuario familiar o un Scouter con vinculación en varios grupos (o en el equipo asociativo de SCLM) dispone de una única cuenta de autenticación (`ApplicationUser`) y un selector de grupo para alternar entre sus membresías activas sin duplicar credenciales.

## Arquitectura de datos e infraestructura

- **Base de datos compartida con aislamiento lógico:** Un único cluster relacional PostgreSQL alojado para SCLM, con una única línea de esquema y migraciones. Esta decisión evita los costes desproporcionados de gestionar decenas de bases de datos para entidades sin ánimo de lucro y simplifica el despliegue de parches.
- **Almacenamiento de archivos particionado:** El almacenamiento privado y público utiliza prefijos aislados por grupo (`/tenants/{organizationId}/private/` y `/tenants/{organizationId}/public/`), manteniendo el control de acceso en el servidor de aplicaciones mediante tokens opacos y políticas por recurso.
- **Notificaciones con identidad del grupo:** Los correos salientes se emiten a través del outbox con el remitente, logotipo, pie y colores del grupo emisor.

## Relación con la documentación del proyecto

El análisis exhaustivo, el modelo de tokens CSS y el componente vectorial de pañoleta se detallan en el documento de propuesta arquitectónica. Las decisiones de producto y técnicas quedan recogidas en [ADR10 y ADR11](../architecture/decisiones-arquitectura.md) y en la decisión [DC01](../planificacion/decisiones-y-riesgos.md).

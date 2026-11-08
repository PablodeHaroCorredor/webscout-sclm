# WebScout SCLM ⚜️

> Plataforma web multitenant en .NET 10 LTS con aislamiento de datos, personalización de pañoletas y tematización grupal para **Scouts de Castilla-La Mancha (SCLM)**, con el Grupo Scout Lestonnac 330 como grupo piloto.

---

## 📌 Visión del Proyecto

WebScout SCLM proporciona a cada grupo scout de la federación su propio portal de gestión asociativa, con:
- **Aislamiento lógico estricto por tenant** (`OrganizationId`) en base de datos PostgreSQL compartida.
- **Identidad scout personalizada:** motor de pañoleta (fondo, ribetes, cintas, escudo), colores y contraste verificado según el estándar **WCAG 2.2 Nivel AA** ($\ge 4.5:1$).
- **Seguridad y privacidad por diseño:** autorización por recurso, documentos inmutables (hash SHA-256), sin exposición pública de datos de menores.
- **Patrón Outbox:** persistencia de eventos y notificaciones con tolerancia a caídas del servicio de correo.

---

## 🛠️ Herramientas de Desarrollo y Servicios Locales

El repositorio incluye la infraestructura local en Docker para desarrollo y pruebas:

| Servicio | Tecnología | Puerto Local | Descripción |
| :--- | :--- | :--- | :--- |
| **Base de Datos** | PostgreSQL 17 | `5432` | Cluster relacional único con soporte multitenant |
| **Correo de Pruebas** | Mailpit | `1025` (SMTP) / `8025` (Web UI) | Capturador de emails para validar notificaciones sin envíos externos |
| **Almacenamiento Privado** | MinIO | `9000` (S3 API) / `9001` (Consola Web) | Almacenamiento seguro de PDFs firmados, fotos y documentos privados |

### Iniciar los Servicios de Desarrollo
```powershell
docker compose -f docker-compose.dev.yml up -d
```

### Configuración de Entorno
Copiar la plantilla de configuración:
```powershell
Copy-Item .env.example .env
```

---

## 🚀 Estructura de la Solución (.NET)

La solución técnica sigue una arquitectura limpia de monolito modular:

```text
WebScout.slnx
├── src/
│   ├── WebScout.Domain/          # Entidades puras, Value Objects (PanoletaConfig), Invariantes
│   ├── WebScout.Application/     # Casos de uso, ITenantContext, Autorización por recurso
│   ├── WebScout.Infrastructure/  # WebScoutDbContext (EF Core), S3/MinIO, SMTP Outbox
│   └── WebScout.Web/             # ASP.NET Core, TenantResolutionMiddleware, Razor/MVC, CSS Tokens
├── tools/
│   └── WebScout.Migration/       # Seeds de grupos SCLM y migrador WordPress (Lestonnac 330)
├── tests/
│   ├── WebScout.DomainTests/     # Pruebas unitarias de reglas de negocio y cálculo WCAG
│   ├── WebScout.ArchitectureTests/ # Verificación de límites de capas y multitenancy
│   ├── WebScout.IntegrationTests/ # Pruebas con PostgreSQL real y S3 (PA01 - PA19)
│   └── WebScout.WebTests/        # Pruebas de navegación, accesibilidad y subdominio
└── docs/                         # Índice y especificación técnica exhaustiva
```

---

## 🧪 Pruebas y Trazabilidad (PA01 - PA19)

El proyecto cuenta con 19 Pruebas de Aceptación formales:
- `PA01` - Aislamiento estricto entre Familias A y B.
- `PA02` - Ámbito de revisor de sección y revocación inmediata.
- `PA04` - Justificante de pago compartido entre hermanos.
- `PA06` - Inmutabilidad y trazabilidad de versiones de archivos.
- `PA11` - Resiliencia de cola de avisos ante caída de servidor SMTP.
- `PA18` - Verificación de no cruce de datos entre diferentes grupos scouts.

Para consultar el estado actual del desarrollo y las decisiones aprobadas, revisar [Estado del proyecto](Web_Scout_Documentacion_Inicio_v0_1/docs/estado-proyecto.md).

---

## 🤖 Skills y Automatización para Agentes

El directorio `.agents/skills/` contiene runbooks especializados para el desarrollo asistido:
- `scout-architecture-guard`: Salvaguarda de capas, `OrganizationId` e invariantes.
- `efcore-multitenant-migrations`: Gestión de migraciones, Global Query Filters y seeds.
- `scout-theming-wcag`: Fórmulas matemáticas de luminancia, contraste WCAG 2.2 AA y SVG de pañoleta.
- `scout-acceptance-testing`: Protocolo de ejecución y registro para PA01 a PA19.
- `git-workflow-conventions`: Políticas de ramas, Conventional Commits y protección de secretos.

---

## 📦 Control de Versiones en GitHub

Para sincronizar con el repositorio personal en GitHub:
```powershell
git remote add origin https://github.com/<tu-usuario>/<tu-repo>.git
git branch -M main
git push -u origin main
```

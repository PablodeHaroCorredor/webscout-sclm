## 📋 Descripción del Cambio
<!-- Explica de forma concisa qué problema resuelve este Pull Request o qué funcionalidad introduce. -->

## 🔗 Referencias y Trazabilidad
- **Hito / Paso:** <!-- ej. Paso 3: Publicación y Gestión de Actividades -->
- **Issue(s) relacionado(s):** <!-- ej. Closes #14, Relacionado con #8 -->

## 🏷️ Tipo de Cambio
- [ ] ✨ `feat`: Nueva funcionalidad
- [ ] 🐛 `fix`: Corrección de error
- [ ] ♻️ `refactor`: Refactorización de código sin cambio de comportamiento
- [ ] 📝 `docs`: Cambios exclusivamente en documentación
- [ ] 🧪 `test`: Incorporación o mejora de pruebas
- [ ] 🔧 `chore`: Tareas de configuración, herramientas o dependencias

## ✅ Checklist de Aceptación y Calidad (DoD)
Antes de solicitar revisión o fusionar este PR, confirma que se cumplen todos los puntos:

- [ ] **Compilación limpia:** `dotnet build WebScout.slnx` se ejecuta sin errores.
- [ ] **Batería de Pruebas:** Todos los tests de arquitectura, unitarios y de integración pasan (`dotnet test`).
- [ ] **Aislamiento Multi-inquilino:** Todas las entidades nuevas de datos implementan `IEntidadOrganizacion` y respetan el filtro por `OrganizacionId`.
- [ ] **Ubicuidad y Castellano:** Nombres de entidades, propiedades, tablas, servicios y vistas están íntegramente en castellano.
- [ ] **Protección de Datos (RGPD / Menores):** 
  - ❌ Cero datos personales reales de educandos o familias en código, fixtures o semillas.
  - ❌ Cero fichas médicas o datos sanitarios reales.
- [ ] **Seguridad:** Cero secretos, tokens, contraseñas o cadenas de conexión en el repositorio.
- [ ] **Diseño MudBlazor:** Componentes estándar, diseño profesional, limpio y sin personalizaciones cromáticas superfluas.
- [ ] **Migraciones EF Core:** Si hay cambios en el modelo de base de datos, se ha generado y probado la migración correspondiente.

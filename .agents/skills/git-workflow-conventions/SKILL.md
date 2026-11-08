---
name: git-workflow-conventions
description: >-
  Guía integral del flujo de trabajo de Git y GitHub para WebScout SCLM. Establece
  la estrategia de branching (GitHub Flow), nomenclatura de ramas, Conventional Commits,
  plantillas de Pull Request, política de Squash & Merge, protección de main y trazabilidad.
---

# Estrategia de Git & GitHub Workflow — WebScout SCLM

Este documento y skill rige la gestión del código fuente, branching, trazabilidad y control de calidad en el repositorio GitHub `webscout-sclm`.

---

## 1. Principios de Trazabilidad y Calidad

1. **`main` es siempre desplegable y estable:**
   - Ningún commit se realiza directamente sobre `main` sin pasar por Pull Request (excepto inicialización del repo o correcciones de documentación urgentes autorizadas).
   - Todo código en `main` debe compilar en limpio (`dotnet build`) y superar el 100% de las pruebas (`dotnet test`).
2. **Historial Lineal y Atómico (Squash & Merge):**
   - La estrategia de integración oficial es **Squash and Merge**.
   - Cada funcionalidad o corrección se consolida en un único commit en `main` con mensaje convencional y enlace a la PR:
     `feat(actividades): gestión y publicación de salidas por rama (#12)`
   - Beneficios: Historial limpio, facilidad para `git bisect`, rollbacks atómicos inmediatos mediante `git revert <commit-id>` y trazabilidad 1:1 con las PRs de GitHub.
3. **Cero Secretos y Cero Datos Reales (RGPD):**
   - ❌ **Prohibido commit de datos de menores o familias:** Usar exclusivamente datos ficticios sintéticos.
   - ❌ **Prohibido commit de credenciales o cadenas de conexión reales:** Emplear `dotnet user-secrets` o `.env`.

---

## 2. Nomenclatura de Ramas

Toda nueva tarea o funcionalidad parte de `main` actualizado y utiliza la siguiente convención:

| Prefijo | Propósito | Ejemplo |
| :--- | :--- | :--- |
| `feat/` | Nueva funcionalidad o módulo de negocio | `feat/actividades-publicacion-kraal`, `feat/inscripciones-pasarela` |
| `fix/` | Corrección de fallos o bugs | `fix/filtro-multitenant-actividades`, `fix/validacion-dias-plazo` |
| `refactor/` | Reestructuración de código sin alterar comportamiento | `refactor/unificar-dtos-actividad`, `refactor/limpieza-servicios` |
| `test/` | Incorporación o mejora de tests unitarios o integración | `test/aislamiento-actividades-inquilinos` |
| `docs/` | Cambios en documentación técnica o guías | `docs/actualizar-arquitectura-mvp` |
| `chore/` | Mantenimiento de herramientas, NuGet, CI/CD | `chore/actualizar-dependencias-mudblazor` |

---

## 3. Formato de Commits (Conventional Commits)

```text
<tipo>(<ámbito>): <descripción concisa en imperativo y castellano>

[Cuerpo opcional detallando el motivo del cambio y arquitectura]

[Referencias: Closes #X, Paso 3 MVP, RF04]
```

### Ejemplos válidos:
- `feat(actividades): incorporar entidad ActividadScout con precio en euros y ramas destinatarias`
- `feat(kraal): vista de creación y publicación de campamentos con MudBlazor`
- `fix(inscripcion): corregir bloqueo de plazas tras fecha límite`
- `test(actividades): añadir pruebas de aislamiento multi-inquilino en repositorio`

---

## 4. Ciclo de Vida de una Funcionalidad (Paso a Paso)

### Paso 1: Sincronizar y crear rama de trabajo
```powershell
git checkout main
git pull origin main
git checkout -b feat/actividades-publicacion-kraal
```

### Paso 2: Desarrollo y commits atómicos locales
- Realizar cambios modulares y tests.
- Probar localmente antes de cada commit:
```powershell
dotnet test
git add <archivos-relevantes>
git commit -m "feat(actividades): implementar IServicioActividades y ServicioActividades"
```

### Paso 3: Verificación Previa a la Pull Request
Antes de publicar la rama, ejecutar la verificación obligatoria:
1. `dotnet build WebScout.slnx` -> 0 errores.
2. `dotnet test` -> 100% tests pasados.
3. `git status` -> No dejar archivos basura, temporales o secretos.

### Paso 4: Publicar rama en GitHub
```powershell
git push -u origin feat/actividades-publicacion-kraal
```

### Paso 5: Abrir y Completar Pull Request
- Rellenar la plantilla `.github/pull_request_template.md`.
- Verificar que el pipeline de GitHub Actions (`CI WebScout SCLM`) finalice en verde.
- Revisión de código y aprobación.

### Paso 6: Integración y Limpieza
- En la interfaz de GitHub, seleccionar **"Squash and merge"**.
- Confirmar el mensaje con formato convencional y número de PR.
- Eliminar la rama remota en GitHub.
- En local:
```powershell
git checkout main
git pull origin main
git branch -d feat/actividades-publicacion-kraal
```

---
name: git-workflow-conventions
description: >-
  Guía integral del flujo de trabajo de Git con estándar GitLab Flow / GitFlow adaptado
  para WebScout SCLM. Establece las ramas principales (main para producción, develop para
  desarrollo), nomenclatura de ramas con número de issue, Conventional Commits,
  estrategia de Pull Requests y trazabilidad.
---

# Estrategia de Git & Branching — WebScout SCLM

Este documento y skill rige la gestión del código fuente, branching, trazabilidad y control de versiones en el repositorio `webscout-sclm`.

---

## 1. Arquitectura de Ramas

Seguimos el estándar extendido de **GitLab Flow / GitFlow adaptado**:

| Rama | Entorno / Rol | Origen | Destino de Merge | Descripción |
| :--- | :--- | :--- | :--- | :--- |
| **`main`** | **Producción** | N/A | Tags de Release | Rama de producción. Solo código estable, validado y etiquetado con releases (`v1.0.0`). |
| **`develop`** | **Desarrollo** | `main` | `main` (vía release) | Rama base de integración continua. Toda rama de funcionalidad se origina aquí. |
| **`feat/...`** | Funcionalidad | `develop` | `develop` | Desarrollo de nuevas características y tareas del backlog. |
| **`bugfix/...`** | Corrección | `develop` | `develop` | Corrección de defectos detectados durante desarrollo o testing. |
| **`refactor/...`**| Refactorización | `develop` | `develop` | Mejoras de arquitectura o diseño sin alterar funcionalidad. |
| **`hotfix/...`** | Urgencia Producción | `main` | `main` y `develop` | Parches críticos de fallos en producción. |
| **`release/v...`**| Preparación Release | `develop` | `main` y `develop` | Congelación de versión previa a despliegue en producción. |

---

## 2. Nomenclatura Estándar de Ramas

Las ramas se nombran siguiendo el patrón:
```text
<tipo>/<nombre-descriptivo>-<numero-issue>
```

### Tipos Permitidos:
- `feat/`: Nueva funcionalidad (ej. `feat/actividades-campamentos-ramas-3`).
- `bugfix/`: Corrección de errores en desarrollo (ej. `bugfix/filtro-multitenant-actividades-12`).
- `refactor/`: Refactorizaciones de código (ej. `refactor/unificar-dtos-actividad-15`).
- `hotfix/`: Arreglo crítico para producción (ej. `hotfix/vulnerabilidad-sesion-99`).
- `chore/`: Tareas de configuración, herramientas o CI/CD (ej. `chore/actualizar-ci-net10-5`).

---

## 3. Formato de Commits (Conventional Commits)

```text
<tipo>(<ámbito>): <descripción concisa en imperativo y castellano>

[Cuerpo opcional detallando el motivo del cambio y arquitectura]

[Referencias: Closes #3, Hito Paso 3 MVP, RF04]
```

---

## 4. Ciclo de Trabajo Diario

### 1. Iniciar una tarea (desde develop)
```powershell
git checkout develop
git pull origin develop
git checkout -b feat/actividades-campamentos-ramas-3
```

### 2. Desarrollo con commits atómicos
```powershell
dotnet test
git add <archivos>
git commit -m "feat(actividades): incorporar entidad ActividadScout con precio en euros"
```

### 3. Verificación Pre-PR (Gatekeeper)
```powershell
dotnet build WebScout.slnx
dotnet test
```

### 4. Publicación y Pull Request
```powershell
git push -u origin feat/actividades-campamentos-ramas-3
```
- Abrir PR con base en **`develop`**.
- Completar la plantilla `.github/pull_request_template.md`.
- El pipeline de GitHub Actions compilará y ejecutará las pruebas en Ubuntu + PostgreSQL.
- Tras la aprobación, se realiza **Squash and Merge** hacia `develop`.

---

## 5. Salida de Releases hacia Producción (`main`)

Periódicamente, cuando un conjunto de funcionalidades en `develop` alcanza madurez de release:
1. Se crea rama `release/vX.Y.Z` desde `develop`.
2. Se realizan ajustes finales de versión y changelog.
3. Se mergea a `main` y se añade tag `git tag -a vX.Y.Z -m "Release vX.Y.Z"`.
4. Se sincroniza de vuelta con `develop`.

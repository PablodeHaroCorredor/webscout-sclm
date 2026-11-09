---
name: github-pr-lifecycle
description: >-
  Automatiza y guía el ciclo completo de Pull Requests en WebScout SCLM sobre develop y main:
  creación de ramas por funcionalidad (feat/<nombre>-<issue>), verificación de compilación y tests,
  preparación del cuerpo de la PR según la plantilla del repositorio, y procedimiento de fusión limpia.
---

# GitHub PR Lifecycle — Procedimiento Operativo

Esta skill proporciona los pasos directos para abrir, gestionar y cerrar cambios mediante ramas y Pull Requests con alta trazabilidad bajo el modelo GitLab Flow / GitFlow adaptado.

---

## 1. Verificación y Creación de Issue Previa

Antes de crear cualquier rama o escribir código:
1. Comprobar si ya existe una Issue abierta en GitHub para la tarea:
   ```powershell
   $env:GH_PAGER="cat"
   gh issue list --state open
   ```
2. Si no existe, crear la Issue formal con sus criterios de aceptación y etiquetas:
   ```powershell
   gh issue create --title "[Paso X] Título descriptivo" --body "..." --label "..."
   ```

---

## 2. Creación y Publicación de la Rama de Funcionalidad

Partir siempre de **`develop`** actualizado:
```powershell
git checkout develop
git pull origin develop
git checkout -b feat/<nombre-descriptivo>-<numero-issue>
git push -u origin feat/<nombre-descriptivo>-<numero-issue>
```

---

## 3. Protocolo de Calidad Pre-PR (Gatekeeper)

Antes de considerar una rama lista para Pull Request:

1. **Compilación de la solución completa:**
   ```powershell
   dotnet build WebScout.slnx
   ```
2. **Ejecución de la suite completa de pruebas:**
   ```powershell
   dotnet test
   ```
3. **Inspección de archivos en staging:**
   ```powershell
   git status
   git diff --name-only origin/develop
   ```
   *Verificar que no existan ficheros `.env`, volcados o datos con nombres reales de menores.*

---

## 4. Publicación y Creación de PR

1. **Crear la Pull Request en GitHub hacia `develop`:**
   ```powershell
   gh pr create --head feat/<nombre-descriptivo>-<numero-issue> --base develop --title "feat(...): ..." --body "..."
   ```
2. **Estructura obligatoria del cuerpo de la PR (.github/pull_request_template.md):**
   - Enlace formal al Issue: `Closes #<numero-issue>`
   - Hito / Paso correspondiente.
   - Checklist de DoD completado.

---

## 5. Estrategia de Fusión: Squash and Merge (Conservando Ramas)

1. Asegurar que la batería de pruebas (`dotnet test`) y el pipeline de CI están en verde.
2. Realizar **Squash and Merge** a través de GitHub CLI o la interfaz web:
   ```powershell
   gh pr merge <numero-pr> --squash --delete-branch=false
   ```
3. **IMPORTANTE:** NUNCA eliminar la rama remota (`--delete-branch=false`). Las ramas deben permanecer visibles en el repositorio para trazabilidad e histórico de auditoría.
4. Cerrar la issue asociada si no se cerró automáticamente y dejar comentario de trazabilidad:
   ```powershell
   gh issue close <numero-issue> --comment "Completado y verificado mediante PR #<numero-pr>."
   ```
5. En local:
   ```powershell
   git checkout develop
   git pull origin develop
   ```

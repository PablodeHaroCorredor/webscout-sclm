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

## 1. Comprobaciones Previas al Inicio de Rama

Antes de iniciar cualquier desarrollo, siempre partir de **`develop`**:
```powershell
# 1. Asegurar estar en develop y con el árbol limpio
git checkout develop
git status
git pull origin develop
```

---

## 2. Creación de la Rama de Funcionalidad

Seguir la convención: `<tipo>/<nombre-descriptivo>-<numero-issue>`:
```powershell
# Ejemplos:
git checkout -b feat/actividades-campamentos-ramas-3
git checkout -b bugfix/inscripcion-fecha-limite-14
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

1. **Subir rama al remoto:**
   ```powershell
   git push -u origin feat/actividades-campamentos-ramas-3
   ```
2. **Destino del PR:**
   - Base branch: **`develop`** (para features y bugfixes normales).
   - Base branch: **`main`** (únicamente para `release/v...` o `hotfix/...`).

3. **Estructura del PR (plantilla .github/pull_request_template.md):**
   - Enlace al Issue: `Closes #3`
   - Checklist de DoD completado.

---

## 5. Estrategia de Fusión: Squash and Merge

1. Asegurar que el pipeline de GitHub Actions (`ci.yml`) está en verde.
2. Realizar **Squash and Merge** hacia `develop`.
3. Título del commit en `develop`:
   `feat(actividades): publicar campamentos y actividades por ramas (#3)`
4. Borrar la rama remota de la feature.
5. En local:
   ```powershell
   git checkout develop
   git pull origin develop
   git branch -d feat/actividades-campamentos-ramas-3
   ```

---
name: github-pr-lifecycle
description: >-
  Automatiza y guía el ciclo completo de Pull Requests en WebScout SCLM: creación de
  ramas por funcionalidad, verificación de compilación y tests, preparación del cuerpo
  de la PR según la plantilla del repositorio, y procedimiento de fusión limpia.
---

# GitHub PR Lifecycle — Procedimiento Operativo

Esta skill proporciona los pasos directos para abrir, gestionar y cerrar cambios mediante ramas y Pull Requests con alta trazabilidad.

---

## 1. Comprobaciones Previas al Inicio de Rama

Antes de iniciar cualquier desarrollo:
```powershell
# 1. Asegurar estar en main y sin cambios sucios
git checkout main
git status
git pull origin main
```

---

## 2. Creación de la Rama de Funcionalidad

Seguir la convención: `<tipo>/<hito-o-modulo>-<descripcion>`:
```powershell
# Ejemplos:
git checkout -b feat/paso3-actividades-kraal
git checkout -b fix/inscripcion-fecha-limite
```

---

## 3. Protocolo de Calidad Pre-PR (Gatekeeper)

Antes de considerar una rama lista para Pull Request, se debe ejecutar imperativamente la siguiente secuencia:

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
   git diff --name-only origin/main
   ```
   *Verificar que no existan ficheros `.env`, volcados o datos con nombres reales de menores.*

---

## 4. Estructura de la Descripción del PR

Al crear la Pull Request (o preparar su texto para GitHub), aplicar el formato estándar:

```markdown
## 📋 Descripción del Cambio
Implementación de [Módulo/Funcionalidad]. Permite a [Rol] realizar [Acción].

## 🔗 Referencias y Trazabilidad
- **Hito:** Paso [X] del MVP
- **Issue:** Closes #[N]

## 🏷️ Tipo de Cambio
- [x] ✨ `feat`: Nueva funcionalidad

## ✅ Checklist de Aceptación y Calidad (DoD)
- [x] Compilación limpia (`dotnet build`).
- [x] Todos los tests pasan (`dotnet test`).
- [x] Aislamiento multi-inquilino (`IEntidadOrganizacion` verificado).
- [x] Ubicuidad y castellano estricto en entidades y vistas.
- [x] Cero datos reales de menores (RGPD).
- [x] Cero credenciales o secretos en código.
- [x] Componentes MudBlazor estándar y diseño limpio.
```

---

## 5. Estrategia de Fusión: Squash and Merge

Para mantener el historial de `main` limpio, lineal y trazable:
1. Asegurar que el workflow de GitHub Actions (`ci.yml`) está en verde.
2. Realizar **Squash and Merge** en la interfaz de GitHub o mediante comando.
3. Formato del commit resultante:
   `feat(actividades): publicar campamentos y actividades por ramas (#3)`
4. Borrar la rama de funcionalidad una vez fusionada.

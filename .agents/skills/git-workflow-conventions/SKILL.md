---
name: git-workflow-conventions
description: >-
  Use this skill to guide version control operations for WebScout SCLM. Enforces
  Conventional Commits, branch naming strategies, pre-commit hygiene (zero secrets,
  no real minor data), and GitHub synchronization.
---

# Git Workflow & Security Conventions

Esta skill define las directrices obligatorias para el control de versiones en el repositorio **WebScout SCLM** alojado en GitHub.

## 1. Reglas Innegociables de Seguridad y Protección de Datos

En estricto cumplimiento del RGPD / LOPDGDD y las directrices éticas scouts:
- ❌ **JAMÁS hacer commit de datos reales de menores:** Nombres reales, DNIs, fechas de nacimiento, direcciones o teléfonos deben ser siempre datos sintéticos (ej. "Juan Pérez", "test_scout@example.org").
- ❌ **JAMÁS hacer commit de documentos médicos o fichas sanitarias reales:** Usar exclusivamente PDFs sintéticos de prueba con texto de muestra.
- ❌ **JAMÁS hacer commit de credenciales o secretos:** Cadenas de conexión a PostgreSQL, claves de acceso a S3/AWS, tokens de API o contraseñas deben gestionarse mediante `dotnet user-secrets` o variables de entorno locales (`.env`), que están excluidas por `.gitignore`.
- ❌ **JAMÁS hacer commit de volcados de producción:** Los volcados de WordPress o copias de seguridad reales deben almacenarse en repositorios seguros cifrados fuera del árbol de Git.

## 2. Estrategia de Ramas (Git Flow Simplificado)

- `main`: Rama de producción / entregables estables. Todo código aquí debe compilar y haber superado la batería de tests.
- `develop` (o ramas de función directamente si es equipo reducido): Rama de integración continua.
- Ramas temáticas por tarea o entrega:
  - `feat/E0-tenant-prototype`
  - `feat/E1-domain-models`
  - `fix/pa02-revocation-check`
  - `docs/update-adr10-refinements`

## 3. Formato de Mensajes de Commit (Conventional Commits)

Los mensajes de commit deben seguir la convención internacional estructurada:

```text
<tipo>(<ámbito>): <descripción concisa en imperativo o presente>

[cuerpo opcional detallando el motivo del cambio y cómo se valida]

[referencia a requisitos o ADRs, ej: Cierra #12, Cumple RF04, Valida PA01]
```

### Tipos Permitidos:
- `feat`: Nueva funcionalidad o componente (ej: `feat(tenant): resolver subdominios dinámicamente`).
- `fix`: Corrección de un defecto o fallo de validación (ej: `fix(auth): forzar revocación de sesión en cambio de sección`).
- `docs`: Modificaciones únicamente en documentación (ej: `docs(adr): incorporar ADR10 y ADR11`).
- `test`: Añadir o modificar pruebas (ej: `test(pa01): verificar aislamiento de datos entre familias`).
- `refactor`: Refactorización de código sin cambio de comportamiento (ej: `refactor(domain): extraer ValueObject PanoletaConfig`).
- `chore`: Tareas de configuración, herramientas o dependencias (ej: `chore(tools): actualizar paquetes en Directory.Packages.props`).

## 4. Flujo de Trabajo con el Repositorio de GitHub

### Vincular el Repositorio Remoto Personal de GitHub:
```powershell
# Comprobar si existe remoto configurado
git remote -v

# Añadir el repositorio de GitHub personal como origin (sustituyendo <tu-usuario> y <tu-repo>)
git remote add origin https://github.com/<tu-usuario>/webScoutsSclm.git

# Subir la rama principal con seguimiento
git push -u origin main
```

### Ciclo Diario de Trabajo:
```powershell
# 1. Verificar estado y archivos modificados
git status

# 2. Añadir cambios de forma selectiva
git add <archivos-o-rutas>

# 3. Comprobar que no se han colado archivos no deseados ni secretos
git diff --cached

# 4. Crear commit con formato convencional
git commit -m "feat(branding): implementar cálculo de contraste WCAG 2.2 AA"

# 5. Sincronizar con GitHub
git push
```

# El token nunca se escribe en este archivo. Orden de búsqueda:
#   1. Parámetro -Token
#   2. Variable de entorno GITHUB_TOKEN
#   3. Sesión de GitHub CLI (gh auth login)
param (
    [string]$Token = $env:GITHUB_TOKEN,
    [string]$Repo = "PablodeHaroCorredor/webscout-sclm"
)

if ([string]::IsNullOrWhiteSpace($Token) -and (Get-Command gh -ErrorAction SilentlyContinue)) {
    $Token = (gh auth token 2>$null)
}

if ([string]::IsNullOrWhiteSpace($Token)) {
    throw "No hay token de GitHub. Ejecute 'gh auth login', defina GITHUB_TOKEN o pase -Token."
}

$headers = @{
    "Authorization" = "Bearer $Token"
    "Accept" = "application/vnd.github+json"
    "X-GitHub-Api-Version" = "2022-11-28"
    "User-Agent" = "Antigravity-Backlog-Sync"
}

function Invoke-GitHubApi {
    param(
        [string]$Uri,
        [string]$Method = "GET",
        [object]$Body = $null
    )
    
    try {
        if ($null -ne $Body) {
            $json = $Body | ConvertTo-Json -Depth 10
            $bytes = [System.Text.Encoding]::UTF8.GetBytes($json)
            return Invoke-RestMethod -Uri $Uri -Headers $headers -Method $Method -Body $bytes -ContentType "application/json; charset=utf-8"
        } else {
            return Invoke-RestMethod -Uri $Uri -Headers $headers -Method $Method
        }
    } catch {
        Write-Warning "Fallo en API ($Method $Uri): $_"
        return $null
    }
}

Write-Host "==========================================" -ForegroundColor Cyan
Write-Host "1. Creando Sistema de Etiquetas (Labels)..." -ForegroundColor Cyan
Write-Host "==========================================" -ForegroundColor Cyan

$labels = @(
    # Prioridades
    @{ name = "P0: Bloqueante"; color = "b60205"; description = "Prioridad P0 crítica que bloquea la salida" },
    @{ name = "P1: MVP Piloto"; color = "d93f0b"; description = "Prioridad P1 imprescindible para el piloto Lestonnac 330" },
    @{ name = "P2: Evolutivo"; color = "0e8a16"; description = "Mejora o característica planificada tras el piloto" },

    # Entregas / Fases
    @{ name = "E0: Prototipos"; color = "6f42c1"; description = "Entrega 0 - Descubrimiento y prototipos técnicos" },
    @{ name = "E1: Base y Acceso"; color = "5319e7"; description = "Entrega 1 - Solución, identidad, permisos y persistencia" },
    @{ name = "E2: Actividades"; color = "1b5e20"; description = "Entrega 2 - Campamentos, inscripciones y autorizaciones" },
    @{ name = "E3: Gestion"; color = "006b75"; description = "Entrega 3 - Fotos, pedidos sencillos y solicitudes" },
    @{ name = "E4: Web Publica"; color = "fb8500"; description = "Entrega 4 - Portada de grupo, pañoleta y WCAG 2.2 AA" },
    @{ name = "E5: Piloto"; color = "d73a4a"; description = "Entrega 5 - Migración WordPress y corte operativo" },

    # Módulos Funcionales
    @{ name = "mod:tenancy"; color = "0052cc"; description = "Resolución de subdominios y aislamiento por OrganizationId" },
    @{ name = "mod:auth-identity"; color = "0366d6"; description = "Autenticación, roles y autorización por recurso" },
    @{ name = "mod:actividades"; color = "28a745"; description = "Gestión de salidas, campamentos y fechas límite" },
    @{ name = "mod:documental"; color = "6f42c1"; description = "Almacenamiento privado S3/MinIO y PDFs firmados inmutables" },
    @{ name = "mod:branding-wcag"; color = "f9a825"; description = "Personalización de pañoletas y contraste accesible WCAG 2.2" },
    @{ name = "mod:pedidos"; color = "d4ac0d"; description = "Pedidos sencillos de pañoletas y camisetas (Core 1.29)" },
    @{ name = "mod:fotos"; color = "e91e63"; description = "Álbumes privados y carga progresiva de fotografías" },
    @{ name = "mod:migracion"; color = "546e7a"; description = "Importación histórica de datos de WordPress (Lestonnac 330)" },

    # Trazabilidad de Calidad
    @{ name = "PA: Aceptacion"; color = "008672"; description = "Trazable a una de las 19 Pruebas de Aceptación PA01-PA19" },
    @{ name = "seguridad-rgpd"; color = "800000"; description = "Protección de datos de menores y consentimiento informado" }
)

foreach ($lbl in $labels) {
    Write-Host "Configurando etiqueta: $($lbl.name)..."
    $res = Invoke-GitHubApi -Uri "https://api.github.com/repos/$Repo/labels" -Method POST -Body $lbl
    if ($null -eq $res) {
        # Intentar actualizar si ya existía
        $encodedName = [System.Uri]::EscapeDataString($lbl.name)
        Invoke-GitHubApi -Uri "https://api.github.com/repos/$Repo/labels/$encodedName" -Method PATCH -Body @{ color = $lbl.color; description = $lbl.description } | Out-Null
    }
}

Write-Host "`n==========================================" -ForegroundColor Cyan
Write-Host "2. Creando Hitos del Proyecto (Milestones)..." -ForegroundColor Cyan
Write-Host "==========================================" -ForegroundColor Cyan

$milestonesData = @(
    @{ title = "E0: Descubrimiento y Prototipos"; description = "Decisiones críticas de inicio, inventario WordPress y validación técnica (IN01 a IN06)." },
    @{ title = "E1: Base y Acceso"; description = "Monolito modular, autenticación multitenant, políticas de seguridad, persistencia y outbox." },
    @{ title = "E2: Actividades y Documentos"; description = "Inscripciones familiares, justificantes compartidos entre hermanos y revisión por sección." },
    @{ title = "E3: Gestión Cotidiana"; description = "Subida progresiva de fotos de campamento, pedidos sencillos y buzón de contacto." },
    @{ title = "E4: Web Pública y Tematización"; description = "Portada de grupo, personalización de colores de pañoleta y cumplimiento estricto WCAG 2.2 AA." },
    @{ title = "E5: Piloto Lestonnac 330 y Corte"; description = "Importación final de WordPress, verificación PA01-PA19, formación y puesta en marcha." }
)

$milestoneMap = @{}

foreach ($m in $milestonesData) {
    Write-Host "Creando Hito: $($m.title)..."
    $res = Invoke-GitHubApi -Uri "https://api.github.com/repos/$Repo/milestones" -Method POST -Body $m
    if ($null -ne $res) {
        $milestoneMap[$m.title] = $res.number
    } else {
        # Obtener los existentes si ya estaban
        $existing = Invoke-GitHubApi -Uri "https://api.github.com/repos/$Repo/milestones" -Method GET
        foreach ($ex in $existing) {
            $milestoneMap[$ex.title] = $ex.number
        }
    }
}

Write-Host "Hitos mapeados: $($milestoneMap.Count)" -ForegroundColor Green

Write-Host "`n==========================================" -ForegroundColor Cyan
Write-Host "3. Creando Tareas Iniciales de E0 (IN01 a IN06)..." -ForegroundColor Cyan
Write-Host "==========================================" -ForegroundColor Cyan

$e0Tasks = @(
    @{
        title = "IN01: Taller de alcance, actores y prioridades del piloto"
        milestone = $milestoneMap["E0: Descubrimiento y Prototipos"]
        labels = @("E0: Prototipos", "P0: Bloqueante", "mod:tenancy")
        body = @"
## 🎯 Objetivo
Definir y consensuar con los responsables de SCLM y Lestonnac 330 el alcance exacto del piloto, los actores y las decisiones críticas de inicio (DC01 a DC03).

### Criterios de Aceptación
- [ ] Ratificar DC01 (Plataforma Multitenant con Lestonnac 330 como piloto).
- [ ] Identificar a los responsables de Producto, Técnica, Revisión Funcional y Operación.
- [ ] Listar las funciones indispensables para la salida del piloto.

**Referencia documental:** `docs/planificacion/plan-entregas-y-backlog.md` y `docs/planificacion/decisiones-y-riesgos.md`.
"@
    },
    @{
        title = "IN02: Inventario y análisis de datos históricos de WordPress (Lestonnac 330)"
        milestone = $milestoneMap["E0: Descubrimiento y Prototipos"]
        labels = @("E0: Prototipos", "P1: MVP Piloto", "mod:migracion")
        body = @"
## 🎯 Objetivo
Analizar la base de datos actual de WordPress de Lestonnac 330: volumen de usuarios, actividades abiertas, modelos de formularios de inscripción y archivos adjuntos existentes.

### Criterios de Aceptación
- [ ] Inventario de entradas, páginas y archivos por procedencia.
- [ ] Identificar actividades abiertas en `signed_pdf` y `legacy_form`.
- [ ] Comprobar volumen total de fotografías y documentos para estimar almacenamiento.

**Referencia documental:** `docs/migracion/plan-migracion-wordpress.md`.
"@
    },
    @{
        title = "IN03: Prototipo de autorización por recurso y revocación inmediata"
        milestone = $milestoneMap["E0: Descubrimiento y Prototipos"]
        labels = @("E0: Prototipos", "P0: Bloqueante", "mod:auth-identity", "PA: Aceptacion")
        body = @"
## 🎯 Objetivo
Validar que la autorización evalúa siempre `(Usuario, Organización, Sección, Capacidad, Recurso)` y que la revocación de permisos tiene efecto inmediato en la siguiente petición.

### Criterios de Aceptación
- [ ] Validar PA01: La Familia A no puede acceder a recursos de la Familia B.
- [ ] Validar PA02: Un Scouter solo revisa su sección asignada; la revocación bloquea la siguiente operación (403).
- [ ] Validar PA18: Peticiones en `lestonnac` jamás retornan datos de `sanvicente`.

**Referencia documental:** `docs/architecture/politica-seguridad-y-permisos.md` y `docs/testing/estrategia-y-trazabilidad.md`.
"@
    },
    @{
        title = "IN04: Prototipo documental y almacenamiento privado (S3/MinIO)"
        milestone = $milestoneMap["E0: Descubrimiento y Prototipos"]
        labels = @("E0: Prototipos", "P0: Bloqueante", "mod:documental", "PA: Aceptacion")
        body = @"
## 🎯 Objetivo
Probar el flujo de almacenamiento privado de documentos y fotografías: inmutabilidad con SHA-256, sin acceso anónimo en `wwwroot` y tolerancia a interrupciones móviles.

### Criterios de Aceptación
- [ ] Validar PA06: Los documentos firmados conservan su hash inmutable y no se sobrescriben.
- [ ] Validar PA10: Prueba de carga de 100 fotos con pausa, reintento y finalización parcial.
- [ ] Comprobar generación de URLs prefirmadas o streaming autenticado.

**Referencia documental:** `docs/architecture/politica-ciclo-vida-y-versiones.md`.
"@
    },
    @{
        title = "IN05: Prototipo de transacciones y patrón Outbox de notificaciones"
        milestone = $milestoneMap["E0: Descubrimiento y Prototipos"]
        labels = @("E0: Prototipos", "P0: Bloqueante", "mod:actividades", "PA: Aceptacion")
        body = @"
## 🎯 Objetivo
Demostrar que una caída temporal del servidor SMTP (Mailpit) o de la red no interrumpe la transacción de una inscripción o pedido, persistiendo el aviso en la tabla Outbox.

### Criterios de Aceptación
- [ ] Validar PA11: Trámite guardado en base de datos con servidor de correo apagado.
- [ ] El worker de segundo plano reintenta y entrega los mensajes pendientes al reiniciar el servicio de correo.
- [ ] Deduplicación de intención para no enviar correos repetidos.

**Referencia documental:** `docs/architecture/decisiones-arquitectura.md` (ADR07).
"@
    },
    @{
        title = "IN06: Estimación de costes de infraestructura y plan de recuperación (DR)"
        milestone = $milestoneMap["E0: Descubrimiento y Prototipos"]
        labels = @("E0: Prototipos", "P1: MVP Piloto", "mod:tenancy")
        body = @"
## 🎯 Objetivo
Cerrar las decisiones de proveedor de nube/hosting (DC03, DC05, DC08) y definir el protocolo de copias de seguridad de PostgreSQL y almacenamiento de archivos.

### Criterios de Aceptación
- [ ] Dimensionamiento de costes para los 15-30 grupos de SCLM.
- [ ] Validar PA19: Ensayo de copia de seguridad coherente y restauración medida dentro de los objetivos RPO y RTO.

**Referencia documental:** `docs/deployment/operacion-y-recuperacion.md`.
"@
    }
)

foreach ($task in $e0Tasks) {
    Write-Host "Creando Issue: $($task.title)..."
    Invoke-GitHubApi -Uri "https://api.github.com/repos/$Repo/issues" -Method POST -Body $task | Out-Null
    Start-Sleep -Milliseconds 400
}

Write-Host "`n==========================================" -ForegroundColor Cyan
Write-Host "4. Creando Épicas del Backlog (BL01 a BL12)..." -ForegroundColor Cyan
Write-Host "==========================================" -ForegroundColor Cyan

$epics = @(
    @{
        title = "BL01: Base de acceso, autenticación multitenant y gestión de archivos privados"
        milestone = $milestoneMap["E1: Base y Acceso"]
        labels = @("E1: Base y Acceso", "P0: Bloqueante", "mod:auth-identity", "mod:documental", "PA: Aceptacion")
        body = @"
## 📖 Descripción de la Épica
Construir la infraestructura de seguridad e identidad de la plataforma WebScout SCLM: cuentas de usuario, sesiones con aislamiento por `OrganizationId`, almacenamiento privado en S3 y auditoría de eventos.

### Requisitos Vinculados
- **RF01 a RF08:** Cuentas, perfiles de familia, vinculación a sección y capacidades por recurso.
- **RF35 a RF38:** Almacenamiento seguro, inmutabilidad y streaming de documentos privados.
- **RF40, RF60:** Registro inmutable en `AuditEvent`.

### Pruebas de Aceptación
- [ ] **PA01:** Familia A frente a Familia B (cero cruce de documentos).
- [ ] **PA02:** Revisor con alcance limitado a su sección y revocación inmediata.
- [ ] **PA03:** Contenido público accesible sin abrir fichas privadas.
- [ ] **PA09:** Concurrencia y reintentos ante fallos de red.
- [ ] **PA14:** Supresión y conservación legal.
"@
    },
    @{
        title = "BL02: Comunicaciones privadas y noticias públicas del grupo"
        milestone = $milestoneMap["E2: Actividades y Documentos"]
        labels = @("E2: Actividades", "P1: MVP Piloto", "mod:actividades")
        body = @"
## 📖 Descripción de la Épica
Publicación de avisos generales del grupo scout, tablón de anuncios por sección y blog público sin exponer datos ni activos privados.

### Requisitos Vinculados
- **RF09:** Notificaciones y avisos automáticos a las familias.
- **RF17 a RF23:** Creación, moderación y copia pública de artículos y circulares.

### Pruebas de Aceptación
- [ ] **PA12:** Copia pública de avisos y moderación sin filtración de archivos privados.
- [ ] **PA17:** Accesibilidad y lectura clara en dispositivos móviles.
"@
    },
    @{
        title = "BL03: Formulario de contacto, buzón y solicitudes de admisión"
        milestone = $milestoneMap["E3: Gestión Cotidiana"]
        labels = @("E3: Gestion", "P1: MVP Piloto", "mod:actividades")
        body = @"
## 📖 Descripción de la Épica
Gestión de solicitudes de información y lista de espera para nuevos educandos que quieren unirse al grupo scout.

### Requisitos Vinculados
- **RF10 a RF14:** Formulario de contacto, captcha accesible, confirmación a la familia y seguimiento por secretaría sin alta de cuenta automática.

### Pruebas de Aceptación
- [ ] **PA11:** Envío duradero vía Outbox con tolerancia a caídas del correo.
- [ ] **PA13:** Consentimiento explícito de privacidad (sin casillas premarcadas).
"@
    },
    @{
        title = "BL04: Formularios dinámicos y compatibilidad histórica"
        milestone = $milestoneMap["E2: Actividades y Documentos"]
        labels = @("E2: Actividades", "P1: MVP Piloto", "mod:actividades")
        body = @"
## 📖 Descripción de la Épica
Soporte para campos personalizados en actividades scouts (dietas, autorizaciones especiales) y preservación de respuestas históricas.

### Requisitos Vinculados
- **RF15 a RF16:** Captura abierta de campos y compatibilidad con formatos heredados.

### Pruebas de Aceptación
- [ ] **PA15:** Compatibilidad histórica de inscripciones sin pérdida de etiquetas ni atribuciones.
"@
    },
    @{
        title = "BL05: Gestión de actividades, inscripciones familiares y justificantes compartidos"
        milestone = $milestoneMap["E2: Actividades y Documentos"]
        labels = @("E2: Actividades", "P0: Bloqueante", "mod:actividades", "PA: Aceptacion")
        body = @"
## 📖 Descripción de la Épica
Módulo central de salidas, acampadas y campamentos de verano: publicación de fechas, plazos de inscripción, cuotas y subida de autorizaciones firmadas.

### Requisitos Vinculados
- **RF24 a RF34:** Modelo de actividad, inscripción de participantes, justificante bancario compartido entre hermanos y revisión por Scouters.

### Pruebas de Aceptación
- [ ] **PA02:** Revisión de inscripciones limitada a la sección del Scouter.
- [ ] **PA04:** Dos hermanos con un único justificante bancario compartido; corregir uno conserva el otro.
- [ ] **PA05:** Rechazo de inscripciones fuera de plazo o con modelos obsoletos sin crear registros parciales.
- [ ] **PA06:** Conservación de versiones y huellas SHA-256 de autorizaciones.
"@
    },
    @{
        title = "BL06: Álbumes de campamento, subida progresiva y miniaturas privadas"
        milestone = $milestoneMap["E3: Gestión Cotidiana"]
        labels = @("E3: Gestion", "P1: MVP Piloto", "mod:fotos")
        body = @"
## 📖 Descripción de la Épica
Galería fotográfica privada para que las familias vean el día a día de sus hijos en los campamentos con descarga segura y carga masiva para Scouters.

### Requisitos Vinculados
- **RF39, RF41 a RF47:** Álbumes por actividad y sección, carga progresiva de lotes de hasta 100 fotos y generación de miniaturas WebP.

### Pruebas de Aceptación
- [ ] **PA09:** Concurrencia y reanudación ante pérdida de conexión móvil.
- [ ] **PA10:** Prueba de 100 fotos con pausa y reintento sin duplicados.
"@
    },
    @{
        title = "BL07: Catálogo y pedidos sencillos de material scout (Core 1.29)"
        milestone = $milestoneMap["E3: Gestión Cotidiana"]
        labels = @("E3: Gestion", "P1: MVP Piloto", "mod:pedidos")
        body = @"
## 📖 Descripción de la Épica
Petición de pañoletas, uniformes, camisetas y cantimploras del grupo según el modelo simplificado acordado (Core 1.29): sin bloqueo bancario rígido y con trazabilidad de 6 estados.

### Requisitos Vinculados
- **RF48 a RF54:** Catálogo, variantes por talla, estados del pedido e historial administrativo.

### Pruebas de Aceptación
- [ ] **PA07:** Pedido sencillo: entrega permitida sin recibo; no existen acciones económicas rígidas 1.28.
- [ ] **PA08:** Rectificación administrativa de pedidos con registro de auditoría.
"@
    },
    @{
        title = "BL08: Protección de datos (RGPD), consentimientos y conservación legal"
        milestone = $milestoneMap["E1: Base y Acceso"]
        labels = @("E1: Base y Acceso", "P0: Bloqueante", "seguridad-rgpd", "PA: Aceptacion")
        body = @"
## 📖 Descripción de la Épica
Cumplimiento normativo estricto del RGPD y LOPDGDD para la protección de menores, imágenes y datos médicos scouts.

### Requisitos Vinculados
- **RF55 a RF59:** Consentimiento informado, registro de versiones de cláusulas legales y bloqueo de borrado para expedientes vinculados a subvenciones vigentes.

### Pruebas de Aceptación
- [ ] **PA13:** Consentimiento explícito sin casillas premarcadas con texto versionado.
- [ ] **PA14:** Supresión y conservación justificada ante solicitudes de familias.
"@
    },
    @{
        title = "BL09: Portal web público, personalización de pañoletas y WCAG 2.2 AA"
        milestone = $milestoneMap["E4: Web Pública y Tematización"]
        labels = @("E4: Web Publica", "P1: MVP Piloto", "mod:branding-wcag")
        body = @"
## 📖 Descripción de la Épica
Diseño e identidad visual de la web del grupo scout: personalización de colores de pañoleta, escudo, tokens CSS sanitizados y cumplimiento de accesibilidad WCAG 2.2 Nivel AA.

### Requisitos Vinculados
- **RF61 a RF63:** Identidad scout, motor de pañoleta SVG y contraste mínimo 4.5:1.

### Pruebas de Aceptación
- [ ] **PA16:** Guardado y reset de configuración visual sin alterar ajustes de otros grupos.
- [ ] **PA17:** Navegación por teclado, foco visible, reflujo móvil y contraste verificado.
"@
    },
    @{
        title = "BL10: Panel de administración del grupo, diagnóstico de outbox y backups"
        milestone = $milestoneMap["E1: Base y Acceso"]
        labels = @("E1: Base y Acceso", "P1: MVP Piloto", "mod:tenancy")
        body = @"
## 📖 Descripción de la Épica
Panel de control para la Coordinación del grupo: altas de miembros en secciones, monitorización de la cola de correos y estado de copias de seguridad.

### Requisitos Vinculados
- **RF64 a RF65:** Ajustes del grupo scout, visor de auditoría y diagnóstico de workers.

### Pruebas de Aceptación
- [ ] **PA11:** Reintento observable de notificaciones en cola.
- [ ] **PA19:** Verificación del estado de copias de seguridad.
"@
    },
    @{
        title = "BL11: Guías y ayuda contextual para familias y Scouters"
        milestone = $milestoneMap["E3: Gestión Cotidiana"]
        labels = @("E3: Gestion", "P2: Evolutivo", "mod:actividades")
        body = @"
## 📖 Descripción de la Épica
Asistencia integrada en la interfaz que guía paso a paso a padres y educandos novatos en su primera inscripción o compra de material scout.

### Requisitos Vinculados
- **RF66 a RF67:** Asistentes contextuales por perfil de usuario y progreso persistente.

### Pruebas de Aceptación
- [ ] **PA17:** Ayuda accesible sin interrumpir la navegación por teclado.
"@
    },
    @{
        title = "BL12: Herramienta de migración WordPress (Lestonnac 330) y corte operativo"
        milestone = $milestoneMap["E5: Piloto Lestonnac 330 y Corte"]
        labels = @("E5: Piloto", "P0: Bloqueante", "mod:migracion", "PA: Aceptacion")
        body = @"
## 📖 Descripción de la Épica
Consola de migración de datos (`WebScout.Migration`): importación de usuarios, historial de actividades y posts desde WordPress, conciliación de sumas de verificación y corte final a producción.

### Requisitos Vinculados
- **RF68 a RF71:** Extracción, transformación, carga (ETL), verificación de integridad y ensayo de restauración en desastre.

### Pruebas de Aceptación
- [ ] **PA15:** Conciliación histórica de datos sin pérdida de autorías ni fechas.
- [ ] **PA18:** Carga y aislamiento de organización con volumen de referencia medido.
- [ ] **PA19:** Corte y recuperación completa demostrada antes de apagar el servidor antiguo.
"@
    }
)

foreach ($epic in $epics) {
    Write-Host "Creando Épica: $($epic.title)..."
    Invoke-GitHubApi -Uri "https://api.github.com/repos/$Repo/issues" -Method POST -Body $epic | Out-Null
    Start-Sleep -Milliseconds 400
}

Write-Host "`n==========================================" -ForegroundColor Green
Write-Host "✅ Proceso completado con éxito en GitHub!" -ForegroundColor Green
Write-Host "==========================================" -ForegroundColor Green

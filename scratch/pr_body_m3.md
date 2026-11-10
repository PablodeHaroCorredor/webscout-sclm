Resuelve #38

### Descripción
Implementa el conmutador dinámico de contexto de trabajo (**"Modo Kraal"** vs **"Modo Familiar"**) en la barra de navegación lateral (`MainLayout.razor`) para resolver la sobrecarga cognitiva en usuarios con rol dual (*"Scouter-Padre"*):

1. **Detección Automática de Rol Dual**:
   - Se identifica si el usuario activo combina rol de gestión (`Scouter`, `Coordinacion`, `Administracion`) con contexto familiar (rol `Familia` o educandos censados bajo su tutela legal).
   - Usuarios monofunción (solo Familia o solo Scouter sin hijos) mantienen su navegación estándar sin conmutadores innecesarios.

2. **Conmutador Segmentado de Alta Fidelidad**:
   - Componente de selector segmentado en el lateral con iconos distintivos (`Diversity3` para Kraal y `FamilyRestroom` para Familia).
   - Indicador de estado activo también en el AppBar superior (`Modo Kraal` / `Modo Familiar`).
   - Contraste WCAG 2.2 AA verificado en tests unitarios.

3. **Filtrado Limpio de Menú**:
   - En **Modo Familiar**: visualización exclusiva de los módulos familiares (*Mi Espacio Familiar*, *Mis Educandos*, *Actividades y Salidas*, *Mis Inscripciones*, *Material y Uniformidad*).
   - En **Modo Kraal**: visualización de los módulos de gestión y sección (*Mesa de Inscripciones*, *Censo de Educandos*, *Gestor de Actividades*, *Buzón*, *Pedidos*, *Administración*).

4. **Sincronización Bidireccional de Rutas y Transición Fluida**:
   - Al pulsar el conmutador, si el usuario se encuentra en una página del otro modo, se redirige de forma coherente (`/familias` o `/kraal/inscripciones`).
   - Si el usuario accede por URL directa a un recurso del otro modo, el conmutador se sincroniza automáticamente sin bloquear la navegación.
   - Estado de sesión mantenido en el circuito mediante `UserContextModeService`.

5. **Actualizaciones en Semillas y Autorizaciones**:
   - Usuario demo `Elena Akela` (`scouter.manada@scoutslestonnac.es`) configurada como Scouter-Padre en Lestonnac 330 con su hijo Mateo censado en Castores.
   - Autorizaciones en `FamilyDashboard.razor` y `FamilyParticipants.razor` ampliadas para admitir el rol dual.

### Verificación
- 128/128 pruebas automáticas superadas (100% de éxito).
- Cobertura con nuevos tests de contraste WCAG y pruebas unitarias de `UserContextModeService`.

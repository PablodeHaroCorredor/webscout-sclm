# Manual de Identidad Corporativa y Sistema de Diseño

**Federación Scouts de Castilla-La Mancha (SCLM)**  
**Código documental:** `SCLM-DIR-DIS-01`  
**Versión:** 1.0.0 · Documento normativo de identidad y producto digital  
**Fecha de emisión:** Noviembre de 2026  
**Ámbito de aplicación:** Plataforma WebScout SCLM, portales locales de grupos federados, comunicaciones transaccionales e interfaces de usuario.

---

## 1. Introducción y Marco Normativo

### 1.1. Objeto y Alcance
El presente manual constituye el marco preceptivo de diseño e identidad visual para la plataforma tecnológica **WebScout SCLM** y todos los servicios digitales derivados de la Federación Scouts de Castilla-La Mancha. Su finalidad es fijar directrices objetivas, reproducibles y verificables que aseguren:

1. **Coherencia institucional:** Articular la convivencia armónica entre la identidad corporativa regional (SCLM) y la identidad tradicional de cada grupo scout federado (*tenant*).
2. **Accesibilidad universal por contrato:** Cumplir de forma estricta los criterios de conformidad del nivel **AA** de las **Pautas de Accesibilidad para el Contenido Web (WCAG 2.2)**, de acuerdo con la norma europea EN 301 549.
3. **Calidad de ingeniería en interfaz (Frontend Performance):** Diseñar patrones visuales optimizados para dispositivos móviles con baja latencia (*Largest Contentful Paint* $\le 1.0\text{ s}$), minimizando la sobrecarga de red y garantizando la robustez en entornos de campo o cobertura deficiente.

### 1.2. Principios Fundacionales de Diseño

| Principio | Criterio de aplicación |
| :--- | :--- |
| **Sobriedad funcional** | Toda decisión visual responde a una necesidad de comunicación o interacción. Se prescinde de elementos puramente ornamentales que añadan ruido visual o comprometan el rendimiento. |
| **Diseño con vocación de servicio** | Las personas destinatarias prioritarias son familias y educadores scouts (*scouters*). Los flujos de tramitación, consulta médica y abonos económicos deben ser inequívocos, legibles y directos. |
| **Independencia del color** | La información crítica (estados de expediente, plazos, errores de validación) nunca se transmite exclusivamente mediante variaciones cromáticas. Debe complementarse siempre con texto explícito o iconografía normalizada. |
| **Seguridad estructural en runtime** | La personalización de los grupos scouts se parametriza mediante un esquema de datos tipado y tokens CSS acotados. Queda expresamente prohibida la inyección de hojas de estilo arbitrarias o código script externo en el cliente. |

---

## 2. Arquitectura de Marca y Modelo Federativo

WebScout SCLM implementa un modelo de **marca respaldada (*endorsed branding*)**, donde la solvencia y garantías técnicas de la Federación Regional respaldan la personalidad histórica de cada Grupo Scout local:

```
┌────────────────────────────────────────────────────────────────────────┐
│                   NIVEL 1: MARCA INSTITUCIONAL SCLM                    │
│      Gobernanza regional, marco legal, plataforma tecnológica central   │
└───────────────────────────────────┬────────────────────────────────────┘
                                    │ Respalda y aloja
                                    ▼
┌────────────────────────────────────────────────────────────────────────┐
│                   NIVEL 2: GRUPOS SCOUTS FEDERADOS                     │
│    (Tenant: Grupo Scout Lestonnac 330, San Vicente 412, etc.)          │
│    Pañoleta oficial, heráldica local, subdominio '{slug}.scoutsclm.org'│
└───────────────────────────────────┬────────────────────────────────────┘
                                    │ Estructura pedagógica
                                    ▼
┌────────────────────────────────────────────────────────────────────────┐
│              NIVEL 3: RAMAS Y SECCIONES EDUCATIVAS                     │
│    Castores (6-8), Manada (8-11), Sección Scout (11-14),               │
│    Unidad Esculta (14-17), Clan Rover (17-21)                          │
└────────────────────────────────────────────────────────────────────────┘
```

* **Presencia del Nivel 1 en interfaces de grupo:** Toda cabecera o pie de portal local incluye la referencia federativa «*Miembro de Scouts de Castilla-La Mancha · ASDE*», vinculando a los servicios y normativas centrales.
* **Autonomía del Nivel 2:** El grupo federado define su esquema de pañoleta, escudo oficial, denominación y paleta cromática principal, sujeto a las restricciones de contraste de la Sección 4.
* **Coherencia del Nivel 3:** La asignación de nombres y rangos de color por rama educativa es uniforme en toda la plataforma regional, garantizando que un educando o familia reconozca inmediatamente su sección con independencia del grupo al que pertenezca.

---

## 3. Elementos Gráficos e Identidad Simbólica

### 3.1. Isotipo Institucional: La Flor de Lis
La Flor de Lis es el emblema internacional del Escultismo y el signo identificador de SCLM.
* **Geometría y proporciones:** Se preserva el vector oficial con sus tres pétalos, anillo de unión y aguja de orientación.
* **Área de seguridad:** Ningún elemento textual o gráfico debe situarse a una distancia inferior a $0.5 \times X$, donde $X$ es la anchura total del isotipo.
* **Tamaño mínimo digital:** $24 \times 24\text{ px}$ en entornos de interfaz compacta (iconografía de barra de herramientas) y $16 \times 16\text{ px}$ para *favicon*.

### 3.2. La Pañoleta Scout: Geometría Vectorial Paramétrica
La pañoleta scout se digitaliza como un **activo vectorial escalable (SVG)** generado en servidor a partir del registro tipado `PanoletaConfig`. Se desaconseja terminantemente el uso de fotografías o mapas de bits para representar la pañoleta en elementos de navegación.

```
       (0, 0) ════════════════════════════════════════════════════ (100, 0)
         ╲                                                        ╱
          ╲                                                      ╱
           ╲                Cinta Central (opcional)            ╱
            ╲                       │                          ╱
         Ribete                     │                       Ribete
       Perimetral                   │                     Perimetral
             ╲                      │                        ╱
              ╲                     │                       ╱
               ╲                    ▼                      ╱
                ╲               [Emblema]                 ╱
                 ╲                                       ╱
                  ╲                                     ╱
                   ╲                                   ╱
                    ════════════════ (50, 75) ═════════
```

#### Especificación Geométrica (Sistema de Coordenadas 100 × 75)
* **Triángulo base exterior:** Polígono de vértices $(0,0)$, $(100,0)$ y $(50,75)$. En pañoletas con ribete perimetral, este elemento actúa como fondo del ribete.
* **Triángulo interior de fondo:** Polígono con retranqueo uniforme de 6 unidades respecto a los bordes exteriores: $(7,4)$, $(93,4)$ y $(50,67)$. Relleno con el color base del grupo (`BaseColorHex`).
* **Cinta central divisoria:** Trazo vertical comprendido entre $(50,4)$ y $(50,67)$, con anchura de trazo fija de 4 unidades normalizadas (`stroke-width: 4; stroke-linecap: square`).
* **Insignia / Emblema de vértice:** Círculo centrado en $(50, 42)$ con radio de 8 unidades, fondo neutro de contraste y silueta vectorial heráldica del grupo o flor de lis.

---

## 4. Sistema Cromático y Tokens de Diseño

### 4.1. Paleta Institucional SCLM
La identidad regional se construye a partir de tonos que combinan el púrpura tradicional del escultismo con colores representativos del patrimonio y geografía manchega:

| Referencia de Token | Valor HEX | RGB | CMYK | Uso y Función en Interfaz |
| :--- | :--- | :--- | :--- | :--- |
| `--sclm-purple-base` | `#4A148C` | `74, 20, 140` | `83, 100, 14, 18` | Color institucional principal SCLM, cabeceras centrales, botones corporativos |
| `--sclm-purple-dark` | `#2A0845` | `42, 8, 69` | `89, 100, 27, 56` | Superficies de contraste alto, textos sobre fondos claros de realce |
| `--sclm-ochre-accent` | `#C25E00` | `194, 94, 0` | `18, 73, 100, 7` | Tono acento (cerámica/tierra), llamadas a la acción secundarias |
| `--sclm-canvas-light` | `#F8FAFC` | `248, 250, 252` | `2, 1, 0, 0` | Superficie base de lectura en viewport general |
| `--sclm-text-primary` | `#0F172A` | `15, 23, 42` | `85, 75, 49, 61` | Tipografía principal (cumple ratio 16.1:1 contra fondo blanco) |

### 4.2. Tokens Dinámicos por Grupo Scout (*Tenant Branding*)
La personalización de cada grupo se materializa inyectando de forma estricta las siguientes variables en la raíz `:root` del documento:

```css
:root {
  /* Marca e Interacción del Grupo */
  --color-brand-primary:          #1B5E20; /* Color primario validado */
  --color-brand-primary-hover:    #144618; /* Variante -12% luminosidad */
  --color-brand-on-primary:       #FFFFFF; /* Calculado: ratio >= 4.5:1 */
  --color-brand-secondary:        #FBC02D; /* Color complementario */
  --color-brand-on-secondary:     #1E293B;

  /* Pañoleta Oficial */
  --panoleta-color-base:          #1B5E20;
  --panoleta-color-border:        #FBC02D;
  --panoleta-color-stripe:        #FFFFFF;

  /* Superficies Neutras y Bordes */
  --color-surface-canvas:         #F8FAFC;
  --color-surface-panel:          #FFFFFF;
  --color-border-subtle:          #E2E8F0;
  --color-border-default:         #CBD5E1;
  --color-border-focus:           #FBC02D;

  /* Tipografía Neutra */
  --color-text-primary:           #0F172A;
  --color-text-secondary:         #475569;
  --color-text-muted:             #64748B;
}
```

### 4.3. Código Cromático Oficial de Ramas Pedagógicas
Las secciones educativas mantienen una codificación inmutable en toda la federación, estandarizada con valores que satisfacen contraste accesible:

| Sección | Rango de Edad | Lema Oficial | Color Base | Token HEX Fondo | Token HEX Texto | Ratio WCAG |
| :--- | :--- | :--- | :--- | :--- | :--- | :--- |
| **Castores** | 6 a 8 años | *Compartir* | Naranja / Celeste | `#FFF3E0` | `#9A3412` | 5.8:1 (AA) |
| **Manada** | 8 a 11 años | *Haremos lo mejor* | Amarillo / Tierra | `#FEF9C3` | `#854D0E` | 5.5:1 (AA) |
| **Sección Scout** | 11 a 14 años | *Siempre listos* | Azul / Verde | `#E0F2FE` | `#075985` | 6.2:1 (AA) |
| **Unidad Esculta**| 14 a 17 años | *Unidad* | Rojo / Blanco | `#FEE2E2` | `#991B1B` | 6.8:1 (AA) |
| **Clan Rover** | 17 a 21 años | *Servir* | Verde / Burdeos | `#DCFCE7` | `#166534` | 6.4:1 (AA) |

### 4.4. Algoritmo de Verificación de Contraste (WCAG 2.2 AA)
El backend de WebScout SCLM ejecuta la fórmula oficial de luminancia relativa previa al guardado de cualquier configuración cromática:

$$L = 0.2126 \cdot R_{sRGB} + 0.7152 \cdot G_{sRGB} + 0.0722 \cdot B_{sRGB}$$

$$\text{Ratio} = \frac{L_1 + 0.05}{L_2 + 0.05} \ge 4.5:1 \quad (\ge 3.0:1 \text{ para componentes de interfaz})$$

* **Decisión automática de legibilidad:** Si el color principal del grupo tiene una luminancia $L > 0.179$, el color de texto superpuesto se fuerza automáticamente a `#0F172A` (carbón oscuro). Si $L \le 0.179$, se fuerza a `#FFFFFF`.
* **Bloqueo de guardado:** Ninguna configuración que resulte en un ratio $< 4.5:1$ para textos de navegación o botones primarios puede persistirse en la base de datos.

---

## 5. Sistema Tipográfico

### 5.1. Pila Tipográfica del Sistema (*Native System Font Stack*)
Para suprimir peticiones externas de tipografía web, evitar desfases de maquetación (*Layout Shift*) y reducir a cero el tiempo de renderizado de fuentes en redes móviles, se adopta de forma imperativa la pila nativa del sistema operativo:

```css
font-family: -apple-system, BlinkMacSystemFont, "Segoe UI", Roboto, "Helvetica Neue", Arial, sans-serif;
```

### 5.2. Escala Tipográfica Modular (Ratio 1.250 · *Major Third*)

| Nivel Semántico | Token de Tamaño | Rem / Px | Peso (*Weight*) | Interlineado | Espaciado (*Tracking*) |
| :--- | :--- | :--- | :--- | :--- | :--- |
| **Display** | `--text-display` | `2.441rem` (~39px) | 800 (ExtraBold) | `1.15` | `-0.025em` |
| **Título 1 (H1)** | `--text-h1` | `1.953rem` (~31px) | 700 (Bold) | `1.20` | `-0.020em` |
| **Título 2 (H2)** | `--text-h2` | `1.563rem` (~25px) | 700 (Bold) | `1.25` | `-0.015em` |
| **Título 3 (H3)** | `--text-h3` | `1.250rem` (~20px) | 600 (SemiBold) | `1.35` | `-0.010em` |
| **Párrafo / Base**| `--text-base` | `1.000rem` (16px) | 400 (Regular) | `1.50` | `0` |
| **Secundario** | `--text-sm` | `0.875rem` (14px) | 500 (Medium) | `1.45` | `+0.010em` |
| **Etiquetas / Censo**| `--text-xs`| `0.750rem` (12px) | 700 (Bold) | `1.40` | `+0.040em` |

---

## 6. Sistema Espacial, Grillas y Elevación

### 6.1. Escala de Espaciado Base 4 / 8 px
Todos los márgenes, rellenos (*padding*) y huecos de grilla (*gap*) derivan de una escala matemática fija:

| Token | Dimensión | Uso Habitual |
| :--- | :--- | :--- |
| `--space-1` | `4px` | Separación mínima entre icono y texto, micro-ajustes |
| `--space-2` | `8px` | Espaciado interno en insignias, campos de entrada compactos |
| `--space-3` | `12px` | Relleno lateral de botones, separación vertical de párrafos |
| `--space-4` | `16px` | Padding interno de tarjetas, separación estándar de bloques |
| `--space-6` | `24px` | Separación entre módulos funcionales, relleno en diálogos |
| `--space-8` | `32px` | Margen de cabeceras, respiro de secciones de página |
| `--space-12`| `48px` | Separadores de bloques temáticos principales |

### 6.2. Radios de Curvatura (*Border Radius*)
* Superficies modulares (tarjetas, paneles): `10px` (`--radius-card`).
* Controles interactivos (botones, selectores, inputs): `8px` (`--radius-control`).
* Píldoras e insignias de estado: `9999px` (`--radius-pill`).

### 6.3. Sombras de Elevación
* **Nivel 1 (Reposo en tarjeta):** `0 1px 3px rgba(0, 0, 0, 0.05), 0 1px 2px rgba(0, 0, 0, 0.03)`
* **Nivel 2 (Interacción / Hover):** `0 4px 6px -1px rgba(0, 0, 0, 0.08), 0 2px 4px -1px rgba(0, 0, 0, 0.04)`
* **Nivel 3 (Modales y paneles flotantes):** `0 10px 15px -3px rgba(0, 0, 0, 0.10), 0 4px 6px -2px rgba(0, 0, 0, 0.05)`

---

## 7. Catálogo de Componentes de Interfaz (UI Kit)

### 7.1. Botones y Elementos Táctiles Móviles
* **Dimensión mínima obligatoria:** Área táctil no inferior a $44 \times 44\text{ px}$ (Criterio de conformidad WCAG 2.5.8 *Target Size*).
* **Indicador de foco por teclado:** Anillo continuo de $3\text{ px}$ con contraste suficiente (`outline: 3px solid var(--color-border-focus); outline-offset: 2px`).

| Variante | Función Semántica | Estilo y Contraste |
| :--- | :--- | :--- |
| **Primario** | Acción unívoca determinante de la pantalla (*«Firmar autorización»*, *«Confirmar pedido»*) | Fondo `--color-brand-primary`, texto `--color-brand-on-primary`. |
| **Secundario** | Acciones de soporte o consulta (*«Descargar borrador»*, *«Ver histórico»*) | Superficie blanca, borde `1px solid --color-border-default`, texto `--color-text-primary`. |
| **Destructivo** | Acciones irreversibles o de cancelación (*«Anular inscripción»*, *«Borrar borrador»*) | Fondo `#DC2626`, texto `#FFFFFF`. Requiere diálogo modal de confirmación. |
| **Subsanación** | Corrección de expedientes incompletos o rechazados | Borde continuo de `2px solid #D97706`, texto `#B45309`, fondo de advertencia suave. |

### 7.2. Insignias Semánticas de Estado (*Status Badges*)
Para garantizar la comprensión por personas con discromatopsia o daltonismo, **nunca se emplea el color en solitario**:

```
[✓ Confirmado]    -> Fondo #DCFCE7, Borde #86EFAC, Texto #166534
[⏳ En revisión]   -> Fondo #FEF3C7, Borde #FCD34D, Texto #92400E
[⚠️ Subsanar]     -> Fondo #FEE2E2, Borde #FCA5A5, Texto #991B1B
[ℹ️ Informativo]  -> Fondo #E0F2FE, Borde #7DD3FC, Texto #075985
[🔒 Cerrado]      -> Fondo #F1F5F9, Borde #CBD5E1, Texto #475569
```

### 7.3. Patrón de Tarjeta Familiar de Educando
En la zona privada familiar (*Mi Espacio*), cada participante (hijo o tutelado) dispone de una tarjeta estructurada e independiente:
1. **Encabezado modular:** Nombre legal del educando + Insignia de sección pedagógica activa.
2. **Cuerpo de trámites:** Listado segmentado por actividad en curso (campamento, salida de fin de semana), con fecha de cierre perentoria y estado individual de cada requisito:
   * Autorización legal (Firma v1, v2 o pendiente).
   * Justificante de abono bancario (Adjunto vs Validado contablemente).
   * Ficha médica anual (Vigente vs Requiere renovación).
3. **Pie de tarjeta:** Acceso directo a fichas médicas históricas y consulta de autorizaciones previas.

### 7.4. Campos de Formulario y Retroalimentación
* Las etiquetas (`<label>`) son visibles y persistentes; nunca se delega en el `placeholder` la función de etiquetado (evita pérdida de contexto al escribir).
* En caso de error, el campo se marca con `aria-invalid="true"` y se vincula mediante `aria-describedby` al identificador del mensaje de error textual.

---

## 8. Tono de Voz y Redacción (UX Writing)

El lenguaje de la plataforma es una extensión del ideario scout: **claro, riguroso, respetuoso y constructivo**.

### 8.1. Vocabulario Normalizado
Queda expresamente fijado el uso de la terminología del [Glosario Oficial](../glosario.md):

| Término Preceptivo | Término Desaconsejado o Prohibido | Motivo de la norma |
| :--- | :--- | :--- |
| **Scouter** | Monitor, monitora, profesor, vigilante | Dignidad asociativa del voluntariado scout educador |
| **Kraal** | Cuadro de profesores, equipo directivo, monitores | Término histórico y estatutario del equipo de scouters del grupo |
| **Educando / Participante** | Niño, alumno, cliente, menor | Enfoque pedagógico activo donde el educando es protagonista |
| **Ronda Solar** | Año escolar, curso académico, temporada | Ciclo vital anual scout (octubre a septiembre) |
| **Rama / Sección** | Categoría, clase, curso, división | Unidad pedagógica por franjas de edad reconocidas |
| **Justificante adjunto** | Pago completado, ingreso verificado | Un archivo bancario no equivale a dinero conciliado en tesorería |

### 8.2. Redacción de Errores y Alertas
Los mensajes de error deben explicar con exactitud **qué ha ocurrido**, **cuál es la causa** y **qué acción específica debe adoptar el usuario**:
* *Incorrecto:* «Error 409: Conflicto en base de datos al persistir autorización.»
* *Correcto:* «No se ha podido enviar la autorización porque Lucas ya cuenta con una solicitud registrada para este campamento. Puedes revisar o corregir la solicitud existente en tu panel de trámites.»

---

## 9. Gobernanza y Mantenimiento del Sistema

1. **Custodia y responsabilidad:** La conservación de este manual corresponde a la Vocalía de Comunicación y Tecnologías de SCLM, en coordinación con la persona responsable de arquitectura de software.
2. **Versionado:** Las modificaciones al sistema cromático o al esquema de tokens requerirán una revisión de versión menor (`1.x.0`) y la ejecución de la batería de pruebas de contraste automatizadas.
3. **Auditorías de conformidad:** Antes del cierre de cada ciclo de entrega (*milestone*), se ejecutará el guion de pruebas de accesibilidad y contrastes sobre pantallas móviles y de escritorio.

---
*Aprobado por el Equipo Técnico y de Comunicación de Scouts de Castilla-La Mancha (SCLM).*
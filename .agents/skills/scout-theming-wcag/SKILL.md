---
name: scout-theming-wcag
description: >-
  Use this skill to configure, validate, or generate Scout Group branding, pañoleta
  identity, CSS design tokens, SVG vector neckerchiefs, and WCAG 2.2 AA contrast compliance.
---

# Scout Theming & WCAG 2.2 AA Accessibility

Esta skill proporciona las fórmulas matemáticas, los criterios de diseño y las reglas de validación para el motor de identidad visual y tematización scout de **WebScout SCLM**.

## Identidad Visual Scout: La Pañoleta

La pañoleta scout es el elemento más sagrado y distintivo de un grupo scout. Cada grupo federado en SCLM posee una configuración cromática oficial:
1. **Fondo de Pañoleta (`BaseColorHex`):** Color dominante del paño triangular.
2. **Ribete Exterior (`BorderColorHex`):** Cinta perimetral que bordea los dos catetos o los tres lados.
3. **Cintas Interiores (`CentralStripeColorHex`, `SecondaryBorderColorHex`):** Franjas longitudinales o ribetes secundarios (opcionales según estatutos del grupo).
4. **Escudo / Heráldica (`HasEmblem`, `ShieldUrl`):** Escudo bordado en el pico trasero de la pañoleta.

### Estructura de Configuración de Tenant (`PanoletaConfig`)
```json
{
  "baseColorHex": "#1b5e20",
  "borderColorHex": "#fbc02d",
  "centralStripeColorHex": "#ffffff",
  "secondaryBorderColorHex": null,
  "hasEmblem": true,
  "shieldUrl": "/tenants/lestonnac/shield.webp"
}
```

## Algoritmo de Validación de Accesibilidad (WCAG 2.2 AA)

El sistema debe garantizar que los textos sobre fondos de color cumplan con el ratio de contraste oficial W3C WCAG 2.2 Nivel AA:
- **Texto Normal (< 18pt o < 14pt negrita):** Ratio mínimo de **4.5:1**.
- **Texto Grande (>= 18pt o >= 14pt negrita) y componentes UI:** Ratio mínimo de **3:1**.

### Fórmulas Matemáticas de Contraste

1. **Luminancia Relativa ($L$):**
   Dado un canal RGB normalizado $sRGB \in [0, 1]$:
   $$C = \begin{cases} \frac{sRGB}{12.92} & \text{si } sRGB \le 0.04045 \\ \left(\frac{sRGB + 0.055}{1.055}\right)^{2.4} & \text{si } sRGB > 0.04045 \end{cases}$$
   
   La luminancia relativa ponderada es:
   $$L = 0.2126 \cdot R + 0.7152 \cdot G + 0.0722 \cdot B$$

2. **Ratio de Contraste ($CR$):**
   Entre el color más claro ($L_1$) y el color más oscuro ($L_2$):
   $$CR = \frac{L_1 + 0.05}{L_2 + 0.05}$$

3. **Cálculo Automático de Texto Superpuesto:**
   Para cualquier color de fondo `$hex`, el backend calcula:
   $$CR_{\text{blanco}} = \text{Contrast}(L_{\text{fondo}}, 1.0)$$
   $$CR_{\text{negro}} = \text{Contrast}(L_{\text{fondo}}, 0.0)$$
   Si $CR_{\text{blanco}} \ge 4.5$, el texto de primer plano será blanco (`#FFFFFF`); en caso contrario, será un gris oscuro accesible (`#18181B`).

## Inyección de Tokens CSS (Seguridad Anti-XSS)

Está **estrictamente prohibido** que los usuarios administradores suban archivos `.css` libres o código JavaScript ejecutable. La personalización se realiza inyectando variables CSS tipadas y sanitizadas en el `<head>` del layout principal:

```html
<style id="scout-tenant-theme">
  :root {
    /* Paleta Principal */
    --scout-primary: #1b5e20;
    --scout-primary-hover: #144618;
    --scout-primary-text: #ffffff;
    
    --scout-secondary: #fbc02d;
    --scout-secondary-text: #1a1a1a;
    
    --scout-accent: #d32f2f;
    --scout-accent-text: #ffffff;
    
    /* Identidad de Pañoleta */
    --panoleta-base: #1b5e20;
    --panoleta-ribete: #fbc02d;
    --panoleta-cinta: #ffffff;
    
    /* Superficies y Cabeceras */
    --header-bg: var(--scout-primary);
    --header-text: var(--scout-primary-text);
    --badge-active-border: var(--scout-secondary);
  }
</style>
```

## Renderizado Vectorial SVG de la Pañoleta

Para avatares, cabeceras de actividades, carnets o emails, se utiliza un componente SVG paramétrico:
- Triángulo isósceles que representa la tela doblada.
- Trazos perimetrales proporcionales con el color de `BorderColorHex`.
- Franjas interiores vectoriales calculadas según `CentralStripeColorHex`.
- Permite renderizado nítido en cualquier resolución sin pixelado y bajo peso en transferencia (< 2 KB).

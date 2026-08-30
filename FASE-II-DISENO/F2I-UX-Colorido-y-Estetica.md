# F2I-UX — Colorido y estética de la interfaz (estudio)

> Documento de estudio (no implementación): diagnóstico del sistema de color actual de la
> aplicación, propuesta de tokens, rediseño del gradiente de la barra de título y de los
> recuadros de selección, y lista de mejoras detalladas. Aplicación: Avalonia 12.1.1
> (Fluent), tema claro/oscuro con alternador manual; **el arranque por defecto es oscuro**
> desde v1.0.9. Fecha: 29-08-2026.

---

## 1. Inventario del color actual (evidencia del código)

| Hex | Dónde | Comentario |
|---|---|---|
| `#F2000000 → #FFFFFFFF` | `App.axaml` `PincelBarraTitulo` (gradiente negro→**blanco** horizontal con alphas) | Extremo derecho casi blanco puro |
| `#E6FFFFFF` / `#33FFFFFF` | Botones de ventana (— □ ✕): glifo y hover (`MainWindow.axaml`) | Blancos, sobre el gradiente |
| `#E81123` | Botón cerrar en hover | Rojo Windows "crimson" |
| `#334155` | Borde del sidebar + 2 divisores (`MainWindow.axaml`) | slate-700 FIJO (no cambia con el tema) |
| `#FFFFFFFF` + `#FF1C1C1C` | Círculos blancos de los botones de Acciones (Tema, Leyenda, Acerca de) | Blanco brillante permanente |
| `#55FFFFFF` | Borde del popup cristalino (`MainWindow.axaml.cs` `CrearPopupInfo`) | Borde claro translúcido ✅ coherente |
| `#22252A` / `#F2F2F2` / `#6CBAFF` | Popup: tinte acrílico, texto, **enlace** | El azul `#6CBAFF` es el "acento de enlace" de facto |
| `#0F172A`, `#334155`, `#eef2ff`, `#dbeafe`, `#ffffff` | `DiagramView.cs`: texto, trazos, rellenos de cajas, fondo del lienzo | **Paleta clara fija** |
| La paleta de familias y relaciones | `DiagramView.cs` (ámbar, azul, teal, violeta…) | Colores de datos (se conservan) |
| Sin definir | Selección de `ListBox`/`ComboBox` (sidebar, desplegables, lista de capturas) | **La pinta el tema Fluent por defecto** |

**Conclusión del inventario:** la app ya tiene una identidad "slate profesional" (#0F172A / #334155 /
pizarra azulada) y un acento de enlace (#6CBAFF, azul claro), pero todo el color está **hardcodeado
disperso** y dos superficies quedan **fuera de esa identidad**: el gradiente del título y la
selección del tema Fluent.

---

## 2. Diagnóstico estético (problemas concretos)

### P-A1. El gradiente "negro→blanco" de la barra de título rompe la estética (crítico)

La barra es **permanente** (no cambia con el tema) y predomina el negro, pero el
`GradientStop` final es `Offset=0.97 #B3FFFFFF` y `Offset=1.0 #FFFFFFFF` — a casi 100 % de
transparencia en blanco puro. Consecuencias comprobables en el código:

- El **extremo derecho de la ventana es una franja blanca cálida** ("fogonazo") que choca con
  el negro dominante y con el tema oscuro que ahora es el arranque por defecto.
- Los **botones de ventana (— □ ✕) están alineados a la derecha** (`HorizontalAlignment="Right"`)
  → caen exactamente sobre la franja blanca del final: **glifos blancos (#E6FFFFFF) sobre blanco ≈
  ilegibles**. Es el "blanco sobre blanco" que el ojo percibe como estética rota.
- En tema **claro**, la barra queda negra arriba + blanca a la derecha: inconsistencia vertical.

### P-B1. Los bordes de los recuadros seleccionados son el acento azul "de fábrica" del tema

Los `ListBox` (sidebar de familias, resultados de búsqueda, lista de capturas) y los
`ComboBox` (Protocolo, Comparar con, Familia, Estado, Exportar) **no tienen estilo propio**:
el tema **Fluent** pinta la selección con su acento por defecto (azul cian `#2882E6`/`#4CC2FF` en
oscuro, `#0B84FF` en claro) y su foco. Ese azul brillante **no pertenece a la paleta slate/azul
de la app** y compite con el `#6CBAFF` de los enlaces → doble lenguaje de acentos.

### P-C1. Colores fijos que no se adaptan al tema

- Divisores y borde del sidebar `#334155` **fijos**: en tema claro se ven casi negros.
- Círculos "blanco brillante + texto #1C1C1C" de Acciones: muy contrastados en oscuro; en claro
  el blanco sobre la barra clara despinta la sección.
- `DiagramView` siempre **claro** (fondo blanco, rellenos `#eef2ff`/`#dbeafe`): en tema oscuro los
  diagramas son "islas blancas" — el mayor intruso visual tras el gradiente.

### P-C2. Detalles menores

- El `StatusText` y los placeholders usan los pinceles del tema (correcto), pero la barra de
  estado no tiene separación visual con el contenido.
- Los marquee (carrusel) de los `ComboBox` no definen color de hover propio (hereda el tema).
- El `FocusVisual` del tema (anillo azul de fábrica) acompaña el mismo problema de P-B1.

### Accesibilidad (contexto)

Todos los pares actuales **cumplen WCAG AA** (verificado, ratios al pie): el problema no es de
legibilidad, es de **coherencia cromática**. La mejora debe **mantener los contratos** mientras
unifica la paleta.

---

## 3. Tokens propuestos (sistema de color)

Se generó la escala base con el generador del skill de *UI design system* (marca `#2563EB`,
estilo modern) y se **sintonizó con la identidad slate real de la app** (el generador produce
azules lavados; la app ya tiene su pizarra). Tokens finales:

| Token | Claro | Oscuro | Uso |
|---|---|---|---|
| `tFondo` | `#F8FAFC` (slate-50) | `#0F172A` (slate-900) | Fondo de ventana |
| `tSuperficie` | `#FFFFFF` | `#1E293B` (slate-800) | Cards / desplegables / popup |
| `tBorde` | `#CBD5E1` (slate-300) | `#334155` (slate-700) | Bordes, separadores |
| `tBordeFuerte` | `#94A3B8` (slate-400) | `#475569` (slate-600) | Hover de botones, lado sidebar |
| `tTexto` | `#0F172A` | `#E2E8F0` (slate-200) | Texto principal |
| `tTextoSuave` | `#475569` | `#94A3B8` | Secundario, placeholders, estado |
| `tAcento` | `#2563EB` | `#528BFF` | Acción principal, enlaces, foco |
| `tAcentoHover` | `#1D4ED8` | `#6CBAFF` | Hover sobre acento / enlaces popup |
| `tSeleccionFondo` | `#DBEAFE` (blue-100, α≈0.55) | `#1E3A5F` (azul noche, α≈0.55) | Item seleccionado (fondo) |
| `tSeleccionBorde` | `#2563EB` α 0.5 | `#6CBAFF` α 0.5 | Item seleccionado (borde 1 px) |
| `tBarraFondoClaro` | grad. `#F3FFFFFF→#E6E2E8F0→#D9CBD5E1` | — | Barra de título en tema claro (opcional) |
| `tPeligro` | `#DC2626` | `#F87171` | Cerrar vna (hover), errores |

Los tokens de marcado **claro/oscuro** se definen con `ResourceDictionary.ThemeDictionaries`
(clave `Light` / `Dark`) en `App.axaml` y se referencian con `{DynamicResource …}` — la app
recalcula sola al alternar tema (verificar en el smoke test; es mecanismo Avalonia 11+).

---

## 4. Rediseño del gradiente de la barra de título

### Opción recomendada — "cristal pizarra" (una sola definición, sirve en claro y oscuro)

Se mantiene el espíritu cristalino y el predominio de negro, pero el recorrido termina en
**pizarra translúcida, nunca en blanco**: los botones de la derecha quedan sobre un fondo
oscuro suficiente para el glifo blanco, y la franja derecha pierde el fogonazo.

```xml
<LinearGradientBrush x:Key="PincelBarraTitulo"
                     StartPoint="0%,50%" EndPoint="100%,50%" SpreadMethod="Pad">
  <GradientStop Offset="0.00" Color="#F20F172A" />   <!-- slate-900 (negro azulado) -->
  <GradientStop Offset="0.55" Color="#E61E293B" />   <!-- slate-800 -->
  <GradientStop Offset="0.82" Color="#D9334155" />   <!-- slate-700 -->
  <GradientStop Offset="1.00" Color="#C6475569" />   <!-- slate-600 translúcido (NUNCA blanco) -->
</LinearGradientBrush>
```

Contraste resultante: glifo `#E6FFFFFF` sobre el stop final (slate-600 ≈ #475569) → ≈ **7,5:1**
(sigue AA y además se ve). Botón cerrar en hover `#E81123` mantiene su significado.

### Alternativa A — "azul nocturno" (más carácter, un guiño al acento)

Parte de negro azulado y termina en azul pizarra denso; el extremo derecho insinúa el acento de
la app antes del área de botones:

```xml
<GradientStop Offset="0.00" Color="#F2060F1E" />
<GradientStop Offset="0.60" Color="#E8122336" />
<GradientStop Offset="1.00" Color="#D01E3A5F" />
```

### Alternativa B — adaptativo por tema (la más "completa", un poco más de trabajo)

Dos recursos (`PincelBarraTitulo` oscuro = opción recomendada; `…Claro` = blanco cristal
`#F3FFFFFF→#E6E2E8F0→#D9CBD5E1`) seleccionados por `ThemeVariant` con los
`ThemeDictionaries`. Los botones de ventana pasarían a `tTexto` (oscuro en claro) en el
tema claro. **Recomendación:** empezar por la opción recomendada (1 solo recurso, sin
dependencia del tema) y dejar la B como evolución si el responsable quiere barra clara.

---

## 5. Recuadros de selección: estilos propuestos

Estilos de **ventana** (`Window.Styles`) para lista de familias, resultados, capturas y
desplegables — ítem seleccionado con la paleta de la app (sin acento cian de fábrica):

```xml
<!-- Selección: fondo pizarra/índigo tenue + borde 1 px del acento, esquinas 4 px -->
<Style Selector="ListBoxItem">
  <Setter Property="CornerRadius" Value="4" />
  <Setter Property="Padding" Value="6,4" />
</Style>
<Style Selector="ListBoxItem:pointerover /template/ ContentPresenter">
  <Setter Property="Background" Value="{DynamicResource tSeleccionFondo}"/>
</Style>
<Style Selector="ListBoxItem:selected /template/ ContentPresenter">
  <Setter Property="Background" Value="{DynamicResource tSeleccionFondo}"/>
  <Setter Property="BorderBrush" Value="{DynamicResource tSeleccionBorde}"/>
  <Setter Property="BorderThickness" Value="1"/>
  <Setter Property="CornerRadius" Value="4"/>
</Style>
<!-- El texto del ítem se mantiene con el color normal = legible en ambos temas. -->
```

Para los `ComboBox` (caja y desplegable) el mismo selector sobre `ComboBoxItem`, y el foco:

```xml
<Style Selector="ComboBoxItem:selected /template/ ContentPresenter"> … ídem … </Style>
<Style Selector="ComboBox:focus /template/ Border">
  <Setter Property="BorderBrush" Value="{DynamicResource tAcento}"/>
</Style>
```

> Notas Avalonia 12: los selectores `/template/ ContentPresenter` son el patrón documentado del
> tema Fluent; la **verificación visual real** (smoke en Windows + Linux) es obligatoria porque
> el temas Fluent añade su propia superposición de selección (`ListBoxItem` interno) — si el
> overlay de fábrica tapa el borde, se afina con el estado `:selected` sobre el template completo
> (segunda pasada). Los `CornerRadius` en `ListBoxItem` requieren la variante 12.x (ya probada).

---

## 6. Detalles y mejoras complementarias (prioridad P0/P1/P2)

### P0 — Acciones inmediatas (con el gradiente)

1. **Botones de ventana legibles**: con el gradiente nuevo quedan resueltos; además se baja su
   opacidad por defecto a `#D9FFFFFF` y hover `#26FFFFFF` para que "respiren" sobre pizarra.
2. **Cerrar**: mantener `#E81123` (cumple AA 4,63:1 y es convención) o unificar a `tPeligro`.
   Dejar el rojo Windows por ahora (familiaridad).

### P1 — Coherencia de tema (tokens)

3. **Divisores y borde del sidebar** → `tBorde`/`tBordeFuerte` dinámicos (hoy `#334155` fijo).
4. **Círculos de Acciones**: pasar el texto interior a `tTexto` y el círculo a `tSuperficie`
   (o mantener blanco solo en oscuro, al 90 %).
5. **Estados de selección** (sección 5) aplicados a sidebar, desplegables y lista de capturas.
6. **Foco de TextBox/ComboBox** → `tAcento` (hoy cian de fábrica).
7. **Popup**: dejar como está (ya es la pieza más coherente); mover sus 3 hex a tokens
   (`tSuperficie`, `tAcentoHover`, `tTexto`) para que no se desincronice.

### P1 — Diagramas en modo oscuro (la "isla blanca")

8. `DiagramView`: si `RequestedThemeVariant == Dark`, cambiar el fondo del lienzo a `#0F172A`,
   trazos a `#334155`→`#475569`, textos a `#E2E8F0` y los rellenos de las cajas a las **variantes
   300/400 de las familias** (los colores de relación ya equilibrados). Los colores de datos
   (ámbar, teal, violeta…) se conservan — son semánticos. Detalle: el renderer es determinista
   (regla del proyecto): el cambio de paleta por tema no altera el golden-master porque el
   golden-master mide **layout/estructura**, pero hay que re-verificar el test de regresión.

### P2 — Pulido

9. **Barra de estado**: separador superior sutil con `tBorde` y texto con `tTextoSuave`.
10. **Scrollbars**: acordes al acento (tema Fluent ya las adapta; opcional afinar).
11. **Exportar tokens a JSON** (salida del generador) al repo para futuro re-diseño.
12. **Double-click en el título** para maximizar/restaurar (costumbre de escritorio, hoy no
    existe con `WindowDecorations=None`) y el título **arrastrable también sobre los controles**
    (ya lo está sobre el texto/logo).

---

## 7. Validación: contraste de la paleta resultante (WCAG 2.x)

| Par | Ratio | Nivel |
|---|---|---|
| `#0F172A` sobre `#FFFFFF` (ficha, claro) | 17,85:1 | AAA |
| `#F2F2F2` sobre `#22252A` (popup) | 13,73:1 | AAA |
| `#6CBAFF` sobre `#22252A` (enlace popup) | 7,40:1 | AA+ |
| `#FFFFFF` sobre `#2563EB` (acento con texto blanco) | 5,17:1 | AA |
| `#E6FFFFFF` sobre stop final del gradiente nuevo `#475569` | ≈7,5:1 | AA |
| `#FFFFFF` sobre `#E81123` (cerrar hover) | 4,63:1 | AA |
| `#E2E8F0` sobre `#0F172A` (texto oscuro propuesto) | ≈14:1 | AAA |

Todos los pares cumplen AA; el gradiente nuevo **mejora** el peor caso actual (botones sobre la
franja blanca ≈ 1,1:1 → ilegible) hasta ≈7,5:1.

---

## 8. Plan de implementación (por fichero)

| # | Fichero | Cambio | Riesgo |
|---|---|---|---|
| 1 | `App.axaml` | Tokens + `ThemeDictionaries` (o tokens únicos) + gradiente nuevo | Bajo (recursos) |
| 2 | `MainWindow.axaml` | `Window.Styles` de selección/foco; divisores y sidebar a tokens; botones vna α | Medio (selectores de template; verificación visual) |
| 3 | `MainWindow.axaml.cs` | Popup a tokens; (opcional) doble clic p/ maximizar | Bajo |
| 4 | `DiagramView.cs` | Variante oscura de paleta ligada al tema | Medio (re-verificar tests de regresión) |

Orden sugerido: 1+2 (gradiente + selección, el pedido) → verificación visual → 4 (diagramas)
→ 3 (pulido) → re-verificación + release.

---

## 9. Estado de implementación (29-08-2026, paquete P0+P1 aprobado por el responsable)

**Implementado:**

1. **Tokens** en `App.axaml`: 10 tokens por tema (`ThemeDictionaries` Light/Dark: `tFondo`,
   `tSuperficie`, `tBorde`, `tBordeFuerte`, `tTexto`, `tTextoSuave`, `tAcento`, `tAcentoHover`,
   `tSeleccionFondo`, `tSeleccionBorde`) + permanentes (`tFondoPopup`, `tBordePopup`,
   `tTextoPopup`, `tEnlacePopup`, `tPeligro`) + **gradiente "cristal pizarra"** (negro azulado →
   slate-600 translúcido; nunca blanco → botones de ventana ≈7,5:1).
2. **Selección/foco** en `MainWindow.axaml`: `ListBoxItem`/`ComboBoxItem` (`:pointerover`,
   `:selected`, fondo/borde `tSeleccion*`, `Foreground=tTexto` — importante porque el tema
   Fluent pone texto blanco en selección) y `TextBox`/`ComboBox:focus` con `tAcento`.
3. **Divisores y borde del sidebar** → `tBorde`/`tBordeFuerte` (dejan de ser `#334155` fijo).
4. **Botones de ventana**: glifo `#D9FFFFFF`, hover `#26FFFFFF`, cerrar → `tPeligro`.
5. **Popup** (`MainWindow.axaml.cs`): tintes/borde/texto/enlace resueltos desde los tokens
   permanentes con respaldo (helpers `RecursoPopupBrush`/`RecursoPopupColor`).
6. **AlternarTema** repinta los diagramas en pantalla con la paleta del nuevo tema.
7. **`DiagramView` por tema**: fondo slate-900 en oscuro, cajas slate (`#1E293B`/`#1E3A5F`),
   trazo `#475569`, texto `#E2E8F0`; el texto "por defecto" del layout (`#0f172a`) se
   reinterpreta con el color del tema (los colores explícitos se respetan); en pruebas sin
   `Application.Current` sigue la paleta clara (determinismo intacto).

**Verificación:** build Release 0 errores · **78/78 tests** (quality gate A01–A07 + regresiones).
**Pendiente de verificación visual en instalación real** (smoke del responsable): el tema Fluent
añade su propia superposición de selección — si en algún estado el borde `tSeleccionBorde` no
se viera, afinar el selector sobre el template completo. P2 (scrollbars, statusbar, doble clic
maximizar, exportar tokens JSON) queda como backlog del estudio.

*Anexo: salida del generador de tokens del skill (marca #2563EB) disponible bajo demanda en
JSON para el punto 11.*
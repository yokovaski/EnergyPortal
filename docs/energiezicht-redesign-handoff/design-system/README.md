# EnergieZicht design system

EnergieZicht is a personal home-energy dashboard. A Raspberry Pi in the meter cupboard reads the smart meter (P1/DSMR) and the solar inverter every 10 seconds. The app shows what the house is doing now and what it did over time. One household, one meter, one person checking it. The design is calm, factual and readable at a glance, like a well-made meter rather than a trading terminal.

Files in this folder:

- `tokens.json`: the source of truth (colours for both themes, type, spacing, radii, shadow, sizes).
- `tokens.css`: CSS custom properties (`--ez-*`) generated from `tokens.json`, plus base styles and `.ez-<type-style>` helpers.
- `vuetify-theme.js`: the same colours as a Vuetify 3 theme (`light` and `dark`) with suggested component defaults.

## Principles

1. **Now first.** Actueel opens with one answer: how much power the house uses this second, and where it comes from (grid, sun) or goes (back to the grid). Everything else is secondary.
2. **One chart, one control.** Show series together in one combined chart with one range control. Never repeat near-identical charts or controls.
3. **Only what applies.** A household without solar panels never sees Opwekking or Teruglevering: no zero cards, no flat lines. The "Systeem met zonnepanelen" setting drives this everywhere.
4. **Numbers in Dutch.** Use `0,264 kW`, `4.277,964 kWh`, `€ 3,40`, `do 19 nov`, `21:17`. Never `0.264`, `Thursday` or `21:0`.
5. **Honest state.** Show when data last arrived, show skeletons while loading, and say plainly when the Pi is offline. Never show "N/A" or a fake 0.

## Content

- **Language:** Dutch everywhere, including login and account pages. Address the user as *je*, informal but not chatty.
- **Domain terms are fixed.** Use them exactly, capitalised as nouns in labels:
  - **Verbruik**: what the house consumes (opname + opwekking − teruglevering)
  - **Opname**: drawn from the grid
  - **Teruglevering**: fed back into the grid
  - **Opwekking**: solar generation
  - **Normaaltarief / Daltarief**: peak / off-peak (`mode` 2 / 1)
  - **Meterstanden**: meter readings
  - **Vaste kosten**: fixed monthly fees
- **Casing:** sentence case for titles, buttons and labels ("Opname vandaag", "Naar Instellingen"). No ALL CAPS, not even on toggles.
- **Buttons are verbs:** "Opslaan", "Herstellen", "Inloggen", "Exporteren".
- **Messages state the fact, then the fix:** "Niet alle prijzen zijn bekend. Stel de gasprijs in onder Instellingen." / "Geen nieuwe metingen sinds 18:02. Controleer de Raspberry Pi in de meterkast."
- **Units always,** separated by a space: `kW` for power (now), `kWh` for energy (totals), `m³` for gas, `€` before the amount. Tariff units: `€/kWh`, `€/m³`, `€/maand`.
- **Decimals:** live power 3 (`0,264 kW`); KPIs 1 (`12,0 kWh`); gas 2 (`1,84 m³`); money 2; meter readings 3.
- **No emoji, no exclamation marks.**

## Colour

Colours are semantic tokens with a light theme (`light`, "Licht", the default) and a dark theme (`dark`, "Donker").

- **Surfaces:** page `surface`, cards `surface-raised`, tracks and skeletons `surface-sunken`. Separate cards with a `line` hairline, not shadows.
- **Text:** `ink` for everything that matters, `ink-muted` for units, captions and axis labels. Both pass 4.5:1 on all three surfaces in both themes.
- **Brand:** `brand` (petrol) is for interaction only: primary button, selected segment, switch on, active tab, links, focus. Put `on-brand` text on a `brand` fill, never literal white. `brand-soft` is the hover and active tint.
- **Series:** each energy flow has one colour, used for every swatch, line, area, bar and ring of that flow:

| Token | Flow | Drawn as |
|---|---|---|
| `series-verbruik` | Verbruik | a 2.5px **line**, never filled: the top edge of the stack |
| `series-opname` | Opname | area/bar stacked **on top of** direct solar |
| `series-opwekking` | Opwekking | area/bar at the bottom of the stack: the solar used directly (min(opwekking, verbruik)) |
| `series-teruglevering` | Teruglevering | area/bar **below** zero: the solar the house didn't use |
| `series-gas` | Gas | its own chart; violet, never the Opname red |

  Above zero, direct solar + Opname add up exactly to Verbruik; Opwekking = direct solar + Teruglevering. Fills are flat tints that never overlap.
  The palette follows Okabe & Ito. Opname (dark vermillion) and Teruglevering (bluish green) also differ in lightness and in direction (above or below the axis), so they can be told apart with red–green colour blindness. Each series colour holds ≥3:1 against `surface-raised` as a graphic mark. They are not text colours: put labels in `ink` beside a swatch.
- **Tariff:** `tariff-dal-*` (indigo, moon) and `tariff-normaal-*` (sand, sun), only in `TariffBadge`.
- **Comparisons:** today vs. yesterday is a bar with an `ink` tick, plus neutral words ("1,6 kWh meer dan gisteren"). Never green or red.
- **Status:** `status-live` (with the word "Live"), `status-warning` on `status-warning-bg`, `status-danger` on `status-danger-bg`. Status is always icon + words, never colour alone.

## Type

- **Hanken Grotesk** (Google Fonts, 400/500/600/700) for everything; it has true tabular figures, which `body` turns on globally so numbers don't jitter as they update every 10 s. **JetBrains Mono** 500 only for `meter` (Meterstanden).
- Figures: `num-hero` (44/48, the one Verbruik value in the Nu-panel, one per screen), `num-lg` (28/32, KPIs, totals), `num-md` (18/24, costs, secondary).
- Text: `title-lg` (24/30 page titles, desktop), `title` (17/24 card titles), `body` (15/22), `body-strong`, `label` (13/18 field and KPI labels), `caption` (12/16 axis ticks, timestamps, deltas).
- Units inside a figure are 55% size in `ink-muted`, after a space.

## Space, shape, elevation

- 4px base: `space-1` 4 · `space-2` 8 · `space-3` 12 · `space-4` 16 · `space-5` 24 · `space-6` 32 · `space-7` 48.
- Desktop: content max `content-max` (1200px), gutters `space-6`, card padding and card gaps `space-5`. Mobile (≤ 560px): gutters, card padding and gaps `space-4`; KPI grid gaps `space-3`.
- Radii: `radius-sm` badges and segments, `radius-md` buttons and inputs, `radius-lg` cards, banners and menus, `radius-pill` switches, dots and the tab pill.
- Elevation: flat cards with a `line` border. `shadow-float` only for things that float (menus, popovers, tooltip, the dirty `SaveBar`, the login card over its photo).
- Controls are `control-h` (40px) tall on desktop and `control-h-touch` (44px) on touch.
- **Focus:** a 2px solid `focus-ring` outline, offset 2px, on every interactive element. It holds ≥8:1 on every surface in both themes.

## Charts

- ApexCharts. There is no toolbar, no hamburger menu and no wheel zoom. If zoom is ever needed, make it an explicit opt-in button.
- Actueel uses **one** stacked power chart (direct solar + Opname up to the Verbruik line, Teruglevering below zero) under a single Nu · Uur · Dag · Week control, with the period totals as key figures above it and a crosshair tooltip. Historie uses bars, with Elektriciteit and Gas side by side on desktop and tabbed on mobile, under a kWh · € control.
- Axis ticks: `HH:mm` within a day, `ma 17 nov` for days, `nov` for months, `2026` for years. Never rotate labels; thin them instead (4–5 on mobile). In cost mode both the axis and the tooltip show `€`.
- Gridlines in `line`, the zero line in `line-strong`, tick labels in `ink-muted` `caption`.
- Every chart has three states: skeleton, data, and empty ("Nog geen metingen in deze periode").

## Layout per page

- **Actueel:** `PowerFlow` (full width) → `KpiGroup` "Vandaag" (one card, `KpiTile`s with a today-vs-yesterday bar) → `ChartCard` "Vermogen" (8/12) beside `MeterReadings` (4/12). On mobile everything stacks, and the PowerFlow plus a 2×2 KPI grid fit the first screen.
- **Historie:** page title + `PeriodPicker` + kWh/€ `SegmentedControl` in one toolbar → `Banner` if tariffs are missing → Elektriciteit and Gas `ChartCard`s with period totals in their legends.
- **Instellingen:** grouped sections (Algemeen, Tarieven, Account) on one page, `Switch` rows, tariffs as `TextField`s in a two-column grid with units, one `SaveBar`.
- **Login & Account:** same tokens, components and Dutch copy as the app. A centred `surface-raised` card, max 400px wide, with the wordmark above it.
- **Navigation:** `AppBar` at ≥ 768px, `BottomTabBar` below 768px (Actueel · Historie · Instellingen). The account shows initials, never the full email address.

## Iconography

- **Material Design Icons** (`@mdi/js`, the set the app already ships), as inline SVG.
- Domain mapping: `mdiTransmissionTower` = Net/Opname/Teruglevering, `mdiHome` = Verbruik, `mdiSolarPanel` = Opwekking, `mdiFire` = Gas, `mdiWeatherNight` (dal) / `mdiWeatherSunny` (normaal) = tariff, `mdiAlertCircleOutline` = offline/warning.
- Navigation: `mdiViewDashboard` Actueel, `mdiChartBar` Historie, `mdiCog` Instellingen.
- Icons always sit beside a word, except the avatar. Use 24px in flow nodes and tabs, 18–20px inline, 14px in badges.

## Logo

There is no logo. The wordmark is set in type: **Energie** in `ink` + **Zicht** in `brand`, Hanken Grotesk 700, letter-spacing −0.02em. Don't draw a mark until one is designed. On the login photo the wordmark is white + `#a8ece3` with a soft text shadow.

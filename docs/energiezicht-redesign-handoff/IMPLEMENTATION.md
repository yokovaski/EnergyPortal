# EnergieZicht redesign: implementation spec

This is the build spec for the redesign. Read `README.md` first for the package layout, then `design-system/README.md` for the rules every screen follows. Screenshots in `screenshots/` and the clickable prototypes in `prototype/` show the target. When this document and a mockup disagree, this document wins.

The original problem analysis (current site, API contract, 24 numbered issues) is in `reference/original-handoff.md`. Issue numbers below (`#1` … `#24`) refer to that file.

---

## 1. Scope

| Screen | Route | Mockups |
|---|---|---|
| Actueel (live dashboard) | `/` | `01-actueel*.png`, `02-actueel-mobiel.png` |
| Historie | `/history` | `03-historie*.png`, `04-historie-mobiel*.png` |
| Instellingen | `/home/settings` | `05-instellingen*.png`, `06-instellingen-mobiel.png` |
| Inloggen (+ 2FA code step) | `/Identity/Account/Login`, `/Identity/Account/LoginWith2fa` | `07-inloggen*.png`, `08-inloggen-mobiel.png` |
| Account (was "Beheer") | `/Identity/Account/Manage` | `09-account.png` |

Not designed yet, build in the same style: wachtwoord vergeten / reset, e-mail bevestigen, herstelcode-login, uitgelogd, 404/500, Account on mobile (stack the sections; the side menu becomes a horizontal scrollable chip row or is dropped).

## 2. Technical direction

- **One UI stack.** Drop Bootstrap and the `bg-dark` navbar. The Vue 3 app renders the shell (AppBar / BottomTabBar) itself. Keep Vuetify 3 if you want, themed from `design-system/vuetify-theme.js`, or use plain components on `design-system/tokens.css`. Either way **every colour, size and radius comes from the tokens**; no hex values in components.
- **Client routing.** Add `vue-router` (`/`, `/history`, `/settings`, `/account`). The Razor views can all serve the same SPA entry. Keep server routes working (`/home/settings` → redirect or alias to `/settings`).
- **Identity pages.** Restyle the Razor Identity pages (Login, LoginWith2fa, LoginWithRecoveryCode, ForgotPassword, ResetPassword, Manage/*) with `tokens.css` and Dutch copy, or replace them with Vue views that call the Identity endpoints. Either is fine; they must look like the mockups. Account management is **one page** with sections (see §7.5) instead of the left-nav sub-pages.
- **Fonts.** Hanken Grotesk 400/500/600/700, JetBrains Mono 500. Self-host (`@fontsource/hanken-grotesk`, `@fontsource/jetbrains-mono`) rather than Google Fonts, so the dashboard has no third-party requests.
- **Icons.** `@mdi/js` paths (already in use). Mapping in `design-system/README.md`.
- **Charts.** ApexCharts (`vue3-apexcharts`), configured per §6. No toolbar, no zoom, no selection.
- **Theme.** `light` default, `dark` optional, plus "Systeem" (`prefers-color-scheme`). Apply as `data-theme` on `<html>` (CSS) and Vuetify `theme.global.name`.
- **Locale.** All formatting through one `format.ts` module (§4). Never call `toFixed` in templates.

## 3. Shared components

Build these first; every page is composed from them. Names match the design system.

| Component | Notes |
|---|---|
| `AppBar` | ≥768px. 56px, `surface-raised`, bottom `line`. Wordmark (link to `/`), nav pills Actueel · Historie · Instellingen (icon + label, 40px high, active = `brand-soft` bg + `brand` text + `aria-current="page"`), avatar button with initials (40px circle, `brand-soft`) opening a menu: Account, Instellingen, Uitloggen. Content width 1200 + 32px gutters. |
| `BottomTabBar` | <768px. 64px + safe-area inset, fixed bottom, `surface-raised`, top `line`. 3 tabs, icon 22px in a 56×28 pill (`brand-soft` when active) + 12px label. Top bar on mobile: wordmark + 44px avatar. |
| `Card` | `surface-raised`, 1px `line`, `radius-lg`, padding 24 (16 on mobile). No shadow. |
| `Wordmark` | "Energie" `ink` + "Zicht" `brand`, 700, −0.02em. |
| `LiveIndicator` | 8px `status-live` dot + "Live" + caption "bijgewerkt HH:mm:ss". Stale/offline variants in §5. |
| `TariffBadge` | `label` text, 14px icon, `radius-sm`, padding 2×8. Normaal = sun + `tariff-normaal-*`; Dal = moon + `tariff-dal-*`. Driven by `mode` (2 = normaal, 1 = dal). |
| `Figure` | number + unit span (`.ez-unit`: 55%, `ink-muted`). Optional `€` prefix in the same style. Variants hero / lg / md. |
| `KpiTile` | swatch + label, `Figure lg`, comparison bar (6px `surface-sunken` track, series-colour fill = today, 2×14px `ink` tick = yesterday at the same time; scale = max(today, yesterday) × 1.12), caption "x meer/minder dan gisteren" or "Gelijk aan gisteren". Never red/green. |
| `SegmentedControl` | track `surface-sunken`, `radius-md`, 3px padding; segments 34px (38px touch), selected = `brand` fill + `on-brand` text 600, `aria-pressed`. |
| `Switch` | `role="switch"`, 48×28 (52×32 touch). On: `brand` track, `on-brand` knob. Off: `surface-sunken` track with 1px `line-strong` border, `ink-muted` knob. |
| `TextField` / `MoneyField` | 40px (44 touch), 1px `line-strong`, `radius-md`. MoneyField: `€` prefix, right-aligned value, unit suffix (`/kWh`, `/m³`, `/maand`) in caption. Focus ring on the wrapper (`:focus-within`). Error: 2px `status-danger` border + message with icon. |
| `Banner` | `radius-lg`, icon + bold fact + fix (+ link). Variants warning / danger. |
| `SaveBar` | Floating, `shadow-float`, sticky bottom 16px. "Je hebt n wijziging(en) die nog niet is/zijn opgeslagen." + Herstellen (secondary) + Opslaan (primary). Only visible when the form is dirty. After save: small status "Instellingen opgeslagen." with check icon. |
| `ChartCard` | Card with title, caption (period/resolution), legend-with-totals row, chart, footnote caption. Owns skeleton / empty states. |
| `LegendTotal` | swatch (line 14×3 for Verbruik/Totaal, square 10×10 for areas/bars) + label, `Figure md` total underneath. |
| `PeriodPicker` | Button (calendar icon, preset name bold, range + grouping muted, chevron) opening a popover (desktop) / bottom sheet (mobile). See §7.2. |
| `Skeleton` | `surface-sunken` blocks with the shape of the content; no shimmer needed. |

Buttons: primary = `brand` fill / `on-brand` text / 600; secondary = `surface-raised` + 1px `line-strong` + `ink`; danger = 1px `status-danger` border + `status-danger` text. **Disabled buttons are not used for "nothing changed"** (#8, #24): the SaveBar simply isn't shown.

## 4. Formatting (`format.ts`)

All via `Intl` with locale `nl-NL` and the user's time zone setting (default `Europe/Amsterdam`).

| What | Function | Example |
|---|---|---|
| Live power (kW) | 3 decimals | `0,640 kW` |
| Energy KPI / totals (kWh) | 1 decimal | `6,8 kWh`, `68,3 kWh` |
| Gas (m³) | 2 decimals | `3,42 m³` |
| Money | 2 decimals, `€` + space before | `€ 1,87`; credit/negative in axis: `−€ 1` |
| Meter readings | 3 decimals + thousands `.` | `4.277,964 kWh`, `2.914,286 m³` |
| Tariffs | 4 decimals | `€ 0,2720 /kWh` |
| Time | `HH:mm` (zero-padded), live `HH:mm:ss` | `13:42:30` |
| Day | short weekday + day + short month | `do 19 nov` (`vr 13 nov`) |
| Month / year ticks | `nov` / `2026` | |
| Range | `13 – 19 nov` (en dash with spaces) | |

Weekday names: `ma di wo do vr za zo`; months `jan feb mrt apr mei jun jul aug sep okt nov dec`. Use `Intl.DateTimeFormat('nl-NL', …)` and strip trailing dots. Units are always in a separate span so they can be styled.

Meterstanden: the API returns **Wh and dm³** in `totals` → divide by 1000 (#9).

## 5. Data, polling and states

### Endpoints used (existing, see reference §4–5)

| Data | Endpoint | Used by |
|---|---|---|
| Last reading (live) | `GET /webapi/v3/metrics/last-reading` every 10 s | PowerFlow, LiveIndicator, TariffBadge (`mode`) |
| Today totals | `GET /webapi/v3/energy/total-today` | Vandaag KPIs |
| Meter readings | `GET /webapi/v3/energy/totals` | Meterstanden |
| Series | `GET /webapi/v3/metrics/{tenseconds\|minutes\|hours\|days\|months\|years}?from&to` | Vermogen chart, Historie charts, Per dag table |
| Settings | `GET/PUT /webapi/v3/settings`, `GET /webapi/v3/settings/time-zones` | Instellingen, solar visibility, theme, time zone |

### Derived values

- `verbruik = intake + solar − redelivery`
- `direct = solar − redelivery` (= min(solar, verbruik)) — the bottom of the stack
- `opname = intake` (= verbruik − direct), `teruglevering = redelivery`
- Self-use sentence on Actueel: `round(direct / solar × 100)` % — only when solar > 0.

### API additions needed

1. **Yesterday until the same time** for the KPI comparison bars: e.g. `GET /webapi/v3/energy/total-yesterday-until-now` returning the same shape as `total-today` (+ costs). Until it exists, hide the bars and captions rather than faking them.
2. **Costs today** (electricity + gas, excluding vaste kosten) for the Kosten KPI. Can be computed client-side from `total-today` and tariffs if simpler.
3. **Theme** in settings (`theme: "licht" | "donker" | "systeem"`), or store it in `localStorage` per device.
4. Settings `solarSystem` must be respected on **every** page (#4), not only Historie.

### States (every data block)

| State | Show |
|---|---|
| Loading | Skeletons in the final layout (no layout shift). Never "N/A" or 0 (#16). |
| Live | `LiveIndicator` "Live · bijgewerkt 13:42:30" — time = `created` of the last reading. |
| Stale (last reading 30 s – 5 min old) | Dot `status-warning` + "Vertraagd" + "laatste meting 13:41:02". Values stay. |
| Offline (> 5 min, or request fails) | Danger `Banner` at the top of Actueel: **"Geen nieuwe metingen sinds 13:02.** Controleer de Raspberry Pi in de meterkast." + indicator "Offline · laatste meting 13:02:10". Keep last values visible (screenshot `01b`). |
| Empty period | Chart area replaced by "Nog geen metingen in deze periode". |
| Tariffs missing | Warning `Banner` on Historie in € mode: "**Niet alle prijzen zijn bekend.** Stel de gasprijs in onder Instellingen." (name the missing one). The affected chart shows the empty state with a "Naar Instellingen" button (`03b`). Table Kosten header says "€, zonder gas". |
| No solar (`solarSystem = false`) | Hide Opwekking/Teruglevering everywhere: flow nodes, KPIs, legend items, series, table columns, meter rows (`01c`). No zero values. |

Thresholds (30 s / 5 min) are a proposal; make them constants.

## 6. Charts (ApexCharts)

Common options: `chart.toolbar.show=false`, `chart.zoom.enabled=false`, `chart.selection.enabled=false`, `chart.animations.enabled=false` for live updates, `legend.show=false` (custom `LegendTotal` row instead), `dataLabels.enabled=false`, `grid.borderColor=line`, `xaxis.labels.rotate=0`, `xaxis.labels.hideOverlappingLabels=true`, label style `ink-muted` 12px Hanken Grotesk, `tooltip` custom HTML in tokens (Card style + `shadow-float`), `yaxis.labels.formatter` via `format.ts`. Add a zero line annotation in `line-strong` whenever negatives are present. Fixes #10, #11, #14, #15, #20, #22.

### 6.1 Actueel — "Vermogen" (stacked areas)

- Series (in this order): `direct` (area, `series-opwekking`, fill opacity 0.45), `opname` (area, `series-opname`, 0.40), `teruglevering` as **negative** values (area, `series-teruglevering`, 0.45), `verbruik` (line, `series-verbruik`, 2.5px, no fill). Areas stacked so direct + opname = verbruik exactly.
- One `SegmentedControl` Nu · Uur · Dag (default) · Week (#2):
  - Nu → `tenseconds`, last 10 min, ticks every 2 min `HH:mm`
  - Uur → `minutes`, last 60 min, ticks every 10 min
  - Dag → `hours`, last 24 h, ticks every 2 h (mobile every 6 h), plus a dashed vertical line at 00:00 and a second tick row with the day (`wo 18 nov` / `do 19 nov`)
  - Week → `hours`, last 7 d, ticks per day `ma 16 nov` (mobile `ma` / `16` on two lines)
- Above the chart: totals for the visible period per series (`LegendTotal`, kWh 1 decimal). Caption "Afgelopen 24 uur, gemiddeld per uur" (adjust per range).
- Y-axis unit label `kW` at the top. Crosshair tooltip: header `do 19 nov, 12:00–13:00`, rows per series with swatch, value 3 decimals + `kW`.
- No solar: only `opname` area + `verbruik` line, no negative axis.

### 6.2 Historie — "Elektriciteit" (stacked bars)

- kWh mode: bars `direct` (`series-opwekking`) + `opname` (`series-opname`) stacked up, `teruglevering` negative (`series-teruglevering`), plus a Verbruik marker: a 2.5px `series-verbruik` cap on top of each stack (implement as a line series with `stroke.curve='stepline'`, or as `annotations`, or a custom bar with `colors` border; any is fine if it reads as the top edge).
- € mode: `opname €` up, `teruglevering €` down (credit), `Totaal` (= opname € − teruglevering €) as an `ink` cap. Axis and tooltip in `€` (#10).
- Footnote: "Teruglevering staat onder de nul. Vandaag is nog niet compleet." / € mode: "Bedragen zonder vaste kosten. …".
- Hovered bar: `surface-sunken` column highlight + tooltip `wo 18 nov` with per-series values.

### 6.3 Historie — "Gas"

- Single bar series `series-gas`, m³ (2 decimals) or € mode. Footnote "De gasmeter werkt eens per uur bij. Vandaag is nog niet compleet."

### Tick formats by grouping

| Grouping | Tick | Tooltip header |
|---|---|---|
| tenseconds / minutes | `HH:mm` | `do 19 nov, 13:42:30` / `13:42` |
| hours | `HH:mm` (+ day row) | `do 19 nov, 12:00–13:00` |
| days | `vr 13 nov` (mobile two lines) | `vr 13 nov` |
| months | `nov` | `november 2026` |
| years | `2026` | `2026` |

The current bug where a "2026-11-20 (Thursday)" tick appears (#22) comes from timezone handling of day buckets: bucket and label in the user's time zone.

## 7. Pages

### 7.1 Actueel (`01`–`02`)

Desktop order: page title "Actueel" + date caption → **Nu** card → **Vandaag** card → row with **Vermogen** (flex 2, min 640) and **Meterstanden** (flex 1, min 320). Mobile: everything stacks; first screen = Nu card + KPI grid (#19).

- **Nu card**: header "Nu" + `TariffBadge` + `LiveIndicator` (right). Power flow: Opwekking node (64px ring `series-opwekking`, solar icon) → connector (3px line + arrow, caption "0,640 kW direct gebruikt") → Verbruik node (88px ring `series-verbruik`, home icon, `num-hero` value) → connector (`series-teruglevering`, "0,500 kW naar het net") → Teruglevering node. When the house draws from the grid instead, the right node becomes **Opname** (`series-opname`) and its arrow points to the house. Caption under the flow: "Je gebruikt nu 56% van je zonnestroom zelf. Opname van het net: 0,000 kW." No solar: Net (Opname) → Huis only. Mobile: hero value on top, compact 48px nodes in a row below.
- **Vandaag card**: caption "tot 13:42 · de streep is gisteren tot hetzelfde tijdstip". KpiTiles: Verbruik, Opname, Opwekking, Teruglevering, Gas, Kosten (grid `auto-fit, minmax(160px,1fr)`; mobile 2 columns, tiles on `surface` background). No solar: Verbruik, Gas, Kosten.
- **Vermogen**: §6.1.
- **Meterstanden**: title + caption "Stand om 13:42:30". Rows with **correct labels** (#9): Opname normaal (`usageTotalHigh`), Opname dal (`usageTotalLow`), Teruglevering normaal (`redeliveryTotalHigh`), Teruglevering dal (`redeliveryTotalLow`), Opwekking totaal (`solarTotal`), Gas (`usageGasTotal`). Values ÷ 1000, `meter` mono style, right-aligned, unit in `ink-muted`, zebra `surface-sunken`. Footnote "Gas werkt de meter eens per uur bij." No solar: hide Teruglevering and Opwekking rows.

### 7.2 Historie (`03`–`04`)

- **Toolbar** (one row, wraps on small screens): title, `PeriodPicker`, kWh/€ `SegmentedControl` (right). Replaces the "Toon kosten" switch and the two "Bereik" selects (#7, #22).
- **PeriodPicker popover** (`03b`), 640px wide, `shadow-float`:
  - Left "Snelkeuze": Afgelopen 24 uur · Laatste 7 dagen (default) · Afgelopen maand · Afgelopen jaar · Afgelopen 5 jaar. Selected = `brand-soft` + check icon.
  - Right "Aangepast bereik": Van / Tot en met text fields (`dd-mm-jjjj`), **one** compact month calendar (Monday first, range band `brand-soft`, endpoints `brand`, future days disabled), "Groeperen per" select (Uur / Dag / Maand / Jaar) with helper "Per uur kan tot 30 dagen, per dag tot 700 dagen." (#6).
  - Footer: Annuleren / Toepassen.
  - Server limits: pick the default grouping automatically for presets (24 uur → uur, 7 dagen → dag, maand → dag, jaar → maand, 5 jaar → jaar). For a custom range, disable groupings that exceed the limit instead of offering "Negeer waarschuwing".
  - Mobile: same content in a bottom sheet with full-width 44px controls.
- **Banner** when tariffs are missing (only relevant in € mode, but show it in both so the user knows).
- **Charts**: Elektriciteit + Gas side by side on desktop (each flex 1, min 480); on mobile one at a time with tabs "Elektriciteit | Gas" (`role="tablist"`, active = 2px `brand` underline). Legends show period totals.
- **Per dag table** (new): columns Dag · Verbruik · Opname · Opwekking · Teruglevering · Gas · Kosten (units in a caption row under each header), right-aligned numbers, "Totaal" row on `surface-sunken`, "Exporteren" button (CSV, replaces the chart hamburger download, #15). Mobile: Dag · Verbruik · Gas · Kosten. Rows follow the selected grouping (per uur / dag / maand / jaar → header "Per …").

### 7.3 Instellingen (`05`–`06`)

Desktop: left section menu (Algemeen, Tarieven, Account; anchors, scroll-spy optional) + form column max 760px. Mobile: stacked cards. Inputs are **not** full width (#8).

- **Algemeen**: rows with title + one-line description + control on the right, separated by `line`:
  - "Systeem met zonnepanelen" `Switch` — "Toont Opwekking en Teruglevering op Actueel en Historie. Uit: je ziet alleen Verbruik en Opname."
  - "Dagnaam in grafieken" `Switch` — "Bijvoorbeeld ‘do 19 nov’ in plaats van ‘19 nov’."
  - "Thema" `SegmentedControl` Licht / Donker / Systeem (previews immediately).
  - "Tijdzone" select, 280px, labels like "Amsterdam (UTC+1)", searchable if the list is long.
- **Tarieven**: intro "Hiermee rekent EnergieZicht je kosten uit. Vul de bedragen in zoals op je contract, inclusief btw."
  - "Stroom per kWh" grid: columns Normaaltarief / Daltarief (`TariffBadge` headers), rows Opname / Teruglevering → 4 `MoneyField`s `/kWh`.
  - Below a divider: Gas (`/m³`), Vaste kosten stroom and Vaste kosten gas (`/maand`).
  - Changed field: 2px `brand` border + caption "Opname daltarief gewijzigd, was € 0,2540."
  - Empty gas price: 2px `status-warning` border + "Nog niet ingesteld. Zonder gasprijs zie je geen gaskosten." (`05b`).
  - Validation: accept `,` and `.` as decimal separator, store as number, show 4 decimals; negative not allowed except Teruglevering if the contract has negative feed-in prices (ask product owner).
- **Account**: rows E-mailadres (`naam@…` + Wijzigen), Wachtwoord ("Laatst gewijzigd op …" + Wijzigen), Tweestapsverificatie ("Aan, met een authenticator-app" + Beheren), Raspberry Pi (live dot + "Gekoppeld, laatste meting …" + Sleutel bekijken), and an "Uitloggen" button. Links go to the Account page sections.
- **One SaveBar** for Algemeen + Tarieven together (#8, #24). Herstellen restores the last saved values. Leaving the page with unsaved changes asks for confirmation.

### 7.4 Inloggen (`07`–`08`)

- Full-screen photo background: `prototype/assets/login-zonnepanelen.jpg` (Pexels, Michael Pointner; free licence, credit optional but shown) with `background-size: cover; background-position: center 55%`, **blur 8px** (scale the layer 1.05 to hide blurred edges) and a flat tint `rgba(24,16,8,0.60)` on top. Credit bottom-right in a small dark pill (`rgba(20,14,8,0.6)`, 12px, `#f6f1ea`).
- Centred column: wordmark above the card (34px, "Energie" `#ffffff` + "Zicht" `#a8ece3`, text-shadow `0 1px 2px rgba(0,0,0,.35), 0 2px 16px rgba(0,0,0,.45)`), then the card: `surface-raised`, max 400px, padding 32/24, `radius-lg`, `shadow-float`.
- Step 1 "Inloggen" — subtitle "Bekijk wat je huis nu verbruikt."; E-mailadres; Wachtwoord with "Wachtwoord vergeten?" link right of the label and a "Toon/Verberg" button inside the field; checkbox "Ingelogd blijven"; primary "Inloggen" full width (44px).
- Error (`07b`): danger box inside the card "**E-mailadres of wachtwoord klopt niet.** Probeer het opnieuw of stel een nieuw wachtwoord in." + both fields with 2px `status-danger` border. Lockout: "Te veel pogingen. Probeer het over 5 minuten opnieuw."
- Step 2 "Bevestig dat jij het bent" (`07c`): 6-digit code field (52px, 24px, letter-spacing .3em, `inputmode="numeric"`, `autocomplete="one-time-code"`), checkbox "Dit apparaat 30 dagen onthouden", "Bevestigen", links "Geen toegang tot je app? Gebruik een herstelcode" and "Terug naar inloggen".
- No registration link (accounts are provisioned). Dark theme: same photo; card follows the theme.

### 7.5 Account (`09`)

Breadcrumb "Instellingen › Account", title "Account", left menu (anchors) + 760px column of cards:

- **Profiel**: Naam, Telefoonnummer (optioneel), Opslaan.
- **E-mailadres**: current address + "Bevestigd" (check icon, `status-live`), Nieuw e-mailadres, "Bevestigingsmail sturen".
- **Wachtwoord**: Huidig, Nieuw (helper "Minimaal 12 tekens. Een zin van een paar woorden werkt goed."), Herhaal, "Wachtwoord wijzigen". Match the server's password policy text.
- **Tweestapsverificatie**: status badge "Aan"; rows Authenticator-app (Opnieuw instellen), Herstelcodes ("Nog 8 van de 10 over…", Nieuwe codes maken), Onthouden apparaten (Vergeten), Tweestapsverificatie uitzetten (danger button). Set-up flow (QR + code) uses the same card style.
- **Persoonlijke gegevens**: Gegevens downloaden (Exporteren); danger zone `status-danger-bg` "Account verwijderen" with explanation and danger button → confirm dialog requiring the password.

## 8. Responsive and accessibility

- Breakpoints: ≥768 AppBar, <768 BottomTabBar; ≤560 mobile spacing (16px gutters/padding, 12px KPI gaps, 44px controls).
- No horizontal scrolling at 360px, except inside the Per dag table container (`overflow-x: auto`).
- All controls are real `<button>`, `<a>`, `<input>`, `<select>` with labels; switches `role="switch"` + `aria-checked`; segmented controls `aria-pressed`; tabs `role="tab"` + `aria-selected`; charts have an `aria-label` and the Per dag table is the accessible data alternative.
- Focus ring on everything (token `focus-ring`). Touch targets ≥ 44px on mobile.
- Colour is never the only signal: series have labels, status has icon + words, Teruglevering is below the axis.
- Live values: wrap the hero figure in `aria-live="polite"` but throttle announcements (e.g. only when the user focuses the card), otherwise screen readers speak every 10 s.

## 9. Acceptance checklist (from the original issues)

| # | Fixed by |
|---|---|
| 1, 2 | One Vermogen chart with one range control (§6.1) |
| 3 | Nu card with flow + TariffBadge; KPI comparison with yesterday |
| 4 | `solarSystem` hides solar UI on all pages |
| 5 | Meterstanden beside the chart; no tall empty column |
| 6, 7 | PeriodPicker popover with one calendar and a separate "Groeperen per" |
| 8, 24 | Compact settings form, one SaveBar, no faint disabled buttons |
| 9 | Correct meter labels, ÷1000, units |
| 10 | € on axis and tooltip in cost mode |
| 11, 12 | `format.ts` (nl-NL, zero-padded times, Dutch day names) |
| 13 | `series-gas` violet |
| 14, 15 | No zoom/toolbar; CSV via Exporteren |
| 16, 17 | Skeletons, LiveIndicator, stale/offline states |
| 18 | Identity pages restyled in Dutch with the same tokens |
| 19, 20, 21 | Mobile Actueel: compact Nu card, 2-column KPI grid, thinned ticks, aligned tiles |
| 22 | Mobile toolbar without overlap, unrotated two-line ticks, correct day buckets |
| 23 | BottomTabBar; initials instead of email |

## 10. Notes on the mock data

All numbers in the mockups are illustrative and consistent within each page (e.g. Verbruik = Opname + Opwekking − Teruglevering). They are **not** consistent across pages: the Kosten tile on Actueel (€ 1,87) does not match the sample tariffs used on Historie (which would give about € 5,75 for today including gas). Always compute from the API.

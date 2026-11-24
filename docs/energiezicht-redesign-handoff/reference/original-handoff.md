# EnergieZicht — Redesign Handoff

Live site: https://energiezicht.nl (login required). Screenshots in `screenshots/` were taken 2026-11-19. Files `01`–`19` are desktop at 1440px. Files `m01`–`m10` are mobile at a 390px viewport (iPhone width), rendered in a same-origin iframe with grey margins around it.

> The screenshots show the owner's email address in the navbar. Blur or crop it before you share them outside the design session.

---

## 1. What the product is

EnergieZicht is a personal home-energy dashboard written in Dutch. A Raspberry Pi in the meter cupboard runs `energy_reader` (Python). It reads the Dutch smart meter (**P1/DSMR telegram**) and an optional **Fronius solar inverter**. Every **10 seconds** it POSTs a reading to the portal. The portal stores the readings at 10-second, minute and hour resolution and shows them live and historically. Users can enter their energy tariffs to see costs.

Each user has one Raspberry Pi, linked through an `RpiKey`. It is a single-household, single-meter product.

### Data per reading (from `energy_reader/reader.py` → `POST /api/v3/energy`)

| Field | Meaning | Unit sent |
|---|---|---|
| `mode` | Active tariff (1 = low/dal, 2 = normal/hoog) | int |
| `usageNow` | Current grid **intake** | W |
| `redeliveryNow` | Current grid **export** | W |
| `solarNow` | Current inverter output (PAC) | W |
| `usageTotalHigh` / `usageTotalLow` | Meter reading, intake, normal/low tariff | Wh |
| `redeliveryTotalHigh` / `redeliveryTotalLow` | Meter reading, export, normal/low tariff | Wh |
| `solarTotal` | Inverter lifetime total | Wh |
| `usageGasNow` | always 0 (not read) | — |
| `usageGasTotal` | Gas meter reading (updates **hourly** on the meter) | dm³ (×1000 = m³) |
| `created` | UTC timestamp | ISO |

**Derived values** (server side): **Verbruik (usage) = intake + solar − redelivery**, which is what the house actually consumes. **Opname** = intake from the grid. **Teruglevering** = export to the grid. **Opwekking** = solar generation.

### Domain vocabulary (keep the Dutch UI terms)
- **Verbruik**: total house consumption
- **Opname**: drawn from the grid
- **Teruglevering**: fed back into the grid
- **Opwekking**: solar generation
- **Normaaltarief / hoog** and **Daltarief / laag**: peak and off-peak tariff
- **Meterstanden**: meter readings
- **Vaste kosten**: fixed monthly delivery fees

---

## 2. Current tech and constraints

- ASP.NET Core (.NET 9) Razor layout with a **Bootstrap 4** navbar, plus a **Vue 3 + Vuetify 3** app mounted on `#app`. Each Razor view (`/`, `/history`, `/home/settings`) renders the same Vue `App.vue`, which picks a component by URL path. There is no client router.
- Charts use **ApexCharts** (`vue3-apexcharts`). Icons use MDI.
- Two UI stacks are mixed: the Identity pages (login, account management) are stock ASP.NET Identity Razor pages in **English** with Bootstrap. The app pages are Vuetify in **Dutch**.
- Background `#f5f8fa`, dark navbar (`bg-dark`), white elevated cards.
- Series colours (Bootstrap defaults), used everywhere:
  - Verbruik `#007bff` (blue)
  - Opwekking `#ffc107` (amber)
  - Teruglevering `#28a745` (green)
  - Opname `#dc3545` (red)
  - Gas `#dc3545` (red, the same as Opname)

The redesign can drop Bootstrap and unify on Vuetify, or use a different approach. The data and API below are the fixed contract.

---

## 3. Information architecture (current)

```
Navbar: [EnergieZicht] Actueel | Historie                 [user email ▾]
                                                            ├ Beheer        → /Identity/Account/Manage
                                                            ├ Instellingen  → /home/settings
                                                            └ Uitloggen
```
Logged-out users get only a "Login" link. The brand is hidden when logged out. There is no registration link visible.

---

## 4. Pages

### 4.1 Login — `/Identity/Account/Login` · `01-login.jpg`
Stock Identity page: Email, Password, "Remember me?", Log in, "Forgot your password?". It is English with no branding, and the large page is empty below the form.

### 4.2 Actueel (live dashboard) — `/` · `02`–`07`
Component: `frontend/src/Home/HomeDashboard.vue` + `ElectricityChart.vue`

**Row 1: four live stat cards** (polled every 10 s from `GET /webapi/v3/metrics/last-reading`)
Verbruik / Opwekking / Teruglevering / Opname, each a coloured label, a large number and the unit "kW".

**Left column (8/12): four separate area charts**, one each for Verbruik, Opwekking, Teruglevering and Opname. Each chart has its own **Nu / Uur / Dag / Week** toggle, but the toggles are linked, so changing one changes all four.

| Toggle | Endpoint | Window | Resolution |
|---|---|---|---|
| Nu | `/webapi/v3/metrics/tenseconds` | last 10 min | 10 s |
| Uur | `/webapi/v3/metrics/minutes` | last 60 min | 1 min |
| Dag (default) | `/webapi/v3/metrics/hours` | last 24 h | 1 h |
| Week | `/webapi/v3/metrics/hours` | last 7 d | 1 h |

**Right column (4/12): stacked cards**
- Opname vandaag (kWh)
- Teruglevering vandaag (kWh)
- Opwekking vandaag (kWh)
- Gasverbruik vandaag (m³)
- **Meterstanden** table: Opname hoog/laag, Levering hoog/laag, Gas

Data: `GET /webapi/v3/energy/total-today` and `GET /webapi/v3/energy/totals`.

### 4.3 Historie — `/history` · `08`–`15`
Component: `frontend/src/History/History.vue`

- Top bar: **"Toon kosten"** switch (left) and **"Bereik"** select (right) with the options Afgelopen 24 uur · Laatste 7 dagen (default) · Afgelopen maand · Afgelopen jaar · Afgelopen 5 jaar · Selecteer bereik.
- **Selecteer bereik** shows two large inline Vuetify date pickers (from/to), a "Negeer waarschuwing" switch that skips the server's time-span limit, and a second select, also labelled "Bereik", for grouping (Per tien seconden … Per jaar).
- Two bar charts, **Elektriciteit** and **Gas**, with the ApexCharts zoom/pan toolbar.
  - kWh mode: Verbruik, plus Opwekking, Teruglevering and Opname if the "zonnepanelen" setting is on.
  - Cost mode (€): Totaal, plus Opname € and Teruglevering € if solar is on. Gas shows m³ or €.
- If tariffs are missing, an orange toast appears: "Niet alle prijzen zijn bekend. Stel deze in onder Instellingen".

Data: `GET /webapi/v3/metrics/{tenseconds|minutes|hours|days|months|years}?from&to&skipTimeSpanCheck`
Server limits (HTTP 400 when exceeded, unless skipped): 10 s ≤ 3 h, minutes ≤ 12 h, hours ≤ 30 d, days ≤ 700 d.

### 4.4 Instellingen — `/home/settings` · `16`–`17`
Component: `frontend/src/Home/Settings/UserSettings.vue`

- **Instellingen** card has three controls:
  - "Systeem met zonnepanelen" switch. It only affects History; the dashboard always shows solar.
  - "Toon dagnaam in grafiek" switch.
  - "Tijdszone" autocomplete. The default is Europe/Amsterdam.
- **Tarieven** card has seven € fields: Opname normaal/dal, Teruglevering normaal/dal, Gastarief, and Vaste kosten elektriciteit/maand and gas/maand.
- Each card has its own Reset and Opslaan buttons, disabled until something changes.

Data: `GET/PUT /webapi/v3/settings` and `GET /webapi/v3/settings/time-zones`.

### 4.5 Beheer (account) — `/Identity/Account/Manage/*` · `18`–`19`
Stock Identity pages in English: Profile (username, phone), Email, Password, Two-factor authentication, Personal data. They use a left nav and Bootstrap buttons.

---

## 5. API response shape (metrics)

`GET /webapi/v3/metrics/{group}` returns parallel arrays:
```jsonc
{
  "queryTimestamp": "ISO",
  "format": "HH:mm:ss" | "MM-dd HH:mm" | "yyyy-MM-dd" | "yyyy-MM-dd (dddd)" | "yyyy-MM" | "yyyy",
  "userTimeZone": "Europe/Amsterdam",
  "timestamps": ["..."],
  "usage":       [[epochMs, kW|kWh], ...],
  "solar":       [[epochMs, value], ...],
  "redelivery":  [[epochMs, value], ...],
  "intake":      [[epochMs, value], ...],
  "gas":         [[epochMs, m3], ...],
  "usageCosts":  [...], "intakeCosts": [...], "redeliveryCosts": [...], "gasCosts": [...]
}
```
`total-today` returns `{ usageTotalHigh, usageTotalLow, redeliveryTotalHigh, redeliveryTotalLow, totalSolar, totalGas }` in kWh/m³, already differenced and rounded to 3 decimals.
`totals` returns the last raw reading entity (fields from the table in §1, still in Wh/dm³).

---

## 6. Problems observed (design input)

**Layout and hierarchy**
1. The dashboard has **four near-identical charts** stacked vertically. Verbruik and Opname are often the same line when there is no solar. A single combined chart (stacked/overlaid series and a legend) would show more in less space.
2. Each chart repeats the same Nu/Uur/Dag/Week toggle four times, although the toggles are linked. One global range control is enough.
3. All stat cards have the same weight. Nothing answers "how am I doing right now / today vs. usual?". There is no comparison to yesterday or the average, and no indication of the current tariff (dal/normaal), although `mode` is available.
4. When the user has no solar, the Opwekking and Teruglevering cards and charts show permanent zeros (`03`). They should be hidden or collapsed based on the `solarSystem` setting, which the dashboard currently ignores.
5. The right column becomes a tall stack of cards with a lot of empty space below it (`04`).
6. Historie custom range: two **huge inline calendars** take up a whole screen (`14`). Their header text "SELECT DATE / Enter date" is English. A compact date-range picker would fit better.
7. Two different selects are both labelled "Bereik" (range and grouping) (`15`).
8. Settings stretch every input to full width, and the Reset/Opslaan buttons are very faint when disabled (`16`, `17`).

**Data and labels**
9. The Meterstanden table **labels are wrong**. `redeliveryTotalHigh` is shown as "Opname laag" and `usageTotalLow` as "Levering hoog" (`HomeDashboard.vue` `totals`). Values are raw **Wh / dm³ with no unit** (e.g. "4277964").
10. Cost mode has **no € on the axis or tooltip**, and the y-axis keeps the "0.000" kWh formatting (`11`, `13`).
11. Axis labels are malformed: "11-18 22:0", "21:0:32" (unpadded minutes), and day names are in English ("Thursday") in a Dutch UI. Month ticks skip months (`13`).
12. Number formatting mixes `.` decimals with Dutch labels. Dutch convention is `0,264 kW`.
13. Gas uses the same red as Opname. It needs its own colour.

**Interaction**
14. **Scroll-wheel over a History chart zooms the chart instead of scrolling the page**, and it skews the bars (`09`). Zoom should be opt-in.
15. Chart hamburger menus (download SVG/PNG/CSV) are exposed on every chart.
16. There is no loading or empty state; the cards show "N/A" or 0 until data arrives.
17. There is no last-updated or connection indicator, although the Pi can go offline. `queryTimestamp` is available.

**Consistency**
18. There are two design languages (Bootstrap Identity pages and Vuetify app pages) and two languages (English and Dutch).
**Mobile (390px)**
19. The dashboard is **3,800px tall**. The whole first screen shows only the four live stat cards, each about 180px tall and full width (`m01`). A 2×2 grid or a single compact "now" strip would fit on one screen.
20. The four charts each get only two x-axis labels ("11-18 22:0", "11-19 09:0"), and the flat solar/export charts waste about 1,000px (`m02`, `m03`).
21. The "vandaag" cards are **misaligned**. The first sits flush and the rest are indented, because `pl-3` is applied unconditionally on all but the first card (`m04`). The gas unit "M³" is clipped.
22. Historie: the **"Toon kosten" switch label overlaps the Bereik select** (`m07`). Rotated 45° date labels take about 100px below each chart, and a "2026-11-20 (Thursday)" tick appears although today is 11-19.
23. The navbar collapses to a Bootstrap hamburger, and the full email address is shown as the account menu (`m06`). A bottom tab bar (Actueel / Historie / Instellingen) would suit phone use better.
24. Settings work on mobile but are long, with faint disabled buttons at the bottom of each card (`m09`, `m10`).

---

## 7. Suggested direction (open for the design session)

- **Actueel**: hero "now" panel showing the current power flow (grid ⇄ house ⇄ solar), the active tariff badge, and a live indicator with last-update time. Below it, "Vandaag" KPIs (kWh, m³, €) with deltas against yesterday. Then **one** combined power chart with a single range control. Put meter readings in a compact secondary panel with correct labels and units.
- **Historie**: period picker (presets plus a compact range picker), a kWh/€ segmented control, Elektriciteit and Gas side by side or tabbed, and period totals above each chart.
- **Instellingen**: grouped form, solar toggle that drives the visibility of solar UI everywhere, tariffs in a compact two-column grid with units (€/kWh, €/m³, €/maand), and one save bar.
- **Auth/Beheer**: restyle in the same system and language as the app.
- Design tokens: a light/dark theme, distinct series colours (Verbruik, Opname, Teruglevering, Opwekking, Gas) that are colour-blind safe, and Dutch number and date formatting.

---

## 8. Screenshot index

| File | Shows |
|---|---|
| 01-login | Login page (logged out) |
| 02-actueel-top-dag | Dashboard top: live cards, Verbruik chart (Dag), today cards |
| 03-actueel-midden-meterstanden | Flat Opwekking/Teruglevering charts, Meterstanden table |
| 04-actueel-onder-opname | Opname chart, empty right column |
| 05-actueel-range-nu | "Nu" range (10 s resolution, last 10 min) |
| 06-actueel-range-week | "Week" range |
| 07-user-menu | User dropdown: Beheer / Instellingen / Uitloggen |
| 08-historie-7-dagen | History default (7 days, kWh) |
| 09-historie-bug-wheel-zoom | Chart distorted after mouse-wheel zoom |
| 10-historie-gas | Gas bar chart |
| 11-historie-kosten-toast | Cost mode + missing-prices toast |
| 12-historie-bereik-dropdown | Range presets |
| 13-historie-jaar-kosten | Last year, monthly, costs |
| 14-historie-custom-datepickers | Custom range with two inline calendars |
| 15-historie-custom-groupby | Ignore-warning switch + grouping select |
| 16-instellingen-algemeen | General settings |
| 17-instellingen-tarieven | Tariff form |
| 18-beheer-profiel | Identity: Profile |
| 19-beheer-2fa | Identity: 2FA |
| m01-actueel-top | Mobile: first screen, four stacked stat cards |
| m02-actueel-verbruik-chart | Mobile: Verbruik chart, two axis labels |
| m03-actueel-lege-charts | Mobile: flat Opwekking/Teruglevering charts |
| m04-actueel-opname-vandaag | Mobile: misaligned "vandaag" cards |
| m05-actueel-gas-meterstanden | Mobile: gas card, Meterstanden |
| m06-menu-open | Mobile: hamburger menu open |
| m07-historie-top-overlap | Mobile: History, switch label overlaps select |
| m08-historie-gas | Mobile: Gas chart, rotated labels |
| m09-instellingen-top | Mobile: settings |
| m10-instellingen-tarieven | Mobile: tariffs |

## 9. Source files to read

- `EnergyPortal/Views/Shared/_Layout.cshtml`, `_LoginPartial.cshtml` (shell and nav)
- `EnergyPortal/frontend/main.js`, `src/App.vue` (Vue bootstrap and URL switch)
- `EnergyPortal/frontend/src/Home/HomeDashboard.vue`, `ElectricityChart.vue`
- `EnergyPortal/frontend/src/History/History.vue`
- `EnergyPortal/frontend/src/Home/Settings/UserSettings.vue`
- `EnergyPortal/Controllers/WebApi/*` (API contract)
- `../energy_reader/reader.py` (what is measured)

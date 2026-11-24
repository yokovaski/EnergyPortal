# EnergieZicht redesign — hand-off package

This package hands the EnergieZicht redesign to whoever implements it. EnergieZicht is the Dutch home-energy dashboard in `EnergyPortal`: ASP.NET Core with a Vue 3 + Vuetify front end, fed by a Raspberry Pi that posts P1 smart-meter readings every 10 s.

## Reading order

1. **`IMPLEMENTATION.md`**: the build spec. It covers the technical direction, shared components, formatting rules, data and states, chart configuration, every page, and an acceptance checklist mapped to the original issues.
2. **`design-system/README.md`**: the rules behind every screen (principles, copy, colour, type, spacing, charts, icons).
3. **`screenshots/`**: the target design, one PNG per screen and state. Desktop images are 1440px wide; mobile images are 390px wide at 2×.
4. **`prototype/`**: clickable reference prototypes. Open `prototype/index.html` in a browser; no build step is needed. Query parameters switch states (see the index page).
5. **`reference/original-handoff.md`**: the analysis of the current site, the API contract and the 24 numbered problems that the spec refers to as `#1`…`#24`.

## Contents

```
README.md                  this file
IMPLEMENTATION.md          build spec
design-system/
  README.md                design rules
  tokens.json              source of truth for colours (light/dark), type, spacing, radii, sizes
  tokens.css               CSS custom properties (--ez-*) + base + type helpers, generated from tokens.json
  vuetify-theme.js         Vuetify 3 light/dark theme + component defaults, generated from tokens.json
screenshots/               PNG per screen/state (see list below)
prototype/
  index.html               overview with links and state switches
  *.html                   one page per screen (rendered by dc-lite.js)
  dc-lite.js               tiny renderer for the prototypes, NOT for production
  assets/login-zonnepanelen.jpg   login background photo (Pexels, Michael Pointner)
mockups-source/            original .dc.html design files from the design canvas (for reference)
reference/
  original-handoff.md      analysis of the current site and API contract
```

## Screenshot index

| File | Screen / state |
|---|---|
| 01-actueel | Actueel desktop, live, with solar |
| 01b-actueel-offline | Pi offline: danger banner + offline indicator |
| 01c-actueel-zonder-zonnepanelen | Household without solar |
| 01d-actueel-donker | Dark theme |
| 02-actueel-mobiel | Actueel at 390px |
| 03-historie-kwh | Historie, last 7 days, kWh |
| 03b-historie-kosten-periodekiezer | € mode, gas price missing, period picker open |
| 03c-historie-kosten | € mode with all tariffs |
| 04-historie-mobiel / 04b-…-gas | Historie mobile, Elektriciteit and Gas tabs |
| 05-instellingen | Settings with one unsaved change (SaveBar) |
| 05b-instellingen-gasprijs-ontbreekt | Gas price not set |
| 06-instellingen-mobiel | Settings mobile |
| 07-inloggen / 07b-…-fout / 07c-…-code | Login, wrong password, 2FA code step |
| 08-inloggen-mobiel | Login mobile |
| 09-account | Account page (replaces the Identity "Manage" pages) |

## About the prototypes

The prototypes are reference renderings of the design files. They are not code to copy. Re-implement the screens as Vue components on the design tokens.

- **Fonts:** the prototypes load their fonts from Google Fonts; production should self-host them.
- **Data:** all figures are mock data.
- **Links:** navigation between pages works, and so do these interactions: the kWh/€ switch, the period picker, the tabs, the settings switches, the theme choice, the Save bar, and the login steps.

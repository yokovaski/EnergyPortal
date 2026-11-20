# EnergieZicht logo

The logo is the wordmark with the lightning Z: "Energie" plus "Zicht", where the Z is a drawn lightning bolt in the same colour as the rest of "Zicht". This is the only logo. The separate icon (a white lightning Z on petrol) is used only as the favicon and app icon, never next to the wordmark.

`voorbeeld.png` shows every variant on its intended background.

## Files

| File | Use |
|---|---|
| `svg/energiezicht-logo.svg` | Default, on light backgrounds (Energie `#13201d`, Zicht `#0f5257`) |
| `svg/energiezicht-logo-donker.svg` | Dark theme (Energie `#e8eeec`, Zicht `#6fcfc3`) |
| `svg/energiezicht-logo-op-foto.svg` | On the darkened login photo (white, Zicht `#a8ece3`) |
| `svg/energiezicht-logo-wit.svg` | Single-colour white, e.g. on petrol |
| `svg/energiezicht-logo-zwart.svg` | Single-colour dark, e.g. for print or fax |
| `png/*-480.png`, `-960.png`, `-1920.png` | The same five variants as transparent PNGs, 480, 960 and 1920 px wide |
| `favicon/` | Favicon and app icon (white lightning Z on petrol), ready for `wwwroot/` |

The SVGs are converted to outlines, so they don't need the font. The aspect ratio is about 6 : 1.

## Using it in the app

Use the SVG as an image, sized by height. In the app bar the logo is 22 px tall (font size) for both desktop and mobile; as an image that is a height of about 20 px.

```html
<a href="/" class="ez-logo" aria-label="EnergieZicht, naar Actueel">
  <img src="/img/energiezicht-logo.svg" alt="EnergieZicht" height="20">
</a>
```

For the dark theme, swap to `energiezicht-logo-donker.svg`, or inline the SVG and set the two `fill` values to `var(--ez-ink)` and `var(--ez-brand)` so it follows the theme automatically.

Favicon: copy `favicon/*` to `wwwroot/` and add this to the `<head>`:

```html
<link rel="icon" href="/favicon.ico" sizes="48x48">
<link rel="icon" href="/favicon.svg" type="image/svg+xml">
<link rel="apple-touch-icon" href="/apple-touch-icon.png">
<link rel="manifest" href="/site.webmanifest">
<meta name="theme-color" content="#0f5257">
```

## Rules

- **Clear space:** at least the height of the Z on every side.
- **Minimum size:** 16 px cap height, about 120 px wide.
- **Colour:** the Z always has the same colour as "icht". No yellow.
- **Don't** stretch the logo, put the icon next to the wordmark, or set it in a different font.

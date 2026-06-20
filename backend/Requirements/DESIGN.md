# DESIGN.md — Marketplace

> A clean, image-forward, **category-neutral** e-commerce marketplace where **discovery feels like browsing a curated feed** and **buying feels trustworthy**. Built to sell across every category — electronics, fashion, home, books, groceries — without favoring any. Drop this file into your project root and let your coding agent generate matching UI.

---

## 1. Design Philosophy

The product is a multi-vendor marketplace serving three roles — **Customer**, **Vendor**, and **Admin** — across many unrelated product categories. The palette is intentionally **neutral and trust-forward** so the same chrome looks right around a laptop, a dress, a sofa, or a paperback. The *product imagery is the only thing allowed to bring color* — the interface stays out of its way.

Two borrowed structures resolve one tension:

- **Discovery should feel effortless and visual** → from **Pinterest**: a masonry feed, image-first cards, a "Save" gesture, minimal chrome, content that fills the frame.
- **Buying should feel safe and clear** → from **Airbnb**: the pill-shaped search, soft rounded cards, an explicit trust layer (ratings, verified badges, transparent pricing), a structured product page, and a calm checkout.

Vendor and admin screens inherit the same DNA (neutral grays, rounded geometry, one blue accent) but trade image-density for **data-density** as the role gets more operational.

**Three words:** neutral, scannable, trustworthy.

---

## 2. Brand Reference & Influence Map

| Influence | Weight | Where it shows up |
| :--- | :--- | :--- |
| **Pinterest** | ★★★★★ (dominant) | Homepage + Catalog masonry feed, image-first product cards, the "Save" gesture, infinite scroll, blur-up image loading |
| **Airbnb** | ★★★★★ (dominant) | Pill search bar, sidebar filters, rating + review architecture, soft card elevation, two-column product detail, generous whitespace |
| **Amazon / Flipkart** | ★★★ (palette) | The category-neutral **blue + amber** commerce convention: blue for trust/actions, amber for ratings and deals |
| **Stripe** | ★★★ (checkout only) | Checkout wizard, payment forms, order-confirmation receipt, weight-300 numeric elegance |
| **Coinbase / Mastercard** | ★★ (trust cues) | Verified-vendor badges, buyer-protection callouts, payout/settlement clarity |
| **IBM Carbon** | ★★ (admin only) | Dense data tables, multi-filter toolbars, split-view moderation, audit ledgers |
| **Intercom** | ★ (support) | Messaging, notification center, friendly dashboard widgets |

> **Rule of thumb:** if a screen is for *finding or wanting* a product → lean Pinterest (masonry, image-first). If it's for *deciding or paying* → lean Airbnb/Stripe. If it's for *running the business* (vendor/admin) → lean Carbon, but keep the neutral grays and blue accent.
>
> Pinterest's influence here is **structural** (the discovery feed and the Save gesture) — not chromatic. The Save action renders in the neutral brand blue, not Pinterest red.

---

## 3. Color System

The whole point is restraint: **one blue carries brand and action, one amber carries ratings and deals, everything else is neutral.** This keeps the UI category-agnostic — nothing in the chrome whispers "fashion" or "tech."

### Brand
```
--brand-primary:        #2563EB   /* Trust blue — primary CTAs, links, nav-active, selected states */
--brand-primary-hover:  #1D4ED8   /* hover */
--brand-primary-pressed:#1E40AF   /* pressed */
--brand-primary-tint:   #EAF1FE   /* selected-chip bg, focus halo, subtle fills */
```

### Accent (ratings & deals only — never a second primary)
```
--accent:        #F59E0B   /* Amber — rating stars, deal/sale badges, "limited deal" emphasis */
--accent-strong: #B45309   /* text on amber tint */
--accent-tint:   #FEF3E2   /* deal-badge / promo backgrounds */
```
> Amber is the universal e-commerce rating + deal color (Amazon, Flipkart, eBay all use it). It does the warm, attention-grabbing job that the old coral did — but only on ratings and deals, so it never colors the general UI.

### Neutrals (clean, category-agnostic gray ramp)
```
--ink:            #1F1F1F   /* primary text */
--ink-secondary:  #6E6E73   /* secondary text, helper copy */
--ink-tertiary:   #A1A1A6   /* placeholders, disabled */
--border:         #E2E2E5   /* default borders, dividers */
--border-subtle:  #EFEFF1   /* table rows, faint separators */
--surface:        #FFFFFF   /* cards, app background */
--surface-sunken: #F5F5F7   /* page bg behind cards, input fills, dashboard canvas */
--overlay:        rgba(0,0,0,0.55)  /* modal scrim, image gradient for overlay text */
```

### Semantic & Status
```
--success:  #1A8245   --success-bg:  #E7F4EC   /* Delivered, Paid, Active, Approved */
--warning:  #B45309   --warning-bg:  #FEF3E2   /* Shipped, Out for Delivery, Pending settlement */
--error:    #C0362C   --error-bg:    #FBEAE8   /* Cancelled, Failed, Rejected, Suspended */
--info:     #2563EB   --info-bg:     #EAF1FE   /* Confirmed, Initiated, Draft-in-review (shares brand blue) */
--neutral-status: #6E6E73  --neutral-status-bg: #F0F0F2  /* Pending, Draft, Archived */
```

### Status → Color Map (single source of truth)
| Domain | State | Token |
| :--- | :--- | :--- |
| Order | Confirmed → `info` · Shipped → `warning` · Delivered → `success` · Cancelled → `error` |
| Shipment | Pending → `neutral` · Initiated → `info` · Out for Delivery → `warning` · Delivered → `success` |
| Payment | Paid → `success` · Refunded → `info` · Failed → `error` |
| Payout | Pending → `neutral` · Paid → `success` · Failed → `error` |
| Product | Draft → `neutral` · Active → `success` · Archived → `ink-tertiary` |
| Vendor | Pending → `warning` · Approved/Active → `success` · Suspended → `error` |

### Rating & Save
```
--star-filled: #F59E0B   --star-empty: #DDDDDD   /* amber stars */
--save-active: #2563EB                           /* "Save" bookmark fills brand blue when saved */
```

---

## 4. Typography

| Role | Family | Weights | Use |
| :--- | :--- | :--- | :--- |
| **Display / Headings** | `Plus Jakarta Sans` | 600, 700 | Hero, section titles, page headers — warm, geometric voice |
| **Body / UI** | `Inter` | 400, 500, 600 | All interface text, body copy, form labels, dense tables |
| **Numeric / Mono** | `Roboto Mono` | 400, 500 | Order IDs, SKUs, Transaction IDs, settlement amounts in ledgers |

```
--font-display: "Plus Jakarta Sans", system-ui, sans-serif;
--font-body:    "Inter", system-ui, sans-serif;
--font-mono:    "Roboto Mono", ui-monospace, monospace;
```

### Type Scale
| Token | Size / Line | Weight | Font | Use |
| :--- | :--- | :--- | :--- | :--- |
| `display` | 40 / 44 | 700 | display | Homepage hero headline |
| `h1` | 32 / 40 | 700 | display | Page titles |
| `h2` | 24 / 32 | 600 | display | Section headers |
| `h3` | 20 / 28 | 600 | display | Card group titles, product name (PDP) |
| `h4` | 18 / 24 | 600 | body | Sub-sections, modal titles |
| `body-lg` | 16 / 24 | 400 | body | Long-form, product descriptions |
| `body` | 14 / 20 | 400 | body | **Base UI size**, table cells, form fields |
| `caption` | 13 / 18 | 400 | body | Helper text, metadata, timestamps |
| `micro` | 12 / 16 | 500 | body | Badges, status pills, table column headers (uppercase, tracking +0.04em) |
| `price` | 18 / 24 | 600 | body | Product price (tabular-nums) |
| `mono` | 13 / 18 | 400 | mono | IDs, SKUs, amounts in ledgers |

> Numbers in prices, totals, and ledgers use `font-variant-numeric: tabular-nums` so columns align.

---

## 5. Spacing, Radius & Elevation

### Spacing (4px base)
```
--space-1: 4px   --space-2: 8px   --space-3: 12px  --space-4: 16px
--space-5: 20px  --space-6: 24px  --space-8: 32px  --space-10: 40px
--space-12: 48px --space-16: 64px --space-20: 80px
```

### Radius — geometry is generous and rounded (both brands)
```
--radius-xs:   4px    /* inline tags, small badges */
--radius-sm:   8px    /* inputs, small buttons */
--radius-md:   12px   /* buttons, standard cards (Airbnb) */
--radius-lg:   16px   /* image cards, modals (Pinterest) */
--radius-xl:   24px   /* feature/banner image containers */
--radius-pill: 999px  /* search bar, filter chips, category pills, status pills */
```

### Elevation — soft, low-contrast shadows (Airbnb)
```
--shadow-sm: 0 1px 2px rgba(0,0,0,0.08);   /* resting card */
--shadow-md: 0 2px 8px rgba(0,0,0,0.10);   /* card hover lift */
--shadow-lg: 0 6px 16px rgba(0,0,0,0.12);  /* dropdowns, popovers */
--shadow-xl: 0 8px 28px rgba(0,0,0,0.16);  /* modals, drawers */
```
> No hard borders on cards — separate with shadow + whitespace. Borders are for inputs, tables, and dividers only.

---

## 6. Layout & Grid

```
--container-max: 1280px;     /* centered max width for catalog/PDP/dashboards */
--container-fluid: 1600px;   /* masonry feed bleeds wider, capped here */
--gutter: 16px;              /* grid + masonry gutter */
--sidebar-w: 240px;          /* vendor/admin nav rail */
--filter-rail-w: 280px;      /* customer catalog left filters */
```

### Breakpoints
```
xs 360 · sm 640 · md 768 · lg 1024 · xl 1280 · 2xl 1536
```

### Layout patterns
- **Masonry feed (Pinterest)** — column-balanced, variable card heights, 16px gutter. Columns: `2 (xs) → 3 (sm) → 4 (md) → 5 (lg) → 6 (xl)`. Used on Homepage and default Catalog browse.
- **Uniform card grid (Airbnb)** — equal-height cards when filters/sorting demand alignment. Columns: `2 → 3 → 4 → 5`.
- **Two-column detail (Airbnb)** — PDP: sticky image gallery (60%) + sticky buy-box (40%) on `lg+`; stacks on mobile.
- **Centered wizard (Stripe)** — checkout, OTP, single-task forms: max **760px**, centered, one step visible at a time.
- **Sidebar shell (Carbon)** — vendor & admin: fixed 240px left nav + fluid content with metric cards over data tables.

---

## 7. Core Components

### Buttons
| Variant | Style |
| :--- | :--- |
| **Primary** | `--brand-primary` fill, white text, `--radius-md`, 44px tall, `600`. Hover → `--brand-primary-hover`. |
| **Secondary** | white fill, `1px solid --ink`, `--ink` text, `--radius-md`. |
| **Ghost / Text** | no fill/border, `--ink` text, underline on hover (links use `--brand-primary`). |
| **Save (bookmark)** | circular 40px, white bg + `--shadow-sm`, outline bookmark icon; fills `--save-active` (blue) + subtle pop when saved. Overlaid top-right on image cards. |
| **Icon** | 40px square, transparent, `--surface-sunken` on hover. |
| **Destructive** | `--error` text on transparent; `--error` fill only for final confirm (e.g. Cancel Order, Delete Category). |

Disabled: `--ink-tertiary` text, `--surface-sunken` fill, no shadow.

### Search bar (Airbnb signature)
Pill-shaped (`--radius-pill`), white, `--shadow-md`, leading search icon, 48–56px tall. The single most prominent element in the global header. On focus: expands, shadow deepens to `--shadow-lg`.

### Filter chips & category pills
Horizontal scroll row of pills (`--radius-pill`). Default: white + `1px --border`. Selected: `--brand-primary-tint` fill + `--brand-primary` text + `1px --brand-primary`. Used for categories on Homepage and quick-filters on Catalog.

### Sidebar filters (Airbnb catalog)
Left rail (`--filter-rail-w`): collapsible sections for Category tree, Price range (dual slider), Rating (★ & up), with an "Apply"/"Clear all" footer. Collapses into a "Filters" button + modal on mobile.

### Product card — two variants
**A. Discovery card (Pinterest — default feed):** image-first, full-bleed, `--radius-lg`, no border. Image fills card at its natural aspect (4:5 preferred). Save-bookmark overlaid top-right. Below image: product name (1 line, truncate), price, and a compact `★ 4.8 (124)` (amber star). Hover → subtle image zoom + bookmark appears.

**B. Standard card (Airbnb — filtered grid):** uniform height, `--radius-md`, fixed image ratio (1:1 or 4:3), `--shadow-sm` resting → `--shadow-md` hover-lift. Shows image, vendor name (small, `--ink-secondary`), product name, rating, price with optional struck-through original + deal badge.

### Rating
Filled `--star-filled` (amber) stars (or a single star + numeric `4.8`), review count in `--ink-secondary` parentheses. Compact form on cards; full breakdown bars on PDP.

### Price display
`--price` token, tabular-nums. Discount: original price struck through in `--ink-secondary`, sale price in `--ink`, plus a `micro` **deal badge** in `--accent-tint` / `--accent-strong`. "You save ₹X" rendered in `--success`. Variant delta updates price live on selection.

### Status pill
`--radius-pill`, `micro` type, colored per the Status→Color map: tinted background + matching dark text (e.g. Delivered → `--success` on `--success-bg`). Always carries a text label — never color alone.

### Quantity stepper (cart)
`[ − ] n [ + ]`, `--radius-sm`, 36px tall. Decrementing below 1 removes the line item (with an undo toast).

### Inputs & forms
36–44px tall, `--surface-sunken` or white fill, `1px --border`, `--radius-sm`. Focus: `2px --brand-primary` ring + `--brand-primary-tint` halo. Label above, helper/error (`--error`) below. Errors state what to fix, in the interface's voice.

### OTP input
Six separate boxes (`--radius-sm`, 48px), auto-advance, paste-to-fill, with a countdown-timer "Resend" link below.

### Product image gallery (PDP)
Thumbnail strip + large active image, click-to-zoom (lightbox). Sticky on scroll within the two-column layout.

### Variant selector
Pills for size/color (selected = `--brand-primary` outline; out-of-stock = struck + disabled), or dropdown when >8 options. Color variants show swatches.

### Checkout stepper (Stripe)
Numbered horizontal stepper: **1 Address → 2 Review → 3 Pay → Success**. Completed steps get a `--success` check; current step `--brand-primary`. Payment section uses Stripe Elements styling; success screen is a clean receipt with Order ID in `--font-mono`.

### Data table (Vendor/Admin — Carbon-influenced)
`micro` uppercase column headers, 48px rows, `--border-subtle` row dividers, hover `--surface-sunken`, sortable headers, sticky header on scroll, row actions on the right. Pagination footer. Numeric columns right-aligned, tabular-nums, mono for IDs.

### Metric card (dashboards)
`--surface` card, `--radius-md`, `--shadow-sm`: small `caption` label, large `h2` value, optional delta (`--success`/`--error` with ▲/▼) and a sparkline. Used in grids of 3–4.

### Chart
Revenue/sales: line or bar, `--brand-primary` series on `--surface-sunken` gridlines. Use `--accent` (amber) only for a secondary "deals/promotions" series. Keep axes light, label values directly.

### Category tree (Admin designer)
Indented tree with expand/collapse chevrons, inline "Add child / Edit / Delete" on row hover. Delete disabled (with tooltip) when the category contains products.

### Split view (Admin moderation)
Two-pane: list on the left, detail/approval panel on the right; masked GST/PAN, approve/reject actions, reason dialog for suspension.

### Modals, drawers, toasts
Modal: centered, `--radius-lg`, `--shadow-xl`, `--overlay` scrim, max 560px. Drawer: right-side for filters/quick-edit. Toast: bottom-center, `--radius-md`, auto-dismiss, with undo where destructive.

### Supporting
Breadcrumbs (`--ink-secondary`, `/` separators) on Catalog/PDP; Tabs (underline-active in `--brand-primary`) on dashboards; Avatar/Store-logo circle with initials fallback; Empty states = friendly illustration + one-line direction + a primary action (never a dead end).

---

## 8. Imagery & Iconography

**Imagery is the only color the UI permits (Pinterest).** Because the chrome is neutral, product photos from any category — a phone, a jacket, a couch — all sit comfortably in the same frame.
- Default discovery aspect: **4:5 portrait**; uniform grids: **1:1**; banners/hero: **16:9** or **3:1**.
- Always full-bleed inside the card, `--radius-lg`, never letterboxed.
- **Blur-up / skeleton loading** for masonry; lazy-load below the fold.
- Overlay text on images gets a bottom gradient (`--overlay` → transparent) for legibility.
- `alt` text is mandatory — in an image-first product, it's both accessibility and SEO.

**Icons:** outline style, **1.5–2px stroke**, 20px (inline) / 24px (nav), Lucide/Phosphor family. Bookmark = save, star = rating (amber), shield/check = verified vendor.

---

## 9. Motion & Interaction

- Card hover: lift to `--shadow-md` + 1.02 image zoom, **180ms ease-out**.
- Save bookmark: scale-pop `1 → 1.2 → 1` on save, **220ms**.
- Page/route transitions and reveals: **150–250ms ease-out**; modals fade+scale from 0.98.
- Masonry items fade/translate-in on load (staggered ≤ 40ms).
- Skeleton shimmer on loading tables, cards, charts.
- **Always** respect `prefers-reduced-motion: reduce` → disable zoom, pop, and translate; keep instant state changes.

---

## 10. Accessibility

- Contrast **WCAG AA** minimum; brand blue `#2563EB` on white passes for body and UI text. Amber is for fills/icons (stars, badges) — never amber text on white at body size; use `--accent-strong` on `--accent-tint` instead.
- Visible focus ring on every interactive element: `2px --brand-primary` + 2px offset.
- Touch targets ≥ **44×44px**.
- Full keyboard nav; logical tab order in wizards and tables; trap focus in modals.
- Status is never conveyed by color alone — pair with a label/icon (the status pill always carries text).
- All images have `alt`; decorative images `alt=""`.

---

## 11. Role-Based Theming

Same tokens, same components — **image-density dials down and data-density dials up as the role gets more operational.** The neutral palette means none of the three roles ever looks "loud."

| | **Customer** | **Vendor** | **Admin** |
| :--- | :--- | :--- | :--- |
| Dominant influence | Pinterest + Airbnb | Airbnb + Carbon | Carbon (neutral) |
| Accent use | Blue actions + amber ratings/deals, generous | Blue for primary actions; amber on rating widgets only | Blue for status/critical actions; amber sparingly |
| Primary layout | Masonry feed, two-col PDP, centered checkout | Sidebar shell, metric cards + tables, charts | Sidebar shell, dense multi-filter tables, split views |
| Imagery | Hero everywhere | Thumbnails in product/inventory tables | Minimal — data over imagery |
| Surfaces | White on `--surface-sunken` | Same, more cards/charts | More tables, tighter rows |

> Across all three: same font stack, same blue, same neutral grays, same rounded geometry. A vendor or admin screen should still feel unmistakably part of the same marketplace — just quieter and denser.

---

## 12. Screen-by-Screen Application

### Common (Public / Shared)
- **Landing & Homepage** → Hero banner carousel (`--radius-xl`, 16:9) → horizontal category **pill row** → **Pinterest masonry feed** of top-rated products → active-discount promo strip (`--accent-tint` banners with `--accent-strong` text).
- **Product Catalog & Search** → Airbnb **left filter rail** (`--filter-rail-w`: category tree, price dual-slider, rating) + sort dropdown + **uniform card grid**; pill quick-filters on top; pagination footer. Default browse (no filters) can fall back to masonry.
- **Product Details** → **two-column**: sticky gallery (thumbnails + zoom) left, sticky **buy-box** right (name, amber `★` rating, live-updating price on variant select, variant pills, stock status, Add to Cart + Save bookmark). Reviews below with rating-breakdown bars and review images.
- **Registration / Login** → centered card (max 480px), white, `--shadow-lg`; blue primary button; "Remember me" toggle.
- **OTP Verification** → centered wizard, six-box OTP + resend countdown.

### Customer
- **Dashboard / Profile** → left profile-section nav + profile card with edit + account-status toggle.
- **Change Password** → centered single-column form.
- **Address Book** → grid of address cards (Home/Office pills, set-default & delete) + "Add Address" modal.
- **Shopping Cart** → line-item rows (image, quantity stepper, subtotal) + sticky **totals summary card** + promo-code input evaluating discounts live.
- **Checkout** → **Stripe-style stepper**: Address → Review → Pay → receipt success (Order ID in mono).
- **Order History** → data table (Order ID mono, date, total, **status pill**, view action).
- **Order Details & Tracking** → items grid + address summary + payment status + **horizontal shipment progress bar** (Pending → Shipped → Out for Delivery → Delivered) + Cancel (rule-gated).
- **Returns Portal** → select delivered items + quantities, live refund calc, reason dropdown, submit.
- **Wishlist** → **masonry of saved products** (Pinterest), Move-to-Cart / Remove on each.
- **Review Writer** → 5-star selector (amber), title, body, drag-and-drop image upload.
- **Become a Vendor** → application form (Store name, email, GST, PAN, description, logo upload).

### Vendor
- **Dashboard** → metric cards (revenue, active products, pending settlements) + **revenue chart** + recent-orders table + Store Status toggle.
- **Store Profile** → editable store fields + non-editable approved GST/PAN (locked styling).
- **Inventory List** → data table (name, category, **status pill** Draft/Active/Archived, publish toggle, archive).
- **Add/Edit Product Wizard** → stepped form (title, category, description, publish).
- **Variant & Image Workspace** → variant table (label/value, SKU mono, stock, price-delta, default radio) + per-variant image manager (drag-drop, reorder).
- **Order Tracker** → orders with vendor's itemized products + earnings per order.
- **Shipment Console** → filterable shipment table (status pills, tracking numbers mono, ETA).
- **Coupon Console** → coupon generator (code, percent/flat, value, usage limit, expiry) + active-coupon grid.
- **Returns Monitor** → returned-product list with status indicators + customer reasons + pickup details.
- **Payout Ledger** → settlement table (IDs mono, gross, 20% commission, net payout, **status pill**, date), totals row.
- **Store Reviews** → review feed linked to product variants.

### Admin
- **Dashboard** → platform-wide metric cards + total-sales chart + commission ledger summary + notification center (Intercom-style).
- **User Administration** → searchable user table + profile drill-down drawer + suspend / grant-revoke-admin actions.
- **Vendor Moderation** → **split view**: Pending applications | Active/Suspended; approval panel with masked GST/PAN; suspend-with-reason dialog.
- **Category Tree Designer** → interactive **tree view** with inline add-child / edit / delete (delete disabled if products exist).
- **Global Orders & Shipments Hub** → advanced **multi-filter table** (vendor, customer, product, date, status) + export.
- **Return Approvals Queue** → claim list + detail modal with per-item approve/reject checkboxes + refund trigger.
- **Commission / Payment Audit Ledger** → global transaction table (status pills Paid/Failed/Refunded, methods, Transaction IDs mono).
- **Settlements Console** → vendor-payout list, filter by Vendor ID, execute/verify transfer.
- **Global Coupon Hub** → create platform-wide percent/flat discounts + disable + usage metrics.
- **Reviews Moderation Feed** → all-reviews feed, search by product/keyword, delete spam.

---

## 13. Implementation — CSS Custom Properties

```css
:root {
  /* Brand */
  --brand-primary: #2563EB;  --brand-primary-hover: #1D4ED8;
  --brand-primary-pressed: #1E40AF;  --brand-primary-tint: #EAF1FE;

  /* Accent (ratings & deals only) */
  --accent: #F59E0B;  --accent-strong: #B45309;  --accent-tint: #FEF3E2;

  /* Neutrals */
  --ink: #1F1F1F;  --ink-secondary: #6E6E73;  --ink-tertiary: #A1A1A6;
  --border: #E2E2E5;  --border-subtle: #EFEFF1;
  --surface: #FFFFFF;  --surface-sunken: #F5F5F7;  --overlay: rgba(0,0,0,0.55);

  /* Semantic */
  --success: #1A8245;  --success-bg: #E7F4EC;
  --warning: #B45309;  --warning-bg: #FEF3E2;
  --error: #C0362C;    --error-bg: #FBEAE8;
  --info: #2563EB;     --info-bg: #EAF1FE;
  --neutral-status: #6E6E73;  --neutral-status-bg: #F0F0F2;
  --star-filled: #F59E0B;  --star-empty: #DDDDDD;  --save-active: #2563EB;

  /* Type */
  --font-display: "Plus Jakarta Sans", system-ui, sans-serif;
  --font-body: "Inter", system-ui, sans-serif;
  --font-mono: "Roboto Mono", ui-monospace, monospace;

  /* Spacing */
  --space-1: 4px;  --space-2: 8px;  --space-3: 12px;  --space-4: 16px;
  --space-5: 20px; --space-6: 24px; --space-8: 32px;  --space-10: 40px;
  --space-12: 48px; --space-16: 64px; --space-20: 80px;

  /* Radius */
  --radius-xs: 4px; --radius-sm: 8px; --radius-md: 12px;
  --radius-lg: 16px; --radius-xl: 24px; --radius-pill: 999px;

  /* Elevation */
  --shadow-sm: 0 1px 2px rgba(0,0,0,0.08);
  --shadow-md: 0 2px 8px rgba(0,0,0,0.10);
  --shadow-lg: 0 6px 16px rgba(0,0,0,0.12);
  --shadow-xl: 0 8px 28px rgba(0,0,0,0.16);

  /* Layout */
  --container-max: 1280px; --gutter: 16px;
  --sidebar-w: 240px; --filter-rail-w: 280px;
}
```

**Fonts:** load from Google Fonts —
`Plus+Jakarta+Sans:wght@600;700` · `Inter:wght@400;500;600` · `Roboto+Mono:wght@400;500`.

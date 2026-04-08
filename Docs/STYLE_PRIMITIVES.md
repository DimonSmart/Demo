# Demo Style Primitives

## Goal

The design system is shared, not uniform.
Pages may keep different mood, accent color, atmosphere, and motion, but the same semantic role must reuse the same primitive or component.

## Page Modes

- `Utility`: constrained reading width, standard shell, panels, forms, and result sections.
- `Immersive`: edge-to-edge workspace, reduced chrome, full-height content managed by the host layout.

## Canonical Shared Primitives

- `.demo-page` / `.page-container`: top-level vertical page stack.
- `.demo-stack`: vertical content stack for sections inside a page or panel.
- `.demo-cluster`: inline wrapping row for mixed controls or actions.
- `.demo-toolbar` / `.demo-action-bar`: action rows.
- `.demo-panel`: base panel/card/section surface.
- `.demo-panel--muted`: softer panel variant.
- `.demo-panel--tint`: tinted panel variant.
- `.demo-panel--center`: centered panel content.
- `.demo-form-grid`: responsive grid of form controls.
- `.demo-control-group`: labeled control group.
- `.demo-control-group--inline`: inline version of a control group.
- `.demo-card-grid`: responsive grid for showcase cards.
- `.demo-result-grid`: responsive grid for result cards or metrics.
- `.demo-stat-grid`: stats layout.
- `.demo-stat-card`: stat surface with label/value structure.
- `.demo-drop-zone` / `.dropzone`: file or asset intake area.
- `.demo-workspace`: full-height working area.
- `.demo-workspace--split`: main content plus side rail.
- `.demo-workspace__main`: main workspace column.
- `.demo-workspace__side`: side panel or log panel.
- `.demo-canvas-host`: full-height host for renderers, canvases, and board visualizations.

## Naming Rules

- Do not add a page-prefixed class when an existing primitive already matches the role.
- Use page namespaces only for genuinely domain-specific concepts such as `pdd-*`, `md-*`, `inv-*`, `maze-runner-*`.
- When two pages need the same block with different mood, use a modifier or theme token, not a new semantic class.

## Dynamic Class Prefixes

The following prefixes are reserved for deterministic runtime-generated markup and must be safelisted if CSS purging is introduced later:

- `inv-*`: invisible character visualization markers in `MarkdownToWordDemo`

## Allowed Inline Styles

Inline styles are allowed only for runtime-calculated values, for example:

- progress widths
- canvas or viewport sizes driven by parameters
- dynamic colors derived from state
- generated grid styles that depend on runtime dimensions

Static layout, spacing, typography, and surface rules must live in shared CSS or component/page stylesheets.

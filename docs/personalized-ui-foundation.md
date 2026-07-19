# Personalized UI foundation

## Architecture findings

- `frontend/src/App/AppRoutes.tsx` owns top-level React Router routes.
- `frontend/src/Components/Page/` owns the application shell, header, responsive sidebar, and shared page layout.
- `frontend/src/Store/Actions/index.js` composes Redux sections. `Store/Middleware/createPersistState.js` persists explicitly listed paths per Radarr instance, so personalized client-only preferences can follow the existing pattern without backend changes.
- `frontend/src/Settings/UI/` is the existing UI settings page. Server-backed settings remain untouched; the new appearance controls use a separate persisted Redux section so upstream API contracts do not change.
- `frontend/src/Styles/Themes/` supplies CSS variables through `App/ApplyTheme.tsx`. The new OLED palette extends that system and personalization is applied as document data attributes/CSS custom properties.
- `frontend/src/Movie/Index/Posters/` owns poster cards and already provides details, progress, search, edit, and external-link actions.
- Shared primitives in `frontend/src/Components/` (modal, forms, links, icons, labels, progress, scrollers) are reused.

## Planned impact

- Appearance: add `personalizedUiActions`, selectors/types, `AppearanceSettings`, an OLED theme, and root-level preference application.
- Navigation: update `Page`, `PageHeader`, `PageSidebar`, and `PageSidebarItem`; retain every current destination and add `/dashboard`.
- Command palette: add a global modal using existing movie Redux data and router navigation.
- Movie cards: enhance `MovieIndexPoster` and its CSS while retaining current actions and information.
- Dashboard: add a route and reusable widget shell backed by existing movies, queue, calendar, wanted, system, and disk-space state where populated. Persist widget order/visibility in the personalized UI Redux section.
- Styling: add reusable personalization tokens and responsive/accessibility rules to the existing global and CSS-module structure.

## Rebase boundaries

No .NET project, API model, database migration, or generated `_output` file is changed. New behavior is isolated in new frontend modules, with small integration edits to the app shell, route table, settings page, action registry, and poster card.

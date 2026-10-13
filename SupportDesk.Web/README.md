# Support Desk Web

Angular frontend for Support Desk. See the [root README](../README.md) for the project overview, API setup and business rules.

## Development

Requires the API running on `http://localhost:5105` (see the root README).

```bash
npm install
npm start        # dev server on http://localhost:4200
npm run build    # production build to dist/
```

The API base URL is set in `src/app/api-config.ts`.

## Structure

```
src/app/
├── pages/
│   ├── ticket-list/      # list with search, filters, pagination
│   ├── ticket-detail/    # ticket view, comments, status and agent actions
│   └── ticket-form/      # create and edit
├── shared/
│   ├── components/
│   │   └── confirm-dialog/
│   └── http-error.ts     # maps API ProblemDetails to messages
├── services/             # HTTP access (TicketService, AgentService)
├── models/               # API types, enums and display labels
├── app.routes.ts
├── app.config.ts
├── page-title-strategy.ts  # browser tab titles ("SD: Tickets")
└── api-config.ts
```

Global styles (design tokens, light/dark theme, buttons, forms, badges) live in `src/styles.scss`; component `.scss` files only hold page-specific layout.

Components never use `HttpClient` directly; all API calls go through `services/`.

# End-to-end tests

The Playwright suite starts a production-mode Apisite on `127.0.0.1:5187` with a temporary SQLite database. It builds the React UI when `dist` is absent, copies that build into the API release output, then visits the UI through `/ui`.

Run the complete suite from `src/AgileConfig.Server.UI/react-ui-antd`:

```powershell
npx playwright install chromium
npm run test:e2e
```

Run an individual layer with `npm run test:e2e:ui` or `npm run test:e2e:protocol`. Failed runs retain traces, screenshots, and video in `test-results`.

The Docker cluster test is intentionally separate from the browser suite. It builds the image, starts MySQL plus three AgileConfig nodes, then publishes through the admin node and verifies a WebSocket reload and configuration retrieval from a second node:

```powershell
npm run test:e2e:docker
```

It requires a running Docker daemon. GitHub Actions runs it nightly at 02:00 China Standard Time and on manual dispatch.

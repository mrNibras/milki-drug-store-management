# Frontend deep dive

## Stack and entry

The manifest in `frontend/package.json` confirms React 19, TypeScript 5.9, Vite 7, React Router 7, Zustand 5, Axios, Tailwind CSS 4, Recharts, and date-fns. `main.tsx` mounts the React app and initializes the stored theme; `App.tsx` builds the routes and shared layout. There is no `test` script or frontend `.test`/`.spec` file in the inspected tree.

## Route and page map

`App.tsx` routes public auth pages (`/login`, `/forgot-password`, `/reset-password`) and authenticated pages under `MainLayout`: dashboard, medicines, POS, inventory, purchases, suppliers, reports, notifications, damage/expiry, users, settings, audit logs, and branches. `ProtectedRoute` checks authentication and role for navigation. Treat this as presentation-side access control only: API controllers also use `[Authorize]` and role constraints, which is the real enforcement boundary.

Most pages under `frontend/src/pages/` combine the screen and workflow behavior. `DashboardPage`, `MedicinesPage`, `POSPage`, `InventoryPage`, `PurchasesPage`, `SuppliersPage`, `UsersPage`, `NotificationsPage`, `DamageExpiryPage`, report pages, settings and audit pages correspond to API modules. Shared controls are under `components/`; `ResponsiveTable`, `Modal`, `Button`, `Badge`, `ReceiptDialog`, and `MainLayout` are examples. `ErrorBoundary` catches React render errors; it does not handle HTTP failures.

## API request lifecycle

1. A page calls an action in `store/appStore.ts` or an API helper.
2. `config.ts` reads `VITE_API_BASE_URL` at build time and falls back to `http://localhost:5000`; `services/api.ts` appends `/api` to that value. If the deployed build lacks the environment setting, a remote browser can try to call its own localhost. The actual Vercel build setting was not visible.
3. A request interceptor reads `auth_token` from localStorage and attaches a Bearer header.
4. On 401, the response interceptor coordinates a refresh-token request to avoid concurrent refresh requests, stores a returned token pair, and retries queued requests. If refresh is unavailable/invalid, it removes browser auth state and redirects to login. A 403 is a permission rejection; it should not be treated as proof the session is invalid.
5. Controller response data updates Zustand state, after which components render the result.

The precise retry behavior should be rehearsed from `services/api.ts`, not guessed. UI hydration in `App.tsx` trusts a stored token/user pair to initialize the view, but backend calls determine whether it remains valid.

## State, validation and persistence

Zustand store actions hold current user, branch, product lists, sales and other page state. Auth and theme values are persisted manually in browser localStorage; the application does not use a Zustand persistence middleware. The login action stores access/refresh tokens, user and branch. The app periodically asks for token refresh every seven hours while authenticated. `logout()` only removes browser state; no server logout endpoint was found.

Forms use local page state and UI checks, but client validation is not authoritative. Backend validators are present but appear not registered; service checks and database constraints remain the actual validation in many paths. A sale or purchase mutation may update local state or refetch. To prove durable data, inspect the API response, then reload/refetch from the server; a Zustand state update alone is not persistence evidence.

## Security and maintainability observations

- **VERIFIED concern:** `appStore.ts` logs the full login response, including token fields, to the browser console. Other sale and purchase/POS paths log transaction/product values. Remove or redact before real production use.
- Tokens are kept in localStorage, making them available to any successful same-origin script injection. HttpOnly secure cookies or a backend-for-frontend session are safer alternatives, with CSRF design considered.
- Client-side roles and hidden buttons improve UX but cannot secure data. Keep controller/API checks and field-level redaction.
- The Vite build succeeded on 10 October 2026 but warned that `settingsApi.ts` is both dynamically and statically imported and that the main JS chunk is about 994 kB before gzip. This is a performance/build warning, not a functional test.
- No frontend automated test runner is declared; the frontend has no confirmed automated coverage in this checkout.

## Presentation explanation

“The browser app is React with TypeScript. React Router selects the page, Zustand shares UI state, and Axios centralizes the API URL and bearer token handling. A button can be hidden by role for a cleaner UI, but every permission-sensitive request is checked again by the ASP.NET API. The browser currently stores JWTs in localStorage, so I would prioritize removing token logs and moving toward a safer session storage design.”

## Evidence paths

`frontend/package.json`, `frontend/src/main.tsx`, `frontend/src/App.tsx`, `frontend/src/components/ProtectedRoute.tsx`, `frontend/src/services/api.ts`, `frontend/src/config.ts`, `frontend/src/store/appStore.ts`, `frontend/src/pages/`, `frontend/vite.config.ts`, `frontend/vercel.json`.

## Representative frontend code walkthrough

Line references match the inspected checkout. In code excerpts below, an ellipsis means intervening source lines are intentionally omitted.

### API setup and bearer token

Source: [api.ts](../../frontend/src/services/api.ts#L6), lines 6–20.

~~~typescript
export const api = axios.create({
  baseURL: `${API_BASE_URL}/api`,
  headers: { 'Content-Type': 'application/json' },
});

api.interceptors.request.use((config) => {
  const token = localStorage.getItem('auth_token');
  if (token) {
    config.headers.Authorization = `Bearer ${token}`;
  }
  ...
  return config;
});
~~~

This centralizes the API prefix and attaches the stored access token. Without the interceptor, authenticated calls would omit the bearer header. Debug by checking the request URL and status in the browser network panel, while never copying token values into notes or screenshots.

**Say aloud:** “Axios centralizes the API base and adds the access token to requests; the API still decides whether the user is allowed.”

### 401 refresh versus 403 authorization

Source: [api.ts](../../frontend/src/services/api.ts#L47), lines 47–99.

~~~typescript
if (status === 403) {
  return Promise.reject(error);
}
...
if (status === 401 && originalRequest && !originalRequest._retry && !isAuthEndpoint) {
  const refreshToken = localStorage.getItem('refresh_token');
  ...
  const res = await axios.post(
    `${API_BASE_URL}/api/auth/refresh`,
    { refreshToken }
  );
  const newToken = res.data.token;
  localStorage.setItem('auth_token', newToken);
  ...
  return api(originalRequest);
}
~~~

The distinction matters: 403 means the principal is authenticated but lacks permission, so logging out would be wrong. A 401 can trigger one refresh attempt and retry. Removing the retry guard risks loops; stale refresh failures clear browser state. Test with a dedicated test account and inspect status codes, not the token.

**Say aloud:** “The client attempts one refresh after an expired access token, but a forbidden role response is surfaced without logging the user out.”

### Authentication hydration and route guard

Sources: [App.tsx](../../frontend/src/App.tsx#L30), lines 30–50; [ProtectedRoute.tsx](../../frontend/src/components/ProtectedRoute.tsx#L10), lines 10–22.

~~~typescript
const storedToken = localStorage.getItem('auth_token');
const storedUser = localStorage.getItem('current_user');
if (storedToken && storedUser) {
  useAppStore.setState({
    token: storedToken,
    currentUser: JSON.parse(storedUser),
    isAuthenticated: true,
  });
}
~~~

~~~typescript
if (!isAuthenticated) return <Navigate to="/login" replace />;
if (roles && roles.length > 0 && userRole && !roles.includes(userRole)) {
  return <Navigate to="/dashboard" replace />;
}
return <>{children}</>;
~~~

This restores the UI from browser storage and guards navigation. Removing initialization can cause protected routes to redirect before state is ready. But localStorage can be edited, and when the role value is absent this guard does not reject; that is why API authorization is essential. Debug with a clean browser session and then verify the same role through an API request.

**Say aloud:** “The route guard controls navigation and improves user experience; the backend remains the security boundary.”

### Login logging and state update

Source: [appStore.ts](../../frontend/src/store/appStore.ts#L289), lines 289–316.

~~~typescript
const res = await api.post<LoginResponse>('/auth/login', {
  email: normalizedEmail,
  password,
});
console.log('[login] login response', res.status, res.data);
const token = res.data.token;
localStorage.setItem('auth_token', token);
if (res.data.refreshToken) {
  localStorage.setItem('refresh_token', res.data.refreshToken);
}
set({ token, currentUser: user, isAuthenticated: true });
~~~

The successful response becomes token/user state and is persisted. The console line logs the entire response and is a security finding; remove it before production use. If the state update is removed, the page may remain unauthenticated even though login succeeded. Debug using status and sanitized error metadata only.

**Say aloud:** “The login response populates client session state; the current payload log must be removed because it can expose credentials.”

### POS write path

Source: [appStore.ts](../../frontend/src/store/appStore.ts#L738), lines 738–770; [POSPage.tsx](../../frontend/src/pages/POSPage.tsx#L92).

The POS action posts a sale payload to the sales endpoint, then handles the returned response and refreshes related state. It also logs request/response data in the current source. Follow the API request in the network panel, match it to SalesController and the sale command handler, then confirm stock through a fresh API read. If a response is ambiguous after commit, do not retry blindly; inspect the test database first.

**Say aloud:** “The POS builds a request and delegates all pricing and stock authority to the API; the client only renders the server result.”

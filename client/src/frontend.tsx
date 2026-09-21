/**
 * This file is the entry point for the React app, it sets up the root
 * element and renders the App component to the DOM.
 *
 * It is included in `src/index.html`.
 */

import { StrictMode } from "react";
import { createRoot } from "react-dom/client";
import { App } from "./App";
import toast, { Toaster } from "react-hot-toast";

// Every failed request throws the response; its ProblemDetails body carries the message.
// index.html turns the unhandled rejection into this event (see the script there).
window.addEventListener("request-failed", async (e) => {
  const reason = (e as CustomEvent).detail;
  const body = reason?.error ?? (await reason?.json?.().catch(() => null));
  toast.error(body?.title ?? String(reason));
});

const elem = document.getElementById("root")!;
const app = (
  <StrictMode>
      <Toaster />
    <App />
  </StrictMode>
);

if (import.meta.hot) {
  // With hot module reloading, `import.meta.hot.data` is persisted.
  const root = (import.meta.hot.data.root ??= createRoot(elem));
  root.render(app);
} else {
  // The hot module reloading API is not available in production.
  createRoot(elem).render(app);
}

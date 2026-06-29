import { StrictMode } from "react";
import { createRoot } from "react-dom/client";
import "./index.css";
import App from "./App";

// Initialize theme from localStorage
try {
  const savedTheme = localStorage.getItem('mdsms-theme');
  if (savedTheme === 'dark') {
    document.documentElement.classList.add('dark');
  }
} catch (e) {
  // localStorage unavailable (e.g., private browsing)
}

createRoot(document.getElementById("root")!).render(
  <StrictMode>
    <App />
  </StrictMode>
);

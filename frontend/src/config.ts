export const API_BASE_URL = import.meta.env.VITE_API_BASE_URL || 'http://localhost:5000';
export const APP_NAME = import.meta.env.VITE_APP_NAME || 'Milki Drug Store';
export const APP_VERSION = import.meta.env.VITE_APP_VERSION || '1.0.0';

export const config = {
  apiBaseUrl: API_BASE_URL,
  appName: APP_NAME,
  appVersion: APP_VERSION,
};

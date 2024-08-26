import { defineConfig } from 'vite';
import react from '@vitejs/plugin-react';
import dotenv from 'dotenv';
dotenv.config();

const isDevelopment = process.env.NODE_ENV === 'development';
const backendUrl = isDevelopment
    ? process.env.DEVELOPMENT_BACKEND_URL
    : process.env.DEPLOYMENT_BACKEND_URL;

// https://vitejs.dev/config/
export default defineConfig({
  server: {
    proxy: {
      '/api': {
        target: backendUrl,
        changeOrigin: true,
      },
    }
  },
  plugins: [react()],
})

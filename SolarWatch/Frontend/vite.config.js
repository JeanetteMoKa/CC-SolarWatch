import { defineConfig } from 'vite';
import react from '@vitejs/plugin-react';
import { fileURLToPath } from 'url'
import dotenv from 'dotenv';
dotenv.config();

const isDevelopment = process.env.NODE_ENV === 'development';
const backendUrl = isDevelopment
    ? process.env.DEVELOPMENT_BACKEND_URL
    : process.env.DEPLOYMENT_BACKEND_URL;

const defaultConfig = {
  plugins: [react()]
}

// https://vitejs.dev/config/
export default defineConfig(({ command, mode }) => {

  if (command === 'serve') {
    const isDev = mode === 'development'

    return {
      ...defaultConfig,
      server: {
        proxy: {
          '/api': {
            target: backendUrl,
            changeOrigin: isDev
          }
        }
      }
    }
  } else {
    return defaultConfig
  }
  
})

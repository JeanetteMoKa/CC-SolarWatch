import { defineConfig } from 'vite'
import react from '@vitejs/plugin-react'

// https://vitejs.dev/config/
export default defineConfig({
  server: {
    proxy: {
      '/api': {
        target: 'https://solarwatchapi-frggbueydffzfren.polandcentral-01.azurewebsites.net',
        changeOrigin: true,
      },
    }
  },
  plugins: [react()],
})

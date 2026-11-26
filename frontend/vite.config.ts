import { fileURLToPath, URL } from 'node:url'
import tailwindcss from '@tailwindcss/vite'
import react from '@vitejs/plugin-react'
import { defineConfig, loadEnv } from 'vite'

// https://vite.dev/config/
export default defineConfig(({ mode }) => {
  const env = loadEnv(mode, process.cwd())

  return {
    plugins: [react(), tailwindcss()],
    resolve: {
      alias: {
        '@': fileURLToPath(new URL('./src', import.meta.url)),
      },
    },
    server: {
      // En desarrollo, /api va a la API local: mismo origen para el navegador, sin CORS.
      proxy: {
        '/api': {
          target: env.VITE_API_PROXY || 'http://localhost:5167',
          changeOrigin: true,
        },
      },
    },
  }
})

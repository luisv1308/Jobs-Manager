import { defineConfig } from 'vite'
import react from '@vitejs/plugin-react-swc'

// https://vite.dev/config/
export default defineConfig({
  plugins: [react()],
  server: {
    proxy: {
      '/jobHub': {
        target: 'http://localhost:5049',
        ws: true,
        changeOrigin: true
      }
    }
  }
})

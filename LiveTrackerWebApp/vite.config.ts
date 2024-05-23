import { defineConfig } from 'vite'
import react from '@vitejs/plugin-react'

// https://vitejs.dev/config/

//docker
// export default defineConfig({
//   plugins: [react()],
//   server: {
//     host: true,
//     port: 8070
//   }
// })

//local
export default defineConfig({
  plugins: [react()]
})

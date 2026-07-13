import { execFileSync } from 'node:child_process'
import { existsSync, mkdirSync, readFileSync } from 'node:fs'
import { dirname, resolve } from 'node:path'
import { fileURLToPath } from 'node:url'
import { defineConfig, type UserConfig } from 'vite'
import react from '@vitejs/plugin-react'
import tailwindcss from '@tailwindcss/vite'

const projectDirectory = dirname(fileURLToPath(import.meta.url))
const certificateDirectory = resolve(projectDirectory, 'certs')
const certificateFilePath = resolve(certificateDirectory, 'asp-net-core-dev.pem')
const keyFilePath = resolve(certificateDirectory, 'asp-net-core-dev.key')

function exportHttpsCertificateIfDoesntExist() {
  if (existsSync(certificateFilePath) && existsSync(keyFilePath)) {
    return
  }

  mkdirSync(certificateDirectory, { recursive: true })
  execFileSync('dotnet', [
    'dev-certs',
    'https',
    '--export-path',
    certificateFilePath,
    '--key-path',
    keyFilePath,
    '--format',
    'PEM',
    '--no-password',
  ], { stdio: 'inherit' })
}

// https://vite.dev/config/
export default defineConfig(({ command }) => {
  const config: UserConfig = {
    plugins: [react(), tailwindcss()],
  }

  if (command === 'serve') {
    exportHttpsCertificateIfDoesntExist()

    config.server = {
      host: 'localhost',
      https: {
        cert: readFileSync(certificateFilePath),
        key: readFileSync(keyFilePath),
      },
      port: 5173,
    }
  }

  return config
})

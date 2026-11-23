import js from '@eslint/js'
import { defineConfig, globalIgnores } from 'eslint/config'
import reactHooks from 'eslint-plugin-react-hooks'
import reactRefresh from 'eslint-plugin-react-refresh'
import globals from 'globals'
import tseslint from 'typescript-eslint'

/** Una feature no importa otra feature ni app/; shared/ tampoco (16-arquitectura §9). */
const importsProhibidos = (mensaje) => [
  'error',
  { patterns: [{ group: ['@/features/**', '@/app/**'], message: mensaje }] },
]

export default defineConfig([
  globalIgnores(['dist']),
  {
    files: ['**/*.{ts,tsx}'],
    extends: [
      js.configs.recommended,
      tseslint.configs.recommended,
      reactHooks.configs.flat.recommended,
      reactRefresh.configs.vite,
    ],
    languageOptions: {
      ecmaVersion: 2023,
      globals: globals.browser,
    },
  },
  {
    files: ['src/features/**/*.{ts,tsx}'],
    rules: {
      'no-restricted-imports': importsProhibidos(
        'Una feature no importa otra feature ni app/; lo común va en @/shared.',
      ),
    },
  },
  {
    files: ['src/shared/**/*.{ts,tsx}'],
    rules: {
      'no-restricted-imports': importsProhibidos('shared/ no depende de features ni de app/.'),
    },
  },
])

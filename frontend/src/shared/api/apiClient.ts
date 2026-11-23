import axios from 'axios'

const API_BASE_URL = import.meta.env.VITE_API_BASE_URL

if (!API_BASE_URL) {
  console.error('ERROR CRITICO: No se detectó VITE_API_BASE_URL en frontend/.env')
}

/**
 * Cliente HTTP principal. withCredentials permite enviar/recibir cookies HttpOnly
 * (refresh) junto con el Bearer del access token.
 */
export const apiClient = axios.create({
  baseURL: API_BASE_URL,
  headers: {
    'Content-Type': 'application/json',
  },
  withCredentials: true,
})

import axios from 'axios'

const API_BASE_URL = import.meta.env.VITE_API_BASE_URL

/**
 * Cliente sin interceptores de sesión. Usar solo para login/refresh/logout
 * para no entrar en bucle ante un 401 del refresh.
 */
export const authClient = axios.create({
  baseURL: API_BASE_URL,
  headers: {
    'Content-Type': 'application/json',
  },
  withCredentials: true,
})

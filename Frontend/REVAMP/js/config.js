// config.js
const API_CONFIG = {
  BASE_URL: 'https://localhost:7058/api', // Replace with your actual API URL
  ENDPOINTS: {
    AUTH: {
      LOGIN: '/auth/login',
      REGISTER: '/auth/register',
      LOGOUT: '/auth/logout'
    },
    PRODUCTS: '/products',
    CATEGORIES: '/categories',
    DASHBOARD: '/admin/dashboard'
  }
};
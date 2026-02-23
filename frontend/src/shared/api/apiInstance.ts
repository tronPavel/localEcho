import axios from "axios";

const API_BASE = 'http://localhost:5015/api';
const STATIC_BASE = 'http://localhost:5015';

export const api = axios.create({
    baseURL: API_BASE,
    headers: { 'Content-Type': 'application/json' },
});
api.interceptors.request.use((config) => {
    const token = localStorage.getItem('token');
    if (token) {
        config.headers.Authorization = `Bearer ${token}`;
    }
    return config;
});

export const getImageUrl = (path?: string | null): string | undefined => {
    if (!path) return undefined;
    if (path.startsWith('http') || path.startsWith('blob:')) return path;
    return `${STATIC_BASE}${path.startsWith('/') ? '' : '/'}${path}`;
};

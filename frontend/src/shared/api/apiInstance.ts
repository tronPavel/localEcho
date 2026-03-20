import axios, { AxiosError, type InternalAxiosRequestConfig } from "axios";
import { useAuthStore } from "@/features/auth/model/authStore";
import { refreshToken } from "@/features/auth/model/authApi"; // если нужно напрямую

const API_BASE = 'http://localhost:5015/api';
const STATIC_BASE = 'http://localhost:5015';

export const api = axios.create({
    baseURL: API_BASE,
    headers: { 'Content-Type': 'application/json' },
});

let isRefreshing = false;
let failedQueue: Array<{ resolve: (value?: any) => void; reject: (error?: any) => void }> = [];

const processQueue = (error: any, token: string | null = null) => {
    failedQueue.forEach(({ resolve, reject }) => {
        if (error) reject(error);
        else resolve(token);
    });
    failedQueue = [];
};

api.interceptors.request.use(
    (config: InternalAxiosRequestConfig) => {
        const token = localStorage.getItem('token');
        if (token) config.headers.Authorization = `Bearer ${token}`;
        return config;
    }
);

api.interceptors.response.use(
    (response) => response,
    async (error: AxiosError) => {
        const originalRequest = error.config as InternalAxiosRequestConfig & { _retry?: boolean };

        if (error.response?.status !== 401 || originalRequest._retry) {
            return Promise.reject(error);
        }

        if (isRefreshing) {
            return new Promise((resolve, reject) => {
                failedQueue.push({ resolve, reject });
            }).then((token) => {
                originalRequest.headers.Authorization = `Bearer ${token}`;
                return api(originalRequest);
            });
        }

        originalRequest._retry = true;
        isRefreshing = true;

        try {
            const token = localStorage.getItem('token');
            const refresh = localStorage.getItem('refreshToken');
            if (!token || !refresh) throw new Error();

            const refreshedUser = await refreshToken(token, refresh);
            useAuthStore.getState().setUser(refreshedUser); // обновляем store

            processQueue(null, refreshedUser.token);
            originalRequest.headers.Authorization = `Bearer ${refreshedUser.token}`;
            return api(originalRequest);
        } catch (refreshError) {
            processQueue(refreshError);
            useAuthStore.getState().clearUser();
            return Promise.reject(refreshError);
        } finally {
            isRefreshing = false;
        }
    }
);

export const getImageUrl = (path?: string | null): string | undefined => {
    if (!path) return undefined;
    if (path.startsWith('http') || path.startsWith('blob:')) return path;
    return `${STATIC_BASE}${path.startsWith('/') ? '' : '/'}${path}`;
};
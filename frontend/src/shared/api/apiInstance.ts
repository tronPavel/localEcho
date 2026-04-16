import axios, { AxiosError, type InternalAxiosRequestConfig } from "axios";
import { useAuthStore } from "@/features/auth/model/authStore";

const API_URL = import.meta.env.VITE_API_URL || 'http://localhost:5015/api';
const STATIC_URL = import.meta.env.VITE_STATIC_URL || 'http://localhost:5015';

export const api = axios.create({
    baseURL: API_URL,
    headers: { 'Content-Type': 'application/json' },
});

export const pureAxios = axios.create({
    baseURL: API_URL,
    headers: { 'Content-Type': 'application/json' },
});

let isRefreshing = false;
let failedQueue: Array<{ resolve: (value?: any) => void; reject: (reason?: any) => void }> =[];

const processQueue = (error: any, token: string | null = null) => {
    failedQueue.forEach(prom => {
        if (error) {
            prom.reject(error);
        } else {
            prom.resolve(token);
        }
    });
    failedQueue =[];
};

api.interceptors.request.use(
    (config: InternalAxiosRequestConfig) => {
        const token = useAuthStore.getState().user?.token;
        if (token) {
            config.headers.Authorization = `Bearer ${token}`;
        }
        return config;
    }
);

api.interceptors.response.use(
    (response) => response,
    async (error: AxiosError) => {
        const originalRequest = error.config as InternalAxiosRequestConfig & { _retry?: boolean };

        if (
            !originalRequest ||
            error.response?.status !== 401 ||
            originalRequest._retry ||
            originalRequest.url?.includes('/auth/refresh')
        ) {
            return Promise.reject(error);
        }

        if (isRefreshing) {
            return new Promise(function (resolve, reject) {
                failedQueue.push({ resolve, reject });
            })
                .then(token => {
                    originalRequest.headers.Authorization = `Bearer ${token}`;
                    return api(originalRequest);
                })
                .catch(err => Promise.reject(err));
        }

        originalRequest._retry = true;
        isRefreshing = true;

        try {
            const store = useAuthStore.getState();
            const user = store.user;

            if (!user?.token || !user?.refreshToken) {
                throw new Error("Нет токенов для обновления");
            }

            const response = await pureAxios.post('/auth/refresh', {
                token: user.token,
                refreshToken: user.refreshToken
            });

            const refreshedUser = response.data.data;

            store.setUser(refreshedUser);

            processQueue(null, refreshedUser.token);

            originalRequest.headers.Authorization = `Bearer ${refreshedUser.token}`;
            return api(originalRequest);

        } catch (refreshError) {
            processQueue(refreshError, null);
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

    const cleanPath = path.startsWith('/') ? path.substring(1) : path;
    const cleanBase = STATIC_URL.endsWith('/') ? STATIC_URL.slice(0, -1) : STATIC_URL;

    return `${cleanBase}/${cleanPath}`;
};
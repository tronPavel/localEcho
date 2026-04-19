import axios, { AxiosError, type InternalAxiosRequestConfig } from "axios";
import { useAuthStore } from "@/features/auth/model/authStore";
import { toast } from "sonner";

const API_URL = import.meta.env.VITE_API_URL || 'http://localhost:5015/api';
const STATIC_URL = import.meta.env.VITE_STATIC_URL || 'http://localhost:5015';

/**
 * Основной экземпляр для всех запросов
 */
export const api = axios.create({
    baseURL: API_URL,
    headers: { 'Content-Type': 'application/json' },
});

/**
 * Экземпляр БЕЗ интерцепторов (только для обновления токена)
 */
export const pureAxios = axios.create({
    baseURL: API_URL,
    headers: { 'Content-Type': 'application/json' },
});

let isRefreshing = false;
let failedQueue: Array<{ resolve: (value?: any) => void; reject: (reason?: any) => void }> = [];

const processQueue = (error: any, token: string | null = null) => {
    failedQueue.forEach(prom => {
        if (error) prom.reject(error);
        else prom.resolve(token);
    });
    failedQueue = [];
};

// 1. Добавление Token к каждому запросу
api.interceptors.request.use(
    (config: InternalAxiosRequestConfig) => {
        const token = useAuthStore.getState().user?.token;
        if (token) {
            config.headers.Authorization = `Bearer ${token}`;
        }
        return config;
    }
);

// 2. Обработка ответов и авто-обновление токена
api.interceptors.response.use(
    (response) => response,
    async (error: AxiosError) => {
        const originalRequest = error.config as InternalAxiosRequestConfig & { _retry?: boolean };

        if (!originalRequest) return Promise.reject(error);

        const status = error.response?.status;

        if (status === 401 && !originalRequest._retry && !originalRequest.url?.includes('/auth/refresh')) {
            if (isRefreshing) {
                return new Promise((resolve, reject) => {
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
                const { user, setUser } = useAuthStore.getState();

                if (!user?.refreshToken) throw new Error("Сессия истекла");

                // Используем чистый axios для обновления
                const response = await pureAxios.post('/auth/refresh', {
                    token: user.token,
                    refreshToken: user.refreshToken
                });

                const refreshedData = response.data.data;
                setUser(refreshedData);

                processQueue(null, refreshedData.token);
                originalRequest.headers.Authorization = `Bearer ${refreshedData.token}`;

                return api(originalRequest);
            } catch (refreshError) {
                processQueue(refreshError, null);
                useAuthStore.getState().clearUser();
                toast.error("Ваша сессия завершена. Войдите заново.");
                return Promise.reject(refreshError);
            } finally {
                isRefreshing = false;
            }
        }

        if (status === 403) {
            toast.error("Доступ запрещен: недостаточно прав для этого действия");
        }

        if (status && status >= 500) {
            toast.error("Проблема на стороне сервера. Мы уже работаем над этим.");
        }

        return Promise.reject(error);
    }
);

/**
 * Хелпер для получения корректной ссылки на изображение
 */
export const getImageUrl = (path?: string | null): string | undefined => {
    if (!path) return undefined;
    if (path.startsWith('http') || path.startsWith('blob:')) return path;

    const cleanPath = path.startsWith('/') ? path.substring(1) : path;
    const cleanBase = STATIC_URL.endsWith('/') ? STATIC_URL.slice(0, -1) : STATIC_URL;

    return `${cleanBase}/${cleanPath}`;
};
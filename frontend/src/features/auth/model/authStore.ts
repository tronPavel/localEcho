import { create } from 'zustand';
import { persist, createJSONStorage } from 'zustand/middleware';
import type { AuthResponseDto } from './types';
import { refreshToken } from './authApi';
import {useEffect} from "react";

interface AuthState {
    user: AuthResponseDto | null;
    isAuthenticated: boolean;

    setUser: (user: AuthResponseDto) => void;
    clearUser: () => void;
    refreshSession: () => Promise<boolean>;
}

export const useAuthStore = create<AuthState>()(
    persist(
        (set, get) => ({
            user: null,
            isAuthenticated: false,

            setUser: (user) => {
                localStorage.setItem('token', user.token);
                localStorage.setItem('refreshToken', user.refreshToken);
                set({ user, isAuthenticated: true });
            },

            clearUser: () => {
                localStorage.removeItem('token');
                localStorage.removeItem('refreshToken');
                set({ user: null, isAuthenticated: false });
            },

            refreshSession: async () => {
                const token = localStorage.getItem('token');
                const refresh = localStorage.getItem('refreshToken');
                if (!token || !refresh) return false;

                try {
                    const refreshedUser = await refreshToken(token, refresh);
                    get().setUser(refreshedUser);
                    return true;
                } catch (error) {
                    console.error('Refresh failed:', error);
                    get().clearUser();
                    return false;
                }
            },
        }),
        {
            name: 'auth-storage',
            storage: createJSONStorage(() => localStorage),
            partialize: (state) => ({ user: state.user }), // сохраняем только user
        }
    )
);

// Кастомный хук для начальной загрузки (остаётся)
export const useRestoreAuth = () => {
    const refreshSession = useAuthStore((state) => state.refreshSession);

    useEffect(() => {
        refreshSession();
    }, [refreshSession]);
};
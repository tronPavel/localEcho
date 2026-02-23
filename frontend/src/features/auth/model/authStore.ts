import { create } from 'zustand';
import type { AuthResponseDto } from './types';
import { refreshToken } from './authApi';
import { useEffect } from 'react';

interface AuthState {
    user: AuthResponseDto | null;
    isAuthenticated: boolean;
    setUser: (user: AuthResponseDto) => void;
    clearUser: () => void;
    restoreSession: () => Promise<void>;
}

export const useAuthStore = create<AuthState>((set) => ({
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
    restoreSession: async () => {
        const token = localStorage.getItem('token');
        const refresh = localStorage.getItem('refreshToken');
        if (token && refresh) {
            try {
                const refreshedUser = await refreshToken(token, refresh);
                set({ user: refreshedUser, isAuthenticated: true });
            } catch (error) {
                console.error('Session restore failed:', error);
                localStorage.removeItem('token');
                localStorage.removeItem('refreshToken');
                set({ user: null, isAuthenticated: false });
            }
        }
    },
}));

export const useRestoreAuth = () => {
    const restoreSession = useAuthStore((state) => state.restoreSession);

    useEffect(() => {
        restoreSession();
    }, [restoreSession]);
};
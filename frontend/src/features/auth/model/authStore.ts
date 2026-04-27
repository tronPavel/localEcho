import { create } from 'zustand';
import { persist, createJSONStorage } from 'zustand/middleware';
import type { AuthResponseDto } from './types';
import { pureAxios } from '@/shared/api/apiInstance';
import { useEffect } from "react";

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

            setUser: (user) => set({ user, isAuthenticated: true }),

            clearUser: () => set({ user: null, isAuthenticated: false }),

            refreshSession: async () => {
                const user = get().user;
                if (!user?.token || !user?.refreshToken) return false;

                try {
                    const response = await pureAxios.post('/auth/refresh', {
                        token: user.token,
                        refreshToken: user.refreshToken
                    });

                    get().setUser(response.data.data);
                    return true;
                } catch (error) {
                    console.error('Восстановление сессии не удалось:', error);
                    get().clearUser();
                    return false;
                }
            },
        }),
        {
            name: 'auth-storage',
            storage: createJSONStorage(() => localStorage),
            partialize: (state) => ({ user: state.user, isAuthenticated: state.isAuthenticated }),
        }
    )
);

export const useRestoreAuth = () => {
    const refreshSession = useAuthStore((state) => state.refreshSession);

    useEffect(() => {
        refreshSession();
    }, [refreshSession]);
};

export const usePermissions = () => {
    const { user, isAuthenticated } = useAuthStore();

    const roles = (user?.roles || []).map(r => r.toLowerCase());

    const isAdmin = roles.includes('admin');
    const isModerator = roles.includes('moderator');
    const isOfficial = roles.includes('official');

    return {
        isAuthenticated,
        isAdmin,
        isModerator,
        isOfficial,
        canAccessDashboard: isAdmin || isModerator || isOfficial,
        canDrawPolygons: isAdmin || isOfficial,
        canResolveMarkers: isAdmin || isOfficial,
        isOwner: (createdById: string) => user?.userId === createdById,
    };
};
import { api } from "@/shared/api/apiInstance";
import type { UserProfileDto } from "@/entities/user/model/types";

export const adminUserApi = {
    search: async (query: string): Promise<UserProfileDto[]> => {
        const response = await api.get('/admin/users/search', { params: { q: query } });
        return response.data;
    },
    changeRole: async (userId: string, role: string) => {
        await api.post(`/admin/users/${userId}/role`, JSON.stringify(role), {
            headers: { 'Content-Type': 'application/json' }
        });
    },
    removeRole: async (userId: string, role: string) => {
        await api.delete(`/admin/users/${userId}/role/${role}`);
    }
};
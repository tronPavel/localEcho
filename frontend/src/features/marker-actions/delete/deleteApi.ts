import { api } from "@/shared/api/apiInstance";

export const deleteApi = {
    deleteMarker: async (id: string) => {
        const response = await api.delete(`/markers/${id}`);
        return response.data;
    }
};
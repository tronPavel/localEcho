import { api } from "@/shared/api/apiInstance";
import  type{ MarkerDetailResponse, MarkerMapResponse } from "../model/types";

export const markerApi = {
    getForMap: async (params: any): Promise<MarkerMapResponse[]> => {
        const queryParams = Object.fromEntries(
            Object.entries(params).filter(([_, v]) => v != null && v !== '')
        );
        const response = await api.get('/markers', { params: queryParams });
        return response.data;
    },

    getDetails: async (id: string): Promise<MarkerDetailResponse> => {
        const response = await api.get(`/markers/${id}`);
        return response.data;
    }
};
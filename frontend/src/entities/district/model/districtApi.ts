import { api } from "@/shared/api/apiInstance";
import type {
    DistrictBriefDto,
    DistrictMapDto,
    DistrictDetailDto
} from "../model/types";
import type { Coordinate } from "@/entities/marker";

export const districtApi = {

    getList: async (): Promise<DistrictBriefDto[]> => {
        const response = await api.get('/districts');
        return response.data;
    },

    getForMap: async (): Promise<DistrictMapDto[]> => {
        const response = await api.get('/districts/map');
        return response.data;
    },

    getDetails: async (id: string): Promise<DistrictDetailDto> => {
        const response = await api.get(`/districts/${id}/details`);
        return response.data;
    },

    findByCoords: async (lat: number, lng: number): Promise<DistrictBriefDto> => {
        const response = await api.get('/districts/find', {
            params: { lat, lng }
        });
        return response.data;
    },


    admin: {
        create: async (data: { name: string; description: string; geometry: Coordinate[] }) => {
            const response = await api.post('/admin/districts', data);
            return response.data;
        },
        update: async (id: string, data: { name: string; description: string; geometry: Coordinate[] }) => {
            await api.put(`/admin/districts/${id}`, data);
        },
        delete: async (id: string) => {
            await api.delete(`/admin/districts/${id}`);
        }
    }
};
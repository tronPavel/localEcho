import { api } from "@/shared/api/apiInstance";
import type {Coordinate} from "@/entities/marker/model/types.ts";
import type {DistrictDto} from "@/features/auth/model/types.ts";

export interface DistrictMapDto {
    id: string;
    name: string;
    geometry: Coordinate[];
    centroid: Coordinate;
}
export interface DistrictBriefDto {
    id: string;
    name: string;
}
export const getDistrictsList = async (): Promise<DistrictBriefDto[]> => {
    const response = await api.get('/districts'); // На бэке это GetListAsync()
    return response.data;
};

export const getDistrictsForMap = async (): Promise<DistrictMapDto[]> => {
    const response = await api.get('/districts/map');
    return response.data;
};

export const getDistrictDetails = async (id: string) => {
    const response = await api.get(`/districts/${id}/details`);
    return response.data;
};
export const findDistrictByCoords = async (lat: number, lng: number): Promise<DistrictDto> => {
    const response = await api.get('/districts/find', {
        params: { lat, lng }
    });
    return response.data.data;
};
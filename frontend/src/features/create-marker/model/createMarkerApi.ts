import {api} from "../../../shared/api/apiInstance.ts";
import type {CreateMarkerDto, MarkerMapDto} from "../../../entities/marker/model/types.ts";

export const createMarker = async (data: CreateMarkerDto) => {
    await api.post('/markers', data);
};

interface GetMarkersParams {
    category?: string;
    status?: string;
    minLat?: number;
    maxLat?: number;
    minLng?: number;
    maxLng?: number;
    limit?: number; // <-- ДОБАВЛЕНО
}

export const getMarkers = async (params: GetMarkersParams = {}): Promise<MarkerMapDto[]> => {
    const queryParams = Object.fromEntries(
        Object.entries(params).filter(([_, v]) => v != null && v !== '')
    );

    const response = await api.get('/markers', { params: queryParams });
    return response.data;
};
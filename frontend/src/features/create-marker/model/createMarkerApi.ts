import {api} from "../../../shared/api/apiInstance.ts";
import type {CreateMarkerDto, MarkerMapDto} from "../../../entities/marker/model/types.ts";

export const createMarker = async (data: CreateMarkerDto) => {
    const formData = new FormData();
    formData.append('Title', data.title);
    formData.append('Latitude', data.latitude.toString());
    formData.append('Longitude', data.longitude.toString());
    formData.append('Category', data.category);
    if (data.description) formData.append('Description', data.description);

    if (data.imageFiles) {
        data.imageFiles.forEach(file => {
            formData.append('ImageFiles', file);
        });
    }

    await api.post('/markers', formData, {
        headers: { 'Content-Type': 'multipart/form-data' },
    });
};

interface GetMarkersParams {
    category?: string;
    status?: string;
    minLat?: number;
    maxLat?: number;
    minLng?: number;
    maxLng?: number;
    limit?: number;
}

export const getMarkers = async (params: GetMarkersParams = {}): Promise<MarkerMapDto[]> => {
    const queryParams = Object.fromEntries(
        Object.entries(params).filter(([_, v]) => v != null && v !== '')
    );

    const response = await api.get('/markers', { params: queryParams });
    return response.data;
};
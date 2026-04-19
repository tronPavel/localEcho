import {api} from "../../../shared/api/apiInstance.ts";
import type {CreateMarkerDto, MarkerMapDto} from "../../../entities/marker/model/types.ts";
export const createMarker = async (data: CreateMarkerDto) => {
    const formData = new FormData();
    formData.append('Title', data.title);
    formData.append('Category', data.category);
    if (data.description) formData.append('Description', data.description);
    if (data.scheduledAt) formData.append('ScheduledAt', data.scheduledAt);

    data.points.forEach((p, i) => {
        formData.append(`Points[${i}].Lat`, p.lat.toString());
        formData.append(`Points[${i}].Lng`, p.lng.toString());
    });

    if (data.imageFiles) {
        data.imageFiles.forEach(f => formData.append('ImageFiles', f));
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
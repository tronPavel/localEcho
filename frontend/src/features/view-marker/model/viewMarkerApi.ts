import type { MarkerDetailDto } from "../../../entities/marker/model/types.ts";
import { api } from "../../../shared/api/apiInstance.ts";

export const getMarkerDetails = async (id: string): Promise<MarkerDetailDto> => {
    const response = await api.get(`/markers/${id}`);
    return response.data;
};


export interface ChangeStatusRequest {
    markerId: string;
    newStatus: string;
    comment?: string;
    imageFiles?: File[];
}

export const changeMarkerStatus = async (data: ChangeStatusRequest) => {
    const formData = new FormData();
    formData.append('NewStatus', data.newStatus);
    if (data.comment) formData.append('Comment', data.comment);

    if (data.imageFiles) {
        data.imageFiles.forEach(file => formData.append('ImageFiles', file));
    }

    await api.patch(`/markers/${data.markerId}/status`, formData, {
        headers: { 'Content-Type': 'multipart/form-data' },
    });
};

export const deleteMarker = async (id: string): Promise<void> => {
    await api.delete(`/markers/${id}`);
};
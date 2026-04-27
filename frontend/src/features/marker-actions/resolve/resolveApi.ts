import { api } from "@/shared/api/apiInstance";

export interface ResolveMarkerRequest {
    markerId: string;
    newStatus: string;
    comment: string;
    imageFiles?: File[];
}

export const resolveApi = {
    submitResolution: async (data: ResolveMarkerRequest) => {
        const formData = new FormData();
        formData.append('NewStatus', data.newStatus);
        formData.append('Comment', data.comment);

        if (data.imageFiles) {
            data.imageFiles.forEach(file => {
                formData.append('ImageFiles', file);
            });
        }

        const response = await api.patch(`/markers/${data.markerId}/status`, formData, {
            headers: { 'Content-Type': 'multipart/form-data' },
        });
        return response.data;
    }
};
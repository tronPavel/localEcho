import { api } from "@/shared/api/apiInstance";
import type { CreateMarkerRequest } from "@/entities/marker";

export const createMarkerApi = {
    create: async (data: CreateMarkerRequest) => {
        const formData = new FormData();
        formData.append('Title', data.title);
        formData.append('Category', data.category);
        if (data.description) formData.append('Description', data.description);
        if (data.startDate) formData.append('StartDate', data.startDate);
        if (data.endDate) formData.append('EndDate', data.endDate);


        data.points.forEach((p, index) => {
            formData.append(`Points[${index}].Lat`, p.lat.toString());
            formData.append(`Points[${index}].Lng`, p.lng.toString());
        });

        if (data.imageFiles) {
            data.imageFiles.forEach(f => formData.append('ImageFiles', f));
        }

        const response = await api.post('/markers', formData, {
            headers: { 'Content-Type': 'multipart/form-data' },
        });
        return response.data;
    }
};
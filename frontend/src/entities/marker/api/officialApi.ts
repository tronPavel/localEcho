import { api } from "@/shared/api/apiInstance";
import type {MarkerCategory, MarkerStatus} from "../model/types";

export interface MarkerWorkItem {
    id: string;
    title: string;
    category: MarkerCategory;
    status: MarkerStatus;
    creatorName: string;
    districtId?: string;
    districtName?: string;
    createdAt: string;
    rating: number;
}

export const officialApi = {
    getTasks: async (districtId?: string): Promise<MarkerWorkItem[]> => {
        const response = await api.get('/official/queue', {
            params: { districtId, limit: 50 }
        });
        return response.data;
    }
};

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
export interface GetTasksParams {
    status?: string;
    category?: string;
    districtId?: string;
    cityId?: string;
}
export const officialApi = {
    getTasks: async (params: GetTasksParams = {}): Promise<MarkerWorkItem[]> => {
        const response = await api.get('/official/queue', { params });
        return response.data;
    }
};

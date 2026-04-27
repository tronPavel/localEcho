import { api } from "@/shared/api/apiInstance";
import type {GlobalAnalytics} from "@/entities/analytics/model/types.ts";

export const analyticsApi = {
    getCityPulse: async (): Promise<GlobalAnalytics> => {
        const response = await api.get('/statistics/city-pulse');
        return response.data.data;
    }
};
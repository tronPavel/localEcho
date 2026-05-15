import { api } from "@/shared/api/apiInstance";
import type {GlobalAnalytics} from "@/entities/analytics/model/types.ts";

export const analyticsApi = {
    getCityPulse: async (cityId?: string | null): Promise<GlobalAnalytics> => {
        const response = await api.get('/statistics/city-pulse', {
            params: { cityId }
        });
        return response.data.data;
    }
};
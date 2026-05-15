import { api } from "@/shared/api/apiInstance";
import type {CityBriefDto} from "../model/types";

export const cityApi = {
    getList: async (): Promise<CityBriefDto[]> => {
        const response = await api.get('/cities');
        return response.data;
    }
};
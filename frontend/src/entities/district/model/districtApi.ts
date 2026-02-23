import { api } from "@/shared/api/apiInstance";
import type { DistrictDto } from "@/features/auth/model/types";

export const getDistricts = async (): Promise<DistrictDto[]> => {
    const response = await api.get('/districts');
    return response.data.data;
};
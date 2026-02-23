import type { MarkerDetailDto } from "../../../entities/marker/model/types.ts";
import { api } from "../../../shared/api/apiInstance.ts";

export const getMarkerDetails = async (id: string): Promise<MarkerDetailDto> => {
    const response = await api.get(`/markers/${id}`);
    return response.data;
};
import { api } from '@/shared/api/apiInstance';
import type { LeaderboardEntryDto } from "@/features/auth/model/types";

export interface GetLeaderboardParams {
    cityId?: string;
    districtId?: string;
}

export const getLeaderboard = async (params: GetLeaderboardParams = {}): Promise<LeaderboardEntryDto[]> => {
    const response = await api.get('/leaderboard', { params });
    return response.data;
};
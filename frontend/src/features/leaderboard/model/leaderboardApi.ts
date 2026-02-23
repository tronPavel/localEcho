import { api } from '@/shared/api/apiInstance';
import type { LeaderboardEntryDto } from "@/features/auth/model/types";

export const getLeaderboard = async (districtId?: string): Promise<LeaderboardEntryDto[]> => {
    const response = await api.get('/leaderboard', {
        params: { districtId }
    });
    return response.data;
};
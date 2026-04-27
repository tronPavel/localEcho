import { api } from "@/shared/api/apiInstance";

export interface VoteRequest {
    markerId: string;
    isUpvote: boolean;
}

export const voteApi = {
    vote: async ({ markerId, isUpvote }: VoteRequest) => {
        const response = await api.post(`/markers/${markerId}/vote`, {
            isUpvote
        });

        return response.data;
    }
};
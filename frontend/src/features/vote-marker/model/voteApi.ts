import {api} from "../../../shared/api/apiInstance.ts";

export const vote = async (markerId: string, isUpvote: boolean) => {
    await api.post(`/markers/${markerId}/vote`, { isUpvote });
};
import {api} from "../../../shared/api/apiInstance.ts";

export interface CommentDto {
    id: string;
    text: string;
    user: { id: string; name: string; avatarUrl?: string };
    createdAt: string;
}

export const getComments = async (markerId: string): Promise<CommentDto[]> => {
    const response = await api.get(`/markers/${markerId}/comments`);
    return response.data;
};

export const addComment = async (markerId: string, text: string) => {
    await api.post(`/markers/${markerId}/comments`, { text });
};
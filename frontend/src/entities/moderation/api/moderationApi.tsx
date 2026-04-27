import { api } from "@/shared/api/apiInstance";

export interface ReportItem {
    id: string;
    markerId: string;
    markerTitle: string;
    reporterName: string;
    reason: string;
    comment: string;
    createdAt: string;
}

export const moderationApi = {
    getReports: async (): Promise<ReportItem[]> => {
        const response = await api.get('/moderation/reports');
        return response.data;
    },
    approveMarker: async (markerId: string) => {
        await api.post(`/moderation/markers/${markerId}/approve`);
    },
    deleteMarker: async (markerId: string) => {
        await api.delete(`/moderation/markers/${markerId}`);
    }
};
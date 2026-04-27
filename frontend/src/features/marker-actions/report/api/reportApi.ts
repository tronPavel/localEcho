import { api } from "@/shared/api/apiInstance";

export const ReportReason = {
    Spam: 0,
    Offense: 1,
    Inaccurate: 2,
    Fake: 3,
    Other: 4,
} as const;
export type ReportReason = typeof ReportReason[keyof typeof ReportReason];
export interface ReportRequest {
    markerId: string;
    reason: ReportReason;
    comment?: string;
}

export const reportApi = {
    sendReport: async (data: ReportRequest) => {
        const response = await api.post(`/markers/${data.markerId}/report`, {
            reason: data.reason,
            comment: data.comment
        });
        return response.data;
    }
};
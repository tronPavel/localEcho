export interface CategoryMetric {
    label: string;
    categoryKey: string;
    count: number;
}

export interface GlobalAnalytics {
    counters: {
        totalUsers: number;
        totalMarkers: number;
        totalActiveMarkers: number;
        pendingReports: number;
    };
    efficiency: {
        resolvedCount: number;
        inProgressCount: number;
        totalIssues: number;
        percentage: number;
    };
    categoryBreakdown: CategoryMetric[];
    topDistricts: Array<{ id: string; name: string; totalMarkers: number; successRate: number }>;
}
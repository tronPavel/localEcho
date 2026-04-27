import type { Coordinate } from "@/entities/marker";

export interface DistrictBriefDto {
    id: string;
    name: string;
}

export interface DistrictMapDto {
    id: string;
    name: string;
    geometry: Coordinate[];
    centroid: Coordinate;
}

export interface DistrictStatsDto {
    totalMarkers: number;
    residentsCount: number;
    resolvedIssuesCount: number;
    successRate: number;
    ongoingEventsCount: number;
    newSuggestionsCount: number;
    categoryCounts: Record<string, number>;
}

export interface DistrictDetailDto {
    id: string;
    name: string;
    description?: string;
    stats: DistrictStatsDto;
}
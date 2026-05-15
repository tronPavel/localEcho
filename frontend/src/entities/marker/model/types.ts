export type MarkerCategory = 'Issue' | 'Event' | 'Announcement' | 'Suggestion' | 'Project';
export type MarkerStatus =
    | 'Active' | 'InProgress' | 'Resolved'
    | 'Upcoming' | 'Ongoing' | 'Passed'
    | 'Current' | 'Archived'
    | 'Review' | 'Accepted' | 'Rejected';
export interface Coordinate {
    lat: number;
    lng: number;
}

export interface MarkerResolutionResponse {
    comment: string;
    authorName: string;
    createdAt: string;
    imageUrls: string[];
}

export interface MarkerMapResponse {
    id: string;
    title: string;
    category: MarkerCategory;
    status: string;
    geometryType: 'Point' | 'Polygon';
    coordinates: Coordinate[];
    centroid: Coordinate;
    sOfficial: boolean;
}

export interface MarkerDetailResponse {
    id: string;
    title: string;
    description?: string;
    imageUrls: string[];
    category: MarkerCategory;
    status: MarkerStatus;
    creatorId: string;
    creatorName: string;
    creatorAvatarUrl?: string;
    rating: number;
    userVote: number;
    createdAt: string;
    updatedAt?: string;
    geometryType: 'Point' | 'Polygon';
    coordinates: Coordinate[];
    resolutions?: MarkerResolutionResponse[];
    scheduledAt?: string;
    expiresAt?: string;
    isOfficial: boolean;
}

export interface CreateMarkerRequest {
    title: string;
    description?: string;
    category: MarkerCategory;
    points: Coordinate[];
    imageFiles?: File[];
    startDate?: string;
    endDate?: string;
}


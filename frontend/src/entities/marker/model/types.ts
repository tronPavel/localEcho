export interface MarkerMapDto {
    id: string;
    latitude: number;
    longitude: number;
    category: 'Issue' | 'Event' | 'Announcement';
    status: 'Active' | 'InProgress' | 'Resolved';
    title: string;
}

export interface MarkerDetailDto {
    id: string;
    title: string;
    description?: string;
    imageUrls?: string[];
    category: 'Issue' | 'Event' | 'Announcement';
    status: 'Active' | 'InProgress' | 'Resolved';

    creatorId: string;
    creatorName: string;
    creatorAvatarUrl?: string;

    rating: number;
    userVote: number;

    createdAt: string;
    updatedAt?: string;
}

export interface CreateMarkerDto {
    title: string;
    latitude: number;
    longitude: number;
    description?: string;
    category: 'Issue' | 'Event' | 'Announcement';
    imageFiles?: File[];
}
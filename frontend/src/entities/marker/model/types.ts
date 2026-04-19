export type MarkerCategory = 'Issue' | 'Event' | 'Announcement' | 'Suggestion' | 'Project';
export interface Coordinate {
    lat: number;
    lng: number;
}

export interface MarkerMapDto {
    id: string;
    title: string;
    category: 'Issue' | 'Event' | 'Announcement' | 'Suggestion' | 'Project';
    status: string;
    geometryType: 'Point' | 'Polygon'; // Приходит с бэкенда
    coordinates: Coordinate[];        // Массив точек фигуры
    centroid: Coordinate;              // Геометрический центр для иконки
}
export interface MarkerResolutionDto {
    comment: string;
    authorName: string;
    createdAt: string;
    imageUrls: string[];
}

export interface MarkerDetailDto {
    expiresAt?: string;
    scheduledAt?: string;
    id: string;
    title: string;
    description?: string;
    imageUrls: string[];
    category: MarkerCategory;
    status: string; // Зависит от категории (см. бэкенд)

    creatorId: string;
    creatorName: string;
    creatorAvatarUrl?: string;

    rating: number;
    userVote: number;

    createdAt: string;
    updatedAt?: string;

    geometryType: 'Point' | 'Polygon';
    coordinates: Coordinate[];

    resolution?: MarkerResolutionDto;
}

export interface CreateMarkerDto {
    title: string;
    description?: string;
    category: MarkerCategory;
    points: Coordinate[]; // МАССИВ вместо одиночных Lat/Lng
    imageFiles?: File[];
    scheduledAt?: string; // Для эвентов
}
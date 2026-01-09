export interface CreateMarkerDto {
    title: string;
    latitude: number;
    longitude: number;
    description?: string;
    category: string;
}
export interface MarkerDto {
    id: string;
    title: string;
    latitude: number;
    longitude: number;
    description?: string;
    category: string;
    status: string;
    createdAt: string;
    updatedAt?: string;
}
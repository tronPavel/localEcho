export interface UpdateProfileDto {
    name?: string;
    bio?: string;
    homeAddress?: string;
    cityId?: string;
    districtId?: string;
    avatarFile?: File;
}

export interface UserProfileDto {
    id: string;
    email: string;
    name: string;
    bio?: string;
    avatarUrl?: string;
    homeAddress?: string;
    points: number;
    createdAt: string;
    city?: { id: string; name: string };
    district?: { id: string; name: string };
    roles: string[];
    latitude?: number;
    longitude?: number;
}
export interface LoginDto {
    email: string;
    password: string;
}

export interface RegisterDto {
    email: string;
    password: string;
    confirmPassword: string;
    name: string;
    districtId: string;
    homeAddress?: string;
}

export interface RefreshTokenDto {
    token: string;
    refreshToken: string;
}

export interface AuthResponseDto {
    token: string;
    refreshToken: string;
    expires: string;
    userId: string;
    email: string;
    name: string;
    avatarUrl?: string;
    districtId?: string;
    districtName?: string;
    isVerified: boolean;
    points: number;
    roles: string[];
}

export interface UserProfileDto {
    id: string;
    email: string;
    name: string;
    avatarUrl?: string;
    homeAddress?: string;
    isVerified: boolean;
    points: number;
    lastSeen: string;
    createdAt: string;
    district?: DistrictDto;
    roles: string[];
}

export interface UpdateProfileDto {
    name?: string;
    homeAddress?: string;
}

export interface ChangeDistrictDto {
    districtId: string;
    homeAddress?: string;
}

export interface DistrictDto {
    id: string;
    name: string;
    description?: string;
    centerLat: number;
    centerLng: number;
    iconColor: string;
}

export interface LeaderboardEntryDto {
    id: string;
    name: string;
    avatarUrl?: string;
    points: number;
}
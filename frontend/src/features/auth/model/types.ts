export interface LoginDto {
    email: string;
    password: string;
}

export interface RegisterDto {
    email: string;
    password: string;
    confirmPassword: string;
    name: string;
    cityId: string;
    districtId?: string;
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
    latitude?: number;
    longitude?: number;
}

export interface LeaderboardEntryDto {
    id: string;
    name: string;
    avatarUrl?: string;
    points: number;
}
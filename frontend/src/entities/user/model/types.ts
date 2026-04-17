import type {DistrictDto} from "@/features/auth/model/types.ts";

export interface UserDto {
    id: string;
    name: string;
    email: string;
    avatarUrl?: string;
    districtId?: string;
    points: number;
    roles: string[];
}

export interface UpdateProfileDto {
    name?: string;
    homeAddress?: string;
    districtId?: string;
    avatarFile?: File;
}
export interface UserProfileDto {
    id: string;
    email: string;
    name: string;
    avatarUrl?: string;
    homeAddress?: string;
    isVerified: boolean;
    points: number;
    createdAt: string;
    district?: DistrictDto;
    roles: string[];
    latitude?: number;
    longitude?: number;
}
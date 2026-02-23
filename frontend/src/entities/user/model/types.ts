export interface UserDto {
    id: string;
    name: string;
    email: string;
    avatarUrl?: string;
    districtId?: string;
    points: number;
    roles: string[];
}
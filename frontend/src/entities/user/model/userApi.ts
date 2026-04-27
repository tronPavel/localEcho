import { api } from "@/shared/api/apiInstance";
import type {UpdateProfileDto, UserProfileDto} from "@/entities/user/model/types.ts";

export const getMyProfile = async (): Promise<UserProfileDto> => {
    const response = await api.get('/users/profile');
    return response.data.data;
};

export const updateProfile = async (data: UpdateProfileDto) => {
    const formData = new FormData();
    if (data.name) formData.append('Name', data.name);
    if (data.homeAddress) formData.append('HomeAddress', data.homeAddress);
    if (data.districtId) formData.append('DistrictId', data.districtId);
    if (data.avatarFile) formData.append('AvatarFile', data.avatarFile);

    await api.put('/users/profile', formData, {
        headers: { 'Content-Type': 'multipart/form-data' },
    });
};
export const searchUsers = async (query: string): Promise<UserProfileDto[]> => {
    const response = await api.get('/admin/users/search', { params: { q: query } });
    return response.data;
};

export const updateUserRole = async (userId: string, roleName: string) => {
    await api.post(`/admin/users/${userId}/role`, JSON.stringify(roleName), {
        headers: { 'Content-Type': 'application/json' }
    });
};
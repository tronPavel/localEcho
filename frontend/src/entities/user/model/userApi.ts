import { api } from "@/shared/api/apiInstance";
import type {UpdateProfileDto} from "@/entities/user/model/types.ts";
import type {UserProfileDto} from "@/features/auth/model/types.ts";

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

    // Вызываем один метод PUT /profile
    await api.put('/users/profile', formData, {
        headers: { 'Content-Type': 'multipart/form-data' },
    });
};
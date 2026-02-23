import { api } from "@/shared/api/apiInstance";
import type { UserProfileDto, UpdateProfileDto, ChangeDistrictDto } from "@/features/auth/model/types";

export const getMyProfile = async (): Promise<UserProfileDto> => {
    const response = await api.get('/users/profile');
    return response.data.data;
};

export const updateProfile = async (data: UpdateProfileDto) => {
    await api.put('/users/profile', data);
};

export const changeDistrict = async (data: ChangeDistrictDto) => {
    await api.post('/users/change-district', data);
};

export const uploadAvatar = async (file: File): Promise<string> => {
    const formData = new FormData();
    formData.append('file', file);

    const response = await api.post('/users/avatar', formData, {
        headers: { 'Content-Type': 'multipart/form-data' },
    });
    return response.data.avatarUrl;
};
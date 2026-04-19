import { api } from "@/shared/api/apiInstance";
import type { AuthResponseDto, LoginDto, RegisterDto } from "./types";

export const login = async (data: LoginDto): Promise<AuthResponseDto> => {
    const response = await api.post('/auth/login', data);

    return response.data.data;
};

export const register = async (data: RegisterDto): Promise<AuthResponseDto> => {
    const response = await api.post('/auth/register', data);
    return response.data.data;
};

export const refreshToken = async (token: string, refreshToken: string): Promise<AuthResponseDto> => {
    const response = await api.post('/auth/refresh', { token, refreshToken });
    return response.data.data;
};

export const logout = async () => {
    await api.post('/auth/logout');
};

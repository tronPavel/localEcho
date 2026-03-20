import { api } from '@/shared/api/apiInstance';

export const uploadFile = async (file: File): Promise<string> => {
    const formData = new FormData();
    formData.append('file', file);
    const response = await api.post('/files/upload', formData, {
        headers: { 'Content-Type': 'multipart/form-data' },
    });
    return response.data.url;
};

export const deleteFile = async (url: string): Promise<void> => {
    await api.delete('/files/delete', { data: { url } });
};
import { useMutation, useQueryClient } from '@tanstack/react-query';
import { toast } from 'sonner';
import { useNavigate } from 'react-router-dom';
import {deleteApi} from "@/features/marker-actions/delete/deleteApi.ts";

export const useDeleteMarker = () => {
    const queryClient = useQueryClient();
    const navigate = useNavigate();

    return useMutation({
        mutationFn: deleteApi.deleteMarker,
        onSuccess: () => {
            queryClient.invalidateQueries({ queryKey: ['markers'] });
            toast.warning("Метка удалена");
            navigate('/');
        },
        onError: (err: any) => {
            toast.error(err.response?.data?.detail || "Ошибка при удалении");
        }
    });
};
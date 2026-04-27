import { useMutation, useQueryClient } from '@tanstack/react-query';
import { toast } from 'sonner';
import {resolveApi, type ResolveMarkerRequest} from "@/features/marker-actions/resolve/resolveApi.ts";

export const useResolveMarker = (markerId: string) => {
    const queryClient = useQueryClient();

    return useMutation({
        mutationFn: (data: Omit<ResolveMarkerRequest, 'markerId'>) =>
            resolveApi.submitResolution({ ...data, markerId }),
        onSuccess: () => {
            queryClient.invalidateQueries({ queryKey: ['marker', markerId] });
            queryClient.invalidateQueries({ queryKey: ['markers'] });

            toast.success("Обновление опубликовано", {
                description: "Ваш ответ добавлен в историю изменений метки.",
                icon: '⚖️'
            });
        },
        onError: (err: any) => {
            const detail = err.response?.data?.detail || "Ошибка при смене статуса";
            toast.error("Действие отклонено", { description: detail });
        }
    });
};
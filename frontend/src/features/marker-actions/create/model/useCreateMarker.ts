import { useMutation, useQueryClient } from '@tanstack/react-query';
import { toast } from 'sonner';
import {createMarkerApi} from "@/features/marker-actions/create/createMarkerApi.ts";
import {useMapInteractionStore} from "@/features/map-control/model/interactionStore.ts"; // используем общий стор

export const useCreateMarker = () => {
    const queryClient = useQueryClient();
    const clearMap = useMapInteractionStore(s => s.clear);

    return useMutation({
        mutationFn: createMarkerApi.create,
        onSuccess: () => {
            queryClient.invalidateQueries({ queryKey: ['markers'] });
            toast.success("Метка добавлена", {
                description: "Событие успешно опубликовано на карте",
            });
            clearMap();
        },
        onError: (err: any) => {
            const msg = err.response?.data?.detail || "Ошибка при создании";
            toast.error("Не удалось создать", { description: msg });
        }
    });
};
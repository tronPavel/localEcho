import { useMutation, useQueryClient } from '@tanstack/react-query';
import { toast } from 'sonner';
import {voteApi} from "@/features/marker-actions/vote/voteApi.ts";

export const useVote = (markerId: string) => {
    const queryClient = useQueryClient();

    return useMutation({
        mutationFn: (isUpvote: boolean) => voteApi.vote({ markerId, isUpvote }),
        onSuccess: () => {
            queryClient.invalidateQueries({ queryKey: ['marker', markerId] });
            queryClient.invalidateQueries({ queryKey: ['markers'] });
        },
        onError: (err: any) => {
            toast.error(err.response?.data?.detail || "Ошибка голосования");
        }
    });
};
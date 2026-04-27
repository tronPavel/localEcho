import { useMutation } from '@tanstack/react-query';
import { toast } from 'sonner';
import { reportApi, type ReportRequest } from '../api/reportApi';

export const useReport = (markerId: string) => {
    return useMutation({
        mutationFn: (data: Omit<ReportRequest, 'markerId'>) =>
            reportApi.sendReport({ ...data, markerId }),
        onSuccess: () => {
            toast.success("Жалоба отправлена", {
                description: "Модераторы проверят эту метку в ближайшее время."
            });
        },
        onError: (err: any) => {
            toast.error(err.response?.data?.detail || "Не удалось отправить жалобу");
        }
    });
};
import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { moderationApi } from '@/entities/moderation/api/moderationApi';
import { Button } from '@/shared/ui/Button/Button';
import { formatDate } from '@/shared/lib/utils/formatDate';
import { useNavigate } from 'react-router-dom';
import { toast } from 'sonner';
import styles from './ModerationList.module.css';

export const ModerationList = () => {
    const queryClient = useQueryClient();
    const navigate = useNavigate();

    const { data: reports = [], isLoading } = useQuery({
        queryKey: ['moderation-reports'],
        queryFn: moderationApi.getReports,
    });

    const approve = useMutation({
        mutationFn: moderationApi.approveMarker,
        onSuccess: () => {
            queryClient.invalidateQueries({ queryKey: ['moderation-reports'] });
            toast.success("Метка одобрена");
        }
    });

    const remove = useMutation({
        mutationFn: moderationApi.deleteMarker,
        onSuccess: () => {
            queryClient.invalidateQueries({ queryKey: ['moderation-reports'] });
            toast.error("Контент удален");
        }
    });

    if (isLoading) return <div>Загрузка очереди...</div>;
    if (reports.length === 0) return <div className={styles.empty}>Жалоб нет. Город спит спокойно</div>;

    return (
        <div className={styles.container}>
            {reports.map(r => (
                <div key={r.id} className={styles.card}>
                    <div className={styles.content}>
                        <div className={styles.header}>
                            <span className={styles.target}>Метка: «{r.markerTitle}»</span>
                            <span className={styles.date}>{formatDate(r.createdAt)}</span>
                        </div>
                        <p className={styles.reason}>
                            <b>Причина:</b> {r.comment || 'Нарушение правил'}
                        </p>
                        <div className={styles.meta}>
                            Автор жалобы: <b>{r.reporterName}</b>
                        </div>
                    </div>
                    <div className={styles.actions}>
                        <Button variant="outline" size="small" onClick={() => navigate(`/marker/${r.markerId}`)}>
                            🔎 Проверить
                        </Button>
                        <Button variant="primary" size="small" onClick={() => approve.mutate(r.markerId)}>
                            ✅ Оставить
                        </Button>
                        <Button variant="secondary" size="small" onClick={() => remove.mutate(r.markerId)}>
                            🗑 Удалить
                        </Button>
                    </div>
                </div>
            ))}
        </div>
    );
};
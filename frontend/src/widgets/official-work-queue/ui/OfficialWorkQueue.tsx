import { useQuery } from '@tanstack/react-query';
import { getStatusLabel } from '@/entities/marker';
import { formatDate } from '@/shared/lib/utils/formatDate';
import { useNavigate } from 'react-router-dom';
import styles from './OfficialWorkQueue.module.css';
import {officialApi} from "@/entities/marker/api/officialApi.ts";

export const OfficialWorkQueue = () => {
    const navigate = useNavigate();
    const { data: tasks = [], isLoading } = useQuery({
        queryKey: ['official-tasks'],
        queryFn: () => officialApi.getTasks(),
    });

    if (isLoading) return <div>Загрузка задач...</div>;

    return (
        <div className={styles.container}>
            <table className={styles.table}>
                <thead>
                <tr>
                    <th>Тип</th>
                    <th>Заголовок</th>
                    <th>Рейтинг</th>
                    <th>Создано</th>
                    <th>Статус</th>
                </tr>
                </thead>
                <tbody>
                {tasks.map(t => (
                    <tr key={t.id} onClick={() => navigate(`/marker/${t.id}`)} className={styles.row}>
                        <td>{t.category === 'Issue' ? '⚠️' : '💡'}</td>
                        <td className={styles.title}>{t.title}</td>
                        <td><span className={styles.rating}>{t.rating} pts</span></td>
                        <td className={styles.date}>{formatDate(t.createdAt)}</td>
                        <td>
                                <span className={styles.statusBadge}>
                                    {getStatusLabel(t.status as any)}
                                </span>
                        </td>
                    </tr>
                ))}
                </tbody>
            </table>
        </div>
    );
};
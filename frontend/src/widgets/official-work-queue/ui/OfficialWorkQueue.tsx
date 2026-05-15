import { useQuery } from '@tanstack/react-query';
import { getStatusLabel } from '@/entities/marker';
import { formatDate } from '@/shared/lib/utils/formatDate';
import { useNavigate } from 'react-router-dom';
import styles from './OfficialWorkQueue.module.css';
import {officialApi} from "@/entities/marker/api/officialApi.ts";
import {useState} from "react";
import { Select } from "@/shared/ui/Select/Select";
import {useCityStore} from "@/features/city-selector/model/cityStore.ts";

export const OfficialWorkQueue = () => {
    const navigate = useNavigate();
    const [status, setStatus] = useState<string>('');
    const [category, setCategory] = useState<string>('');
    const { currentCityId } = useCityStore();
    const { data: tasks = [], isLoading } = useQuery({
        queryKey: ['official-tasks', currentCityId, status, category],
        queryFn: () => officialApi.getTasks({
            cityId: currentCityId || undefined,
            status: status || undefined,
            category: category || undefined
        }),
    });

    if (isLoading) return <div>Загрузка задач...</div>;

    return (
        <div className={styles.container}>
            <header className={styles.header}>
                <h2>Очередь: {useCityStore.getState().currentCityName}</h2>
            </header>
            <div className={styles.filterBar}>
                <div className={styles.filterGroup}>
                    <Select label="Статус" value={status} onChange={e => setStatus(e.target.value)}>
                        <option value="">Все статусы</option>
                        <option value="Active">Новые (Active)</option>
                        <option value="InProgress">В работе</option>
                    </Select>
                </div>
                <div className={styles.filterGroup}>
                    <Select label="Категория" value={category} onChange={e => setCategory(e.target.value)}>
                        <option value="">Все категории</option>
                        <option value="Issue">⚠️ Проблемы</option>
                        <option value="Suggestion">💡 Предложения</option>
                    </Select>
                </div>
            </div>
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
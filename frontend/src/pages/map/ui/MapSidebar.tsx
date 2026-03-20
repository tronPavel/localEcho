import { Select } from '@/shared/ui/Select/Select.tsx';
import { useFilterStore } from '@/features/filter-markers/model/filterStore.ts';
import styles from './MapPage.module.css';

export const MapSidebar = () => {
    const { category, status, setCategory, setStatus } = useFilterStore();

    return (
        <aside className={styles.sidebar}>
            <h3>Фильтры</h3>
            <div className={styles.filterGroup}>
                <label>Категория</label>
                <Select value={category || ''} onChange={(e) => setCategory(e.target.value || null)}>
                    <option value="">Все категории</option>
                    <option value="Issue">Проблемы</option>
                    <option value="Event">Мероприятия</option>
                    <option value="Announcement">Объявления</option>
                </Select>
            </div>
            <div className={styles.filterGroup}>
                <label>Статус</label>
                <Select value={status || ''} onChange={(e) => setStatus(e.target.value || null)}>
                    <option value="">Все статусы</option>
                    <option value="Active">Активные</option>
                    <option value="InProgress">В работе</option>
                    <option value="Resolved">Решены</option>
                </Select>
            </div>
        </aside>
    );
};
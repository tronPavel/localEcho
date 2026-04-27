import { useFilterStore } from '../model/filterStore';
import { CATEGORY_STATUSES, getStatusLabel } from '@/entities/marker';
import { classNames } from '@/shared/lib/utils/classNames';
import styles from './StatusFilters.module.css';

export const StatusFilters = () => {
    const { category, status, setStatus } = useFilterStore();

    // Если категория не выбрана, список статусов не имеет смысла
    if (!category) return null;

    const availableStatuses = CATEGORY_STATUSES[category as keyof typeof CATEGORY_STATUSES] || [];

    return (
        <div className={styles.filterList}>
            {availableStatuses.map(s => (
                <div
                    key={s}
                    className={classNames(styles.filterItem, status === s && styles.active)}
                    onClick={() => setStatus(status === s ? null : s)}
                >
                    <span className={styles.statusText}>{getStatusLabel(s)}</span>
                    {/* Точка-индикатор цвета статуса */}
                    <div className={classNames(styles.dot, styles[s.toLowerCase()])} />
                </div>
            ))}
        </div>
    );
};
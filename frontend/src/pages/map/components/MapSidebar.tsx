import { useFilterStore } from '@/features/filter-markers/model/filterStore';
import { CATEGORY_STATUSES, getStatusLabel } from '@/entities/marker/lib/statusHelper';
import { classNames } from '@/shared/lib/utils/classNames';
import styles from './MapSidebar.module.css';

export const MapSidebar = () => {
    const { category, setCategory, status, setStatus } = useFilterStore();

    const categories = [
        { id: 'Issue', label: 'Проблемы ЖКХ', icon: '⚠️' },
        { id: 'Event', label: 'Мероприятия', icon: '🎉' },
        { id: 'Announcement', label: 'Объявления', icon: '📢' },
        { id: 'Suggestion', label: 'Предложения', icon: '💡' },
        { id: 'Project', label: 'Проекты города', icon: '🏗' },
    ];

    const availableStatuses = category
        ? CATEGORY_STATUSES[category as keyof typeof CATEGORY_STATUSES]
        : [];

    return (
        <aside className={styles.sidebar}>
            <div className={styles.group}>
                <h4 className={styles.groupTitle}>Тип события</h4>
                <div className={styles.filterList}>
                    <div
                        className={classNames(styles.filterItem, !category && styles.active)}
                        onClick={() => { setCategory(null); setStatus(null); }}
                    >
                        <span>🌏 Все записи</span>
                    </div>
                    {categories.map(c => (
                        <div
                            key={c.id}
                            className={classNames(styles.filterItem, category === c.id && styles.active)}
                            onClick={() => { setCategory(c.id); setStatus(null); }}
                        >
                            <span>{c.icon} {c.label}</span>
                            {category === c.id && <div className={styles.indicator} />}
                        </div>
                    ))}
                </div>
            </div>

            {category && (
                <div className={styles.group}>
                    <h4 className={styles.groupTitle}>Стадия решения</h4>
                    <div className={styles.filterList}>
                        {availableStatuses.map(s => (
                            <div
                                key={s}
                                className={classNames(styles.filterItem, status === s && styles.active)}
                                onClick={() => setStatus(status === s ? null : s)}
                            >
                                <span className={styles.statusText}>{getStatusLabel(s)}</span>
                                <div className={classNames(styles.dot, styles[s.toLowerCase()])} />
                            </div>
                        ))}
                    </div>
                </div>
            )}
        </aside>
    );
};
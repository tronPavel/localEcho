import { useFilterStore } from '../model/filterStore';
import { classNames } from '@/shared/lib/utils/classNames';
import styles from './CategoryFilters.module.css';

const CATEGORIES = [
    { id: 'Issue', label: 'Проблемы ЖКХ', icon: '⚠️' },
    { id: 'Event', label: 'Мероприятия', icon: '🎉' },
    { id: 'Announcement', label: 'Объявления', icon: '📢' },
    { id: 'Suggestion', label: 'Предложения', icon: '💡' },
    { id: 'Project', label: 'Проекты города', icon: '🏗' },
] as const;

export const CategoryFilters = () => {
    const { category, setCategory, setStatus } = useFilterStore();

    const handleSelect = (id: string | null) => {
        setCategory(id);
        setStatus(null);
    };

    return (
        <div className={styles.filterList}>
            <div
                className={classNames(styles.filterItem, !category && styles.active)}
                onClick={() => handleSelect(null)}
            >
                <span>🌏 Все записи</span>
            </div>
            {CATEGORIES.map(c => (
                <div
                    key={c.id}
                    className={classNames(styles.filterItem, category === c.id && styles.active)}
                    onClick={() => handleSelect(c.id)}
                >
                    <span>{c.icon} {c.label}</span>
                    {category === c.id && <div className={styles.indicator} />}
                </div>
            ))}
        </div>
    );
};
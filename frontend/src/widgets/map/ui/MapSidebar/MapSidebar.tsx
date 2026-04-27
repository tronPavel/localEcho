import { CategoryFilters, StatusFilters } from '@/features/filter-markers';
import styles from './MapSidebar.module.css';

export const MapSidebar = () => {
    return (
        <aside className={styles.sidebar}>
            <section className={styles.group}>
                <h4 className={styles.groupTitle}>Тип события</h4>
                <CategoryFilters />
            </section>

            <section className={styles.group}>
                <StatusFilters />
            </section>
        </aside>
    );
};
import styles from './StatusTimeline.module.css';
import {CATEGORY_STATUSES, getStatusLabel} from "@/entities/marker/lib/statusHelper.ts";

export const StatusTimeline = ({ category, currentStatus }: { category: any, currentStatus: string }) => {
    const statuses = CATEGORY_STATUSES[category] || [];
    const currentIndex = statuses.indexOf(currentStatus);

    return (
        <div className={styles.timeline}>
            {statuses.map((s, idx) => {
                const isPast = idx < currentIndex;
                const isCurrent = idx === currentIndex;

                return (
                    <div key={s} className={`${styles.step} ${isPast ? styles.past : ''} ${isCurrent ? styles.active : ''}`}>
                        <div className={styles.dot} />
                        <span className={styles.label}>{getStatusLabel(s)}</span>
                        {idx !== statuses.length - 1 && <div className={styles.line} />}
                    </div>
                );
            })}
        </div>
    );
};
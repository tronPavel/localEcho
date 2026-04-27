import { CATEGORY_STATUSES, getStatusLabel } from '@/entities/marker';
import styles from './StatusTimeline.module.css';
import type {MarkerCategory, MarkerStatus} from "@/entities/marker";
import {classNames} from "@/shared/lib/utils/classNames.ts";

interface StatusTimelineProps {
    category: MarkerCategory;
    currentStatus: MarkerStatus;
}
export const StatusTimeline = ({ category, currentStatus }: StatusTimelineProps) => {
    const statuses = CATEGORY_STATUSES[category] || [];
    const activeIndex = statuses.indexOf(currentStatus);

    return (
        <div className={styles.timeline}>
            {statuses.map((s, index) => {
                const isCurrent = index === activeIndex;
                const isPast = index < activeIndex;

                return (
                    <div
                        key={s}
                        className={classNames(
                            styles.step,
                            isCurrent && styles.active,
                            isPast && styles.past
                        )}
                    >
                        <div className={styles.dot}>
                            {isPast && <span className={styles.check}>✓</span>}
                        </div>
                        <span className={styles.label}>{getStatusLabel(s)}</span>
                        {index < statuses.length - 1 && (
                            <div className={classNames(styles.line, isPast && styles.linePast)} />
                        )}
                    </div>
                );
            })}
        </div>
    );
};
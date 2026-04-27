import { useQuery } from '@tanstack/react-query';
import { analyticsApi } from '@/entities/analytics/api/analyticsApi';
import styles from './AnalyticsWidget.module.css';
import type {CategoryMetric} from "@/entities/analytics/model/types.ts";

export const AnalyticsWidget = () => {
    const { data: stats, isLoading } = useQuery({
        queryKey: ['city-pulse'],
        queryFn: analyticsApi.getCityPulse,
    });

    if (isLoading) return <div className={styles.loader}>Собираем данные по Минску...</div>;
    if (!stats) return null;

    return (
        <div className={styles.wrapper}>
            <div className={styles.heroGrid}>
                <div className={styles.statCard}>
                    <span className={styles.label}>Жителей в системе</span>
                    <h2 className={styles.value}>{stats.counters.totalUsers}</h2>
                </div>
                <div className={styles.statCard}>
                    <span className={styles.label}>Всего событий</span>
                    <h2 className={styles.value}>{stats.counters.totalMarkers}</h2>
                </div>
                <div className={styles.statCard}>
                    <span className={styles.label}>Жалоб на проверке</span>
                    <h2 className={`${styles.value} ${styles.warn}`}>{stats.counters.pendingReports}</h2>
                </div>
            </div>

            <section className={styles.section}>
                <div className={styles.sectionHeader}>
                    <h3>Эффективность служб ЖКХ</h3>
                    <span className={styles.totalBadge}>{stats.efficiency.totalIssues} проблем</span>
                </div>
                <div className={styles.efficiencyCard}>
                    <div className={styles.progressInfo}>
                        <span>{Math.round(stats.efficiency.percentage)}% проблем успешно решено</span>
                    </div>
                    <div className={styles.track}>
                        <div
                            className={styles.fill}
                            style={{ width: `${stats.efficiency.percentage}%` }}
                        />
                    </div>
                    <div className={styles.miniStats}>
                        <span>В работе: <b>{stats.efficiency.inProgressCount}</b></span>
                        <span>Решено: <b>{stats.efficiency.resolvedCount}</b></span>
                    </div>
                </div>
            </section>

            <div className={styles.mainGrid}>
                <div className={styles.subSection}>
                    <h4>Распределение событий</h4>
                    <div className={styles.categoryList}>
                        {stats.categoryBreakdown.map((cat: CategoryMetric) => (
                            <div key={cat.label} className={styles.catRow}>
                                <span className={styles.catLabel}>{cat.label}</span>
                                <div className={styles.catTrack}>
                                    <div
                                        className={styles.catFill}
                                        style={{ width: `${(cat.count / stats.counters.totalMarkers) * 100}%` }}
                                    />
                                </div>
                                <span className={styles.catCount}>{cat.count}</span>
                            </div>
                        ))}
                    </div>
                </div>

                <div className={styles.subSection}>
                    <h4>Топ районов</h4>
                    <div className={styles.districtList}>
                        {stats.topDistricts.map((d, index) => (
                            <div key={d.id} className={styles.districtRow}>
                                <span className={styles.rank}>#{index + 1}</span>
                                <span className={styles.distName}>{d.name}</span>
                                <span className={styles.distScore}>{Math.round(d.successRate)}%</span>
                            </div>
                        ))}
                    </div>
                </div>
            </div>
        </div>
    );
};
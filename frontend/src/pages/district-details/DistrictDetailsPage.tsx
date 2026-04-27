import { useParams } from 'react-router-dom';
import { useQuery } from '@tanstack/react-query';
import { RoutedModal } from '@/shared/ui/Modal/RoutedModal';
import styles from './DistrictDetails.module.css';
import {districtApi} from "@/entities/district/model/districtApi.ts";

export const DistrictDetailsPage = () => {
    const { id } = useParams();

    const { data: district, isLoading } = useQuery({
        queryKey: ['district-details', id],
        queryFn: () => districtApi.getDetails(id!),
        enabled: !!id
    });

    return (
        <RoutedModal title={`Район: ${district?.name || '...'}`}>
            {isLoading ? (
                <div className={styles.loading}>Анализируем данные территории...</div>
            ) : district ? (
                <div className={styles.stats}>
                    <div className={styles.mainInfo}>
                        <p>{district.description || 'Описание района пока не добавлено администрацией.'}</p>
                    </div>

                    <div className={styles.grid}>
                        <div className={styles.statItem}>
                            <span>Всего событий</span>
                            <strong>{district.stats.totalMarkers}</strong>
                        </div>
                        <div className={styles.statItem}>
                            <span>Жителей в системе</span>
                            <strong>{district.stats.residentsCount}</strong>
                        </div>
                        <div className={styles.statItem}>
                            <span>Эффективность ЖКХ</span>
                            <strong>{district.stats.successRate}%</strong>
                        </div>
                    </div>

                    {district?.stats?.categoryCounts && Object.keys(district.stats.categoryCounts).length > 0 ? (
                        <div className={styles.categorySection}>
                            <h4>Активность по типам:</h4>
                            <div className={styles.categoryList}>
                                {Object.entries(district.stats.categoryCounts).map(([name, count]) => (
                                    <div key={name} className={styles.categoryRow}>
                                        <span>{name}</span>
                                        <b>{count}</b>
                                    </div>
                                ))}
                            </div>
                        </div>
                    ) : (
                        <p className={styles.noData}>В этом районе еще нет зафиксированных событий.</p>
                    )}

                </div>
            ) : (
                <div className={styles.error}>Данные района недоступны</div>
            )}
        </RoutedModal>
    );
};
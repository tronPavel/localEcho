import { useParams } from 'react-router-dom';
import { useQuery } from '@tanstack/react-query';
import { RoutedModal } from '@/shared/ui/Modal/RoutedModal';
import styles from './DistrictDetails.module.css';
import {getDistrictDetails} from "@/entities/district/model/districtApi.ts";

export const DistrictDetailsPage = () => {
    const { id } = useParams();

    const { data: district, isLoading } = useQuery({
        queryKey: ['district-details', id],
        queryFn: () => getDistrictDetails(id!),
        enabled: !!id
    });

    return (
        <RoutedModal title={`Район: ${district?.name || '...'}`}>
            {isLoading ? (
                <div>Загрузка статистики...</div>
            ) : (
                <div className={styles.stats}>
                    <div className={styles.mainInfo}>
                        <p>{district.description}</p>
                    </div>

                    <div className={styles.grid}>
                        <div className={styles.statItem}>
                            <span>Всего меток:</span>
                            <strong>{district.stats.totalMarkers}</strong>
                        </div>
                        <div className={styles.statItem}>
                            <span>Жителей здесь:</span>
                            <strong>{district.stats.residentsCount}</strong>
                        </div>
                        <div className={styles.statItem}>
                            <span>Успешность (ЖКХ):</span>
                            <strong>{district.stats.successRate}%</strong>
                        </div>
                    </div>

                    <h4>Активист месяца:</h4>
                    <div className={styles.leaderSection}>
                        <p className={styles.leader}>{district.stats.topAktivistName || 'Пока нет данных'}</p>
                    </div>
                </div>
            )}
        </RoutedModal>
    );
};
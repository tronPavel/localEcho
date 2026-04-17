import { useQuery } from '@tanstack/react-query';
import { Swiper, SwiperSlide } from 'swiper/react';
import { Navigation, Pagination } from 'swiper/modules';

// Стили Swiper
import 'swiper/css';
import 'swiper/css/navigation';
import 'swiper/css/pagination';

import { getMarkerDetails } from '../model/viewMarkerApi';
import { VoteButtons } from '../../vote-marker/ui/VoteButtons';
import { formatDate } from '../lib/formatDate';
import { getImageUrl } from "@/shared/api/apiInstance";

// Важно: теперь используем общие стили из CSS-модуля
import styles from './MarkerDetailsFeature.module.css';
import {UserAvatar} from "@/entities/user/ui/UserAvatar.tsx";

interface MarkerDetailsFeatureProps {
    id: string;
}

export const MarkerDetailsFeature = ({ id }: MarkerDetailsFeatureProps) => {
    const { data: marker, isLoading, isError } = useQuery({
        queryKey: ['marker', id],
        queryFn: () => getMarkerDetails(id),
        enabled: !!id,
    });

    if (isLoading) return <div className={styles.loading}>Загрузка информации...</div>;

    if (isError || !marker) return <div className={styles.error}>Ошибка загрузки данных или метка не найдена</div>;

    const hasImages = marker.imageUrls && marker.imageUrls.length > 0;

    return (
        <div className={styles.container}>
            {/* 1. БЛОК ИЗОБРАЖЕНИЙ (Слайдер) */}
            {hasImages ? (
                <Swiper
                    modules={[Navigation, Pagination]}
                    navigation={marker.imageUrls.length > 1}
                    pagination={{ clickable: true }}
                    slidesPerView={1}
                    className={styles.slider}
                >
                    {marker.imageUrls.map((url, i) => (
                        <SwiperSlide key={url || i}>
                            <img
                                src={getImageUrl(url)}
                                alt={marker.title}
                                className={styles.slideImage}
                            />
                        </SwiperSlide>
                    ))}
                </Swiper>
            ) : (
                <div className={styles.noImage}>Фото отсутствует</div>
            )}

            {/* 2. ШАПКА И СТАТУС */}
            <div className={styles.header}>
                <h2 className={styles.title}>{marker.title}</h2>
                <span className={`${styles.status} ${styles[marker.status.toLowerCase()]}`}>
                    {marker.status === 'Active' && 'Активно'}
                    {marker.status === 'InProgress' && 'В работе'}
                    {marker.status === 'Resolved' && 'Решено'}
                </span>
            </div>

            {/* 3. ОПИСАНИЕ */}
            {marker.description && (
                <p className={styles.description}>{marker.description}</p>
            )}

            {/* 4. МЕТА-ИНФОРМАЦИЯ (Дата и Автор) */}
            <div className={styles.meta}>
                <UserAvatar
                    user={{
                        name: marker.creatorName,
                        avatarUrl: marker.creatorAvatarUrl
                    }}
                    size="small" // В карточке маркера используем маленький размер
                />
                <span className={styles.authorName}>
        {marker.creatorName || 'Аноним'}
    </span>
                <div className={styles.date}>
                    Создано: {formatDate(marker.createdAt)}
                </div>
            </div>

            {/* 5. ГОЛОСОВАНИЕ */}
            <div className={styles.voteSection}>
                <VoteButtons
                    markerId={marker.id}
                    currentVote={marker.userVote}
                    rating={marker.rating}
                />
            </div>

            {/* 6. КОММЕНТАРИИ (Плейсхолдер) */}
            <div className={styles.commentsSection}>
                <h3 className={styles.commentsTitle}>Комментарии</h3>
                <div className={styles.placeholderBox}>
                    💬 Обсуждение будет доступно в ближайших обновлениях
                </div>
            </div>
        </div>
    );
};
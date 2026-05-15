import { useQuery } from '@tanstack/react-query';
import { useNavigate } from 'react-router-dom';

import { markerApi, StatusTimeline } from '@/entities/marker';
import { VoteButtons } from '@/features/marker-actions/vote';
import { DeleteMarkerButton } from '@/features/marker-actions/delete/ui/DeleteMarkerButton';
import { usePermissions } from '@/features/auth/model/authStore';

import { ImageSlider } from '@/shared/ui/ImageSlider/ImageSlider';
import { UserAvatar } from '@/entities/user/ui/UserAvatar';
import { VerifiedBadge } from '@/entities/user/ui/VerifiedBadge';
import { Button } from '@/shared/ui/Button/Button';

import { formatDate } from '@/shared/lib/utils/formatDate';
import styles from './MarkerDetailsWidget.module.css';

interface MarkerDetailsWidgetProps {
    id: string;
}

export const MarkerDetailsWidget = ({ id }: MarkerDetailsWidgetProps) => {
    const navigate = useNavigate();
    const { canResolveMarkers, isOwner, isAdmin, isModerator } = usePermissions();

    const { data: marker, isLoading, isError } = useQuery({
        queryKey: ['marker', id],
        queryFn: () => markerApi.getDetails(id),
        enabled: !!id,
    });

    if (isLoading) return <div className={styles.loading}>Анализируем детали...</div>;
    if (isError || !marker) return <div className={styles.error}>Метка не найдена.</div>;

    const showDelete = isOwner(marker.creatorId) || isAdmin || isModerator;

    return (
        <div className={styles.container}>
            <ImageSlider urls={marker.imageUrls} height={480} />

            <div className={styles.mainPadding}>
                <header className={styles.topSection}>
                    <div className={styles.titleArea}>
                        {marker.isOfficial && (
                            <div className={styles.officialBadge}>
                                <VerifiedBadge /> Подтверждено службами
                            </div>
                        )}
                        <h1 className={styles.bigTitle}>{marker.title}</h1>
                        <div className={styles.yellowDivider} />
                    </div>
                    <StatusTimeline category={marker.category} currentStatus={marker.status} />
                </header>

                {marker.category === 'Event' && marker.scheduledAt && (
                    <div className={styles.eventTimeRange}>
                        <span>📅 Начало: {formatDate(marker.scheduledAt)}</span>
                        {marker.expiresAt && <span> — Конец: {formatDate(marker.expiresAt)}</span>}
                    </div>
                )}

                <div className={styles.bodyContent}>
                    <p className={styles.description}>{marker.description}</p>
                </div>

                <footer className={styles.authorSection}>
                    <UserAvatar
                        user={{
                            name: marker.creatorName,
                            avatarUrl: marker.creatorAvatarUrl,
                            roles: marker.isOfficial ? ['Official'] : [] // Чтобы аватар тоже подсветился если офиц.
                        }}
                        size="medium"
                    />
                    <div className={styles.authorMeta}>
                        <span className={styles.authorNick}>{marker.creatorName}</span>
                        <div className={styles.timeGroup}>
                            <span>Опубликовано {formatDate(marker.createdAt)}</span>
                            {marker.updatedAt && (
                                <span className={styles.updTime}> • Изменено</span>
                            )}
                        </div>
                    </div>
                </footer>

                <section className={styles.interactiveRow}>
                    <div className={styles.voteSide}>
                        <VoteButtons
                            markerId={marker.id}
                            currentVote={marker.userVote}
                            rating={marker.rating}
                        />
                        <Button
                            variant="outline"
                            size="small"
                            title="Пожаловаться"
                            onClick={() => navigate(`/marker/${marker.id}/report`)}
                        >
                            Пожаловаться
                        </Button>
                    </div>
                    <div className={styles.controlSide}>
                        {canResolveMarkers && marker.status !== 'Resolved' && (
                            <Button
                                size="small"
                                variant="secondary"
                                onClick={() => navigate(`/marker/${id}/resolve`)}
                            >
                                Дать ответ
                            </Button>
                        )}
                        {showDelete && <DeleteMarkerButton markerId={marker.id} />}
                    </div>
                </section>

                {marker.resolutions && marker.resolutions.length > 0 && (
                    <div className={styles.resolutionsHistory}>
                        <h3 className={styles.historyTitle}>Хронология решения</h3>
                        {marker.resolutions.map((res, i) => (
                            <article key={i} className={styles.resCard}>
                                <div className={styles.resHeader}>
                                    <VerifiedBadge />
                                    <span className={styles.resName}>{res.authorName}</span>
                                    <span className={styles.resDate}>{formatDate(res.createdAt)}</span>
                                </div>
                                <p className={styles.resComment}>{res.comment}</p>
                                <ImageSlider urls={res.imageUrls} height={280} />
                            </article>
                        ))}
                    </div>
                )}
            </div>
        </div>
    );
};
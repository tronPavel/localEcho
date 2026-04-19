import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { useNavigate } from 'react-router-dom';

import { deleteMarker, getMarkerDetails } from '../model/viewMarkerApi';
import { VoteButtons } from '../../vote-marker/ui/VoteButtons';
import { formatDate } from '../lib/formatDate';

import { UserAvatar } from "@/entities/user/ui/UserAvatar";
import { usePermissions } from "@/features/auth/model/authStore";
import { Button } from "@/shared/ui/Button/Button";
import { ImageSlider } from "@/shared/ui/ImageSlider/ImageSlider";

import styles from './MarkerDetailsFeature.module.css';
import {VerifiedBadge} from "@/entities/user/ui/VerifiedBadge.tsx";
import {StatusTimeline} from "@/entities/marker/ui/StatusTimeline.tsx";
import {toast} from "sonner";
import {ModalHeader} from "@/shared/ui/Modal/ModalHeader.tsx";

interface MarkerDetailsFeatureProps {
    id: string;
}
export const MarkerDetailsFeature = ({ id }: MarkerDetailsFeatureProps) => {
    const queryClient = useQueryClient();
    const navigate = useNavigate();
    const { isOwner, canAccessAdmin, canResolveMarkers } = usePermissions();

    const { data: marker, isLoading, isError } = useQuery({
        queryKey: ['marker', id],
        queryFn: () => getMarkerDetails(id),
        enabled: !!id,
    });

    const deleteMutation = useMutation({
        mutationFn: () => deleteMarker(id),
        onSuccess: () => {
            toast.success("Метка успешно удалена");
            queryClient.invalidateQueries({ queryKey: ['markers'] });
            navigate('/');
        }
    });

    if (isLoading) return <div className={styles.loading}>Анализируем ситуацию...</div>;
    if (isError || !marker) return <div className={styles.error}>Ошибка данных</div>;

    return (
        <div className={styles.container}>
            <ImageSlider urls={marker.imageUrls} height={480} />

            <div className={styles.mainPadding}>

                <div className={styles.titleSection}>
                    <ModalHeader title={marker.title}/>

                    <StatusTimeline category={marker.category} currentStatus={marker.status} />
                </div>

                {marker.category === 'Event' && marker.scheduledAt && (
                    <div className={styles.dateCard}>
                        <div className={styles.dateInfo}>
                            <span className={styles.dateLabel}>Начало</span>
                            <span className={styles.dateValue}>🗓 {formatDate(marker.scheduledAt)}</span>
                        </div>
                        <div className={styles.dateSeparator} />
                        <div className={styles.dateInfo}>
                            <span className={styles.dateLabel}>Завершение</span>
                            <span className={styles.dateValue}>🏁 {marker.expiresAt ? formatDate(marker.expiresAt) : 'До завершения'}</span>
                        </div>
                    </div>
                )}

                <p className={styles.description}>{marker.description}</p>

                <div className={styles.proAuthorBar}>
                    <div className={styles.authorBrief}>
                        <UserAvatar user={{
                            name: marker.creatorName,
                            avatarUrl: marker.creatorAvatarUrl,
                            roles: marker.resolution ? ['Official'] : [] // Умный аватар
                        }} size="medium" />
                        <div className={styles.authorTexts}>
                            <span className={styles.authorNick}>{marker.creatorName}</span>
                            <span className={styles.createdTime}>Опубликовано {formatDate(marker.createdAt)}</span>
                        </div>
                    </div>
                </div>
                <div className={styles.authorActions}>
                    <VoteButtons
                        markerId={marker.id}
                        currentVote={marker.userVote}
                        rating={marker.rating}
                    />
                    {canResolveMarkers && marker.status !== 'Resolved' && (
                        <Button variant="secondary" onClick={() => navigate(`/marker/${id}/resolve`)}>
                            Ответить
                        </Button>
                    )}
                    {(isOwner(marker.creatorId) || canAccessAdmin) && (
                        <Button variant="outline" className={styles.dangerBtn} onClick={() => deleteMutation.mutate()}>
                            🗑
                        </Button>
                    )}
                </div>
                {marker.resolution && (
                    <div className={styles.officialResolution}>
                        <div className={styles.resHeader}>
                            <VerifiedBadge />
                            <span className={styles.resName}>{marker.resolution.authorName}</span>
                            <span className={styles.resDate}>{formatDate(marker.resolution.createdAt)}</span>
                        </div>
                        <p className={styles.resText}>{marker.resolution.comment}</p>
                        <ImageSlider urls={marker.resolution.imageUrls} height={240} />
                    </div>
                )}
            </div>
        </div>
    );
};
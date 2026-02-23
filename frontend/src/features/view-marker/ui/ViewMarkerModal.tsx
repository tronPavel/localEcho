import { useQuery } from '@tanstack/react-query';
import { getMarkerDetails } from '../model/viewMarkerApi';
import { VoteButtons } from '../../vote-marker/ui/VoteButtons';
import { formatDate } from '../lib/formatDate';
import styles from './ViewMarkerModal.module.css';
import { useMarkerStore } from '../../../entities/marker/model/store';
import { Modal } from '../../../shared/ui/Modal/Modal';
import {getImageUrl} from "@/shared/api/apiInstance.ts";

export const ViewMarkerModal = () => {
    const { selectedMarker, setSelectedMarker } = useMarkerStore();

    const { data: fullMarker, isLoading, isError } = useQuery({
        queryKey: ['marker', selectedMarker?.id],
        queryFn: () => getMarkerDetails(selectedMarker!.id),
        enabled: !!selectedMarker?.id, // Не делать запрос, если ничего не выбрано
    });

    if (!selectedMarker) return null;

    return (
        <Modal isOpen={!!selectedMarker} onClose={() => setSelectedMarker(null)}>
            <div className={styles.container}>
                {isLoading ? (
                    <div className={styles.loading}>Загрузка информации...</div>
                ) : isError || !fullMarker ? (
                    <div className={styles.error}>Ошибка загрузки данных</div>
                ) : (
                    <>
                        {fullMarker.imageUrl ? (
                            <img src={getImageUrl(fullMarker.imageUrl)} alt={fullMarker.title} className={styles.image} />
                        ) : (
                            <div className={styles.noImage}>Фото отсутствует</div>
                        )}

                        <div className={styles.header}>
                            <h2>{fullMarker.title}</h2>
                            <span className={`${styles.status} ${styles[fullMarker.status.toLowerCase()]}`}>
                                {fullMarker.status === 'Active' && 'Активно'}
                                {fullMarker.status === 'InProgress' && 'В работе'}
                                {fullMarker.status === 'Resolved' && 'Решено'}
                            </span>
                        </div>

                        {fullMarker.description && (
                            <p className={styles.description}>{fullMarker.description}</p>
                        )}

                        <div className={styles.meta}>
                            <div>Создано: {formatDate(fullMarker.createdAt)}</div>
                            <div className={styles.author}>
                                Автор: {fullMarker.creatorName}
                                {fullMarker.creatorAvatarUrl && (
                                    <img src={getImageUrl(fullMarker.creatorAvatarUrl)} alt="" className={styles.avatar} />
                                )}
                            </div>
                        </div>

                        <div className={styles.voteSection}>
                            <VoteButtons
                                markerId={fullMarker.id}
                                currentVote={fullMarker.userVote}
                                rating={fullMarker.rating}
                            />
                        </div>

                        <div className={styles.commentsSection}>
                            <h3>Комментарии</h3>
                            <div className={styles.placeholder}>
                                💬 Комментарии будут доступны в ближайшее время
                            </div>
                        </div>
                    </>
                )}
            </div>
        </Modal>
    );
};
import { useState } from 'react';
import { useQuery } from '@tanstack/react-query';
import { Select } from '@/shared/ui/Select/Select';
import { UserAvatar } from '@/entities/user/ui/UserAvatar';
import { getLeaderboard } from '../model/leaderboardApi';
import { useAuthStore } from '@/features/auth/model/authStore';
import styles from './LeaderboardFeature.module.css';
import {getDistrictsList} from "@/entities/district/model/districtApi.ts";
import {classNames} from "@/shared/lib/utils/classNames.ts";

export const LeaderboardFeature = () => {
    const user = useAuthStore(s => s.user);
    const [districtId, setDistrictId] = useState<string | undefined>(user?.districtId || undefined);

    const { data: leaders = [], isFetching } = useQuery({
        queryKey: ['leaderboard', districtId],
        queryFn: () => getLeaderboard(districtId),
    });

    const { data: districts = [] } = useQuery({
        queryKey: ['districts-list'],
        queryFn: getDistrictsList,
    });

    const getCrown = (rank: number) => {
        if (rank === 0) return '👑';
        if (rank === 1) return '🥈';
        if (rank === 2) return '🥉';
        return rank + 1;
    };

    return (
        <div className={styles.container}>
            <div className={styles.filterCard}>
                <Select
                    label="Рейтинг по территории"
                    value={districtId || ''}
                    onChange={(e) => setDistrictId(e.target.value || undefined)}
                >
                    <option value="">Глобальный рейтинг</option>
                    {districts.map(d => <option key={d.id} value={d.id}>🏘 {d.name}</option>)}
                </Select>
            </div>

            <div className={styles.list}>
                {leaders.map((u, index) => (
                    <div
                        key={u.id}
                        className={classNames(styles.item, u.id === user?.userId && styles.isCurrentUser)}
                    >
                        <div className={styles.rankSide}>{getCrown(index)}</div>
                        <UserAvatar user={u} size="small" />
                        <div className={styles.info}>
                            <span className={styles.name}>{u.name}</span>
                        </div>
                        <div className={styles.score}>{u.points}</div>
                    </div>
                ))}
                {isFetching && <div className={styles.loadingState}>Обновление данных...</div>}
            </div>
        </div>
    );
};
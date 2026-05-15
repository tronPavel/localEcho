import { useState } from 'react';
import { useQuery } from '@tanstack/react-query';
import { Select } from '@/shared/ui/Select/Select';
import { UserAvatar } from '@/entities/user/ui/UserAvatar';
import { getLeaderboard } from '../model/leaderboardApi';
import { useAuthStore } from '@/features/auth/model/authStore';
import styles from './LeaderboardFeature.module.css';
import {districtApi} from "@/entities/district/model/districtApi.ts";
import {classNames} from "@/shared/lib/utils/classNames.ts";
import {cityApi} from "@/entities/city/api/cityApi.ts";

export const LeaderboardFeature = () => {
    const user = useAuthStore(s => s.user);
    const [cityId, setCityId] = useState<string>('');
    const [districtId, setDistrictId] = useState<string>('');

    const { data: cities = [] } = useQuery({ queryKey: ['cities'], queryFn: cityApi.getList });

    const { data: districts = [] } = useQuery({
        queryKey: ['districts', cityId],
        queryFn: () => districtApi.getList(cityId),
        enabled: !!cityId
    });

    const { data: leaders = [], isFetching } = useQuery({
        queryKey: ['leaderboard', cityId, districtId],
        queryFn: () => getLeaderboard({ cityId, districtId }),
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
                <Select label="Город" value={cityId} onChange={e => { setCityId(e.target.value); setDistrictId(''); }}>
                    <option value="">Весь мир</option>
                    {cities.map(c => <option key={c.id} value={c.id}>{c.name}</option>)}
                </Select>

                <Select label="Район" value={districtId} onChange={e => setDistrictId(e.target.value)} disabled={!cityId}>
                    <option value="">Все районы</option>
                    {districts.map(d => <option key={d.id} value={d.id}>{d.name}</option>)}
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
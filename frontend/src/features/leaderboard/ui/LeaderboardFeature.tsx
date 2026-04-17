import { useState } from 'react';
import { useQuery } from '@tanstack/react-query';
import { Select } from '@/shared/ui/Select/Select';
import { UserAvatar } from '@/entities/user/ui/UserAvatar';
import { getLeaderboard } from '../model/leaderboardApi';
import { getDistricts } from '@/entities/district/model/districtApi';
import { useAuthStore } from '@/features/auth/model/authStore';
import styles from './LeaderboardFeature.module.css';

export const LeaderboardFeature = () => {
    const user = useAuthStore(s => s.user);
    const [districtId, setDistrictId] = useState<string | undefined>(user?.districtId || undefined);

    const { data: leaders = [] } = useQuery({
        queryKey: ['leaderboard', districtId],
        queryFn: () => getLeaderboard(districtId),
    });

    const { data: districts = [] } = useQuery({
        queryKey: ['districts'],
        queryFn: getDistricts,
    });

    return (
        <div className={styles.container}>
            <div className={styles.filter}>
                <label>Фильтр района:</label>
                <Select
                    value={districtId || ''}
                    onChange={(e) => setDistrictId(e.target.value || undefined)}
                >
                    <option value="">Весь город</option>
                    {districts.map(d => (
                        <option key={d.id} value={d.id}>{d.name}</option>
                    ))}
                </Select>
            </div>

            <div className={styles.list}>
                {leaders.map((u, index) => (
                    <div key={u.id} className={styles.item}>
                        <span className={styles.rank}>{index + 1}</span>
                        <UserAvatar user={{ name: u.name, avatarUrl: u.avatarUrl }} size="small" />
                        <span className={styles.name}>{u.name}</span>
                        <span className={styles.points}>{u.points} pts</span>
                    </div>
                ))}
            </div>
        </div>
    );
};
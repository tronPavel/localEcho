import { Modal } from '@/shared/ui/Modal/Modal';
import { useQuery } from '@tanstack/react-query';
import { getLeaderboard } from '../model/leaderboardApi';
import { UserAvatar } from '@/entities/user/ui/UserAvatar';
import styles from './LeaderboardModal.module.css';
import { useState } from 'react';
import { useAuthStore } from '@/features/auth/model/authStore';
import {Select} from "@/shared/ui/Select/Select.tsx";

export const LeaderboardModal = ({ isOpen, onClose }: { isOpen: boolean; onClose: () => void }) => {
    const { user } = useAuthStore();
    const [districtId, setDistrictId] = useState<string | undefined>(user?.districtId);

    const { data: leaders = [] } = useQuery({
        queryKey: ['leaderboard', districtId],
        queryFn: () => getLeaderboard(districtId),
    });

    return (
        <Modal isOpen={isOpen} onClose={onClose}>
            <h2>Топ активистов</h2>

            <Select
                value={districtId || ''}
                onChange={(e) => setDistrictId(e.target.value || undefined)}
            >
                <option value="">Весь город</option>
                {/* TODO: позже загружать реальные районы */}
            </Select>

            <div className={styles.list}>
                {leaders.map((u, index) => (
                    <div key={u.id} className={styles.item}>
                        <span>{index + 1}</span>
                        <UserAvatar user={u} />
                        <span>{u.name}</span>
                        <span>{u.points} баллов</span>
                    </div>
                ))}
            </div>
        </Modal>
    );
};
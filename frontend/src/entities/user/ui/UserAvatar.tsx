import { getImageUrl } from '@/shared/api/apiInstance';
import styles from './UserAvatar.module.css';
import {VerifiedBadge} from "@/entities/user/ui/VerifiedBadge.tsx";

interface UserAvatarProps {
    user: {
        name?: string;
        avatarUrl?: string | null;
        roles?: string[];
    } | null | undefined;
    size?: 'small' | 'medium' | 'large';
}

export const UserAvatar = ({ user, size = 'medium' }: UserAvatarProps) => {
    const initials = user?.name?.charAt(0).toUpperCase() || '?';

    const checkIsOfficial = () => {
        if (!user) return false;


        const roles = user.roles || [];
        return roles.some(role => role === 'Official' || role === 'Admin');
    };

    const isOfficial = checkIsOfficial();

    return (
        <div className={styles.wrapper}>
            <div className={`${styles.avatar} ${styles[size]} ${isOfficial ? styles.officialBorder : ''}`}>
                {user?.avatarUrl ? (
                    <img src={getImageUrl(user.avatarUrl)} alt={user.name} />
                ) : (
                    <span className={styles.placeholder}>{initials}</span>
                )}
            </div>

            {isOfficial && (
                <div className={styles.verifiedBadgeWrapper}>
                    <VerifiedBadge />
                </div>
            )}
        </div>
    );
};
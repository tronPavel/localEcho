import styles from './UserAvatar.module.css';
import { getImageUrl } from '@/shared/api/apiInstance';

interface AvatarUser {
    name?: string;
    avatarUrl?: string | null;
}

interface UserAvatarProps {
    user: AvatarUser;
    size?: 'small' | 'medium' | 'large';
}

export const UserAvatar = ({ user, size = 'medium' }: UserAvatarProps) => {
    const initials = user?.name?.charAt(0).toUpperCase() || '?';

    const containerClasses = `${styles.avatar} ${styles[size]}`;

    return (
        <div className={containerClasses}>
            {user?.avatarUrl ? (
                <img src={getImageUrl(user.avatarUrl)} alt={user.name} />
            ) : (
                <span className={styles.placeholder}>{initials}</span>
            )}
        </div>
    );
};
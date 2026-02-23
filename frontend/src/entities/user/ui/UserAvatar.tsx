import styles from './UserAvatar.module.css';
import type {UserDto} from "../model/types.ts";
import {getImageUrl} from "@/shared/api/apiInstance.ts";

interface UserAvatarProps {
    user: UserDto;
    size?: 'small' | 'medium' | 'large';
}

export const UserAvatar = ({ user, size = 'medium' }: UserAvatarProps) => {
    const avatarSize = {
        small: 32,
        medium: 48,
        large: 64,
    }[size];

    return (
        <div className={styles.avatar} style={{ width: avatarSize, height: avatarSize }}>
            {user.avatarUrl ? (
                <img src={getImageUrl(user.avatarUrl)} alt={user.name} />
            ) : (
                <span>{user.name[0]}</span>
            )}
        </div>
    );
};
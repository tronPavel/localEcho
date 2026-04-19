import { useAuthStore } from '@/features/auth/model/authStore';
import { UserAvatar } from '@/entities/user/ui/UserAvatar';
import { Button } from '@/shared/ui/Button/Button';
import styles from './MapHeader.module.css';

export const MapHeader = ({ onOpenLeaderboard, onOpenProfile, onOpenLogin, onOpenRegister }: any) => {
    const { isAuthenticated, user } = useAuthStore();

    return (
        <header className={styles.header}>
            <div className={styles.logoGroup}>
                <h1 className={styles.brand}>Local<span>Echo</span></h1>
            </div>

            <div className={styles.actions}>
                <Button variant="outline" size="small" onClick={onOpenLeaderboard}>
                    🏆 Рейтинг
                </Button>

                {isAuthenticated ? (
                    <div className={styles.profilePill} onClick={onOpenProfile}>
                        <div className={styles.userText}>
                            <span className={styles.nick}>{user?.name}</span>
                            <span className={styles.points}>{user?.points} pts</span>
                        </div>
                        <UserAvatar user={user} size="small" />
                    </div>
                ) : (
                    <div className={styles.authButtons}>
                        <Button onClick={onOpenLogin} size="small" variant="secondary">Войти</Button>
                        <Button onClick={onOpenRegister} size="small">Регистрация</Button>
                    </div>
                )}
            </div>
        </header>
    );
};
import {useAuthStore, usePermissions} from '@/features/auth/model/authStore.ts';
import { UserAvatar } from '@/entities/user/ui/UserAvatar.tsx';
import { Button } from '@/shared/ui/Button/Button.tsx';
import styles from './MapHeader.module.css';
import {useNavigate} from "react-router-dom";

export const MapHeader = ({  onOpenProfile, onOpenLogin, onOpenRegister }: any) => {
    const { isAuthenticated, user } = useAuthStore();
    const {  canAccessDashboard } = usePermissions();
    const navigate = useNavigate();
    return (
        <header className={styles.header}>
            <div className={styles.logoGroup}>
                <h1 className={styles.brand}>Local<span>Echo</span></h1>
            </div>

            <div className={styles.actions}>
                <Button
                    variant="outline"
                    size="small"
                    className={styles.navBtn}
                    onClick={() => navigate('/analytics')}
                >
                    📊 Статистика города
                </Button>

                <Button variant="outline" size="small" onClick={() => navigate('/leaderboard')}>
                    🏆 Лидеры
                </Button>

                {canAccessDashboard && (
                    <Button variant="secondary" size="small" onClick={() => navigate('/dashboard')}>
                        ⚙️ Админка
                    </Button>
                )}

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
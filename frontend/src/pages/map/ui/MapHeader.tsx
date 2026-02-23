import { Button } from '@/shared/ui/Button/Button';
import { useAuthStore } from '@/features/auth/model/authStore';
import styles from './MapPage.module.css';

interface MapHeaderProps {
    onOpenLeaderboard: () => void;
    onOpenProfile: () => void;
    onOpenLogin: () => void;
    onOpenRegister: () => void;
}

export const MapHeader = ({
                              onOpenLeaderboard,
                              onOpenProfile,
                              onOpenLogin,
                              onOpenRegister
                          }: MapHeaderProps) => {
    const isAuthenticated = useAuthStore((state) => state.isAuthenticated);
    const clearUser = useAuthStore((state) => state.clearUser);

    return (
        <header className={styles.header}>
            <h1>Local Echo</h1>
            <div style={{ display: 'flex', gap: '12px' }}>
                <Button onClick={onOpenLeaderboard}>Топ</Button>

                {isAuthenticated ? (
                    <>
                        <Button onClick={onOpenProfile}>Профиль</Button>
                        <Button variant="outline" onClick={clearUser}>Выйти</Button>
                    </>
                ) : (
                    <>
                        <Button onClick={onOpenLogin}>Войти</Button>
                        <Button variant="secondary" onClick={onOpenRegister}>Регистрация</Button>
                    </>
                )}
            </div>
        </header>
    );
};
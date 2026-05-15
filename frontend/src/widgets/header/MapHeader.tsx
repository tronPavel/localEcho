import {useAuthStore, usePermissions} from '@/features/auth/model/authStore.ts';
import { UserAvatar } from '@/entities/user/ui/UserAvatar.tsx';
import { Button } from '@/shared/ui/Button/Button.tsx';
import styles from './MapHeader.module.css';
import {useNavigate} from "react-router-dom";
import {cityApi} from "@/entities/city/api/cityApi.ts";
import {useQuery} from "@tanstack/react-query";
import {useCityStore} from "@/features/city-selector/model/cityStore.ts";
import {Select} from "@/shared/ui/Select/Select.tsx";

export const MapHeader = ({  onOpenProfile, onOpenLogin, onOpenRegister }: any) => {
    const { isAuthenticated, user } = useAuthStore();
    const {  canAccessDashboard } = usePermissions();
    const navigate = useNavigate();

    const { currentCityId, setCity } = useCityStore();
    const { data: cities = [] } = useQuery({
        queryKey: ['cities-list'],
        queryFn: cityApi.getList
    });
    const handleCityChange = (e: React.ChangeEvent<HTMLSelectElement>) => {
        const id = e.target.value;

        if (!id) {
            setCity(null, 'Все города', null);
            return;
        }

        const selectedCity = cities.find(c => c.id === id);

        if (selectedCity) {
            const focus: [number, number] = [selectedCity.lat, selectedCity.lng];
            setCity(id, selectedCity.name, focus);
        }
    };
    return (
        <header className={styles.header}>
            <div className={styles.logoGroup}>
                <h1 className={styles.brand}>Local<span>Echo</span></h1>

            <div className={styles.cityPicker}>
                <Select
                    value={currentCityId || ''}
                    onChange={handleCityChange}
                    className={styles.citySelect}
                >
                    <option value="">Весь мир</option>
                    {cities.map(c => <option key={c.id} value={c.id}>{c.name}</option>)}
                </Select>
            </div>
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
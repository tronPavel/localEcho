import { Outlet, useNavigate } from 'react-router-dom';
import styles from './MapPage.module.css';
import {MapWidget} from "@/widgets/map";
import {MapHeader} from "@/widgets/header/MapHeader.tsx";

export const MapPage = () => {
    const navigate = useNavigate();

    return (
        <div className={styles.page}>
            <MapHeader
                onOpenProfile={() => navigate('/profile')}
                onOpenLeaderboard={() => navigate('/leaderboard')}
                onOpenLogin={() => navigate('/login')}
                onOpenRegister={() => navigate('/register')}
            />

            <main className={styles.content}>
                <MapWidget />
            </main>

            <Outlet />
        </div>
    );
};
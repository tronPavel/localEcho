import { Outlet, useNavigate } from 'react-router-dom';
import { useQuery, keepPreviousData } from '@tanstack/react-query';
import { MapHeader } from '../components/MapHeader';
import { MapSidebar } from '../components/MapSidebar';
import { MapView } from '../components/MapView';
import { getMarkers } from '@/features/create-marker/model/createMarkerApi';
import { useFilterStore } from '@/features/filter-markers/model/filterStore';
import styles from './MapPage.module.css';
import {MapToolbar} from "@/pages/map/components/MapToolbar.tsx";
import {useLoadingDelay} from "@/shared/lib/hooks/useLoadingDelay.ts";

export const MapPage = () => {
    const navigate = useNavigate();
    const { category, status, bounds } = useFilterStore();

    const { data: markers = [], isFetching } = useQuery({
        queryKey: ['markers', category, status, bounds],
        queryFn: () => getMarkers({ category: category || undefined, status: status || undefined, ...bounds }),
        enabled: !!bounds,
        placeholderData: keepPreviousData,
        staleTime: 10000,
    });

    const showLoading = useLoadingDelay(isFetching, 400);

    return (
        <div className={styles.app}>
            <MapHeader
                onOpenProfile={() => navigate('/profile')}
                onOpenLeaderboard={() => navigate('/leaderboard')}
                onOpenLogin={() => navigate('/login')}
                onOpenRegister={() => navigate('/register')}
            />

            <main className={styles.main}>
                <MapSidebar />
                <div className={styles.mapWrapper}>
                    {showLoading && (
                        <div className={styles.loadingIndicator}>
                            <span>Обновление...</span>
                        </div>
                    )}

                    <MapView markers={markers} />

                    <MapToolbar />
                </div>
            </main>

            <Outlet />
        </div>
    );
};
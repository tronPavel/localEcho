import { Outlet, useNavigate } from 'react-router-dom';
import {keepPreviousData, useQuery} from '@tanstack/react-query';
import { MapHeader } from '../components/MapHeader';
import { MapSidebar } from '../components/MapSidebar';
import { CreateMarkerModal } from '@/features/create-marker/ui/CreateMarkerModal';
import { getMarkers } from '@/features/create-marker/model/createMarkerApi';
import { useFilterStore } from '@/features/filter-markers/model/filterStore';
import styles from './MapPage.module.css';
import {MapView} from "@/pages/map/components/MapView.tsx";
import {useEffect, useState} from "react";

export const MapPage = () => {
    const navigate = useNavigate();
    const { category, status, bounds } = useFilterStore();

    const { data: markers = [], isFetching } = useQuery({
        queryKey: ['markers', category, status, bounds],
        queryFn: () => getMarkers({
            category: category || undefined,
            status: status || undefined,
            ...bounds
        }),
        enabled: !!bounds,
        placeholderData: keepPreviousData, // Не убирает старые маркеры при обновлении
        staleTime: 5000, // Данные считаются "свежими" 5 секунд (защита от лишних запросов)
    });
    const [showLoading, setShowLoading] = useState(false);

    useEffect(() => {
        let timer: ReturnType<typeof setTimeout>;

        if (isFetching) {
            // Если грузится долго (больше 400мс), показываем индикатор
            timer = setTimeout(() => setShowLoading(true), 400);
        } else {
            setShowLoading(false);
        }

        return () => clearTimeout(timer);
    }, [isFetching]);
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
                </div>
            </main>

            {/* Место для отрисовки модалок (ProfilePage, RegisterPage и др.) */}
            <Outlet />

            {/* Логика создания маркера через Zustand по клику на карте */}
            <CreateMarkerModal />
        </div>
    );
};
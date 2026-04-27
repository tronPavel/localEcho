import {useLoadingDelay} from "@/shared/lib/hooks/useLoadingDelay.ts";
import {useFilterStore} from "@/features/filter-markers";
import {keepPreviousData, useQuery} from "@tanstack/react-query";
import {markerApi} from "@/entities/marker";
import {MapSidebar} from "@/widgets/map/ui/MapSidebar/MapSidebar.tsx";
import styles from './MapWidget.module.css';
import {MapView} from "@/widgets/map/ui/MapView/MapView.tsx";
import {MapToolbar} from "@/widgets/map/ui/MapToolbar/MapToolbar.tsx";

export const MapWidget = () => {
    const { category, status, bounds } = useFilterStore();

    const { data: markers = [], isFetching } = useQuery({
        queryKey: ['markers', category, status, bounds],
        queryFn: () => markerApi.getForMap({ category, status, ...bounds }),
        enabled: !!bounds,
        placeholderData: keepPreviousData,
    });

    const showLoading = useLoadingDelay(isFetching, 400);

    return (
        <div className={styles.container}>
            <MapSidebar />
            <div className={styles.mapViewport}>
                {showLoading && (
                    <div className={styles.loadingToast}>🌲 Обновление Минска...</div>
                )}
                {/* Рендерим компонент в котором ЕДИНСТВЕННЫЙ MapContainer */}
                <MapView markers={markers} />
                <MapToolbar />
            </div>
        </div>
    );
};
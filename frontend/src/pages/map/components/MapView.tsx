import { MapContainer, TileLayer, Marker, Tooltip } from 'react-leaflet';
import MarkerClusterGroup from "react-leaflet-cluster";
import { createMarkerIcon } from '@/entities/marker/ui/MarkerIcon.tsx';
import { useNavigate } from 'react-router-dom';
import type { MarkerMapDto } from '@/entities/marker/model/types.ts';
import { MapController, MapBoundsTracker, MapClickHandler } from '@/pages/map/components/MapPlugins.tsx'
import styles from './MapView.module.css';

interface MapViewProps {
    markers: MarkerMapDto[];
}

export const MapView = ({ markers }: MapViewProps) => {
    const navigate = useNavigate();

    return (
        <MapContainer
            center={[55.7558, 37.6173]}
            zoom={13}
            className={styles.container}
            zoomControl={false} // для красоты можно убрать стандартный зум
        >
            <TileLayer url="https://{s}.tile.openstreetmap.org/{z}/{x}/{y}.png" />

            {/* Наши плагины управления */}
            <MapController />
            <MapBoundsTracker />
            <MapClickHandler />

            <MarkerClusterGroup chunkedLoading>
                {markers.map((marker) => (
                    <Marker
                        key={marker.id}
                        position={[marker.latitude, marker.longitude]}
                        icon={createMarkerIcon(marker.category)}
                        eventHandlers={{
                            click: () => navigate(`/marker/${marker.id}`),
                        }}
                    >
                        <Tooltip direction="top" offset={[0, -20]}>
                            {marker.title}
                        </Tooltip>
                    </Marker>
                ))}
            </MarkerClusterGroup>
        </MapContainer>
    );
};
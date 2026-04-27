import { MapContainer, TileLayer, Marker, Polygon, Tooltip } from 'react-leaflet';
import MarkerClusterGroup from "react-leaflet-cluster";
import { useNavigate } from 'react-router-dom';
import { createMarkerIcon, getCategoryColor } from '@/entities/marker';


import type { MarkerMapResponse } from '@/entities/marker';
import styles from './MapView.module.css';
import {MapController} from "@/widgets/map/ui/MapView/plugins/MapController.tsx";
import {DistrictsLayer} from "@/widgets/map/ui/MapView/plugins/DistrictsLayer.tsx";
import {MapBoundsTracker} from "@/widgets/map/ui/MapView/plugins/MapBoundsTracker.tsx";
import {GeomanControl} from "@/widgets/map/ui/MapView/plugins/GeomanControl.tsx";
import {PositionControl} from "@/widgets/map/ui/MapView/plugins/PositionControl.tsx";
import {SearchControl} from "@/widgets/map/ui/MapView/plugins/SearchControl.tsx";
import {DrawingPreviewLayer} from "@/widgets/map/ui/MapView/plugins/DrawingPreviewLayer.tsx";

interface MapViewProps {
    markers: MarkerMapResponse[];
}

export const MapView = ({ markers }: MapViewProps) => {
    const navigate = useNavigate();

    return (
        <MapContainer
            center={[53.90, 27.56]}
            zoom={12}
            className={styles.container}
            zoomControl={false}
        >
            <TileLayer url="https://{s}.tile.openstreetmap.org/{z}/{x}/{y}.png" />

            {/* ГИС СЛОИ */}
            <DistrictsLayer />

            {/* ПЛАГИНЫ ЛОГИКИ */}
            <MapController />
            <MapBoundsTracker />
            <GeomanControl />
            <PositionControl />
            <SearchControl />

            {/* ПРЕДПРОСМОТР РИСОВАНИЯ */}
            <DrawingPreviewLayer />

            {/* РЕНДЕРИНГ ПОЛИГОНАЛЬНЫХ МЕТОК */}
            {markers
                .filter(m => m.geometryType === 'Polygon')
                .map(m => (
                    <Polygon
                        key={`poly-${m.id}`}
                        positions={m.coordinates.map(c => [c.lat, c.lng])}
                        pathOptions={{
                            color: getCategoryColor(m.category),
                            fillOpacity: 0.25,
                            weight: 3
                        }}
                        eventHandlers={{ click: () => navigate(`/marker/${m.id}`) }}
                    />
                ))
            }

            {/* РЕНДЕРИНГ ИКОНОК (Точки и Центроиды полигонов) */}
            <MarkerClusterGroup chunkedLoading spiderfyOnMaxZoom>
                {markers.map((m) => (
                    <Marker
                        key={m.id}
                        position={[m.centroid.lat, m.centroid.lng]}
                        icon={createMarkerIcon(m.category, m.status)}
                        zIndexOffset={m.category === 'Project' ? -500 : 100}
                        eventHandlers={{ click: () => navigate(`/marker/${m.id}`) }}
                    >
                        <Tooltip direction="top" offset={[0, -20]}>
                            <strong>{m.title}</strong>
                        </Tooltip>
                    </Marker>
                ))}
            </MarkerClusterGroup>
        </MapContainer>
    );
};
import { MapContainer, TileLayer, Marker, Polygon, Tooltip } from 'react-leaflet';
import MarkerClusterGroup from "react-leaflet-cluster";
import { useNavigate } from 'react-router-dom';
import { createMarkerIcon } from '@/entities/marker/ui/MarkerIcon';
import type { MarkerMapDto } from '@/entities/marker/model/types';
import styles from './MapView.module.css';
import {MapBoundsTracker, MapController} from "@/pages/map/components/MapPlugins.tsx";
import {DrawingPreviewLayer} from "@/pages/map/components/DrawingPreviewLayer.tsx";
import {getCategoryColor} from "@/entities/marker/lib/getCategoryColor.ts";
import {GeomanControl} from "@/pages/map/components/GeomanControl.tsx";
import {DistrictsLayer} from "@/pages/map/components/DistrictsLayer.tsx";
import {SearchControl} from "@/pages/map/components/SearchControl.tsx";
import {PositionControl} from "@/pages/map/components/PositionControl.tsx";

interface MapViewProps {
    markers: MarkerMapDto[];
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
            <DistrictsLayer />
            <MapController />
            <MapBoundsTracker />
            <GeomanControl />
            <DrawingPreviewLayer />
            <SearchControl />
            <PositionControl />
            {markers
                .filter(m => m.geometryType === 'Polygon')
                .map(m => (
                    <Polygon
                        key={m.id}
                        positions={m.coordinates.map(c => [c.lat, c.lng])}
                        pathOptions={{ color: getCategoryColor(m.category), fillOpacity: 0.2 }}
                        eventHandlers={{ click: () => navigate(`/marker/${m.id}`) }}
                    />
                ))
            }

            <MarkerClusterGroup chunkedLoading>
                {markers.map((m) => (
                    <Marker
                        key={m.id}
                        position={[m.centroid.lat, m.centroid.lng]}
                        zIndexOffset={m.category === 'Project' ? -100 : 100}
                        icon={createMarkerIcon(m.category)}
                        eventHandlers={{ click: () => navigate(`/marker/${m.id}`) }}
                    >
                        <Tooltip direction="top" offset={[0, -20]}>
                            {m.title}
                        </Tooltip>
                    </Marker>
                ))}
            </MarkerClusterGroup>
        </MapContainer>
    );
};